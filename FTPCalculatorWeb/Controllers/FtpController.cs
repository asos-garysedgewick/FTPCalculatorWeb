using Microsoft.AspNetCore.Mvc;
using FTPCalculatorWeb.Services;

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

            // Save uploaded file to a temp location
            var tempPath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(tempPath))
            {
                await fitFile.CopyToAsync(stream);
            }

            double ftp;
            try
            {
                ftp = _ftpCalculator.CalculateFtpFromFitFile(tempPath);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error processing file: " + ex.Message;
                return View();
            }
            finally
            {
                // Clean up temp files
                System.IO.File.Delete(tempPath);
                var csvPath = Path.ChangeExtension(tempPath, ".csv");
                if (System.IO.File.Exists(csvPath))
                    System.IO.File.Delete(csvPath);
            }

            ViewBag.Ftp = ftp;
            return View();
        }
    }
}