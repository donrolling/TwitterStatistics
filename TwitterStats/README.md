# Messaging.Service

TODO: Give a short introduction of your project. Let this section explain the objectives or the motivation behind this project.

TODO: Move this readme.md file into the project root and link it to the "Solution Items" solution folder in VS.

## Logging

We're using Serilog for logging in this project. [Seq](https://docs.datalust.co/docs/using-serilog) is a service that runs locally in a container and
provides you searchable structured tail-logging.

Seq is available [here](http://localhost:5340/). Docker-desktop might send you to localhost:5341, but that will not serve properly.

## Rabbit MQ Administration

The base docker image is here: [rabbitmq:3-management](https://hub.docker.com/repository/docker/caliberfsdev/rabbitmq)

### Run RabbitMQ manually

You probably won't need to do this, but to run it manually do this:

`docker run -d --hostname rabbitmq --name base-rabbit -p 8080:15672 rabbitmq:3-management`

### Save container changes after making config changes:

* Note: changes made to volumes will not be saved by committing.

`docker commit [container-id] [registry]/[repository]:[tag]`

Example:

`docker commit 40ad5a5af35d caliberfsdev/rabbitmq:noaa-message-service`

### Push container changes after making config changes:

`docker push caliberfsdev/rabbitmq:noaa-message-service`

### Login

manger admin: guest/guest
pub sub user: testuser/testpassword

### Backup and Restore

* Paths are relative to the rabbitmq terminal.

Examples of the configuration files are here: _RabbitMQ\

#### Modify

Browse to <http://localhost:15672/> and login using guest/guest and make your changes.

Destination of rabbitmq.config file needs to be: /etc/rabbitmq/rabbitmq.config
Destination of definitions.json file needs to be: /etc/rabbitmq/definitions.json

To save these changes to a new version of the container follow these steps:

#### Export

You can copy the definitions.json file from your local folders into the Rabbit container's data volume.

Here's where that is located on my machine. Note that this part will vary: dockercompose5787035549933542585_rabbitmq
`\\wsl$\docker-desktop-data\data\docker\volumes\dockercompose5787035549933542585_rabbitmq-data\_data`

The `\\wsl$` drive should have come into existance as part of the docker desktop setup or the subsequent WSL setup that you did. Not sure which.

Once those settings are there, you can follow the instructions below to move them and commit the image changes.

#### Export Rabbit settings and move them to the init location

`rabbitmqadmin export definitions.json`

The definitions.json will now be at the root. It needs to be moved to /etc/rabbitmq/

`mv definitions.json /etc/rabbitmq/`

Now the container needs to be commited and pushed.

* The following instructions assume the CaliberFSPrototype container registry.

##### Login to the Container Registry

Pick one of these.

DockerHub (currently using 08/26/2022): Login to DockerHub via DockerDesktop
Use the owner credentials for this because the other accounts don't have the required permission levels.

ADO (only if not using DockerHub registry): `az acr login --name CaliberFS`

##### Commit changes

Do this to commit and tag the container changes

`docker commit <containerid> caliberfsdev/rabbitmq:messaging-fresh-service`

Do this to push the container changes

`docker push caliberfsdev/rabbitmq:messaging-fresh-service`

#### Import

You shouldn't need to do this, but if you do:
`rabbitmqadmin import definitions.json`

# Deploy

Requirements for deployment
- deploys as a Windows or Linux service
- an environment specific RabbitMQ instance
- The SeqHost value should be left blank for non-local environments unless there is an available service that we'd like to use
- transformations for the appSettings.json file - TBD
