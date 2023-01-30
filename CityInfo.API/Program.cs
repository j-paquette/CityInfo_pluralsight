using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

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
//Swashbuckle uses Swagger internally and exposes the endpoints and how to interact with them, by default in the MS template
builder.Services.AddEndpointsApiExplorer();
//This registers services that are used for generating the spec
//Configure Swagger to include the XML comments as part of generated spec docs
builder.Services.AddSwaggerGen(setupAction =>
{
    //Uses reflection to get the filename from the Assembly, to get the simplified name (GetName())
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //Use the Path object to get the full path of the file
    //Call Combine, passing thru the BaseDirectory of our app as the first parameter
    //BaseDirectory is where the file resides
    //xmlCommentsFile is the full name
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    //Call IncludeXmlComments to tell Swashbuckle to use this file to read the XML comments from
    setupAction.IncludeXmlComments(xmlCommentsFullPath);

    //Define the security definition by calling into AddSecurityDefinition
    //SecurityScheme is refrenced with Id CityInfoApiBearerAuth
    setupAction.AddSecurityDefinition("CityInfoApiBearerAuth", new OpenApiSecurityScheme()
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid token to access this API"
    });

    //This will automatically send a valid token as authorization header in the request by our documentation UI
    //OpenApiSecurityScheme is a dictionary with an OpenAPISecurityScheme as key
    //We want to reference the OpenAPISecurityScheme that was used when a security definition was added
    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference {
                    //Pass a reference object, that will reference the OpenAPISecurityScheme
                    //that was used when a security definition was added
                    Type = ReferenceType.SecurityScheme,
                    //SecurityScheme is refrenced with Id CityInfoApiBearerAuth
                    //That matches the previously added definition
                    Id = "CityInfoApiBearerAuth" }
                //The value of the ditcionary item is a list of string
                //This is used when working with tokens & scopes
                //Since this app uses basic auth, just pass thru an empty list
            }, new List<string>() }
    });
});

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
        builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"]));

//Register CityInforRepository as a scoped request. Created once per request
//Pass in the contract(ICityInfoRepository) and the implementation(CityInfoRepository)
builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();

//Register AutoMapper's services on the container
//We want to get assembies from the current AppDomain
//Esbures that current assembly (CityInfo.API assembly) will be scanned for profiles
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Register JwtMiddleware services to bearer token authentication
//Need to configure how to validatethe token
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                //Our API will only accept tokens created by our API
                ValidIssuer = builder.Configuration["Authentication:Issuer"],
                ValidAudience = builder.Configuration["Authentication:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"]))
            }; 
        }
    );

//Create a ABAC policy here
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromAntwerp", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Antwerp");
    });

});

//Register versioning package here
//Configure it to choose a default version = 1.0 if none is selected
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
});



var app = builder.Build();

//// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //Adds the middleware that generates the OpenAPI specification
    app.UseSwagger();
    //Add the middleware that uses that spec to generate the default Swagger UI documentation UI
    //The Schemas/models are generated when a method of ActionResult<T> in the Actions
    //The model class/Schemas would NOT be displayed if returning were IActionResult.
    //Use ActionResult<T> whenever possible
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

//For API: use attribute-based routing
app.UseRouting();

//Ensure the authentication is added to the request pipeline
//The order matters: check whether the request is authenticated BEFOR alowing it to go to the next peice of middleware
app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    //adds controller endpoints without specifying routes. Routes will be specified with attributes instead.
    endpoints.MapControllers();
});


app.Run();
