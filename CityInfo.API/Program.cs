using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    //to indicate to consumer which formatter we DONT support
    options.ReturnHttpNotAcceptable = true;
    //Adds support for Xml formatting
}).AddXmlDataContractSerializerFormatters();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Allows us to inject a FileExtensionContentTypeProvider in other parts of our code. Used for different file formats
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

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
