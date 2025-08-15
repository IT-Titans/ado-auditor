# Local docker environment

The local docker environment aims to assist in the development and helps to test changes made locally.

## Setup

You need to provide environment variables for the local docker services to run.

Copy the `.env.sample` file and edit it as needed.

```sh
cp .env.sample .env
```

## Run

To build and run the local docker images, run the services locally using the `docker-compose.yml` file:

```sh
docker compose up -d --build
```

To inspect the logs, run:

```sh
docker compose logs -f
```
