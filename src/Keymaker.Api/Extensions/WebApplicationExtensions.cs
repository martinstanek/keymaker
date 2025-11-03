using Keymaker.Api.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Keymaker.Api.Handlers;
using Keymaker.Model;

namespace Keymaker.Api.Extensions;

public static class WebApplicationExtensions
{
   public static WebApplication UseApiEndpoints(this WebApplication app)
   {
      var conf = KeyMakerApiConfiguration.ReadFromEnvironment();
      var api = app.MapGroup("/");

      if (!conf.IsApiEnabled)
      {
         return app;
      }

      api.MapDelete("/challenge", ([FromServices] RequestHandler handler) => handler.CancelCurrentChallenge())
         .Produces<NoContentResult>();

      api.MapPut("/challenge", ([FromServices] RequestHandler handler) => handler.TriggerChallengeAsync())
         .Produces<NoContentResult>();

      api.MapGet("/challenge/status", ([FromServices] RequestHandler handler) => handler.GetChallengeStatus())
         .Produces<ChallengeStatus>();

      api.MapGet("/challenge/info", ([FromServices] RequestHandler handler) => handler.GetChallengeInfoAsync())
         .Produces<ChallengeInfo>();

      api.MapGet("/certificate",
            async ([FromServices] RequestHandler handler) => await handler.GetMostRecentCertificateInfoAsync())
         .Produces<CertificateInfo>();

      api.MapGet("/console", ([FromServices] RequestHandler handler) => handler.GetConsole())
         .Produces<string>(contentType: "text/plain");

      api.MapGet("/console/clear", ([FromServices] RequestHandler handler) => handler.ClearConsole())
         .Produces<NoContentResult>();

      return app;
   }

   public static WebApplication UseConsoleEndpoints(this WebApplication app)
   {
      var conf = KeyMakerApiConfiguration.ReadFromEnvironment();
      var api = app.MapGroup("/");

      if (!conf.IsLogConsoleEnabled)
      {
         return app;
      }

      api.MapGet("/console", ([FromServices] RequestHandler handler) => handler.GetConsole())
         .Produces<string>(contentType: "text/plain");

      api.MapGet("/console/clear", ([FromServices] RequestHandler handler) => handler.ClearConsole())
         .Produces<NoContentResult>();

      return app;
   }

   public static WebApplication UseOpenApiDocs(this WebApplication app)
   {
      var config = KeyMakerApiConfiguration.ReadFromEnvironment();

      if (!config.IsOpenApiDocEnabled)
      {
         return app;
      }

      app.UseSwagger();
      app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Keymaker API"); });

      return app;
   }

   public static WebApplication UseUi(this WebApplication app)
   {
      var conf = KeyMakerApiConfiguration.ReadFromEnvironment();

      if (!conf.IsUiEnabled)
      {
         return app;
      }

      app.UseDefaultFiles();
      app.UseStaticFiles();

      return app;
   }
}