using AzureFormRecognizerApp.Models;
using AzureFormRecognizerApp.Services;
using ImportProcess.Services;
using OfficeOpenXml;


var builder = WebApplication.CreateBuilder(args);

// 1. Add CORS FIRST
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 2. Add other services
builder.Services.AddControllers();

// 3. Configure Azure Form Recognizer
builder.Services.Configure<AzureFormRecognizerConfig>(
    builder.Configuration.GetSection("AzureFormRecognizer"));

// 4. Register your services
builder.Services.AddScoped<IFormRecognizerService, FormRecognizerService>();
builder.Services.AddScoped<ISampathBankStatementService, SampathBankStatementService>();
builder.Services.AddScoped<ICommercialBankStatementService, CommercialBankStatementService>();
builder.Services.AddScoped<IBOCBankStatementService, BOCBankStatementService>(); // ✅ fixed here
builder.Services.AddScoped<IPeoplesBankStatementService, PeoplesBankStatementService>();
builder.Services.AddScoped<ISeylanBankStatementService, SeylanBankStatementService>();

builder.Services.AddHttpClient();



builder.Services.AddLogging();
// 5. Configure file upload limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
});

builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
});

var app = builder.Build();

// 6. Configure pipeline - ORDER MATTERS!

// Exception handling first
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Routing before CORS
app.UseRouting();

// CORS after routing, before authorization
app.UseCors("AllowAll");

// Authorization
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add a test endpoint to verify CORS
app.MapGet("/test", () => "API is working!");


app.UseStaticFiles(); // To serve HTML
app.UseRouting();
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.Run();