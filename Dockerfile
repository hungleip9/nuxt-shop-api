FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
# Copy csproj và restore dependencies
COPY ["nuxt-shop.csproj", "./"]
RUN dotnet restore
# Copy toàn bộ source code và build ứng dụng
COPY . .
RUN dotnet publish -c Release -o /out
# Stage 2: Runtime (Chạy ứng dụng)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
# Mở cổng cho API
EXPOSE 5000
# Chạy ứng dụng
ENTRYPOINT ["dotnet", "nuxt-shop.dll"]
