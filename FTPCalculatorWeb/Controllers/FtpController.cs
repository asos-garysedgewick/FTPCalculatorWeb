using Microsoft.AspNetCore.Mvc;
using FTPCalculatorWeb.Services;
using System.Text.Json;

namespace FTPCalculatorWeb.Controllers
{
    public class FtpController : Controller
    {
        private readonly FtpCalculator _ftpCalculator;

        public FtpController()
        {
            _ftpCalculator = new FtpCalculator();
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
            List<double> powerValues = null;
            List<double> cadenceValues = null; // Add this line
            try
            {
                ftp = _ftpCalculator.CalculateFtpFromFitFile(tempPath);

                var csvPath = Path.ChangeExtension(tempPath, ".csv");
                _ftpCalculator.CalculateFtpFromFitFile(tempPath); // This will create the CSV file as a side effect

                // Extract power values
                powerValues = typeof(FtpCalculator)
                    .GetMethod("ParsePowerValuesFromCsv", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(_ftpCalculator, new object[] { csvPath }) as List<double>;

                // Extract cadence values (add this block)
                cadenceValues = typeof(FtpCalculator)
                    .GetMethod("ParseCadenceValuesFromCsv", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(_ftpCalculator, new object[] { csvPath }) as List<double>;
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

            ViewBag.Ftp = ftp;
            ViewBag.PowerValuesJson = JsonSerializer.Serialize(powerValues);
            ViewBag.CadenceValuesJson = JsonSerializer.Serialize(cadenceValues); // Add this line

            return View();
        }
    }
}