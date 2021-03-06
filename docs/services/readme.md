# Azure Industrial IoT Microservices

Azure Industrial IoT Microservices use Azure IoT Edge and IoT Hub to connect the cloud and factory networks.

These microservices use the OPC UA components included in this repository to provide discovery, registration, and remote control of industrial devices through REST APIs.  Applications using the REST API do not require an OPC UA SDK, and can be implemented in any programming language and framework that can call an HTTP endpoint.

The following services are part of the platform:

* [OPC Registry Microservice](registry.md)
* [OPC Vault Microservice](vault.md)
* [OPC History Access Microservice](history.md)
* [OPC Twin Microservice](twin.md)
* [OPC Gateway](gateway.md)

The following Agents are part of the platform:

* [OPC Onboarding Agent](onboarding.md)
* [TODO]

## Next steps

* [Explore the Architecture](../architecture.md)
* [Deploy Microservices to Azure](../howto-deploy-microservices.md)
* [Register a server and browse its address space](howto-use-cli.md)
* [Explore the REST API](../api/readme.md)
