// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.OpcUa.Models {

    /// <summary>
    /// Server onboarding request
    /// </summary>
    public class ServerRegistrationRequestModel {

        /// <summary>
        /// User defined registration id
        /// </summary>
        public string RegistrationId { get; set; }

        /// <summary>
        /// Discovery url to use for registration
        /// </summary>
        public string DiscoveryUrl { get; set; }

        /// <summary>
        /// Callback to invoke once registration finishes
        /// </summary>
        public CallbackModel Callback { get; set; }
    }
}
