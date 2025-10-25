# FTPCalculatorWeb

## 1. Overview
FTPCalculatorWeb is a web application built with ASP.NET Core Razor Pages that estimates Functional Threshold Power (FTP) from uploaded cycling FIT files. Users can upload their ride data, and the app processes the file to extract power, cadence, and heart rate metrics, then calculates and visualizes the estimated FTP.

## 2. Framework
- **.NET Version:** .NET 8
- **Web Framework:** ASP.NET Core Razor Pages
- **Test Framework:** xUnit

## 3. Classes and their Purpose

- **FTPCalculatorWeb.Services.FtpCalculator**
  - Handles FIT file conversion, parsing of cycling metrics, and FTP calculation logic.

- **FTPCalculatorWeb.Controllers.FtpController**
  - Manages file upload requests, invokes FTP calculation, and passes results to the Razor Page view.

## 4. Methods and their Purpose

### FtpCalculator
- `CalculateFtpFromFitFile(string fitFilePath)`
  - Converts a FIT file to CSV, parses power values, and calculates FTP.
- `ConvertFitToCsv(string fitFilePath, string csvFilePath)`
  - Converts a FIT file to a CSV file, extracting relevant ride metrics.
- `ParsePowerValuesFromCsv(string csvFilePath)`
  - Reads and returns power values from the CSV.
- `ParseCadenceValuesFromCsv(string csvFilePath)`
  - Reads and returns cadence values from the CSV.
- `ParseHeartRateValuesFromCsv(string csvFilePath)`
  - Reads and returns heart rate values from the CSV.
- `CalculateFtp(List<double> powerValues, int totalSeconds)`
  - Calculates FTP as 95% of the highest 20-minute average power using a rolling window.

### FtpController
- `Upload(IFormFile fitFile)`
  - Handles file upload, invokes FTP calculation, and prepares data for the view.

## 5. Error Handling

- **File Validation:** The application checks if a file is provided and whether it is a valid `.fit` file before processing. If not, a user-friendly error message is displayed.
- **Data Validation:** If the uploaded file does not contain enough data for a 20-minute effort (at least 1200 seconds of power data), the user is informed with a clear error message.
- **Exception Handling:** Errors encountered during file conversion, parsing, or FTP calculation (such as file read errors or invalid data) are caught and displayed to the user via the web interface.
- **User Feedback:** All error messages are shown on the upload page, ensuring users understand what went wrong and how to correct it.
- **Test Coverage:** Unit tests help ensure that error conditions (such as insufficient data) are handled gracefully and return appropriate messages.

## 6. Test Coverage

- **FtpCalculatorTests.CalculateFtp_20Min196W_Returns186Point2**
  - Unit test for `FtpCalculator`'s FTP calculation logic.
  - Simulates a 20-minute ride at 196W and asserts the FTP result is 186.2W.
  - Provides coverage for the private `CalculateFtp` method.

## 7. Repo
https://github.com/asos-garysedgewick/FTPCalculator.git

## 8. Input

- **Accepted File Type:** `.fit` (Flexible and Interoperable Data Transfer) cycling activity files.
- **How to Upload:** Use the web interface to select and upload your `.fit` file via the provided form.
- **Required Data:** The FIT file should contain at least 20 minutes (1200 seconds) of power data to allow for FTP estimation.
- **Extracted Metrics:** The application reads power, cadence, and heart rate values from the uploaded file.
- **Validation:** If the file is missing or does not contain enough data for a 20-minute effort, an error message will be displayed.
- **Example:**  
  1. Click "Choose File" and select your cycling `.fit` file.  
  2. Click "Estimate FTP" to upload and process the file.

## 9. Output

- **Estimated FTP:** After uploading a `.fit` file, the application displays the calculated FTP value (in watts) based on the best 20-minute effort.
- **Charts:** The web interface visualizes power and cadence data over time, highlighting the 20-minute window used for the FTP calculation.
- **Error Messages:** If the file is invalid or does not contain enough data for a 20-minute effort, a user-friendly error message is shown.
- **Test Output:** Unit tests confirm the accuracy of FTP calculations and are run using the xUnit framework