# Stage 1: Build the application
# Use the .NET 9.0 SDK to support the .NET 9.0 target framework
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies (assuming your project file is Program.csproj)
COPY *.csproj .
RUN dotnet restore

# Copy all source code and publish the app
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---

# Stage 2: Runtime environment
# Use the .NET 9.0 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The entry point command to start your application
ENTRYPOINT ["dotnet", "Program.dll"]