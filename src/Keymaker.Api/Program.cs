using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Api.Extensions;
using Keymaker.Api.Services.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAcme();
builder.Services.AddOpenApi();
builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.UseAcmeHandler();
app.UseCustomEndpoints();
app.Run();