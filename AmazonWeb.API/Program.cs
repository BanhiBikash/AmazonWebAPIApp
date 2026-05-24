using AmazonWeb.API.ServiceConfigurations;
using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.ServiceContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using AmazonWeb.Core.ServiceContracts.TokenContracts;
using AmazonWeb.Core.Services;
using AmazonWeb.Infrastructure.DBContext;
using AmazonWeb.Infrastructure.RepositoryContract;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    //allows strings to be serialized as enums in swagger and api responses
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//database
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IFileService,LocalFileService>();
builder.Services.AddScoped<IProductService,ProductService>();
builder.Services.AddScoped<IJWTTokenservice, JWTTokenService>();

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

//Add authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

if (string.IsNullOrEmpty(jwtSettings["SecretKey"]))
{
    throw new InvalidOperationException("JWT secret key is not configured. Please set");
}

builder.Services.AddAuthentication(options =>
{
    //set the deafult authentication and challenge scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    //validate parameters
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Strictly rejects expired access tokens
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),

        // Reduces default token expiration tolerance gap from 5 minutes to zero
        ClockSkew = TimeSpan.Zero
    };
});

//build the app
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
