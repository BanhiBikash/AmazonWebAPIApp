using AmazonWeb.API.ServiceConfigurations;
using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.ServiceContracts;
using AmazonWeb.Core.ServiceContracts.CartContracts;
using AmazonWeb.Core.ServiceContracts.OrderContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using AmazonWeb.Core.ServiceContracts.TokenContracts;
using AmazonWeb.Core.ServiceContracts.TransactionContract;
using AmazonWeb.Core.Services;
using AmazonWeb.Core.Services.OrderService;
using AmazonWeb.Core.Services.TransactionService;
using AmazonWeb.Infrastructure.DBContext;
using AmazonWeb.Infrastructure.RepositoryContract;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Razorpay.Api;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Fetch Razorpay keys from appsettings.json
string razorpayKey = builder.Configuration["Razorpay:KeyId"] ?? string.Empty;
string razorpaySecret = builder.Configuration["Razorpay:KeySecret"] ?? string.Empty;

// 2. Initialize the Razorpay Client globally so the SDK caches the secret key context
if (!string.IsNullOrEmpty(razorpayKey) && !string.IsNullOrEmpty(razorpaySecret))
{
    new RazorpayClient(razorpayKey, razorpaySecret);
}

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Add global authorization filter
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddJsonOptions(options =>
{
    // Allows strings to be serialized as enums in swagger and api responses
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Database
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IFileService, LocalFileService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IJWTTokenservice, JWTTokenService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService,CartService>();
builder.Services.AddScoped<IOrderRepository,OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>(); // For automated swagger api versioning
builder.Services.AddSwaggerGen();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true; // Sets the api version in url
});

// Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
    });
});

// Add Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

if (string.IsNullOrEmpty(jwtSettings["SecretKey"]))
{
    throw new InvalidOperationException("JWT secret key is not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
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

    // ✨ Added: Explicitly force a clean 401 response header context when token expires
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHsts();
app.UseHttpsRedirection();

// Swagger (Only show Swagger UI when running in Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var service = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in service.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseStaticFiles();

//Sequence: Routing -> CORS -> Auth -> Controllers
app.UseRouting();

app.UseCors(); //AFTER UseRouting and BEFORE UseAuthentication

app.UseAuthentication(); //BEFORE UseAuthorization
app.UseAuthorization();

app.MapControllers(); //Single, clean registration call at the very end

app.Run();