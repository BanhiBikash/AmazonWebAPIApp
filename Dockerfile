# Stage 1: Build & Compile the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to optimize layer caching for fast subsequent builds
COPY ["AmazonWeb.API/AmazonWeb.API.csproj", "AmazonWeb.API/"]
COPY ["AmazonWeb.Core/AmazonWeb.Core.csproj", "AmazonWeb.Core/"]
COPY ["AmazonWeb.Infrastructure/AmazonWeb.Infrastructure.csproj", "AmazonWeb.Infrastructure/"]
COPY ["AmazonWeb.Core.UnitTests/AmazonWeb.Core.UnitTests.csproj", "AmazonWeb.Core.UnitTests/"]

# Restore NuGet dependencies across the solution context
RUN dotnet restore "AmazonWeb.API/AmazonWeb.API.csproj"
RUN dotnet restore "AmazonWeb.Core.UnitTests/AmazonWeb.Core.UnitTests.csproj"

# Copy the rest of your clean source code (automatically filtering out .git, .vs, and clientapp)
COPY . .

#running the tests
RUN dotnet test "AmazonWeb.Core.UnitTests/AmazonWeb.Core.UnitTests.csproj" -c Release

# Move into the executable project directory and publish the optimized production binaries
WORKDIR "/src/AmazonWeb.API"
RUN dotnet publish "AmazonWeb.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AmazonWeb.API.dll"]