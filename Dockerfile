# Base image (chạy ứng dụng)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Build image (dùng SDK để build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj trước để tối ưu cache restore
COPY nuxt-shop.csproj ./
WORKDIR /src
RUN dotnet restore "nuxt-shop.csproj"

# Copy toàn bộ mã nguồn và build
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image (dùng để chạy ứng dụng)
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "nuxt-shop.dll"]
