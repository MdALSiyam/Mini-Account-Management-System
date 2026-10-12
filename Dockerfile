# Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project file
COPY ["Siyam_MiniAccountManagementSystem.sln", "./"]
COPY ["Siyam_MiniAccountManagementSystem/Siyam_MiniAccountManagementSystem.csproj", "Siyam_MiniAccountManagementSystem/"]
RUN dotnet restore "Siyam_MiniAccountManagementSystem/Siyam_MiniAccountManagementSystem.csproj"

# Copy all files and build
COPY . .
WORKDIR "/src/Siyam_MiniAccountManagementSystem"
RUN dotnet build "Siyam_MiniAccountManagementSystem.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "Siyam_MiniAccountManagementSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Siyam_MiniAccountManagementSystem.dll"]
