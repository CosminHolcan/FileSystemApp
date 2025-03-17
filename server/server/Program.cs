using server.DAL;
using Microsoft.EntityFrameworkCore;
using server.BLL;
using server.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
    builder
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithOrigins("http://localhost:3000");
}));

var connectionString = "Server=tcp:filesystemappdbserver.database.windows.net,1433;Initial Catalog=filesystemappdb;Persist Security Info=False;User ID=dbadmin;Password=Admin_db17;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
builder.Services.AddDbContext<FileSystemAppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<UsersDAL>();

builder.Services.AddScoped<UsersBLL>();

builder.Services.AddScoped<JWTService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FileSystemAppDbContext>();
    db.Database.Migrate();  // Applies any pending migrations
}

// Configure the HTTP request pipeline.
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