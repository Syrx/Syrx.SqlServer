FROM mcr.microsoft.com/mssql/server:2022-latest

# Set environment variables
ENV ACCEPT_EULA=Y
ENV MSSQL_SA_PASSWORD=YourStrong!Passw0rd123
ENV MSSQL_PID=Developer

# Create directory for initialization scripts
RUN mkdir -p /docker-entrypoint-initdb.d

# Copy initialization scripts
COPY init-scripts/*.sql /docker-entrypoint-initdb.d/

# Copy and set up custom entrypoint
COPY entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/entrypoint.sh

# Expose SQL Server port
EXPOSE 1433

# Use custom entrypoint
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]