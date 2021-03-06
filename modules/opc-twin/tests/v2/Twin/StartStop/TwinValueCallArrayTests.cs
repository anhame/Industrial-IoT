// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Modules.OpcUa.Twin.v2.Twin.StartStop {
    using Microsoft.Azure.IIoT.Modules.OpcUa.Twin.Tests;
    using Microsoft.Azure.IIoT.OpcUa.Registry.Models;
    using Microsoft.Azure.IIoT.OpcUa.Testing.Fixtures;
    using Microsoft.Azure.IIoT.OpcUa.Testing.Tests;
    using Microsoft.Azure.IIoT.OpcUa.Twin;
    using System.Net;
    using System.Threading.Tasks;
    using Xunit;
    using Autofac;
    using System;

    [Collection(WriteCollection.Name)]
    public class TwinValueCallArrayTests {

        public TwinValueCallArrayTests(TestServerFixture server) {
            _server = server;
        }

        private EndpointModel Endpoint => new EndpointModel {
            Url = $"opc.tcp://{Dns.GetHostName()}:{_server.Port}/UA/SampleServer",
            Certificate = _server.Certificate?.RawData
        };

        private CallArrayMethodTests<string> GetTests(EndpointRegistrationModel endpoint,
            IContainer services) {
            return new CallArrayMethodTests<string>(
                () => services.Resolve<INodeServices<string>>(), endpoint.Id);
        }

        private readonly TestServerFixture _server;
        private static readonly bool _runAll = Environment.GetEnvironmentVariable("TEST_ALL") != null;

        [SkippableFact]
        public async Task NodeMethodMetadataStaticArrayMethod1Test() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodMetadataStaticArrayMethod1Test();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodMetadataStaticArrayMethod2Test() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodMetadataStaticArrayMethod2Test();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodMetadataStaticArrayMethod3Test() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodMetadataStaticArrayMethod3Test();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod1Test1() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod1Test1();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod1Test2() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod1Test2();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod1Test3() {
            //  Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod1Test3();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod1Test4() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod1Test4();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod1Test5() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod1Test5();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod2Test1() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod2Test1();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod2Test2() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod2Test2();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod2Test3() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod2Test3();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod2Test4() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod2Test4();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod3Test1() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod3Test1();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod3Test2() {
            Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod3Test2();
                });
            }
        }

        [SkippableFact]
        public async Task NodeMethodCallStaticArrayMethod3Test3() {
            // Skip.IfNot(_runAll);
            using (var harness = new TwinModuleFixture()) {
                await harness.RunTestAsync(Endpoint, async (endpoint, services) => {
                    await GetTests(endpoint, services).NodeMethodCallStaticArrayMethod3Test3();
                });
            }
        }

    }
}
