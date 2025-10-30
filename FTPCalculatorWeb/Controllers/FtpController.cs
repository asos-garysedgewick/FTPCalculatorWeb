using Microsoft.AspNetCore.Mvc;
using FTPCalculatorWeb.Services;
using System.Text.Json;

namespace FTPCalculatorWeb.Controllers
{
    public class FtpController : Controller
    {
        private readonly FtpCalculator _ftpCalculator;

        public FtpController(FtpCalculator ftpCalculator)
        {
            _ftpCalculator = ftpCalculator;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fitFile)
        {
            if (fitFile == null || fitFile.Length == 0)
            {
                ViewBag.Error = "Please select a .fit file.";
                return View();
            }

            var tempPath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(tempPath))
            {
                await fitFile.CopyToAsync(stream);
            }

            double ftp;
            double avgPower;
            List<double> powerValues = null;
            List<double> cadenceValues = null;
            try
            {
                // Use the new method to get both FTP and average power
                (ftp, avgPower) = _ftpCalculator.CalculateFtpAndAverageFromFitFile(tempPath);

                var csvPath = Path.ChangeExtension(tempPath, ".csv");
                // Extract power values
                powerValues = _ftpCalculator.ParsePowerValuesFromCsv(csvPath);

                // Extract cadence values
                cadenceValues = _ftpCalculator.ParseCadenceValuesFromCsv(csvPath);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error processing file: " + ex.Message;
                return View();
            }
            finally
            {
                System.IO.File.Delete(tempPath);
                var csvPath = Path.ChangeExtension(tempPath, ".csv");
                if (System.IO.File.Exists(csvPath))
                    System.IO.File.Delete(csvPath);
            }

            ViewBag.Ftp = Math.Round(ftp, 0); // Show FTP as a whole number
            ViewBag.AveragePower = avgPower; // Pass the full ride average power to the view
            ViewBag.PowerValuesJson = JsonSerializer.Serialize(powerValues);
            ViewBag.CadenceValuesJson = JsonSerializer.Serialize(cadenceValues);

            // After powerValues is loaded
            int ftpWindow = 20 * 60;
            double ftpWindowAverage = 0;
            if (powerValues != null && powerValues.Count >= ftpWindow)
            {
                double maxAvg = 0;
                for (int i = 0; i <= powerValues.Count - ftpWindow; i++)
                {
                    double windowAvg = powerValues.Skip(i).Take(ftpWindow).Average();
                    if (windowAvg > maxAvg)
                    {
                        maxAvg = windowAvg;
                    }
                }
                ftpWindowAverage = Math.Round(maxAvg, 0); // 0 decimal places
            }
            ViewBag.FtpWindowAverage = ftpWindowAverage;

            var powerZones = _ftpCalculator.CalculateCogganPowerZones(powerValues, ftp);
            ViewBag.PowerZonesJson = JsonSerializer.Serialize(powerZones);

            // Calculate time spent in each zone (in minutes, assuming 1Hz sampling)
            var timeInZones = CalculateTimeInZones(powerZones);
            ViewBag.TimeInZones = JsonSerializer.Serialize(timeInZones);

            return View();
        }

        // Updated method to calculate time in zones based on zone assignments
        private List<double> CalculateTimeInZones(List<int> powerZones)
        {
            double[] counts = new double[7];
            foreach (int zone in powerZones)
            {
                if (zone >= 1 && zone <= 7)
                {
                    counts[zone - 1] += 1.0;
                }
            }
            List<double> timeInZones = new List<double>();
            for (int i = 0; i < 7; i++)
            {
                timeInZones.Add(Math.Round(counts[i] / 60.0));
            }
            return timeInZones;
        }
    }
}