FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV TZ=Asia/Ho_Chi_Minh
RUN sed -i 's/TLSv1.2/TLSv1/g' /etc/ssl/openssl.cnf
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuxt-shop.csproj", "."]
RUN dotnet restore "./nuxt-shop.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./nuxt-shop.csproj" -c %BUILD_CONFIGURATION% -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./nuxt-shop.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/Certificates
COPY ./Certificates/aspnetapp.pfx /app/Certificates/

ENTRYPOINT ["dotnet", "nuxt-shop.dll"]