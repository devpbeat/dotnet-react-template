FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install dotnet-ef tool for migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Set environment to Development for hot reload
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_URLS=http://+:8080

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else
COPY . .

# Use dotnet watch for hot reload
ENTRYPOINT ["dotnet", "watch", "run", "--no-restore", "--urls", "http://0.0.0.0:8080"]
