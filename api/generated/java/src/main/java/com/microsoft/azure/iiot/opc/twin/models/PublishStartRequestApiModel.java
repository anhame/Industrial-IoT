/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 *
 * Code generated by Microsoft (R) AutoRest Code Generator 1.0.0.0
 * Changes may cause incorrect behavior and will be lost if the code is
 * regenerated.
 */

package com.microsoft.azure.iiot.opc.twin.models;

import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * Publish request.
 */
public class PublishStartRequestApiModel {
    /**
     * Item to publish.
     */
    @JsonProperty(value = "item", required = true)
    private PublishedItemApiModel item;

    /**
     * Optional request header.
     */
    @JsonProperty(value = "header")
    private RequestHeaderApiModel headerProperty;

    /**
     * Get item to publish.
     *
     * @return the item value
     */
    public PublishedItemApiModel item() {
        return this.item;
    }

    /**
     * Set item to publish.
     *
     * @param item the item value to set
     * @return the PublishStartRequestApiModel object itself.
     */
    public PublishStartRequestApiModel withItem(PublishedItemApiModel item) {
        this.item = item;
        return this;
    }

    /**
     * Get optional request header.
     *
     * @return the headerProperty value
     */
    public RequestHeaderApiModel headerProperty() {
        return this.headerProperty;
    }

    /**
     * Set optional request header.
     *
     * @param headerProperty the headerProperty value to set
     * @return the PublishStartRequestApiModel object itself.
     */
    public PublishStartRequestApiModel withHeaderProperty(RequestHeaderApiModel headerProperty) {
        this.headerProperty = headerProperty;
        return this;
    }

}
