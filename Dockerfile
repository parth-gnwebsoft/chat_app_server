# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies (assuming your project file is Program.csproj)
# Change 'Program.csproj' if your project file has a different name
COPY *.csproj .
RUN dotnet restore

# Copy all source code and publish the app
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---

# Stage 2: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The entry point command to start your application
# Change 'Program.dll' to match your compiled DLL name (usually the .csproj name, e.g., 'ChatServer.dll')
ENTRYPOINT ["dotnet", "Program.dll"]