using FTPCalculatorWeb.Services;
using Xunit;
using System.Collections.Generic;

namespace FtpCalculatorTests
{
    public class FtpCalculatorTests
    {
        /// <summary>
        /// This test verifies the FTP calculation logic in the FtpCalculator class.
        /// It simulates a 20-minute ride with a constant power output of 196 watts.
        /// The test uses reflection to invoke the private CalculateFtp method of FtpCalculator,
        /// which is responsible for computing FTP as 95% of the highest 20-minute average power.
        /// The expected FTP result is 186.2 watts (196 * 0.95).
        /// 
        /// Test coverage:
        /// - Class: FTPCalculatorWeb.Services.FtpCalculator
        /// - Method: private object CalculateFtp(List&lt;double&gt; powerValues, int totalSeconds)
        /// </summary>
        
        [Fact] //The [Fact] attribute in xUnit marks a method as a test method
        public void CalculateFtp_20_Min_196W_Returns_Expected_Ftp()
        {
            // Arrange: Create an instance of FtpCalculator and simulate 20 minutes of 196W power values
            var calculator = new FtpCalculator();
            int totalMinutes= 20;
            var powerValues = new List<double>();
            for (int i = 0; i < totalMinutes; i++)
                powerValues.Add(196);

            // Act: Use reflection to call the private CalculateFtp method
            var method = typeof(FtpCalculator).GetMethod("CalculateFtp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(calculator, new object[] { powerValues, totalMinutes });

            // Assert: The FTP should be 186.2 watts (95% of 196)
            Assert.IsType<double>(result);
            Assert.Equal(186.2, (double)result, 1); // 196 * 0.95 = 186.2
        }

        /// <summary>
        /// This test verifies that FtpCalculator reports only the higher FTP value
        /// when two different 20-minute average power segments are present in the ride.
        /// It simulates a 60-minute ride with three 20-minute segments:
        /// - 20 minutes at 196W
        /// - 20 minutes at 296W (the highest)
        /// - 20 minutes at 50W
        /// The FTP should be 95% of 296W, not 196W or 50W.
        /// </summary>

        [Fact] //The [Fact] attribute in xUnit marks a method as a test method
        public void CalculateFtp_Only_Returns_The_Highest_Ftp_Detected_Given_Multiple_20_Min_Intervals()
        {
            // Arrange: 20 min @ 196W, 20 min @ 296W, 20 min @ 50W
            var calculator = new FtpCalculator();
            int segmentMinutes = 20;
            var powerValues = new List<double>();
            powerValues.AddRange(Enumerable.Repeat(196d, segmentMinutes));
            powerValues.AddRange(Enumerable.Repeat(296d, segmentMinutes));
            powerValues.AddRange(Enumerable.Repeat(50d, segmentMinutes));

            // Act: Use reflection to call the private CalculateFtp method
            var method = typeof(FtpCalculator).GetMethod("CalculateFtp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(calculator, new object[] { powerValues, segmentMinutes });

            // Assert: Only the highest 20-min (281.2W) is returned for FTP
            Assert.IsType<double>(result);
            Assert.Equal(281.2, (double)result, 1); // Should be 281.2
        }

        /// <summary>
        /// This test verifies that the FtpCalculator returns an error message
        /// when the ride duration is less than 20 minutes (i.e., not enough data points
        /// to calculate a 20-minute FTP). It simulates a ride with only 19 minutes of data
        /// and asserts that the result is a string containing an appropriate error message.
        /// 
        /// Test coverage:
        /// - Class: FTPCalculatorWeb.Services.FtpCalculator
        /// - Method: private object CalculateFtp(List&lt;double&gt; powerValues, int totalSeconds)
        /// </summary>

        [Fact] //The [Fact] attribute in xUnit marks a method as a test method
        public void Error_Returned_When_Total_Minutes_Is_Less_Than_Twenty()
        {
            // Arrange: Create an instance of FtpCalculator and simulate 19 minutes of 196W power values
            var calculator = new FtpCalculator();
            int totalMinutes = 19;
            var powerValues = new List<double>();
            for (int i = 0; i < totalMinutes; i++)
                powerValues.Add(196);

            // Act: Use reflection to call the private CalculateFtp method
            var method = typeof(FtpCalculator).GetMethod("CalculateFtp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(calculator, new object[] { powerValues, 20 * 60 });

            // Assert: Error returned due to insufficient data
            Assert.IsType<string>(result);
            Assert.Contains("Not enough data", (string)result);
        }

        /// <summary>
        /// This test verifies that the FtpCalculator returns the error message
        /// "Please select a .fit file." when no .fit file is provided (i.e., the input path is null or empty).
        /// It calls CalculateFtpFromFitFile with a null value and asserts the correct error message is returned.
        /// 
        /// Test coverage:
        /// - Class: FTPCalculatorWeb.Services.FtpCalculator
        /// - Method: public double CalculateFtpFromFitFile(string fitFilePath)
        /// </summary>

        [Fact] //The [Fact] attribute in xUnit marks a method as a test method
        public void Error_Returned_When_No_Fit_File_Provided()
        {
            // Arrange
            var calculator = new FtpCalculator();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => calculator.CalculateFtpFromFitFile(null));
            Assert.Contains("Please select a .fit file.", ex.Message);
        }
    }
}