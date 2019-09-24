namespace IAI {

    using System;
    using System.Collections;
    using System.Security.Cryptography;
    using System.Collections.Generic;
    using System.Numerics;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    //using Microsoft.Azure.Management.ResourceManager.Fluent.Core.DAG;
    using Microsoft.Identity.Client;
    using Microsoft.Rest;
    using Microsoft.Graph;
    using Microsoft.Graph.Auth;

    using Microsoft.Azure.Management.KeyVault.Fluent;
    using Microsoft.Azure.Management.KeyVault.Fluent.Models;
    using Microsoft.Azure.Management.Storage.Fluent;
    using Microsoft.Azure.Management.Storage.Fluent.Models;
    using Microsoft.Azure.Management.IotHub;
    using Microsoft.Azure.Management.IotHub.Models;
    using Microsoft.Azure.Management.CosmosDB.Fluent;
    using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
    using Microsoft.Azure.Management.ServiceBus.Fluent;
    using Microsoft.Azure.Management.ServiceBus.Fluent.Models;
    using Microsoft.Azure.Management.EventHub.Fluent;
    using Microsoft.Azure.Management.EventHub.Fluent.Models;

    using Microsoft.Azure.Management.OperationalInsights;
    using Microsoft.Azure.Management.OperationalInsights.Models;
    using Microsoft.Azure.Management.ApplicationInsights.Management;
    using Microsoft.Azure.Management.ApplicationInsights.Management.Models;
    using Microsoft.Azure.Management.AppService.Fluent.Models;
    using Microsoft.Azure.Management.AppService.Fluent;
    using Microsoft.Azure.Management.ContainerService.Fluent;
    using Microsoft.Azure.Management.ContainerService.Fluent.Models;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.Services.AppAuthentication;
    using System.Text;

    class Program {

        public static Region[] _functionalRegions = new Region[] {
            Region.USEast2,
            Region.USWest2,
            Region.EuropeNorth,
            Region.EuropeWest,
            Region.CanadaCentral,
            Region.IndiaCentral,
            Region.AsiaSouthEast
        };

        public static string APPLICATION_OMP = "omp";

        static void Main(string[] args) {

            //var azureEnvironment = SelectEnvironment();
            var azureEnvironment = AzureEnvironment.AzureGlobalCloud;
            var azureCloudInstance = ToAzureCloudInstance(azureEnvironment);

            // ToDo: Figure out how to sign-in without tenantId
            //var tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";  // microsoft.onmicrosoft.com
            var tenantId = "6e660ce4-d51a-4585-80c6-58035e212354";  // opcwalls.onmicrosoft.com
            //var tenantId = "organizations";  // Generic one for multi-tenant applications

            var tenantIdGuid = new Guid(tenantId);

            // ClientId of AzureIndustrialIoTIAI
            const string iaiClientID = "fb2ca262-60d8-4167-ac33-1998d6d5c50b";

            var microsoftGraphScopes = new string[] {
                "https://graph.microsoft.com/Directory.AccessAsUser.All"
            };

            var azureManagementScopes = new string[] {
                //"https://management.core.windows.net//user_impersonation",
                "https://management.azure.com/user_impersonation"
            };

            var keyVaultScopes = new string[] {
                "https://vault.azure.net/user_impersonation"
            };

            var publicClientApplication = PublicClientApplicationBuilder
                .Create(iaiClientID)
                .WithAuthority(azureCloudInstance, tenantId)
                //.WithAuthority(azureCloudInstance, AadAuthorityAudience.AzureAdMultipleOrgs)
                .WithDefaultRedirectUri()
                .Build();

            // ToDo: Add timeout.
            var microsoftGraphAuthenticatoinResult = publicClientApplication
                    .AcquireTokenInteractive(microsoftGraphScopes)
                    //.WithPrompt(Prompt.SelectAccount)
                    .WithExtraScopesToConsent(azureManagementScopes)
                    .WithExtraScopesToConsent(keyVaultScopes)
                    .ExecuteAsync()
                    .Result;




            //var accounts = publicClientApplication.GetAccountsAsync().Result;
            //var account = SelectAccount(accounts);

            var azureManagementAuthenticatoinResult = publicClientApplication
                .AcquireTokenSilent(azureManagementScopes, microsoftGraphAuthenticatoinResult.Account)
                .ExecuteAsync()
                .Result;

            var keyVaultAuthenticatoinResult = publicClientApplication
                .AcquireTokenSilent(keyVaultScopes, microsoftGraphAuthenticatoinResult.Account)
                .ExecuteAsync()
                .Result;


            var microsoftGraphTokenCredentials = new TokenCredentials(
                new StringTokenProvider(microsoftGraphAuthenticatoinResult.AccessToken, "Bearer"),
                microsoftGraphAuthenticatoinResult.TenantId,
                microsoftGraphAuthenticatoinResult.Account.Username
            );

            var azureManagementTokenCredentials = new TokenCredentials(
                new StringTokenProvider(azureManagementAuthenticatoinResult.AccessToken, "Bearer"),
                azureManagementAuthenticatoinResult.TenantId,
                azureManagementAuthenticatoinResult.Account.Username
            );

            var keyVaultTokenCredentials = new TokenCredentials(
                new StringTokenProvider(keyVaultAuthenticatoinResult.AccessToken, "Bearer"),
                keyVaultAuthenticatoinResult.TenantId,
                keyVaultAuthenticatoinResult.Account.Username
            );


             var azureCredentials = new AzureCredentials(
                azureManagementTokenCredentials,
                microsoftGraphTokenCredentials,
                tenantId,
                azureEnvironment
            );
            
            var authenticated = Azure
                .Configure()
                .Authenticate(azureCredentials);

            //////////////////////////////////////////////////////
            ////Console.WriteLine("Tenants:");
            ////var tenantsList = authenticated.Tenants.List();

            ////foreach (var tenant in tenantsList) {
            ////    Console.WriteLine("Tenant: {0}", tenant.TenantId);
            ////}
            ////Console.WriteLine();
            //////////////////////////////////////////////////////

            var subscription = SelectSubscription(authenticated);

            var azure = authenticated
                .WithSubscription(subscription.SubscriptionId);


            //////////////////////////////////////////////////////
            ////Console.WriteLine("Subscriptoins:");
            ////foreach (var curSubscription in azure.Subscriptions.List()) {
            ////    Console.WriteLine("SubscriptionId: {0}, DisplayName: {1}",
            ////        curSubscription.SubscriptionId, curSubscription.DisplayName);
            ////}
            ////Console.WriteLine();
            //////////////////////////////////////////////////////





            var applicationName = GetApplicationName();

            // Resource Group names
            var resourceGroupName = applicationName;

            var resourceGroup = SelectOrCreateResourceGroup(azure, resourceGroupName);

            resourceGroupName = resourceGroup.Name;
            var aksResourceGroupName = resourceGroupName + "-aks";

            var servicesApplicationName = applicationName + "-services";
            var clientsApplicationName = applicationName + "-clients";

            // KeyVault names
            var keyVaultName = SdkContext.RandomResourceName("keyvault-", 5);

            const string webAppCN = "webapp.services.net";
            const string webAppCertName = "webApp";
            CertificateBundle webAppCert;

            const string aksClusterCN = "aks.cluster.net";
            const string aksClusterCertName = "aksCluster";
            CertificateBundle aksClusterCert;

            // Storage Account names
            var storageAccountName = SdkContext.RandomResourceName("storage", 12);

            // IoT hub names
            var iotHubName = SdkContext.RandomResourceName("iothub-", 12);
            var iotHubStorageContainerName = iotHubName.ToLower();
            var iotHubOnboardingConsumerGroupName = "onboarding";

            // CosmosDB names
            var documentDBName = SdkContext.RandomResourceName("documentDB-", 5);

            // Service Bus Namespace names
            var serviceBusNamespaceName = SdkContext.RandomResourceName("sb-", 5);
            const string serviceBusAuthorizationRuleName = "RootManageSharedAccessKey";

            // Event Hub Namespace names
            var eventHubNamespaceName = SdkContext.RandomResourceName("eventhubnamespace-", 5);
            const string eventHubNamespaceAuthorizationRuleName = "RootManageSharedAccessKey";

            var eventHubName = SdkContext.RandomResourceName("eventhub-", 5);
            var eventHubAuthorizationRuleName = SdkContext.RandomResourceName("iothubroutes-" + eventHubName, 5);

            // Operational Insights workspace name.
            var operationalInsightsWorkspaceName = SdkContext.RandomResourceName("workspace-", 5);

            // Application Insights name.
            var applicationInsightsName = SdkContext.RandomResourceName("appinsights-", 5);

            // AppService Plan name
            var appServicePlanName = SdkContext.RandomResourceName(applicationName + "-", 5);
            var azureWebsiteName = applicationName;

            // AKS cluster name
            var aksClusterName = SdkContext.RandomResourceName("aksCluster", 5);
            var aksDnsPrefix = aksClusterName + "-dns";





            var defaultTagsList = new List<string> {
                azureManagementAuthenticatoinResult.Account.Username,
                APPLICATION_OMP
            };

            var defaultTagsDict = new Dictionary<string, string> {
                { "owner", azureManagementAuthenticatoinResult.Account.Username },
                { "application", APPLICATION_OMP }
            };




            //var authProvider = new InteractiveAuthenticationProvider(
            //    publicClientApplication,
            //    azureManagementScopes
            //);

            var delegateAuthenticationProvider = new DelegateAuthenticationProvider(
                (requestMessage) =>
                {
                    var access_token = microsoftGraphAuthenticatoinResult.AccessToken;
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
                    return Task.FromResult(0);
                }
            );

            //var graphServiceClient = new GraphServiceClient(authProvider);
            var graphServiceClient = new GraphServiceClient(delegateAuthenticationProvider);

            var me = graphServiceClient
                .Me
                .Request()
                .GetAsync()
                .Result;

            // Service Application /////////////////////////////////////////////
            // Register service application

            // Setup AppRoles for service application
            var serviceApplicationAppRoles = new List<AppRole>();

            var serviceApplicationApproverRoleIdGuid = Guid.NewGuid();
            serviceApplicationAppRoles.Add(new AppRole {
                DisplayName = "Approver",
                Value = "Sign",
                Description = "Approvers have the ability to issue certificates.",
                AllowedMemberTypes = new List<string> { "User" },
                Id = serviceApplicationApproverRoleIdGuid
            });

            var serviceApplicationWriterRoleIdGuid = Guid.NewGuid();
            serviceApplicationAppRoles.Add(new AppRole {
                DisplayName = "Writer",
                Value = "Write",
                Description = "Writers Have the ability to change entities.",
                AllowedMemberTypes = new List<string> { "User" },
                Id = serviceApplicationWriterRoleIdGuid
            });

            var serviceApplicationAdministratorRoleIdGuid = Guid.NewGuid();
            serviceApplicationAppRoles.Add(new AppRole {
                DisplayName = "Administrator",
                Value = "Admin",
                Description = "Admins can access advanced features.",
                AllowedMemberTypes = new List<string> { "User" },
                Id = serviceApplicationAdministratorRoleIdGuid
            });

            // Setup RequiredResourceAccess for service application
            var serviceApplicationRequiredResourceAccess = new List<RequiredResourceAccess>();

            // !!!!! Not supported yet !!!!!
            //serviceApplicationRequiredResourceAccess.Add(
            //    GetRequiredResourceAccessByDisplayName(
            //        graphServiceClient,
            //        "Azure Key Vault", 
            //        new List<string> { "user_impersonation" }
            //    )
            //);

            serviceApplicationRequiredResourceAccess.Add(
                new RequiredResourceAccess {
                    //ResourceAppId = "cfa8b339-82a2-471a-a3c9-0fc0be7a4093",  // "Azure Key Vault"
                    ResourceAppId = AzureApps.AzureKeyVault.AppId,
                    ResourceAccess = new List<ResourceAccess> {
                        new ResourceAccess {
                            //Id = new Guid("f53da476-18e3-4152-8e01-aec403e6edc0"),  // "user_impersonation"
                            Id = AzureApps.AzureKeyVault.ResourceAccess["user_impersonation"],
                            Type = "Scope"
                        }
                    }
                }
            );

            // !!!!! Not supported yet !!!!!
            //serviceApplicationRequiredResourceAccess.Add(
            //    GetRequiredResourceAccessByDisplayName(
            //        graphServiceClient,
            //        "Microsoft Graph",
            //        new List<string> { "User.Read" }
            //    )
            //);

            serviceApplicationRequiredResourceAccess.Add(
                new RequiredResourceAccess {
                    //ResourceAppId = "00000003-0000-0000-c000-000000000000",  // "Microsoft Graph"
                    ResourceAppId = AzureApps.MicrosoftGraph.AppId,
                    ResourceAccess = new List<ResourceAccess> {
                        new ResourceAccess {
                            //Id = new Guid("e1fe6dd8-ba31-4d61-89e7-88639da4683d"),  // "User.Read"
                            Id = AzureApps.MicrosoftGraph.ResourceAccess["User.Read"],
                            Type = "Scope"
                        }
                    }
                }
            );

            // Add OAuth2Permissions
            var serviceApplicatoinPermissionUserImpersonationIdGuid = Guid.NewGuid();

            var oauth2Permissions = new List<PermissionScope> {
                new PermissionScope {
                    AdminConsentDescription = string.Format("Allow the app to access {0} on behalf of the signed-in user.", servicesApplicationName),
                    AdminConsentDisplayName = string.Format("Access {0}", servicesApplicationName),
                    Id = serviceApplicatoinPermissionUserImpersonationIdGuid,
                    IsEnabled = true,
                    Type = "User",
                    UserConsentDescription = string.Format("Allow the application to access {0} on your behalf.", servicesApplicationName),
                    UserConsentDisplayName = string.Format("Access {0}", servicesApplicationName),
                    Value = "user_impersonation"
                }
            };

            var serviceApplicationApiApplication = new ApiApplication {
                Oauth2PermissionScopes = oauth2Permissions
            };

            // !!!!! Oauth2AllowImplicitFlow !!!!!
            var serviceApplicationWebApplication = new WebApplication {
                HomePageUrl = "https://localhost",  // This is SignInUrl
                //Oauth2AllowImplicitFlow = false,
                ImplicitGrantSettings = new ImplicitGrantSettings {
                    EnableIdTokenIssuance = true
                }
            };

            var serviceApplicationPasswordCredentials = new List<PasswordCredential> {
                new PasswordCredential {
                    StartDateTime = DateTimeOffset.UtcNow,
                    EndDateTime = DateTimeOffset.UtcNow.AddYears(2),
                    CustomKeyIdentifier = ToBase64Bytes("Service Key"),
                    DisplayName = "Service Key",
                    KeyId = Guid.NewGuid(),
                    SecretText = "not so secret right now"  // !!!!! ToDO !!!!!
                }
            };

            var serviceApplicationIdentifierUri = string.Format("https://{0}/{1}", tenantId, servicesApplicationName);

            var serviceApplicationRequest = new Application {
                DisplayName = servicesApplicationName,
                IsFallbackPublicClient = false,
                IdentifierUris = new List<string> { serviceApplicationIdentifierUri },
                Tags = defaultTagsList,
                SignInAudience = "AzureADMyOrg",
                AppRoles = serviceApplicationAppRoles,
                RequiredResourceAccess = serviceApplicationRequiredResourceAccess,
                Api = serviceApplicationApiApplication,
                Web = serviceApplicationWebApplication,
                PasswordCredentials = serviceApplicationPasswordCredentials
            };

            var serviceApplication = graphServiceClient
                .Applications
                .Request()
                .AddAsync(serviceApplicationRequest)
                .Result;

            // Find service principal for service application
            var serviceAppIdFilterClause = string.Format("AppId eq '{0}'", serviceApplication.AppId);

            var serviceApplicationServicePrincipals = graphServiceClient
                .ServicePrincipals
                .Request()
                .Filter(serviceAppIdFilterClause)
                .GetAsync()
                .Result;


            ServicePrincipal serviceApplicationServicePrincipal;

            if (serviceApplicationServicePrincipals.Count == 0) {
                // Define new service principal
                var serviceApplicationServicePrincipalDefinition = new ServicePrincipal {
                    DisplayName = servicesApplicationName,
                    AppId = serviceApplication.AppId,
                    Tags = defaultTagsList // Add WindowsAzureActiveDirectoryIntegratedApp
                };

                // Create new service principal
                serviceApplicationServicePrincipal = graphServiceClient
                    .ServicePrincipals
                    .Request()
                    .AddAsync(serviceApplicationServicePrincipalDefinition)
                    .Result;
            } else {
                serviceApplicationServicePrincipal = serviceApplicationServicePrincipals.First();
            }

            // Try to add current user as app owner for service application, if it is not owner already
            var serviceIdFilterClause = string.Format("Id eq '{0}'", me.Id);

            var serviceApplicationOwners = graphServiceClient
                .Applications[serviceApplication.Id]
                .Owners
                .Request()
                .Filter(serviceIdFilterClause)
                .GetAsync()
                .Result;

            if (serviceApplicationOwners.Count == 0) {
                graphServiceClient
                    .Applications[serviceApplication.Id]
                    .Owners
                    .References
                    .Request()
                    .AddAsync(me)
                    .Wait();
            }

            // Add current user as Writer, Approver and Administrator
            // !!!!! App role assignment is not supported yet, i.e. adding new app role assignments !!!!!
            var approverAppRoleAssignmentRequest = new AppRoleAssignment {
                //PrincipalDisplayName = "",
                PrincipalType = "User",
                PrincipalId = new Guid(me.Id),
                ResourceId = new Guid(serviceApplicationServicePrincipal.Id),
                ResourceDisplayName = "Approver",
                Id = serviceApplicationApproverRoleIdGuid.ToString(),
                AppRoleId = serviceApplicationApproverRoleIdGuid
            };

            var writerAppRoleAssignmentRequest = new AppRoleAssignment {
                //PrincipalDisplayName = "",
                PrincipalType = "User",
                PrincipalId = new Guid(me.Id),
                ResourceId = new Guid(serviceApplicationServicePrincipal.Id),
                ResourceDisplayName = "Writer",
                Id = serviceApplicationWriterRoleIdGuid.ToString(),
                AppRoleId = serviceApplicationWriterRoleIdGuid
            };

            var adminAppRoleAssignmentRequest = new AppRoleAssignment {
                //PrincipalDisplayName = "",
                PrincipalType = "User",
                PrincipalId = new Guid(me.Id),
                ResourceId = new Guid(serviceApplicationServicePrincipal.Id),
                ResourceDisplayName = "Admin",
                Id = serviceApplicationAdministratorRoleIdGuid.ToString(),
                AppRoleId = serviceApplicationAdministratorRoleIdGuid
            };

            //// !!!!! AddAsync() is not defined !!!!!
            //var appRoleAssignment = graphServiceClient
            //    .ServicePrincipals["{id}"]
            //    .AppRoleAssignments
            //    .Request()
            //    .AddAsync(appRoleAssignmentRequest);

            // Workaround using HttpClient
            AddAppRoleAssignmentAsync(serviceApplicationServicePrincipal, microsoftGraphAuthenticatoinResult.AccessToken, approverAppRoleAssignmentRequest).Wait();
            AddAppRoleAssignmentAsync(serviceApplicationServicePrincipal, microsoftGraphAuthenticatoinResult.AccessToken, writerAppRoleAssignmentRequest).Wait();
            AddAppRoleAssignmentAsync(serviceApplicationServicePrincipal, microsoftGraphAuthenticatoinResult.AccessToken, adminAppRoleAssignmentRequest).Wait();




            // Client Application //////////////////////////////////////////////
            // Register client application
            var clientApplicationRequiredResourceAccess = new List<RequiredResourceAccess>();

            clientApplicationRequiredResourceAccess.Add(
                new RequiredResourceAccess {
                    ResourceAppId = serviceApplication.AppId,  // service application
                    ResourceAccess = new List<ResourceAccess> {
                        new ResourceAccess {
                            Id = serviceApplicatoinPermissionUserImpersonationIdGuid,  // "user_impersonation"
                            Type = "Scope"
                        }
                    }
                }
            );

            clientApplicationRequiredResourceAccess.Add(
                new RequiredResourceAccess {
                    //ResourceAppId = "00000003-0000-0000-c000-000000000000",  // "Microsoft Graph"
                    ResourceAppId = AzureApps.MicrosoftGraph.AppId,
                    ResourceAccess = new List<ResourceAccess> {
                        new ResourceAccess {
                            //Id = new Guid("e1fe6dd8-ba31-4d61-89e7-88639da4683d"),  // "User.Read"
                            Id = AzureApps.MicrosoftGraph.ResourceAccess["User.Read"],
                            Type = "Scope"
                        }
                    }
                }
            );

            var clientApplicationPublicClientApplication = new Microsoft.Graph.PublicClientApplication {
                RedirectUris = new List<string> {
                    "urn:ietf:wg:oauth:2.0:oob"
                }
            };

            // !!!!! Oauth2AllowImplicitFlow = true !!!!!
            // !!!!! Oauth2AllowUrlPathMatching = true !!!!!
            var clientApplicationWebApplicatoin = new WebApplication {
                //Oauth2AllowImplicitFlow = true,
                ImplicitGrantSettings = new ImplicitGrantSettings {
                    EnableIdTokenIssuance = true
                }
            };

            var clientApplicationPasswordCredentials = new List<PasswordCredential> {
                new PasswordCredential {
                    StartDateTime = DateTimeOffset.UtcNow,
                    EndDateTime = DateTimeOffset.UtcNow.AddYears(2),
                    CustomKeyIdentifier = ToBase64Bytes("Client Key"),
                    DisplayName = "Client Key",
                    KeyId = Guid.NewGuid(),
                    SecretText = "not so secret right now"  // !!!!! ToDO !!!!!
                }
            };

            var clientApplicationRequest = new Application {
                DisplayName = clientsApplicationName,
                IsFallbackPublicClient = true,
                Tags = defaultTagsList,
                SignInAudience = "AzureADMyOrg",
                RequiredResourceAccess = clientApplicationRequiredResourceAccess,
                PublicClient = clientApplicationPublicClientApplication,
                Web = clientApplicationWebApplicatoin,
                PasswordCredentials = clientApplicationPasswordCredentials
            };

            var clientApplication = graphServiceClient
                .Applications
                .Request()
                .AddAsync(clientApplicationRequest)
                .Result;

            // Find service principal for client application
            var clientAppIdFilterClause = string.Format("AppId eq '{0}'", clientApplication.AppId);

            var clientApplicationServicePrincipals = graphServiceClient
                .ServicePrincipals
                .Request()
                .Filter(clientAppIdFilterClause)
                .GetAsync()
                .Result;

            ServicePrincipal clientApplicationServicePrincipal;

            if (clientApplicationServicePrincipals.Count == 0) {
                // Define new client principal
                var clientApplicationServicePrincipalDefinition = new ServicePrincipal {
                    DisplayName = clientsApplicationName,
                    AppId = clientApplication.AppId,
                    Tags = defaultTagsList // add WindowsAzureActiveDirectoryIntegratedApp
                };

                // Create new client principal
                clientApplicationServicePrincipal = graphServiceClient
                    .ServicePrincipals
                    .Request()
                    .AddAsync(clientApplicationServicePrincipalDefinition)
                    .Result;
            } else {
                clientApplicationServicePrincipal = clientApplicationServicePrincipals.First();
            }

            // Try to add current user as app owner for client application, if it is not owner already
            var clientIdFilterClause = string.Format("Id eq '{0}'", me.Id);

            var clientApplicationOwners = graphServiceClient
                .Applications[clientApplication.Id]
                .Owners
                .Request()
                .Filter(clientIdFilterClause)
                .GetAsync()
                .Result;

            if (clientApplicationOwners.Count == 0) {
                graphServiceClient
                    .Applications[clientApplication.Id]
                    .Owners
                    .References
                    .Request()
                    .AddAsync(me)
                    .Wait();
            }


            // Update service application to include client applicatoin as knownClientApplications
            serviceApplication = graphServiceClient
                .Applications[serviceApplication.Id]
                .Request()
                .UpdateAsync(new Application {
                    Api = new ApiApplication {
                        KnownClientApplications = new List<Guid> {
                            new Guid(clientApplication.AppId)
                        }
                    }
                })
                .Result;

            // Grant admin consent for service application "user_impersonation" API permissions of client applicatoin
            // ToDo: Most probably I do not need to provide the Id.
            var clientApplicationOAuth2PermissionGrantUserImpersonationRequest = new OAuth2PermissionGrant {
                Id = Guid.NewGuid().ToString(),
                ConsentType = "AllPrincipals",
                ClientId = clientApplicationServicePrincipal.Id,
                ResourceId = serviceApplicationServicePrincipal.Id,
                Scope = "user_impersonation",
                StartTime = DateTimeOffset.UtcNow,
                ExpiryTime = DateTimeOffset.UtcNow.AddYears(2)
            };

            var clientApplicationOAuth2PermissionGrantUserImpersonation = graphServiceClient
                .Oauth2PermissionGrants
                .Request()
                .AddAsync(clientApplicationOAuth2PermissionGrantUserImpersonationRequest)
                .Result;

            // Grant admin consent for Microsoft Graph "User.Read" API permissions of client applicatoin
            // ToDo: Most probably I do not need to provide the Id.
            var clientApplicationOAuth2PermissionGrantUserReadRequest = new OAuth2PermissionGrant {
                Id = Guid.NewGuid().ToString(),
                ConsentType = "AllPrincipals",
                ClientId = clientApplicationServicePrincipal.Id,
                ResourceId = GetServicePrincipalByAppIdAsync(graphServiceClient, AzureApps.MicrosoftGraph.AppId).Result.Id,
                Scope = "User.Read",
                StartTime = DateTimeOffset.UtcNow,
                ExpiryTime = DateTimeOffset.UtcNow.AddYears(2)
            };

            var clientApplicationOAuth2PermissionGrantUserRead = graphServiceClient
                .Oauth2PermissionGrants
                .Request()
                .AddAsync(clientApplicationOAuth2PermissionGrantUserReadRequest)
                .Result;



            // Create generic RestClient for services
            var restClient = RestClient
                .Configure()
                .WithEnvironment(azureEnvironment)
                .WithCredentials(azureCredentials)
                //.WithLogLevel(HttpLoggingDelegatingHandler.Level.BodyAndHeaders)
                .Build();



            // Create Azure KeyVault
            VaultInner keyVault;

            using (var keyVaultManagementClient = new KeyVaultManagementClient(restClient) {
                SubscriptionId = subscription.SubscriptionId
            }) {
                var keyVaultAccessPolicies = new List<AccessPolicyEntry> {
                    new AccessPolicyEntry {
                        TenantId = tenantIdGuid,
                        ObjectId = serviceApplicationServicePrincipal.Id,
                        Permissions = new Microsoft.Azure.Management.KeyVault.Fluent.Models.Permissions {
                            Secrets = new List<string> { "get" },
                            Certificates = new List<string> { "get", "list" }
                        }
                    },
                    new AccessPolicyEntry {
                        TenantId = tenantIdGuid,
                        ObjectId = me.Id,
                        Permissions = new Microsoft.Azure.Management.KeyVault.Fluent.Models.Permissions {
                            Keys = new List<string> { "get", "list", "sign" },
                            Secrets = new List<string> { "get", "list", "set", "delete" },
                            Certificates = new List<string> { "get", "list", "update", "create", "import" }
                        }
                    }
                };

                keyVaultAccessPolicies.ElementAt(0).Validate();
                keyVaultAccessPolicies.ElementAt(1).Validate();

                var keyVaultCreateOrUpdateParametersInner = new VaultCreateOrUpdateParametersInner {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    Properties = new VaultProperties {
                        EnabledForDeployment = false,
                        EnabledForTemplateDeployment = false,
                        EnabledForDiskEncryption = false,
                        TenantId = tenantIdGuid,
                        Sku = new Microsoft.Azure.Management.KeyVault.Fluent.Models.Sku {
                            Name = Microsoft.Azure.Management.KeyVault.Fluent.Models.SkuName.Premium,  // !!!!!
                            //Family = "A" !!!!!
                        },
                        AccessPolicies = keyVaultAccessPolicies
                    }
                };

                keyVaultCreateOrUpdateParametersInner.Validate();

                keyVault = keyVaultManagementClient
                    .Vaults
                    .CreateOrUpdateAsync(
                        resourceGroup.Name,
                        keyVaultName,
                        keyVaultCreateOrUpdateParametersInner
                    )
                    .Result;
            }

            // Add certificates to KeyVault
            var authenticationCallback = new KeyVaultClient.AuthenticationCallback(
                async (authority, resource, scope) => {
                    // ToDo: Fetch from cache.
                    return await Task.FromResult(keyVaultAuthenticatoinResult.AccessToken);
                }
            );

            using (var keyVaultClient = new KeyVaultClient(authenticationCallback)) {
                var certificatePolicy = new CertificatePolicy {
                    KeyProperties = new KeyProperties {
                        Exportable = true,
                        KeyType = "RSA",
                        KeySize = 2048,
                        ReuseKey = false
                    },
                    SecretProperties = new SecretProperties {
                        ContentType = "application/x-pkcs12"
                    },
                    X509CertificateProperties = new X509CertificateProperties {
                        Subject = $"CN={webAppCN}",
                        SubjectAlternativeNames = new SubjectAlternativeNames {
                            DnsNames = new string[] {
                                webAppCN
                            }
                        }
                    },
                    IssuerParameters = new IssuerParameters {
                        Name = "Self"
                    }

                };

                certificatePolicy.Validate();

                var certificateAttributes = new CertificateAttributes {
                };

                var webAppCertificateOperation = keyVaultClient
                    .CreateCertificateAsync(
                        keyVault.Properties.VaultUri,
                        webAppCertName,
                        certificatePolicy,
                        certificateAttributes
                    )
                    .Result;

                while (webAppCertificateOperation.Status.ToLower().Equals("inprogress")) {
                    Task.Delay(1000).Wait();

                    webAppCertificateOperation = keyVaultClient
                        .GetCertificateOperationAsync(
                            keyVault.Properties.VaultUri,
                            webAppCertName
                        )
                        .Result;
                }

                webAppCert = keyVaultClient
                    .GetCertificateAsync(
                        keyVault.Properties.VaultUri,
                        webAppCertName
                    ).Result;

                // Create certificate for AKS cluster
                certificatePolicy.X509CertificateProperties.Subject = $"CN={aksClusterCN}";
                certificatePolicy.X509CertificateProperties.SubjectAlternativeNames.DnsNames[0] = aksClusterCN;

                var aksClusterCertificateOperation = keyVaultClient
                    .CreateCertificateAsync(
                        keyVault.Properties.VaultUri,
                        aksClusterCertName,
                        certificatePolicy,
                        certificateAttributes
                    )
                    .Result;

                while (aksClusterCertificateOperation.Status.ToLower().Equals("inprogress")) {
                    Task.Delay(1000).Wait();

                    aksClusterCertificateOperation = keyVaultClient
                        .GetCertificateOperationAsync(
                            keyVault.Properties.VaultUri,
                            aksClusterCertName
                        )
                        .Result;
                }

                aksClusterCert = keyVaultClient
                    .GetCertificateAsync(
                        keyVault.Properties.VaultUri,
                        aksClusterCertName
                    ).Result;
            }

            // Create Storage Account
            StorageAccountInner storageAccount;
            StorageAccountKey storageAccountKey;
            BlobContainerInner iotHubBlobContainer;

            using (var storageManagementClient = new StorageManagementClient(restClient) {
                SubscriptionId = subscription.SubscriptionId
            }) {
                var storageAccountCreateParameters = new Microsoft.Azure.Management.Storage.Fluent.Models.StorageAccountCreateParameters {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    Kind = Kind.Storage,
                    Sku = new SkuInner {
                        Name = Microsoft.Azure.Management.Storage.Fluent.Models.SkuName.StandardLRS  // !!!!!
                    },
                    EnableHttpsTrafficOnly = true,
                    NetworkRuleSet = new Microsoft.Azure.Management.Storage.Fluent.Models.NetworkRuleSet {
                        Bypass = Bypass.AzureServices,
                        VirtualNetworkRules = new List<Microsoft.Azure.Management.Storage.Fluent.Models.VirtualNetworkRule> {},
                        IpRules = new List<IPRule> {},
                        DefaultAction = Microsoft.Azure.Management.Storage.Fluent.Models.DefaultAction.Allow
                    },
                    Encryption = new Encryption {
                        Services = new EncryptionServices {
                            File = new EncryptionService {
                                Enabled = true
                            },
                            Blob = new EncryptionService {
                                Enabled = true
                            }
                        },
                        KeySource = KeySource.MicrosoftStorage
                    }
                };

                storageAccountCreateParameters.Validate();

                var storageAccountNameCheck = storageManagementClient
                    .StorageAccounts
                    .CheckNameAvailabilityAsync(
                        storageAccountName
                    ).Result;

                if(!storageAccountNameCheck.NameAvailable.Value) {
                    Console.WriteLine("Error: Storage account name is not available: {0}", storageAccountName);
                }

                storageAccount = storageManagementClient
                    .StorageAccounts
                    .CreateAsync(
                        resourceGroup.Name,
                        storageAccountName,
                        storageAccountCreateParameters
                    )
                    .Result;

                var keysList = storageManagementClient.StorageAccounts.ListKeysAsync(resourceGroup.Name, storageAccount.Name).Result;
                storageAccountKey = keysList.Keys.First();

                // ToDo: Figure out if I really need to explicitly create the container. template.json does not seem to do this.
                iotHubBlobContainer = storageManagementClient
                    .BlobContainers
                    .CreateAsync(
                        resourceGroup.Name,
                        storageAccountName,
                        iotHubStorageContainerName,
                        PublicAccess.None
                    ).Result;
            }

            const string storageAccountConectionStringFormat = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix={2}";
            var storageAccountConectionString = string.Format(
                storageAccountConectionStringFormat,
                storageAccount.Name,
                storageAccountKey.Value,
                azureEnvironment.StorageEndpointSuffix
            );

            // Create IoT Hub
            var iotHubRestClient = RestClient
                .Configure()
                .WithEnvironment(azureEnvironment)
                .WithCredentials(azureCredentials)
                //.WithLogLevel(HttpLoggingDelegatingHandler.Level.BodyAndHeaders)
                .Build();

            IotHubDescription iotHubDescription;
            EventHubConsumerGroupInfo eventHubConsumerGroupInfo;

            using (var iotHubClient = new IotHubClient(
                azureCredentials,
                iotHubRestClient.RootHttpHandler,
                iotHubRestClient.Handlers.ToArray()
            ) {
                SubscriptionId = subscription.SubscriptionId
            }) {
                var iotHubSkuInfo = new IotHubSkuInfo(
                    "S1",  // !!!!! ToDo: Add selection.
                    IotHubSkuTier.Standard,  // !!!!! ToDo: Add selection.
                    1  // !!!!! ToDo: Add selection.
                );

                iotHubSkuInfo.Validate();

                var iotHubProperties = new IotHubProperties {
                    IpFilterRules = new List<IpFilterRule>(),
                    EnableFileUploadNotifications = true,
                    Features = "None",
                    EventHubEndpoints = new Dictionary<string, EventHubProperties> {
                        { "events", new EventHubProperties {
                                RetentionTimeInDays = 1,
                                PartitionCount = 4 // !!!!!
                            }
                        },
                        { "operationsMonitoringEvents", new EventHubProperties {
                                RetentionTimeInDays = 1,
                                PartitionCount = 4 // !!!!!
                            }
                        }
                    },
                    Routing = new RoutingProperties {
                        Endpoints = new RoutingEndpoints {
                            ServiceBusQueues = null,
                            ServiceBusTopics = null,
                            EventHubs = null,
                            StorageContainers = null
                        },
                        Routes = null,
                        FallbackRoute = new FallbackRouteProperties {
                            Name = "$fallback",
                            //Source = "DeviceMessages",  // Seem to be set by FallbackRouteProperties constructor.
                            Condition = "true",
                            IsEnabled = true,
                            EndpointNames = new List<string> { "events" }
                        }
                    },
                    StorageEndpoints = new Dictionary<string, StorageEndpointProperties> {
                        { "$default", new StorageEndpointProperties {
                                SasTtlAsIso8601 = TimeSpan.FromHours(1),
                                ConnectionString = storageAccountConectionString,
                                ContainerName = iotHubStorageContainerName
                            }
                        }
                    },
                    MessagingEndpoints = new Dictionary<string, MessagingEndpointProperties> {
                        { "fileNotifications", new MessagingEndpointProperties {
                                LockDurationAsIso8601 = TimeSpan.FromMinutes(1),
                                TtlAsIso8601 = TimeSpan.FromHours(1),
                                MaxDeliveryCount = 10
                            }
                        }
                    },
                    CloudToDevice = new CloudToDeviceProperties {
                        MaxDeliveryCount = 10,
                        DefaultTtlAsIso8601 = TimeSpan.FromHours(1),
                        Feedback = new FeedbackProperties {
                            LockDurationAsIso8601 = TimeSpan.FromMinutes(1),
                            TtlAsIso8601 = TimeSpan.FromHours(1),
                            MaxDeliveryCount = 10
                        }
                    }
                };

                iotHubProperties.Validate();

                var iotHubDescriptionRequest = new IotHubDescription(
                    resourceGroup.RegionName,
                    iotHubSkuInfo,
                    Guid.NewGuid().ToString(),
                    iotHubName,
                    "Microsoft.Devices/Iothubs",
                    defaultTagsDict,
                    null,
                    iotHubProperties
                );

                iotHubDescriptionRequest.Validate();

                iotHubDescription = iotHubClient
                    .IotHubResource
                    .CreateOrUpdate(
                        resourceGroup.Name,
                        iotHubName,
                        iotHubDescriptionRequest
                    );

                eventHubConsumerGroupInfo = iotHubClient
                    .IotHubResource
                    .CreateEventHubConsumerGroup(
                        resourceGroup.Name,
                        iotHubName,
                        "events",
                        iotHubOnboardingConsumerGroupName
                    );
            }

            
            // Create CosmosDB
            DatabaseAccountInner cosmosDBAccount;

            using (var cosmosDBManagementClient = new CosmosDB(restClient) {
                SubscriptionId = subscription.SubscriptionId
            }) {
                var databaseAccountParameters = new DatabaseAccountCreateUpdateParametersInner {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    //DatabaseAccountOfferType = "Standard",
                    Kind = "GlobalDocumentDB",
                    ConsistencyPolicy = new ConsistencyPolicy {
                        DefaultConsistencyLevel = DefaultConsistencyLevel.Strong,  // !!!!! Add selection
                        MaxStalenessPrefix = 10,  // !!!!! Add selection
                        MaxIntervalInSeconds = 5  // !!!!! Add selection
                    },
                    Locations = new List<Microsoft.Azure.Management.CosmosDB.Fluent.Models.Location> {
                        new Microsoft.Azure.Management.CosmosDB.Fluent.Models.Location {
                            LocationName = resourceGroup.RegionName,
                            FailoverPriority = 0,
                            IsZoneRedundant = false
                        }
                    }
                };

                databaseAccountParameters.Validate();

                cosmosDBAccount = cosmosDBManagementClient
                    .DatabaseAccounts
                    .CreateOrUpdateAsync(
                        resourceGroup.Name,
                        documentDBName,
                        databaseAccountParameters
                    ).Result;
            }


            // Create Service Bus Namespace
            NamespaceModelInner serviceBusNamespace;
            //SharedAccessAuthorizationRuleInner serviceBusAuthorizationRule;
            string serviceBusConnectionString;

            using (var serviceBusManagementClient = new Microsoft.Azure.Management.ServiceBus.Fluent.ServiceBusManagementClient(restClient) {
                SubscriptionId = subscription.SubscriptionId
            }) {
                var namespaceModel = new NamespaceModelInner {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    Sku = new Microsoft.Azure.Management.ServiceBus.Fluent.Models.Sku {
                        Name = "Standard",  // !!!!! Add selection
                        Tier = "Standard"  // !!!!! Add selection
                    }
                };

                namespaceModel.Validate();

                serviceBusNamespace = serviceBusManagementClient
                    .Namespaces
                    .CreateOrUpdateAsync(
                        resourceGroup.Name,
                        serviceBusNamespaceName,
                        namespaceModel
                    ).Result;

                //serviceBusAuthorizationRule = serviceBusManagementClient
                //    .Namespaces
                //    .GetAuthorizationRuleAsync(
                //        resourceGroup.Name,
                //        serviceBusNamespaceName,
                //        serviceBusAuthorizationRuleName
                //    ).Result;

                var keysList = serviceBusManagementClient
                    .Namespaces
                    .ListKeysAsync(
                        resourceGroup.Name,
                        serviceBusNamespaceName,
                        serviceBusAuthorizationRuleName
                    ).Result;

                serviceBusConnectionString = keysList.PrimaryConnectionString;
            }

            // Create Azure Event Hub Namespace and Azure Event Hub
            EHNamespaceInner eventHubNamespace;
            string eventHubConnectionString;

            EventhubInner eventHub;

            using (var eventHubManagementClient = new EventHubManagementClient(restClient) {
                SubscriptionId = subscription.SubscriptionId
            }) {
                // Create Azure Event Hub Namespace
                var eventHubNamespaceParameters = new EHNamespaceInner {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    Sku = new Microsoft.Azure.Management.EventHub.Fluent.Models.Sku {
                        Name = Microsoft.Azure.Management.EventHub.Fluent.Models.SkuName.Basic,  // !!!!! Add selection
                        Tier = Microsoft.Azure.Management.EventHub.Fluent.Models.SkuTier.Basic,  // !!!!! Add selection
                        Capacity = 1  // !!!!! Add selection
                    },
                    IsAutoInflateEnabled = false,
                    MaximumThroughputUnits = 0
                };

                eventHubNamespaceParameters.Validate();

                eventHubNamespace = eventHubManagementClient
                    .Namespaces
                    .CreateOrUpdateAsync(
                        resourceGroup.Name,
                        eventHubNamespaceName,
                        eventHubNamespaceParameters
                    ).Result;

                var eventHubAccessKeys = eventHubManagementClient
                    .Namespaces
                    .ListKeysAsync(
                        resourceGroup.Name,
                        eventHubNamespaceName,
                        eventHubNamespaceAuthorizationRuleName
                    ).Result;

                eventHubConnectionString = eventHubAccessKeys.PrimaryConnectionString;

                // Create Azure Event Hub
                var eventHubParameters = new EventhubInner {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    MessageRetentionInDays = 1,
                    PartitionCount = 2,
                    Status = Microsoft.Azure.Management.EventHub.Fluent.Models.EntityStatus.Active
                };

                eventHubParameters.Validate();

                eventHub = eventHubManagementClient
                    .EventHubs
                    .CreateOrUpdateAsync(
                        resourceGroup.Name,
                        eventHubNamespaceName,
                        eventHubName,
                        eventHubParameters
                    ).Result;


                // Create Azure Event Hub Authorization Rule
                var eventHubAuthorizationRuleRights = new List<Microsoft.Azure.Management.EventHub.Fluent.Models.AccessRights> {
                    Microsoft.Azure.Management.EventHub.Fluent.Models.AccessRights.Send
                };

                var eventHubAuthorizationRule = eventHubManagementClient
                    .EventHubs
                    .CreateOrUpdateAuthorizationRuleAsync(
                        resourceGroup.Name,
                        eventHubNamespaceName,
                        eventHubName,
                        eventHubAuthorizationRuleName,
                        eventHubAuthorizationRuleRights
                    ).Result;
            }

            // Create Operational Insights workspace.
            Workspace operationalInsightsWorkspace;

            var operationalInsightsRestClient = RestClient
                .Configure()
                .WithEnvironment(azureEnvironment)
                .WithCredentials(azureCredentials)
                //.WithLogLevel(HttpLoggingDelegatingHandler.Level.BodyAndHeaders)
                .Build();

            using (var operationalInsightsManagementClient = new OperationalInsightsManagementClient(
                azureCredentials,
                operationalInsightsRestClient.RootHttpHandler,
                operationalInsightsRestClient.Handlers.ToArray()
            ) {
                SubscriptionId = subscription.SubscriptionId
            }) {
                // features -> searchVersion is not accesible through SDK and it is also not documented or specified in REST specs.
                // "features": {
                //     "searchVersion": 1
                // }

                var workspaceParameters = new Workspace {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    Sku = new Microsoft.Azure.Management.OperationalInsights.Models.Sku {
                        Name = "PerGB2018"  // !!!!! Add selection.
                    }
                };

                workspaceParameters.Validate();

                operationalInsightsWorkspace = operationalInsightsManagementClient
                    .Workspaces
                    .CreateOrUpdateAsync(
                        resourceGroup.Name,
                        operationalInsightsWorkspaceName,
                        workspaceParameters
                    ).Result;
            }

            // Create Application Insights components.
            var applicationInsightsRestClient = RestClient
                .Configure()
                .WithEnvironment(azureEnvironment)
                .WithCredentials(azureCredentials)
                //.WithLogLevel(HttpLoggingDelegatingHandler.Level.BodyAndHeaders)
                .Build();

            using (var applicationInsightsManagementClient = new ApplicationInsightsManagementClient(
                azureCredentials,
                applicationInsightsRestClient.RootHttpHandler,
                applicationInsightsRestClient.Handlers.ToArray()
            ) {
                SubscriptionId = subscription.SubscriptionId
            }) {
                var applicationInsightsComponentParameters = new ApplicationInsightsComponent() {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    Kind = "web",
                    ApplicationType = "web"
                };

                applicationInsightsComponentParameters.Validate();

                var applicationInsightsComponent = applicationInsightsManagementClient
                    .Components
                    .CreateOrUpdateAsync(
                        resourceGroup.Name,
                        applicationInsightsName,
                        applicationInsightsComponentParameters
                    ).Result;

                var applicationInsightsComponentBillingFeaturesParameters = new ApplicationInsightsComponentBillingFeatures() {
                    CurrentBillingFeatures = new List<string> { "Basic" },
                    DataVolumeCap = new ApplicationInsightsComponentDataVolumeCap(100.0, 24, 90)  // ResetTime is get only, so not available through object initializers
                };

                var applicationInsightsComponentBillingFeatures = applicationInsightsManagementClient
                    .ComponentCurrentBillingFeatures
                    .UpdateAsync(
                        resourceGroup.Name,
                        applicationInsightsName,
                        applicationInsightsComponentBillingFeaturesParameters
                    ).Result;
            }

            // Create AppService Plan to host the Application Gateway Web App
            var appServiceManager = new AppServiceManager(restClient, subscription.SubscriptionId, tenantId);

            //var appServicePlanFluent = appServiceManager
            //    .AppServicePlans
            //    .Define(appServicePlanName)
            //    .WithRegion(resourceGroup.Region)
            //    .WithExistingResourceGroup(resourceGroup)
            //    .WithPricingTier(PricingTier.StandardS1)
            //    .WithOperatingSystem(Microsoft.Azure.Management.AppService.Fluent.OperatingSystem.Linux)
            //    .WithCapacity(0)
            //    .Create();

            //var webSiteFluent = appServiceManager
            //    .WebApps
            //    .Define(azureWebsiteName)
            //    .WithExistingLinuxPlan(appServicePlanFluent)
            //    .WithExistingResourceGroup(resourceGroup);
            //    ...

            using (var webSiteManagementClient = new WebSiteManagementClient(restClient) {
                SubscriptionId = subscription.SubscriptionId
            }) {
                var appServicePlanParameters = new AppServicePlanInner {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    Sku = new Microsoft.Azure.Management.AppService.Fluent.Models.SkuDescription {
                        Name = "S1",
                        Capacity = 0
                    }
                };

                appServicePlanParameters.Validate();

                var appServicePlan = webSiteManagementClient
                    .AppServicePlans
                    .CreateOrUpdateAsync(
                        resourceGroup.Name,
                        appServicePlanName,
                        appServicePlanParameters
                    ).Result;

                var webSiteParameters = new SiteInner {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    Enabled = true,
                    ClientAffinityEnabled = false,
                    ServerFarmId = appServicePlan.Id,
                    SiteConfig = new SiteConfig {
                        AppSettings = new List<NameValuePair> {
                            new NameValuePair{
                                Name = "REMOTE_ENDPOINT",
                                Value = ""  // !!!!! ToDo: Add connection string to ip address of VM ????? !!!!!
                            },
                            new NameValuePair{
                                Name = "REMOTE_ENDPOINT_SSL_THUMBPRINT",
                                Value = Encoding.Unicode.GetString(webAppCert.X509Thumbprint, 0, webAppCert.X509Thumbprint.Length),  // !!!!! ToDo: Add cert to VM !!!!!
                            }
                        },

                        // Coming from Microsoft.Web/sites/config resource
                        NumberOfWorkers = 1,
                        RequestTracingEnabled = true,
                        HttpLoggingEnabled = true,
                        DetailedErrorLoggingEnabled = true,
                        AlwaysOn = true,
                        MinTlsVersion = SupportedTlsVersions.OneFullStopTwo
                    }
                };

                webSiteParameters.Validate();

                var webSite = webSiteManagementClient
                    .WebApps
                    .CreateOrUpdateAsync(
                        resourceGroup.Name,
                        azureWebsiteName,
                        webSiteParameters
                    ).Result;

                var siteSourceControlRequest = new SiteSourceControlInner() {
                    Location = resourceGroup.RegionName,
                    Tags = defaultTagsDict,

                    RepoUrl = "https://github.com/Azure/reverse-proxy-dotnet.git",
                    Branch = "master",
                    IsManualIntegration = true
                };

                siteSourceControlRequest.Validate();

                var siteSourceControl = webSiteManagementClient
                    .WebApps
                    .CreateOrUpdateSourceControlAsync(
                        resourceGroup.Name,
                        azureWebsiteName,
                        siteSourceControlRequest
                    ).Result;
            }

            //var tokenCredentials = new AzureAdTokenCredentials("microsoft.onmicrosoft.com", AzureEnvironments.AzureCloudEnvironment);
            //var tokenProvider = new AzureAdTokenProvider(tokenCredentials);


            //IAzure azure = Azure.Authenticate(credFile).WithDefaultSubscription();

            //AzureCredentials credentials = new Authentication.AzureCredentials();

            //Azure.Authenticate();

            //var tenantId = "tenantId (or directory Id) of your Azure Active Directory";
            //var azureServiceTokenProvider = new AzureServiceTokenProvider();
            //var token = await azureServiceTokenProvider.GetAccessTokenAsync(
            //    "https://management.azure.com",
            //    tenantId
            //);
            //var tokenCredentials = new TokenCredentials(token);
            //var azure = Azure
            //    .Configure()
            //    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
            //    .Authenticate(new AzureCredentials(
            //        tokenCredentials,
            //        tokenCredentials,
            //        tenantId,
            //        AzureEnvironment.AzureGlobalCloud))
            //    .WithDefaultSubscription();

            Console.WriteLine("Hello World!");
        }

        public static int ReadIndex(int indexMaxValue, string selectionPrefix) {
            int? selection = null;

            while (!selection.HasValue) {
                try {
                    if (!string.IsNullOrEmpty(selectionPrefix)) {
                        Console.WriteLine(selectionPrefix);
                    }

                    var selectionTmp = Convert.ToInt32(Console.ReadLine());

                    if (selectionTmp < 0 || selectionTmp >= indexMaxValue) {
                        Console.WriteLine("Invalid Value. Please select a number in range 0 to {0}.",
                            indexMaxValue - 1);
                    }
                    else {
                        selection = selectionTmp;
                    }
                }
                catch (Exception ex) {
                    if (!(ex is FormatException || ex is OverflowException)) {
                        throw;
                    }
                }
            }

            return selection.Value;
        }

        public static AzureEnvironment SelectEnvironment() {
            Console.WriteLine("Please select Azure environment to use:");

            var index = 0;
            foreach (var environment in AzureEnvironment.KnownEnvironments) {
                Console.WriteLine("{0}: {1}", index, environment.Name);
                ++index;
            }

            var selection = ReadIndex(
                AzureEnvironment.KnownEnvironments.Count(),
                "Please choose which environment to use: "
            );

            return AzureEnvironment.KnownEnvironments.ElementAt(selection);
        }

        public static AzureCloudInstance ToAzureCloudInstance(AzureEnvironment azureEnvironment) {
            if (azureEnvironment.Equals(AzureEnvironment.AzureGlobalCloud)) {
                return AzureCloudInstance.AzurePublic;
            }
            else if (azureEnvironment.Equals(AzureEnvironment.AzureChinaCloud)) {
                return AzureCloudInstance.AzureChina;
            }
            else if (azureEnvironment.Equals(AzureEnvironment.AzureGermanCloud)) {
                return AzureCloudInstance.AzureGermany;
            }
            else if (azureEnvironment.Equals(AzureEnvironment.AzureUSGovernment)) {
                return AzureCloudInstance.AzureUsGovernment;
            } else {
                throw new SystemException("Unknown AzureEnvironment: " + azureEnvironment.Name);
            }
        }

        public static string GetTenantID() {
            Console.WriteLine("Please provide your TenantId:");
            var tenantId = Console.ReadLine();
            return tenantId;
        }

        public static IAccount SelectAccount(IEnumerable<IAccount> accounts) {
            IAccount account;

            var accountsCount = accounts.Count();

            if (accountsCount == 0) {
                throw new System.SystemException("The program was not able to find account for current user.");
            }
            else if (accountsCount == 1) {
                account = accounts.FirstOrDefault();

                Console.WriteLine("The following account will be used: {0}.", account.HomeAccountId.ObjectId);
            }
            else {
                Console.WriteLine("The following accounts are available:");

                var index = 0;
                foreach (var curAccount in accounts) {
                    Console.WriteLine("{0}: {1} {2}",
                        index, curAccount.HomeAccountId.ObjectId, curAccount.Username);
                    ++index;
                }

                var selection = ReadIndex(accountsCount, "Please choose which account to use: ");
                account = accounts.ElementAt(selection);
            }

            Console.WriteLine();

            return account;
        }

        public static Microsoft.Azure.Management.ResourceManager.Fluent.ISubscription SelectSubscription(Azure.IAuthenticated authenticated) {
            // ToDo: Handle exceptions when user does npot have enough permissions to list subscriptions.
            var subscriptionsList = authenticated.Subscriptions.List();
            var subscriptionsCount = subscriptionsList.Count();

            Microsoft.Azure.Management.ResourceManager.Fluent.ISubscription subscription;

            if (subscriptionsCount == 0) {
                throw new SystemException("The account does not contain any subscription");
            } else if (subscriptionsCount == 1) {
                subscription = subscriptionsList.First();

                Console.WriteLine("The following subscription will be used: SubscriptionId: {0}, DisplayName: {1}",
                    subscription.SubscriptionId, subscription.DisplayName);
            } else {
                Console.WriteLine("The following subscriptions are available:");

                var index = 0;
                foreach (var curSubscription in subscriptionsList) {
                    Console.WriteLine("{0}: SubscriptionId: {1}, DisplayName: {2}",
                        index, curSubscription.SubscriptionId, curSubscription.DisplayName);

                    ++index;
                }

                var selection = ReadIndex(subscriptionsCount, "Please select which subscription to use: ");
                subscription = subscriptionsList.ElementAt(selection);
            }

            Console.WriteLine();

            return subscription;
        }

        public static IResourceGroup SelectOrCreateResourceGroup(IAzure azure, string resourceGroupDefaultName = null) {
            Console.WriteLine("Do you want to create a new ResourceGroup or use an existing one ? " +
                "Please select N[new] or E[existing]");

            var response = ConsoleKey.Escape;
            while (!ConsoleKey.N.Equals(response) && !ConsoleKey.E.Equals(response)) {
                response = Console.ReadKey(false).Key;

                if (response != ConsoleKey.Enter) {
                    Console.WriteLine();
                }
            }

            IResourceGroup resourceGroup;

            if (ConsoleKey.E.Equals(response)) {
                resourceGroup = SelectExistingResourceGroup(azure);
            } else {
                resourceGroup = CreateNewResourceGroup(azure, resourceGroupDefaultName);
            }

            return resourceGroup;
        }

        public static IResourceGroup SelectExistingResourceGroup(IAzure azure) {
            var resourceGroups = azure.ResourceGroups.List();

            Console.WriteLine("Available resource groups:");

            var index = 0;
            foreach (var resourceGroup in resourceGroups) {
                Console.WriteLine("{0}: {1} {2}", index, resourceGroup.Id, resourceGroup.Name);
                ++index;
            }

            var selection = ReadIndex(resourceGroups.Count(), "Select an option: ");
            return resourceGroups.ElementAt(selection);
        }

        public static IResourceGroup CreateNewResourceGroup(IAzure azure, string resourceGroupDefaultName = null) {
            Console.WriteLine("Please select region where resource group will be created.");
            Console.WriteLine();

            Console.WriteLine("Available regions:");
            var index = 0;
            foreach(var _region in _functionalRegions) {
                Console.WriteLine("{0}: {1}", index, _region.Name);
                ++index;
            }

            var regionSelection = ReadIndex(_functionalRegions.Length, "Select a region: ");
            var region = _functionalRegions[regionSelection];

            if (string.IsNullOrEmpty(resourceGroupDefaultName)) {
                Console.WriteLine("Select resource group name:");
            }
            else {
                Console.WriteLine("Select resource group name, press Enter to use '{0}':", resourceGroupDefaultName);
            }

            var resourceGroupName = Console.ReadLine();
            if (string.IsNullOrEmpty(resourceGroupName)) {
                resourceGroupName = resourceGroupDefaultName;
            }

            var resourceGroupAlreadyExists = azure
                .ResourceGroups
                .ContainAsync(resourceGroupName)
                .Result;

            if (resourceGroupAlreadyExists) {
                throw new System.Exception($"Resource group with name \"{resourceGroupName}\" already exists.");
            }

            var resourceGroup = azure.ResourceGroups
                .Define(resourceGroupName)
                .WithRegion(region)
                .WithTags(new Dictionary<string, string> {
                    { "owner", "kakostan@microsoft.com" },
                    { "application", "omp" }
                })
                .CreateAsync()
                .Result;

            return resourceGroup;
        }

        public static string GetApplicationName() {
            Console.WriteLine("Please provide a name for the AAD application to register:");
            var applicationName = Console.ReadLine();
            return applicationName;
        }

        public static void ListSubscriptionsUsingRestClient(AzureEnvironment azureEnvironment, AzureCredentials azureCredentials) {
            var restClient = RestClient
                .Configure()
                .WithEnvironment(azureEnvironment)
                .WithCredentials(azureCredentials)
                //.WithLogLevel(HttpLoggingDelegatingHandler.Level.BodyAndHeaders)
                .Build();

            using (var subscriptionClient = new SubscriptionClient(restClient)) {
                var subscriptionsList = subscriptionClient.Subscriptions.ListAsync().Result;

                Console.WriteLine("Subscriptions:");

                foreach (var subscription in subscriptionsList) {
                    Console.WriteLine("SubscriptionId: {0}, DisplayName: {1}",
                        subscription.SubscriptionId, subscription.DisplayName);
                }
            };

            Console.WriteLine();
        }




        public static byte[] ToBase64Bytes(string message) {
            return System.Text.Encoding.UTF8.GetBytes(message);
        }

        public static RequiredResourceAccess GetRequiredResourceAccessByDisplayName(
            IGraphServiceClient graphServiceClient,
            string displayName,
            IEnumerable<string> requiredDelegatedPermissions
        ) {
            var displayNameFilterClause = string.Format("DisplayName eq '{0}'", displayName);

            var servicePrincipals = graphServiceClient
                .ServicePrincipals
                .Request().Filter(displayNameFilterClause)
                .GetAsync()
                .Result;

            if (servicePrincipals.Count != 1) {
                var msg = string.Format("Could not find ServicePrincipal with '{0}' DisplayName", displayName);
                throw new SystemException(msg);
            }

            var servicePrincipal = servicePrincipals.First();

            var resourceAccesses = new List<ResourceAccess>();

            foreach (var requiredDelegatedPermission in requiredDelegatedPermissions) {
                // !!!!! ToDo: Use PublishedPermissionScopes instead of oauth2Permissions when available !!!!!
                var oauth2Permissions = servicePrincipal
                    .Oauth2Permissions
                    .Where(permission => permission.Value == requiredDelegatedPermission)
                    .ToList();

                if (oauth2Permissions.Count != 1) {
                    var msg = string.Format("Could not  find Oauth2Permission with '{0}' Value", requiredDelegatedPermission);
                    throw new System.Exception(msg);
                }

                var oauth2Permission = oauth2Permissions.First();

                var resourceAccess = new ResourceAccess {
                    Type = "Scope",
                    Id = oauth2Permission.Id
                };

                resourceAccesses.Add(resourceAccess);
            }

            var requiredResourceAccess = new RequiredResourceAccess {
                ResourceAppId = servicePrincipal.AppId,
                ResourceAccess = resourceAccesses
            };

            return requiredResourceAccess;
        }

        public static async Task AddAppRoleAssignmentAsync(
            ServicePrincipal servicePrincipal,
            string accessToken,
            AppRoleAssignment appRoleAssignment
        ) {
            const string ROLE_ASSIGNMENT_FORMATTER = "https://graph.microsoft.com/beta/servicePrincipals/{0}/appRoleAssignments";
            var url = string.Format(ROLE_ASSIGNMENT_FORMATTER, servicePrincipal.Id);

            using (var client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken
                );

                var content = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(appRoleAssignment),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode) {
                    return;
                }
                else {
                    throw new HttpRequestException(response.ReasonPhrase);
                }
            }
        }

        public static async Task<ServicePrincipal> GetServicePrincipalByAppIdAsync(
            IGraphServiceClient graphServiceClient,
            string AppId
        ) {
            var clientAppIdFilterClause = string.Format("AppId eq '{0}'", AppId);

            var clientApplicationServicePrincipals = await graphServiceClient
                .ServicePrincipals
                .Request()
                .Filter(clientAppIdFilterClause)
                .GetAsync();

            if (clientApplicationServicePrincipals.Count == 0) {
                var msg = string.Format("Unable to find ServicePrincipal with AppId={0}", AppId);
                throw new System.Exception(msg);
            }

            if (clientApplicationServicePrincipals.Count > 1) {
                var msg = string.Format("Found more than one ServicePrincipal with AppId={0}", AppId);
                throw new System.Exception(msg);
            }

            return clientApplicationServicePrincipals.First();
        }

        public static string GeneratePassword(int length) {
            // ToDo: Make it cryptographically sound at some point.
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();
            var password = new string(Enumerable
                .Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)])
                .ToArray()
            );

            return password;
        }
    }
}
