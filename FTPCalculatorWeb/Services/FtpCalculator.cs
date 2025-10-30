using System.Globalization;
using Dynastream.Fit;

namespace FTPCalculatorWeb.Services
{
    // This class provides methods to calculate FTP (Functional Threshold Power) from a FIT file.
    public class FtpCalculator
    {
        // Main method to calculate FTP from a FIT file.
        // It converts the FIT file to CSV, parses power values, and calculates FTP.
        public double CalculateFtpFromFitFile(string fitFilePath)
        {
            string csvFilePath = Path.ChangeExtension(fitFilePath, ".csv");
            ConvertFitToCsv(fitFilePath, csvFilePath);
            var powerValues = ParsePowerValuesFromCsv(csvFilePath);
            var ftpResult = CalculateFtp(powerValues, 20 * 60);

            if (ftpResult is double ftp)
            {
                // Round to 2 decimal places before returning
                return Math.Round(ftp, 2);
            }
            else
            {
                throw new InvalidOperationException(ftpResult.ToString());
            }
        }

        // Converts a FIT file to a CSV file.
        // Extracts data from each record in the FIT file and writes it as a row in the CSV.
        private void ConvertFitToCsv(string fitFilePath, string csvFilePath)
        {
            // Open the FIT file for reading.
            using var fitStream = new FileStream(fitFilePath, FileMode.Open, FileAccess.Read);

            // Create a FIT decoder and message broadcaster.
            var decoder = new Dynastream.Fit.Decode();
            var mesgBroadcaster = new MesgBroadcaster();
            var records = new List<RecordMesg>();

            // Subscribe to events to collect record messages.
            decoder.MesgEvent += mesgBroadcaster.OnMesg;
            decoder.MesgDefinitionEvent += mesgBroadcaster.OnMesgDefinition;

            // When a record message is received, add it to the list.
            mesgBroadcaster.RecordMesgEvent += (sender, e) =>
            {
                if (e.mesg is RecordMesg recordMesg)
                {
                    records.Add(recordMesg);
                }
            };

            // Read and decode the FIT file.
            decoder.Read(fitStream);

            // Write the collected records to a CSV file.
            using var writer = new StreamWriter(csvFilePath);
            
            // Write the CSV header.
            writer.WriteLine("Timestamp,PositionLat,PositionLong,Distance,Speed,HeartRate,Cadence,Altitude,Temperature,Power");
            foreach (var record in records)
            {
                // Write each record's data as a CSV row.
                writer.WriteLine($"{record.GetTimestamp()}," +
                    $"{record.GetPositionLat()}," +
                    $"{record.GetPositionLong()}," +
                    $"{record.GetDistance()}," +
                    $"{record.GetSpeed()}," +
                    $"{record.GetHeartRate()}," +
                    $"{record.GetCadence()}," +
                    $"{record.GetAltitude()}," +
                    $"{record.GetTemperature()}," +
                    $"{record.GetPower()}");
            }
        }

        // Reads the CSV file and extracts the power values from each row.
        internal List<double> ParsePowerValuesFromCsv(string csvFilePath)
        {
            // Read all lines from the CSV file, skipping the header.
            var lines = System.IO.File.ReadAllLines(csvFilePath).Skip(1);
            var powerValues = new List<double>();
            foreach (var line in lines)
            {
                // Split the line into columns.
                var columns = line.Split(',');
                
                // If there are enough columns and the power value can be parsed, add it to the list.
                if (columns.Length > 9 && double.TryParse(columns[9], NumberStyles.Any, CultureInfo.InvariantCulture, out double power))
                {
                    powerValues.Add(power);
                }
            }
            return powerValues;
        }

        // Reads the CSV file and extracts the cadence values from each row.
        internal List<double> ParseCadenceValuesFromCsv(string csvFilePath)
        {
            var cadenceValues = new List<double>();
            using (var reader = new StreamReader(csvFilePath))
            {
                string headerLine = reader.ReadLine();
                var headers = headerLine.Split(',');
                // Use case-insensitive search for "Cadence"
                int cadenceIndex = Array.FindIndex(headers, h => h.Trim().Equals("Cadence", StringComparison.OrdinalIgnoreCase));
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var columns = line.Split(',');
                    if (cadenceIndex >= 0 && cadenceIndex < columns.Length)
                    {
                        if (double.TryParse(columns[cadenceIndex], out double cadence))
                            cadenceValues.Add(cadence);
                        else
                            cadenceValues.Add(0);
                    }
                }
            }
            return cadenceValues;
        }

        // Calculates the FTP value from the list of power values.
        // Uses a rolling window to find the highest average power over the specified window size.
        // FTP is estimated as 95% of the highest 20-minute average power.
        internal object CalculateFtp(List<double> powerValues, int totalSeconds)
        {
            if (powerValues == null || powerValues.Count < totalSeconds)
            {
                return "Not enough data. This could be because a 1 x 20 minute effort or Power data could not be detected to estimate the FTP";
            }

            double maxAvgPower = 0;
            // Use a rolling window to find the highest average over any 20-minute period
            for (int i = 0; i <= powerValues.Count - totalSeconds; i++)
            {
                double windowAvg = powerValues.Skip(i).Take(totalSeconds).Average();
                if (windowAvg > maxAvgPower)
                {
                    maxAvgPower = windowAvg;
                }
            }
            return maxAvgPower * 0.95;
        }

        // Add this method to FtpCalculator
        public double CalculateAveragePower(List<double> powerValues)
        {
            if (powerValues == null || powerValues.Count == 0)
                return 0;
            return Math.Round(powerValues.Average(), 2);
        }

        // Update CalculateFtpFromFitFile to return both FTP and average power
        public (double ftp, double avgPower) CalculateFtpAndAverageFromFitFile(string fitFilePath)
        {
            if (string.IsNullOrEmpty(fitFilePath))
                throw new ArgumentException("Please select a .fit file.");

            string csvFilePath = Path.ChangeExtension(fitFilePath, ".csv");
            ConvertFitToCsv(fitFilePath, csvFilePath);
            var powerValues = ParsePowerValuesFromCsv(csvFilePath);
            var ftpResult = CalculateFtp(powerValues, 20 * 60);

            if (ftpResult is double ftp)
            {
                double avgPower = CalculateAveragePower(powerValues);
                return (Math.Round(ftp, 2), avgPower);
            }
            else
            {
                throw new InvalidOperationException(ftpResult.ToString());
            }
        }

        // Calculates Coggan Power Zones based on the given power values and FTP.
        // Zones are assigned as follows:
        // Zone 1: <55%
        // Zone 2: 55-75%
        // Zone 3: 76-90%
        // Zone 4: 91-105%
        // Zone 5: 106-120%
        // Zone 6: 121-150%
        // Zone 7: >150%
        public List<int> CalculateCogganPowerZones(List<double> powerValues, double ftp)
        {
            var zones = new List<int>();
            foreach (var power in powerValues)
            {
                double percent = (ftp > 0) ? (power / ftp) * 100 : 0;
                if (percent < 55) zones.Add(1);           // Zone 1: <55%
                else if (percent < 76) zones.Add(2);      // Zone 2: 55-75%
                else if (percent < 91) zones.Add(3);      // Zone 3: 76-90%
                else if (percent < 106) zones.Add(4);     // Zone 4: 91-105%
                else if (percent < 121) zones.Add(5);     // Zone 5: 106-120%
                else if (percent < 151) zones.Add(6);     // Zone 6: 121-150%
                else zones.Add(7);                        // Zone 7: >150%
            }
            return zones;
        }
    }
}