using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Api.Extensions;
using Keymaker.Infra.OpenApi.Extensions;
using Keymaker.Service.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHandlers();
builder.Services.ConfigureSerialization();
builder.Services.AddKeymaker();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwaggerApiDoc("Keymaker API");
app.UseKeymaker();
app.UseCustomEndpoints();
app.Run();