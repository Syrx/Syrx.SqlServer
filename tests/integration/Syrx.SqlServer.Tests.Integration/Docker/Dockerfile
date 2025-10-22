# Syrx SQL Server Test Database Docker Image
# This image provides a pre-configured SQL Server instance with all test objects for Syrx.SqlServer.Tests.Integration

FROM mcr.microsoft.com/mssql/server:2022-latest

# Set environment variables for SQL Server
ENV ACCEPT_EULA=Y
ENV MSSQL_SA_PASSWORD=YourStrong!Passw0rd
ENV MSSQL_PID=Developer

# Create directory for initialization scripts
USER root
RUN mkdir -p /opt/mssql-tools/bin
COPY init-scripts/ /opt/mssql-tools/bin/

# Switch back to mssql user
USER mssql

# Expose SQL Server port
EXPOSE 1433

# Copy initialization script and make it executable
COPY --chown=mssql:root init-database.sh /opt/mssql-tools/bin/
USER root
RUN chmod +x /opt/mssql-tools/bin/init-database.sh

# Set the entrypoint to initialize database on startup
ENTRYPOINT ["/opt/mssql-tools/bin/init-database.sh"]