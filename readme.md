This is an implementation of the ZEISS Digital Innovation Partners coding case here:<br>
http://codingcase.zeiss.services/?type=backend

## Requirements

- .NET 5.0 SDK installed
- Docker and Docker Compose installed
- Bourne shell (or a compatible shell) to run `build.sh` and `run.sh`

## Instructions

To run application locally:

1. Run `run.sh`. This builds the application and starts `docker-compose` building containers on the fly.
2. Browse to http://localhost:5000/machines using any HTTP client such as Postman, a browser, curl etc.

To build images (locally only) run `build.sh`.

You can also build from Visual Studio Code by running the tasks `build`, `docker build ingestor` and/or `docker build api`.

## Architecture

The application consists of 3 services:

- `redis`: standard Redis used as semi-persistent storage of machine stati
- `ingestor`: a .NET Core console app that connects to the WebSocket endpoint, receives machine status messages, and updates status per machine in Redis
- `api`: an ASP.NET Core Web API app that provides a simple REST API that provides the last known status for each machine

Redis is configured with a persistent storage volume.

## Configuration

The application is pre-configured to connect to a WebSocket endpoint at `wss://machinestream.herokuapp.com/ws` but this can be overriden by setting the environment variable `INGESTOR_WEBSOCKETENDPOINTURL`.