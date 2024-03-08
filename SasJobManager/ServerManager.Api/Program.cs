using Microsoft.EntityFrameworkCore;
using SasJobManager.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var cn = builder.Configuration.GetConnectionString("db");
builder.Services.AddDbContext<SasContext>(x => x.UseSqlServer(cn));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
