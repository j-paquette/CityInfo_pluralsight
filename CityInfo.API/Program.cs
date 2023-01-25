using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Serilog;

//This sets up a logfile that will br created everyday
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
//They have been commented out because Serilog is being used instead
//clears all logging
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();

//This instructs AspNet to use serilog instead of the default logger
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers(options =>
{
    //to indicate to consumer which formatter we DONT support
    options.ReturnHttpNotAcceptable = true;
    //Adds support for Xml formatting
}).AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters();
//AddNewtonsoftJson() replaces the default JSON input and output formatters with JSON.NET

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Allows us to inject a FileExtensionContentTypeProvider in other parts of our code. Used for different file formats
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

//Register the mail service as a transient service: requested each time it's called
//whenever we inject an IMailService in our code, we want it to provide us with a local instance of LocalMailService
#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

//Register the CitiesDataStore
builder.Services.AddSingleton<CitiesDataStore>();

//Register DbContext with a scoped lifetime
//These options are made available by exposing the DbContextOptions base constructor
//It is configured by passing thru an action. On DbContextOptions, we call into UseSqlLite, and pass a connection string
//The db will live in the application root.
builder.Services.AddDbContext<CityInfoContext>(
    dbContextOptions => dbContextOptions.UseSqlite(
        builder.Configuration["ConnectionsStrings:CityInfoDBConnectionString"]));

var app = builder.Build();

//// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

//For API: use attribute-based routing
app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    //adds controller endpoints without specifying routes. Routes will be specified with attributes instead.
    endpoints.MapControllers();
});


app.Run();
