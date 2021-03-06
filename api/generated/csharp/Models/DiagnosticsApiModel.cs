// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator 1.0.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.IIoT.Opc.History.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Diagnostics configuration
    /// </summary>
    public partial class DiagnosticsApiModel
    {
        /// <summary>
        /// Initializes a new instance of the DiagnosticsApiModel class.
        /// </summary>
        public DiagnosticsApiModel()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the DiagnosticsApiModel class.
        /// </summary>
        /// <param name="level">Requested level of response diagnostics.
        /// (default: Status). Possible values include: 'None', 'Status',
        /// 'Operations', 'Diagnostics', 'Verbose'</param>
        /// <param name="auditId">Client audit log entry.
        /// (default: client generated)</param>
        /// <param name="timeStamp">Timestamp of request.
        /// (default: client generated)</param>
        public DiagnosticsApiModel(DiagnosticsLevel? level = default(DiagnosticsLevel?), string auditId = default(string), System.DateTime? timeStamp = default(System.DateTime?))
        {
            Level = level;
            AuditId = auditId;
            TimeStamp = timeStamp;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets requested level of response diagnostics.
        /// (default: Status). Possible values include: 'None', 'Status',
        /// 'Operations', 'Diagnostics', 'Verbose'
        /// </summary>
        [JsonProperty(PropertyName = "level")]
        public DiagnosticsLevel? Level { get; set; }

        /// <summary>
        /// Gets or sets client audit log entry.
        /// (default: client generated)
        /// </summary>
        [JsonProperty(PropertyName = "auditId")]
        public string AuditId { get; set; }

        /// <summary>
        /// Gets or sets timestamp of request.
        /// (default: client generated)
        /// </summary>
        [JsonProperty(PropertyName = "timeStamp")]
        public System.DateTime? TimeStamp { get; set; }

    }
}
