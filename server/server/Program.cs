using Microsoft.EntityFrameworkCore;
using server.BLL;
using server.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", policy =>
{
    policy
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithOrigins("http://localhost:3000");
}));

// ----------------------
// Database setup
// ----------------------

// Read connection string from Azure Web App Configuration
// Make sure the connection string name in Azure is "DbConnection"
var connectionString = builder.Configuration.GetConnectionString("DbConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DbConnection' not found in configuration.");
}

// Configure DbContext to use SQL Server with Managed Identity / Azure AD token
builder.Services.AddDbContext<FileSystemAppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register DAL services
builder.Services.AddScoped<UsersDAL>();
builder.Services.AddScoped<AppFilesDAL>();
builder.Services.AddScoped<StorageAccountsDAL>();
builder.Services.AddScoped<FileVersionsDAL>();

// Register BLL services
builder.Services.AddScoped<UsersBLL>();
builder.Services.AddScoped<AppFilesBLL>();
builder.Services.AddScoped<StorageAccountsBLL>();
builder.Services.AddScoped<FileVersionsBLL>();

var app = builder.Build();

// ----------------------
// Run database migrations at startup
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FileSystemAppDbContext>();
    db.Database.Migrate(); // Will create database if it doesn't exist and apply any pending migrations
}

// ----------------------
// Middleware
// ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();