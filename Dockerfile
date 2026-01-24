FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 7000 7001

# Copy published files
COPY --from=publish /app/publish .

# Set environment
ENV ASPNETCORE_URLS=https://+:7001;http://+:7000

ENTRYPOINT ["dotnet", "MIMM.Backend.dll"]
