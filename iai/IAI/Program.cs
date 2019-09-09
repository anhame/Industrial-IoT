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

        static void Main(string[] args) {

            //var azureEnvironment = SelectEnvironment();
            var azureEnvironment = AzureEnvironment.AzureGlobalCloud;
            var azureCloudInstance = ToAzureCloudInstance(azureEnvironment);

            // ToDo: Figure out how to sign-in without tenantId
            //var tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";  // microsoft.onmicrosoft.com
            var tenantId = "6e660ce4-d51a-4585-80c6-58035e212354";  // opcwalls.onmicrosoft.com
            //var tenantId = "organizations";  // Generic one for multi-tenant applications

            // ClientId of AzureIndustrialIoTIAI
            const string iaiClientID = "fb2ca262-60d8-4167-ac33-1998d6d5c50b";

            var microsoftGraphScopes = new string[] {
                "https://graph.microsoft.com/Directory.AccessAsUser.All"
            };

            var azureManagementScopes = new string[] {
                //"https://management.core.windows.net//user_impersonation",
                "https://management.azure.com/user_impersonation"
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
                    .ExecuteAsync()
                    .Result;




            //var accounts = publicClientApplication.GetAccountsAsync().Result;
            //var account = SelectAccount(accounts);

            //var managementToken = publicClientApplication.AcquireTokenSilent(azureManagementScopes, account);
            var azureManagementAuthenticatoinResult = publicClientApplication
                .AcquireTokenSilent(azureManagementScopes, microsoftGraphAuthenticatoinResult.Account)
                .ExecuteAsync()
                .Result;

            var provider = new StringTokenProvider(microsoftGraphAuthenticatoinResult.AccessToken, "Bearer");

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

            var azureCredentials = new AzureCredentials(
                //microsoftGraphTokenCredentials,
                azureManagementTokenCredentials,
                microsoftGraphTokenCredentials,
                microsoftGraphAuthenticatoinResult.TenantId,
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
            var azure = authenticated.WithSubscription(subscription.SubscriptionId);


            //////////////////////////////////////////////////////
            ////Console.WriteLine("Subscriptoins:");
            ////foreach (var curSubscription in azure.Subscriptions.List()) {
            ////    Console.WriteLine("SubscriptionId: {0}, DisplayName: {1}",
            ////        curSubscription.SubscriptionId, curSubscription.DisplayName);
            ////}
            ////Console.WriteLine();
            //////////////////////////////////////////////////////


            var resourceGroup = SelectOrCreateResourceGroup(azure);




            var applicationName = GetApplicationName();

            var servicesApplicationName = applicationName + "-services";
            var clientsApplicationName = applicationName + "-clients";

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

            var serviceApplicationApproverRoleId = Guid.NewGuid();
            serviceApplicationAppRoles.Add(new AppRole {
                DisplayName = "Approver",
                Value = "Sign",
                Description = "Approvers have the ability to issue certificates.",
                AllowedMemberTypes = new List<string> { "User" },
                Id = serviceApplicationApproverRoleId
            });

            var serviceApplicationWriterRoleId = Guid.NewGuid();
            serviceApplicationAppRoles.Add(new AppRole {
                DisplayName = "Writer",
                Value = "Write",
                Description = "Writers Have the ability to change entities.",
                AllowedMemberTypes = new List<string> { "User" },
                Id = serviceApplicationWriterRoleId
            });

            var serviceApplicationAdministratorRoleId = Guid.NewGuid();
            serviceApplicationAppRoles.Add(new AppRole {
                DisplayName = "Administrator",
                Value = "Admin",
                Description = "Admins can access advanced features.",
                AllowedMemberTypes = new List<string> { "User" },
                Id = serviceApplicationAdministratorRoleId
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
            var serviceApplicatoinPermissionUserImpersonationId = Guid.NewGuid();

            var oauth2Permissions = new List<PermissionScope> {
                new PermissionScope {
                    AdminConsentDescription = string.Format("Allow the app to access {0} on behalf of the signed-in user.", servicesApplicationName),
                    AdminConsentDisplayName = string.Format("Access {0}", servicesApplicationName),
                    Id = serviceApplicatoinPermissionUserImpersonationId,
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
                Tags = new List<string> { "kakostan@microsoft.com" },
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
                // Create new service principal
                var serviceApplicationServicePrincipalRequest = new ServicePrincipal {
                    DisplayName = servicesApplicationName,
                    AppId = serviceApplication.AppId,
                    Tags = new List<string> {
                        "kakostan@microsoft.com"//,
                        //WindowsAzureActiveDirectoryIntegratedApp
                    }
                };

                serviceApplicationServicePrincipal = graphServiceClient
                    .ServicePrincipals
                    .Request()
                    .AddAsync(serviceApplicationServicePrincipalRequest)
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
                Id = serviceApplicationApproverRoleId.ToString(),
                AppRoleId = serviceApplicationApproverRoleId
            };

            var writerAppRoleAssignmentRequest = new AppRoleAssignment {
                //PrincipalDisplayName = "",
                PrincipalType = "User",
                PrincipalId = new Guid(me.Id),
                ResourceId = new Guid(serviceApplicationServicePrincipal.Id),
                ResourceDisplayName = "Writer",
                Id = serviceApplicationWriterRoleId.ToString(),
                AppRoleId = serviceApplicationWriterRoleId
            };

            var adminAppRoleAssignmentRequest = new AppRoleAssignment {
                //PrincipalDisplayName = "",
                PrincipalType = "User",
                PrincipalId = new Guid(me.Id),
                ResourceId = new Guid(serviceApplicationServicePrincipal.Id),
                ResourceDisplayName = "Admin",
                Id = serviceApplicationAdministratorRoleId.ToString(),
                AppRoleId = serviceApplicationAdministratorRoleId
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
                            Id = serviceApplicatoinPermissionUserImpersonationId,  // "user_impersonation"
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
                Tags = new List<string> { "kakostan@microsoft.com" },
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
                // Create new client principal
                var clientApplicationServicePrincipalRequest = new ServicePrincipal {
                    DisplayName = clientsApplicationName,
                    AppId = clientApplication.AppId,
                    Tags = new List<string> {
                        "kakostan@microsoft.com"//,
                        //WindowsAzureActiveDirectoryIntegratedApp
                    }
                };

                clientApplicationServicePrincipal = graphServiceClient
                    .ServicePrincipals
                    .Request()
                    .AddAsync(clientApplicationServicePrincipalRequest)
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
            var clientApplicationOAuth2PermissionGrantRequest0 = new OAuth2PermissionGrant {
                Id = Guid.NewGuid().ToString(),
                ConsentType = "AllPrincipals",
                ClientId = clientApplicationServicePrincipal.Id,
                ResourceId = serviceApplicationServicePrincipal.Id,
                Scope = "user_impersonation",
                StartTime = DateTimeOffset.UtcNow,
                ExpiryTime = DateTimeOffset.UtcNow.AddYears(2),
            };

            var clientApplicationOAuth2PermissionGrant0 = graphServiceClient
                .Oauth2PermissionGrants
                .Request()
                .AddAsync(clientApplicationOAuth2PermissionGrantRequest0)
                .Result;

            // Grant admin consent for Microsoft Graph "User.Read" API permissions of client applicatoin
            var clientApplicationOAuth2PermissionGrantRequest1 = new OAuth2PermissionGrant {
                Id = Guid.NewGuid().ToString(),
                ConsentType = "AllPrincipals",
                ClientId = clientApplicationServicePrincipal.Id,
                ResourceId = GetServicePrincipalByAppIdAsync(graphServiceClient, AzureApps.MicrosoftGraph.AppId).Result.Id,
                Scope = "User.Read",
                StartTime = DateTimeOffset.UtcNow,
                ExpiryTime = DateTimeOffset.UtcNow.AddYears(2),
            };

            var clientApplicationOAuth2PermissionGrant1 = graphServiceClient
                .Oauth2PermissionGrants
                .Request()
                .AddAsync(clientApplicationOAuth2PermissionGrantRequest1)
                .Result;


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

                    int selectionTmp = Convert.ToInt32(Console.ReadLine());

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
            string tenantId = Console.ReadLine();
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

        public static ISubscription SelectSubscription(Azure.IAuthenticated authenticated) {
            // ToDo: Handle exceptions when user does npot have enough permissions to list subscriptions.
            var subscriptionsList = authenticated.Subscriptions.List();
            var subscriptionsCount = subscriptionsList.Count();

            ISubscription subscription;

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

        public static IResourceGroup SelectOrCreateResourceGroup(IAzure azure) {
            Console.WriteLine("Do you want to create a new ResourceGroup or use an existing one ? " +
                "Please select N[new] or E[existing]");

            ConsoleKey response = ConsoleKey.Escape;
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
                resourceGroup = CreateNewResourceGroup(azure);
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

        public static IResourceGroup CreateNewResourceGroup(IAzure azure) {
            Console.WriteLine("Please select region where resource group will be lovated.");
            Console.WriteLine();

            Console.WriteLine("Available regions:");
            var index = 0;
            foreach(var _region in _functionalRegions) {
                Console.WriteLine("{0}: {1}", index, _region.Name);
                ++index;
            }

            var regionSelection = ReadIndex(_functionalRegions.Length, "Select a region: ");
            var region = _functionalRegions[regionSelection];

            Console.WriteLine("Select resource group name:");
            var resourceGroupName = Console.ReadLine();

            // ToDo: Handle the case where resource group already exists.
            var resourceGroup = azure.ResourceGroups
                .Define(resourceGroupName)
                .WithRegion(region)
                .WithTags(new Dictionary<string, string> {
                    { "owner", "kakostan@microsoft.com" },
                    { "application", "omp" }
                })
                .Create();

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
    }
}
