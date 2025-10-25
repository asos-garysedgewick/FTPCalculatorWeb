using FTPCalculatorWeb.Services;
using Xunit;
using System.Collections.Generic;

namespace FtpCalculatorTests
{
    public class FtpCalculatorTests
    {
        /// <summary>
        /// This test verifies the FTP calculation logic in the FtpCalculator class.
        /// It simulates a 20-minute ride (1200 seconds) with a constant power output of 196 watts.
        /// The test uses reflection to invoke the private CalculateFtp method of FtpCalculator,
        /// which is responsible for computing FTP as 95% of the highest 20-minute average power.
        /// The expected FTP result is 186.2 watts (196 * 0.95).
        /// 
        /// Test coverage:
        /// - Class: FTPCalculatorWeb.Services.FtpCalculator
        /// - Method: private object CalculateFtp(List&lt;double&gt; powerValues, int totalSeconds)
        /// </summary>
        
        [Fact]
        public void Calculate_Ftp_20_Min_196W_Returns_Expected_Ftp()
        {
            // Arrange: Create an instance of FtpCalculator and simulate 20 minutes of 196W power values
            var calculator = new FtpCalculator();
            int totalSeconds = 20 * 60;
            var powerValues = new List<double>();
            for (int i = 0; i < totalSeconds; i++)
                powerValues.Add(196);

            // Act: Use reflection to call the private CalculateFtp method
            var method = typeof(FtpCalculator).GetMethod("CalculateFtp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(calculator, new object[] { powerValues, totalSeconds });

            // Assert: The FTP should be 186.2 watts (95% of 196)
            Assert.IsType<double>(result);
            Assert.Equal(186.2, (double)result, 1); // 196 * 0.95 = 186.2
        }
    }
}