FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
EXPOSE 80
ENV ASPNETCORE_URLS=http://*:80

WORKDIR /app
COPY . .

ENTRYPOINT ["dotnet", "Sucker.Api.dll"]