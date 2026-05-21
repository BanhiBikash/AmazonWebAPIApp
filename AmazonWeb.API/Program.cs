using AmazonWeb.API.ServiceConfigurations;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using AmazonWeb.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using AmazonWeb.Core.Domain.Identities;
using Microsoft.AspNetCore.Identity;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.Services;
using AmazonWeb.Infrastructure.RepositoryContract;
using AmazonWeb.Core.ServiceContracts; // 👈 ADD THIS LINE HERE


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//database
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IFileService,LocalFileService>();

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>,ConfigureSwaggerOptions>();  //for automated swagger api versioning
builder.Services.AddSwaggerGen();

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

//Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

//CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHsts();
app.UseHttpsRedirection();

//swagger
// ✅ Only show Swagger UI when running in the Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var service = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        // Api-version-Description is a list of descriptions of all versions of api
        foreach (var description in service.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

app.MapControllers();
app.UseStaticFiles();

//checks
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
