using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Api.Extensions;
using Keymaker.Service.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeymaker();
builder.Services.AddOpenApi();
builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.UseKeymaker();
app.UseCustomEndpoints();
app.Run();