dotnet tool install --global dotnet-ef

1. dotnet new webapi
2. dotnet restore
3. dotnet build
4. dotnet run
5. dotnet publish -c Release
6. dotnet ef migrations add ...
7. dotnet ef database update
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design


Install-Module -Name OpenSSL
mkdir ssl
openssl req -x509 -newkey rsa:4096 -keyout ssl/skillseek.key -out ssl/skillseek.crt -days 365
openssl pkcs12 -export -out ssl/certificate.pfx -inkey ssl/skillseek.key -in ssl/skillseek.crt
openssl x509 -in ssl/skillseek.crt -noout -text

docker build -t skillseek-backend .
docker run -d -p 9000:8080 -v ${PWD}/Database:/skillseek-backend/Database --name skillseek-backend-container skillseek-backend
docker exec -it skillseek-backend-container bash
docker logs skillseek-backend-container
docker stop skillseek-backend-container
docker rm skillseek-backend-container
docker rmi skillseek-backend
