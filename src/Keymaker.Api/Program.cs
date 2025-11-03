using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Api.Extensions;
using Keymaker.Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddLogging();
builder.Services.AddCors();
builder.Services.ConfigureHandlers();
builder.Services.ConfigureSerialization();
builder.Services.AddKeymaker();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwaggerApiDoc("Keymaker API");
app.UseKeymaker();
app.UseCustomEndpoints();
app.UseCors(p => { p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
app.Run();

public partial class Program;