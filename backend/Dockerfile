FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /workdir

COPY ["nuget.config", "nuget.config"]

# Copy props files
COPY ["Directory.Packages.props", "Directory.Packages.props"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["src/Directory.Build.props", "src/Directory.Build.props"]
COPY ["tests/Directory.Build.props", "tests/Directory.Build.props"]

# Copy csproj files
COPY ["src/AdoExport.Common/AdoExport.Common.csproj", "src/AdoExport.Common/"]
COPY ["src/AdoExport.Client/AdoExport.Client.csproj", "src/AdoExport.Client/"]
COPY ["src/AdoExport.Auditing/AdoExport.Auditing.csproj", "src/AdoExport.Auditing/"]
COPY ["src/AdoExport.Api/AdoExport.Api.csproj", "src/AdoExport.Api/"]

COPY ["tests/AdoExport.Common.Tests/AdoExport.Common.Tests.csproj", "tests/AdoExport.Common.Tests/"]
COPY ["tests/AdoExport.Client.Tests/AdoExport.Client.Tests.csproj", "tests/AdoExport.Client.Tests/"]
COPY ["tests/AdoExport.Auditing.Tests/AdoExport.Auditing.Tests.csproj", "tests/AdoExport.Auditing.Tests/"]
COPY ["tests/AdoExport.Api.Tests/AdoExport.Api.Tests.csproj", "tests/AdoExport.Api.Tests/"]

# Copy solution
COPY ["AdoExport.Api.sln", "./"]

# Restore dependencies
RUN dotnet restore "AdoExport.Api.sln"

# Copy the rest of the source code
COPY . .

# Build the solution
RUN dotnet build "AdoExport.Api.sln" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Tests deliberately do not run here. A failing RUN surfaces as a Docker build error
# with no test report and no coverage attached to the build, which is how a broken
# suite can stay invisible. The pipeline runs them in its own Test stage instead.

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./src/AdoExport.Api/AdoExport.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

# The branch the image is built from. The pipeline passes it in, a local build leaves it
# empty. The application loads appsettings.branch.<branch>.json when that file exists,
# which is how images built from main, the ones deployed to production, get their own
# settings without anything being set on the App Service.
ARG ADOAUDITOR_BUILD_BRANCH=""
ENV ADOAUDITOR__BUILDBRANCH=$ADOAUDITOR_BUILD_BRANCH

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdoExport.Api.dll"]
