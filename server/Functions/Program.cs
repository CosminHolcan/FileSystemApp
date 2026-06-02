using Azure.Monitor.OpenTelemetry.Exporter;
using BLL;
using DAL;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// ----------------------
// OpenTelemetry
// ----------------------
builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults()
    .UseAzureMonitorExporter();

// ----------------------
// Configuration (IMPORTANT)
// ----------------------
var connectionString =
    builder.Configuration.GetConnectionString("DbConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("DbConnection is missing.");
}

// ----------------------
// DbContext
// ----------------------
builder.Services.AddDbContext<FileSystemAppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ----------------------
// DAL
// ----------------------
builder.Services.AddScoped<UsersDAL>();
builder.Services.AddScoped<AppFilesDAL>();
builder.Services.AddScoped<StorageAccountsDAL>();
builder.Services.AddScoped<FileVersionsDAL>();

// ----------------------
// BLL
// ----------------------
builder.Services.AddScoped<UsersBLL>();
builder.Services.AddScoped<AppFilesBLL>();
builder.Services.AddScoped<StorageAccountsBLL>();
builder.Services.AddScoped<FileVersionsBLL>();

builder.Build().Run();