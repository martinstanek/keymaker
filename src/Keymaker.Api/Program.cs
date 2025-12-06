using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Api.Extensions;
using Keymaker.Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddLogging();
builder.Services.AddCors();
builder.Services.AddKeymaker();
builder.Services.AddOpenApiDocs();
builder.Services.AddStateHealthCheck();
builder.Services.ConfigureApiHandlers();
builder.Services.ConfigureSerialization();

var app = builder.Build();

app.UseUi();
app.UseOpenApiDocs();
app.UseKeymaker();
app.UseApiEndpoints();
app.UseCors(p => { p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
app.MapHealthChecks("/health");
app.Run();

public partial class Program;