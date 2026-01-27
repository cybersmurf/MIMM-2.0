FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/MIMM.Backend/MIMM.Backend.csproj", "MIMM.Backend/"]
COPY ["src/MIMM.Shared/MIMM.Shared.csproj", "MIMM.Shared/"]
RUN dotnet restore "MIMM.Backend/MIMM.Backend.csproj"

# Copy all source files
COPY src/ .

# Build the application
WORKDIR "/src/MIMM.Backend"
RUN dotnet build "MIMM.Backend.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MIMM.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user for rootless Docker
RUN groupadd -r appuser && useradd -r -g appuser appuser \
    && chown -R appuser:appuser /app

# Use non-privileged ports (8080/8081) for rootless Docker compatibility
EXPOSE 8080 8081

# Copy published files
COPY --from=publish /app/publish .

# Set environment (use non-privileged ports)
ENV ASPNETCORE_URLS=https://+:8081;http://+:8080

# Switch to non-root user
USER appuser

ENTRYPOINT ["dotnet", "MIMM.Backend.dll"]
