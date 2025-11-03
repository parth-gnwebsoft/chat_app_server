# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file (chat_app_server.csproj) and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy all source code and publish the app
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ---

# Stage 2: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# 🚨 CRITICAL CHANGE HERE: Use the correct DLL name 
ENTRYPOINT ["dotnet", "chat_app_server.dll"]