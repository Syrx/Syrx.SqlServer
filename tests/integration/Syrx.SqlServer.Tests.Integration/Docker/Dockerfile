# Syrx SQL Server Test Database Docker Image
# This image provides a pre-configured SQL Server instance for Syrx.SqlServer.Tests.Integration
# Init scripts are mounted via docker-compose and executed via workflow/fixture

FROM mcr.microsoft.com/mssql/server:2022-latest

# Set environment variables for SQL Server
ENV ACCEPT_EULA=Y
ENV MSSQL_SA_PASSWORD=YourStrong!Passw0rd
ENV MSSQL_PID=Developer

# Expose SQL Server port
EXPOSE 1433