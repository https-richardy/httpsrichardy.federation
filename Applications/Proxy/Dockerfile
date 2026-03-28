# use ASP.NET Core 9.0 runtime image (alpine) as base
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /artifacts
EXPOSE 8080

# use SDK image (Alpine) for build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /workdir

ARG BUILD_CONFIGURATION=Release
ARG SOLUTION=HttpsRichardy.Federation.Proxy

# copy only csproj and sln to restore as early as possible
COPY ["Source/${SOLUTION}.WebApi/${SOLUTION}.WebApi.csproj", "${SOLUTION}.WebApi/"]
COPY ["${SOLUTION}.sln", "./"]

# restore dependencies
RUN dotnet restore "${SOLUTION}.WebApi/${SOLUTION}.WebApi.csproj"

# copy the rest of the source code
COPY Source/ ./Source/

WORKDIR "/workdir/Source/${SOLUTION}.WebApi"

# build in Release mode
RUN dotnet build "${SOLUTION}.WebApi.csproj" -c $BUILD_CONFIGURATION -o /artifacts/build

# publish the project for production
RUN dotnet publish "${SOLUTION}.WebApi.csproj" -c $BUILD_CONFIGURATION -o /artifacts/publish /p:UseAppHost=false

# final image to run the app
FROM base AS final
WORKDIR /artifacts

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
ENV DOTNET_RUNNING_IN_CONTAINER=true

# copy published files from the build stage
COPY --from=build /artifacts/publish .

ENTRYPOINT ["dotnet", "HttpsRichardy.Federation.Proxy.WebApi.dll"]
