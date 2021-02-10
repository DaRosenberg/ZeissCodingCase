#!/bin/sh
dotnet build
docker build -t=zeiss/ingestor:0.0.1 ./Zeiss.Ingestor
docker build -t=zeiss/api:0.0.1 ./Zeiss.Api
