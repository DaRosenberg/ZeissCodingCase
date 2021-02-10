# Base this image on plain .NET 5.0 image (based on Debian 10)
ARG DOTNET_VERSION=5.0
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

ARG APP_PATH=bin/Debug/net5.0/
COPY $APP_PATH app/

WORKDIR /app

CMD dotnet Zeiss.Ingestor.dll