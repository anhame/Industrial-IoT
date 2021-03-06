// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Services.OpcUa.History.v2.Controllers {
    using Microsoft.Azure.IIoT.Services.OpcUa.History.v2.Auth;
    using Microsoft.Azure.IIoT.Services.OpcUa.History.v2.Filters;
    using Microsoft.Azure.IIoT.Services.OpcUa.History.v2.Models;
    using Microsoft.Azure.IIoT.OpcUa.History;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// History insert services
    /// </summary>
    [Route(VersionInfo.PATH + "/insert")]
    [ExceptionsFilter]
    [Produces(ContentEncodings.MimeTypeJson)]
    [Authorize(Policy = Policies.CanUpdate)]
    public class InsertController : Controller {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="historian"></param>
        public InsertController(IHistorianServices<string> historian) {
            _historian = historian ?? throw new ArgumentNullException(nameof(historian));
        }

        /// <summary>
        /// Insert historic values
        /// </summary>
        /// <remarks>
        /// Insert historic values using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="endpointId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history insert request</param>
        /// <returns>The history insert result</returns>
        [HttpPost("{endpointId}/values")]
        public async Task<HistoryUpdateResponseApiModel> HistoryInsertValuesAsync(
            string endpointId,
            [FromBody] [Required] HistoryUpdateRequestApiModel<InsertValuesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryInsertValuesAsync(
                endpointId, request.ToServiceModel(d => d.ToServiceModel()));
            return new HistoryUpdateResponseApiModel(writeResult);
        }

        /// <summary>
        /// Insert historic events
        /// </summary>
        /// <remarks>
        /// Insert historic events using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="endpointId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history insert request</param>
        /// <returns>The history insert result</returns>
        [HttpPost("{endpointId}/events")]
        public async Task<HistoryUpdateResponseApiModel> HistoryInsertEventsAsync(
            string endpointId,
            [FromBody] [Required] HistoryUpdateRequestApiModel<InsertEventsDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryInsertEventsAsync(
                endpointId, request.ToServiceModel(d => d.ToServiceModel()));
            return new HistoryUpdateResponseApiModel(writeResult);
        }

        private readonly IHistorianServices<string> _historian;
    }
}
