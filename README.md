# Azure Industrial IoT Services

## Overview

Azure IoT Industrial IoT Services consists of several microservices that use Azure IoT Hub and other Azure Platform services to connect your factory to Azure.  

- **[OPC Unified Architecture (OPC UA) Device Management](docs/twin/readme.md)** services provide discovery, registration, and remote control of industrial devices through REST APIs.  
- **OPC Unified Architecture (OPC UA) Certificate Management** services enable secure communication among OPC UA enabled devices and the cloud.  (COMING SOON)

These services and components enable you to build application that

### Discover, register and manage your OPC UA Assets in Azure

OPC UA Device Management services allow plant operators to discover OPC UA servers in a factory network and register them in Azure IoT Hub.  

### Analyze, react to events, and control equipment from anywhere

OPC UA Device Management allows operations personnel to subscribe to and react to events on the factory floor from anywhere in the world.  The services' REST APIs mirror the OPC UA services edge-side and are secured using OAUTH authentication and authorization backed by Azure Active Directory (AAD).  This enables your cloud applications to browse server address spaces or read/write variables and execute methods using HTTPS and simple OPC UA JSON payloads.  

### Provision communication certificates and trust groups in your factory

OPC UA Certificate Management enables OT and IT to manage OPC UA Application Certificates and Trust Lists.  Certificates secure client to server communication. Trust Lists determine which client is allowed to talk to which server.  Certificates and private keys can be issued and continuously renewed to keep your OPC UA server endpoints secure.  OPC UA Certificate Management  is built on Azure Key Vault which guards your private keys in a secure hardware location.

### Simple developer experience

The [REST API](api/readme.md) can be used with any programming language through its exposed Open API specification (Swagger).  This means when integrating OPC UA into cloud management solutions, developers are free to choose technology that matches their skills, interests, and architecture choices.  For example, a full stack web developer who develops an application for an alarm and event dashboard can write logic to respond to events in JavaScript or TypeScript without ramping up on a OPC UA SDK, C, C++, Java or C#. 

## Next steps

### Learn more

This repository contains micro services, documentation, and deployment scripts for the [OPC UA device management micro services and components](docs/twin/readme.md).  

Other Azure Industrial IoT components can be found here:

* [Azure Industrial IoT OPC UA components](https://github.com/Azure/azure-iiot-opc-ua)
* [Azure Industrial IoT Service API](https://github.com/Azure/azure-iiot-services-api)
* Azure Industrial IoT Edge Modules
  * [OPC Publisher module](https://github.com/Azure/iot-edge-opc-publisher)
  * [OPC Proxy module](https://github.com/Azure/iot-edge-opc-proxy)
  * [OPC Twin module](https://github.com/Azure/azure-iiot-opc-twin-module)

### Give Feedback

Please enter issues, bugs, or suggestions for any of the components and services as GitHub Issues [here](https://github.com/Azure/azure-iiot-services/issues).

### Contribute

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct).  For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

If you want/plan to contribute, we ask you to sign a [CLA](https://cla.microsoft.com/) (Contribution License Agreement) and follow the project 's [code submission guidelines](docs/contributing.md). A friendly bot will remind you about it when you submit a pull-request. ​ 

## License

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the [MIT](LICENSE) License.  