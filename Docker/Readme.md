# Review Project

This is the lab repository for the Practical Event-Sourcing with C# hands-on session. 
The workshop is taught by [Alper Hankendi](https://twitter.com/alper_hankendi).

Due to the time limitation, there are many aspects of event-sourcing, which we will not be able to cover. 
Some reference materials will be included in this file later on, so start the repo to be informed.

## Prerequisites

- .NET Core 2.2
- C# IDE or editor of your choice (for example Jetbrains Rider, VS Code or Visual Studio 2017)
- Docker Compose

The solution is using C# 7.1 features so if you are using Visual Studio - ensure you have VS 2017. 
Rider supports the latest C# by default or will ask you if you want to enable it.

Note that Docker Compose might _not_ included to your version of Docker, so you need to [download and install](https://docs.docker.com/compose/install/) it. 
[Docker](https://docs.docker.com/install/) is a pre-requisite for Docker Compose.

> In case you are unable to use Docker Compose, please download the following products:

>- [EventStore](https://eventstore.org/downloads/)
>- [RavenDB 4](https://ravendb.net/downloads)
>- [Redis](https://redis.io/download)

*Important:* carefully read this section and ensure that everything works on your machine before the workshop. 

*Important:* highly recommended check the tutorial before starting to eventStore [EventStore tutorial](https://www.katacoda.com/alperhankendi/scenarios/eventstore-workshop)

### Using Docker Compose

You can run the required infrastructure components by issuing a simple command:

```
$ docker-compose up
```

form your terminal command line, whilst being inside the repository rot directory.

It might be a good idea to run the services in detached mode, so you don't accidentally stop them. To do this, execute:

```
$ docker-compose up -d
```

To stop all services, from the repository root execute this command:

```
$ docker-compose stop
```

Hence that this command does not remove containers so you can run them again using `docker-compose up` or `docker-compose start` and your data will be retained from the previous section.
w
If you want to clean up all data, use

```
$ docker-compose down
```

This command stops all services and removes containers. The images will still be present locally so when you do `docker-compose up` - containers will be created almost instantly and everything will start with clean volumes.

## Structure

The workshop source files are located in the `branches` and there are several stages:

- `01-event-store-before` is to get things started
- `02-projection-before` shows how events are being persisted
- `03-snapshot-before`explains how to take snaphot and persist them

```
$ git checkout 01-event-store-before
```

The final code is located in the `{branch_name}-after` branch and it is meant to be close to production quality.

## What to do

Start with trying to get up the Docker Compose images. Do this before the workshop, you will not have enough time to install everything during the workshop.

Start Docker Compose as described above while at home and check that the images are downloaded and everything starts properly. 
Check if EventStore and RavenDb respond via http by visiting the administration consoles:

- [EventStore](http://localhost:2113), user name is "admin" and password is "changeit"
- [RavenDb](http://localhost:8080)

the puzzle starts here ```git clone https://github.com/alperhankendi/workshop-event-sourcing/tree/01-event-store-before```

have fun!

## Command & Query Api Request Samples

[Sample Request Sample](https://documenter.getpostman.com/view/3440876/SVtWxnr3?version=latest)

## Notes
What brings you here of course not unique. I inspired a lot from [WorkshopEventSourcing](https://github.com/UbiquitousAS/WorkshopEventSourcing)
