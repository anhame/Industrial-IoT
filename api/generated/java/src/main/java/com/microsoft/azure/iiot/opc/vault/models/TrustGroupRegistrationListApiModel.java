/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 *
 * Code generated by Microsoft (R) AutoRest Code Generator 1.0.0.0
 * Changes may cause incorrect behavior and will be lost if the code is
 * regenerated.
 */

package com.microsoft.azure.iiot.opc.vault.models;

import java.util.List;
import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * Trust group registration collection model.
 */
public class TrustGroupRegistrationListApiModel {
    /**
     * Group registrations.
     */
    @JsonProperty(value = "registrations")
    private List<TrustGroupRegistrationApiModel> registrations;

    /**
     * Next link.
     */
    @JsonProperty(value = "nextPageLink")
    private String nextPageLink;

    /**
     * Get group registrations.
     *
     * @return the registrations value
     */
    public List<TrustGroupRegistrationApiModel> registrations() {
        return this.registrations;
    }

    /**
     * Set group registrations.
     *
     * @param registrations the registrations value to set
     * @return the TrustGroupRegistrationListApiModel object itself.
     */
    public TrustGroupRegistrationListApiModel withRegistrations(List<TrustGroupRegistrationApiModel> registrations) {
        this.registrations = registrations;
        return this;
    }

    /**
     * Get next link.
     *
     * @return the nextPageLink value
     */
    public String nextPageLink() {
        return this.nextPageLink;
    }

    /**
     * Set next link.
     *
     * @param nextPageLink the nextPageLink value to set
     * @return the TrustGroupRegistrationListApiModel object itself.
     */
    public TrustGroupRegistrationListApiModel withNextPageLink(String nextPageLink) {
        this.nextPageLink = nextPageLink;
        return this;
    }

}
