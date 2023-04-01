# Dapr integration with events published by non dapr enabled legacy systems
Dapr enabled grpc service listening for events published by legacy (non-dapr-enabled) systems, so that:

1. Events are not serialized as json

and/or

2. Events are not wrapped in cloudevents envelope

This projects uses RabbitMq as pubsub component.

## Debugging:
[Dapr Sidekick for .Net](https://github.com/man-group/dapr-sidekick-dotnet) helps immensely to setting up a Dapr enabled .Net application project and also debugging experience.

## Setup
1. Dapr configuration files are in components folder. Project is configured to use these files on startup by Dapr Sidekick configuration in appsettings file.

2. Rabbitmq is used as Dapr pubsub component. A docker container running Rabbitmq with management support can be started with following command:
```powershell
docker run -dt --restart unless-stopped -d -p 5672:5672 -p 15672:15672 --hostname my-rabbit --name rabbitmq3 rabbitmq:3-management
```
Rabbitmq user/password used by Dapr is configured in "host" parameter value in dapr pubsub.yaml configuration file. A user/password pair with these values can be created via RabbitMq management.
