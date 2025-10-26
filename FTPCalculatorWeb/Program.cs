using FTPCalculatorWeb.Services;

// This is the main entry point for your ASP.NET Core web application.

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// This line enables support for controllers and views (MVC pattern).
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<FtpCalculator>();

// Build the web application using the settings and services defined above.
var app = builder.Build();

// Configure the HTTP request pipeline.
// This section sets up how the app handles requests and errors.

if (!app.Environment.IsDevelopment())
{
    // In production, use a custom error page.
    app.UseExceptionHandler("/Home/Error");
    
    // Enable HTTP Strict Transport Security (HSTS) for extra security.
    app.UseHsts();
}

// Redirect HTTP requests to HTTPS for security.
app.UseHttpsRedirection();

// Allow the app to serve static files (like images, CSS, JavaScript).
app.UseStaticFiles();

// Add routing capabilities so the app can match URLs to controllers/actions.
app.UseRouting();

// Enable authorization checks (if you add authentication/authorization later).
app.UseAuthorization();

// Set up the default route for the application.
// This means that if someone visits the root URL, they'll be sent to the FtpController's Upload action.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Ftp}/{action=Upload}/{id?}");

// Start the web application and begin listening for requests.
app.Run();
