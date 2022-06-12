# Camunda, Zeebe and Dapr

Goal of this example is to try features available by Camunda workflow and Zeebe automation processing framework with Dapr to make coding as much as independent as possible. 

## The process description

Expecting a sad person having difficulties learning new language and it would make her happier if someone greets him. But depending on his learning skills (Score Person Learning Curve) he desires the greeting or not. If her score is more than 30% she receives greeting by email. If less, it generates a business error and sends email to administrator. Yes, cruel world.

### BPMN Workflow

![BPMN Workflow](greeting_workflow.png)

### Decide how to greet

![DMN Business Rules](greeting_dmn.png) 

![DMN](decide_greeting_dmn.png)

Request greeting and watch processing to the end.



## How to run

Best experience is from Visual Studio by running docker-compose launch adding debugging options, but it's not necessary. You either have Dapr installed standalone or run as part of the solution.

1. Install [Dapr](https://docs.dapr.io/getting-started/install-dapr-cli/)
1. Start docker-compose in Infrastructure folder by running `docker-compose up`. I left it separate as it is the background for my other projects and also when updating and testing code I don't have to start everything. Make sure images are running.
2. Start `docker-compose up --build` in greetings-camunda folder. This should build the project and create docker image.
3. Navigate to [MailDev](http://localhost:4000/) to see testing emails coming
4. Navigate to [Camunda cloud self hosted](http://localhost:8080/)

    user: demo\
    password: demo

5. Use Camunda Modeler to see [BPMN diagram](src/api/Model/Greetings3.bpmn) and [DMN diagram](src/api/Model/greetings.dmn) and deploy both to Zeebe engine self-hosted. There is also a [playground](src/api/Model/decision-tester.bpmn) to test DMN workflows.

6. Ensure all containers are running. Start simulator in */src/sim* folder as `dotnet run` to process all messages or `dotnet run 10` to process just 10 messages. It's always good to start small first.

## Knows issues

Timezone doesn't work correctly on Mac/Windows in Camunda containers. It's left unresolved for now.

## Remarks

Throwing business error does not send the ErrorMessage to Camunda engine or at least it's not accessible in "Send email to admin" task. Hence setting the property explicitly - feels a bit akward to me.

Whatever I define in Input field is a string. Even though I'd expect it's Time as on. Then using it in DMN diagram is impossible (or don't know how) as it always ends with error comparing ValString and ValTime. 

![Issue](issue.png)
