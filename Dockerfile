# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY Collections.sln ./
COPY Collections.Models/*.csproj ./Collections.Models/
COPY Collections.Data/*.csproj ./Collections.Data/
COPY Collections/*.csproj ./Collections/

# copy csproj and restore as distinct layers
RUN dotnet restore

# copy everything else and build app
COPY . .
WORKDIR /app/Collections.Models
RUN dotnet publish -c Release -o out

WORKDIR /app/Collections.Data
RUN dotnet publish -c Release -o out

WORKDIR /app/Collections
RUN dotnet publish -c Release -o out

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./
CMD ASPNETCORE_URLS=http://*:$PORT dotnet Collections.dll