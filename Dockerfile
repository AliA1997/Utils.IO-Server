FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App
COPY . ./

RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out -r linux-x64

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/out .

# Install curl
RUN apt-get update && apt-get install -y curl

EXPOSE 80
ENTRYPOINT ["dotnet", "Utils.IO Server.dll"]