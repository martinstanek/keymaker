using Keymaker.Api.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Keymaker.Api.Handlers;
using Keymaker.Model;

namespace Keymaker.Api.Extensions;

public static class WebApplicationExtensions
{
   extension(WebApplication app)
   {
      public WebApplication UseApiEndpoints()
      {
         var conf = KeyMakerApiConfiguration.ReadFromEnvironment();
         var api = app.MapGroup("/");

         if (conf.IsApiEnabled)
         {
            api.MapDelete("/challenge", ([FromServices] RequestHandler handler) => handler.CancelCurrentChallenge())
               .Produces<NoContentResult>();

            api.MapPut("/challenge", ([FromServices] RequestHandler handler) => handler.TriggerChallengeAsync())
               .Produces<NoContentResult>();

            api.MapGet("/challenge/info", ([FromServices] RequestHandler handler) => handler.GetChallengeInfoAsync())
               .Produces<ChallengeInfo>();
         }

         if (!conf.IsUiEnabled)
         {
            api.MapGet("/", () => Results.Text("OK"))
               .Produces<string>(contentType: "text/plain");
         }

         if (conf.IsLogConsoleEnabled)
         {
            api.MapGet("/console", ([FromServices] RequestHandler handler) => handler.GetConsole())
               .Produces<string>(contentType: "text/plain");

            api.MapGet("/console/clear", ([FromServices] RequestHandler handler) => handler.ClearConsole())
               .Produces<NoContentResult>();
         }

         return app;
      }

      public WebApplication UseOpenApiDocs()
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

      public WebApplication UseUi()
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
}