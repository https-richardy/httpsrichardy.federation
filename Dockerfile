# use ASP.NET Core 9.0 runtime image (alpine) as base
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080

# use SDK image (Alpine) for build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# copy only csproj and sln to restore as early as possible
COPY ["Source/HttpsRichardy.Federation.WebApi/HttpsRichardy.Federation.WebApi.csproj", "HttpsRichardy.Federation.WebApi/"]
COPY ["HttpsRichardy.Federation.sln", "./"]

# restore dependencies
RUN dotnet restore "HttpsRichardy.Federation.WebApi/HttpsRichardy.Federation.WebApi.csproj"

# copy the rest of the source code
COPY Source/ ./Source/

WORKDIR "/src/Source/HttpsRichardy.Federation.WebApi"

# build in Release mode
RUN dotnet build "HttpsRichardy.Federation.WebApi.csproj" -c Release -o /app/build

# publish the project for production
RUN dotnet publish "HttpsRichardy.Federation.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# final image to run the app
FROM base AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

# copy published files from the build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HttpsRichardy.Federation.WebApi.dll"]
