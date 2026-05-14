using Asp.Versioning;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo() {Title="Amazon Api Version 1.0", Version="1.0" });
    options.SwaggerDoc("v2", new OpenApiInfo() {Title="Amazon Api Version 2.0", Version="2.0" });
});

//api versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;       //sets the api version in url
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHsts();
app.UseHttpsRedirection();

app.UseAuthorization();

//swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json","1.0");
    options.SwaggerEndpoint("/swagger/v2/swagger.json","2.0");
});

app.MapControllers();
app.UseStaticFiles();
app.MapControllers();
app.Run();
