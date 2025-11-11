FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
EXPOSE 80
ENV ASPNETCORE_HTTP_PORTS=80

WORKDIR /app
COPY . .

ENTRYPOINT ["dotnet", "Keymaker.Api.dll"]