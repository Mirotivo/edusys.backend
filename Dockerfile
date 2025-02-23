# Use the official .NET 8.0 SDK image as a build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /avancira-backend

# Copy the .csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# Copy the remaining source code and build the application
COPY . .
RUN dotnet publish -c Release -o out

# Use a .NET 8.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /avancira-backend
COPY --from=build /avancira-backend/out .

# Copy the SSL folder from the host to the container
# COPY ./ssl /avancira-backend/ssl
# Create a volume for the SQLite database file
# VOLUME /Database

# # Install OpenSSL to generate SSL certificates
# RUN apt-get update && apt-get install -y openssl
# # Create and generate SSL certificates dynamically
# RUN mkdir ssl && \
#     openssl req -x509 -newkey rsa:4096 -keyout ssl/avancira.key -out ssl/avancira.crt -days 365 -subj "/C=AU/ST=NSW/L=Strathfield/O=IT/CN=localhost" -passout pass:123456 && \
#     openssl pkcs12 -export -out ssl/avancira.pfx -inkey ssl/avancira.key -in ssl/avancira.crt -passin pass:123456 -passout pass:123456

# Start the application
ENTRYPOINT ["dotnet", "Backend.dll"]