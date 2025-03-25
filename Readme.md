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
openssl req -x509 -newkey rsa:4096 -keyout ssl/avancira.key -out ssl/avancira.crt -days 365
openssl pkcs12 -export -out ssl/certificate.pfx -inkey ssl/avancira.key -in ssl/avancira.crt
openssl x509 -in ssl/avancira.crt -noout -text

docker build -t "mirotivo/avancira-backend:latest" .
docker run -d -p 9000:8080 -v ${PWD}/Database:/avancira-backend/Database --name avancira-backend-container "mirotivo/avancira-backend:latest"
docker exec -it avancira-backend-container bash
docker logs avancira-backend-container
docker stop avancira-backend-container
docker rm avancira-backend-container
docker rmi avancira-backend


docker tag "mirotivo/avancira-backend" "mirotivo/avancira-backend:latest"
docker push "mirotivo/avancira-backend:latest"


docker run -d -p 9000:8080 -v ${PWD}/Database:/avancira-backend/Database --name avancira-backend-container "mirotivo/avancira-backend:latest"