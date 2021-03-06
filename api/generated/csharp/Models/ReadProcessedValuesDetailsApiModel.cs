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
    /// Read processed historic data
    /// </summary>
    public partial class ReadProcessedValuesDetailsApiModel
    {
        /// <summary>
        /// Initializes a new instance of the
        /// ReadProcessedValuesDetailsApiModel class.
        /// </summary>
        public ReadProcessedValuesDetailsApiModel()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// ReadProcessedValuesDetailsApiModel class.
        /// </summary>
        /// <param name="startTime">Start time to read from.</param>
        /// <param name="endTime">End time to read until</param>
        /// <param name="processingInterval">Interval to process</param>
        /// <param name="aggregateTypeId">The aggregate type node ids</param>
        /// <param name="aggregateConfiguration">A configuration for the
        /// aggregate</param>
        public ReadProcessedValuesDetailsApiModel(System.DateTime? startTime = default(System.DateTime?), System.DateTime? endTime = default(System.DateTime?), double? processingInterval = default(double?), string aggregateTypeId = default(string), AggregateConfigApiModel aggregateConfiguration = default(AggregateConfigApiModel))
        {
            StartTime = startTime;
            EndTime = endTime;
            ProcessingInterval = processingInterval;
            AggregateTypeId = aggregateTypeId;
            AggregateConfiguration = aggregateConfiguration;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets start time to read from.
        /// </summary>
        [JsonProperty(PropertyName = "startTime")]
        public System.DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets end time to read until
        /// </summary>
        [JsonProperty(PropertyName = "endTime")]
        public System.DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets interval to process
        /// </summary>
        [JsonProperty(PropertyName = "processingInterval")]
        public double? ProcessingInterval { get; set; }

        /// <summary>
        /// Gets or sets the aggregate type node ids
        /// </summary>
        [JsonProperty(PropertyName = "aggregateTypeId")]
        public string AggregateTypeId { get; set; }

        /// <summary>
        /// Gets or sets a configuration for the aggregate
        /// </summary>
        [JsonProperty(PropertyName = "aggregateConfiguration")]
        public AggregateConfigApiModel AggregateConfiguration { get; set; }

    }
}
