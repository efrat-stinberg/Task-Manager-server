# Use the .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["TodoApi.csproj", "./"]
RUN dotnet restore

# Copy the rest of the code
COPY . .

# Build the app
RUN dotnet build "TodoApi.csproj" -c Release -o /app/build

# Publish the app
RUN dotnet publish "TodoApi.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "TodoApi.dll"]
