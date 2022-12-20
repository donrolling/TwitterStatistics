# Twitter Stats app

This application has several layers. This is to illustrate how I would likely structure a real application. 

The user would either read the console screen for the statistics, or the log file that can be found here: `\bin\Debug\net7.0\Logs\log{date}.txt`

## Domain

Bottom layer containing models and interfaces. Describes what we are doing, but not how.

## Business

Contains the services used to complete the work. This example has two classes:

1. Worker class that represents a Windows/Linux service that would run continuously. 
1. TweetStreamer class that reads from the tweet stream, collects tags, and reports statistics.

## Project Configuration
Contains the Bootstrapper/Dependency Injection startup configuration for any project that needs to startup the application. Use of this layer makes it easier to share configuration between the actual application and an Integration Test project.

## Twitter Stats

The Windows/Linux service application. Can be run in Docker, or normally. This is the deployed asset.

## Integration Tests

These are not unit tests, they are integration tests, so they can be used to run the application just like it would run in production.

## Running the application

Acquire a bearer token for your Twitter application. The application doesn’t need anything else.
In order to run either the tests or the service, you will need to provide the bearer token in this JSON format:
``` json
{
  "TwitterSecrets": {
    "BearerToken": "[Your token here]"
  }
}
```

Since this value is a secret, I used Visual Studio’s built-in User Secrets feature to store and read the secrets. The secret will need to be set for whichever application you run, the IntegrationTests or the TwitterStats service. 
Use the following image as an example of how to access those.

![Accessing User Secrets](https://user-images.githubusercontent.com/1778167/208727048-0ed13592-f1ad-45ae-81cf-908f5c425634.png)

