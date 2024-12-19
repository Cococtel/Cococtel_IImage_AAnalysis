# Use the .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy and restore project files
COPY *.csproj .
RUN dotnet restore

# Copy the rest of the code and publish
COPY . .
RUN dotnet publish -c Release -o out

# Use the .NET runtime for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose the port the application listens on
EXPOSE 5000

# Run the application
ENTRYPOINT ["dotnet", "ComputerVisionAPI.dll"]
