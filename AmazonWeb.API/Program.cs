var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHsts();
app.UseHttpsRedirection();

app.UseAuthorization();

//swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.UseStaticFiles();
app.MapControllers();
app.Run();
