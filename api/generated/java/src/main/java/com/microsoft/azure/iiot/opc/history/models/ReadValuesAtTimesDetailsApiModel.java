/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 *
 * Code generated by Microsoft (R) AutoRest Code Generator 1.0.0.0
 * Changes may cause incorrect behavior and will be lost if the code is
 * regenerated.
 */

package com.microsoft.azure.iiot.opc.history.models;

import java.util.List;
import org.joda.time.DateTime;
import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * Read data at specified times.
 */
public class ReadValuesAtTimesDetailsApiModel {
    /**
     * Requested datums.
     */
    @JsonProperty(value = "reqTimes", required = true)
    private List<DateTime> reqTimes;

    /**
     * Whether to use simple bounds.
     */
    @JsonProperty(value = "useSimpleBounds")
    private Boolean useSimpleBounds;

    /**
     * Get requested datums.
     *
     * @return the reqTimes value
     */
    public List<DateTime> reqTimes() {
        return this.reqTimes;
    }

    /**
     * Set requested datums.
     *
     * @param reqTimes the reqTimes value to set
     * @return the ReadValuesAtTimesDetailsApiModel object itself.
     */
    public ReadValuesAtTimesDetailsApiModel withReqTimes(List<DateTime> reqTimes) {
        this.reqTimes = reqTimes;
        return this;
    }

    /**
     * Get whether to use simple bounds.
     *
     * @return the useSimpleBounds value
     */
    public Boolean useSimpleBounds() {
        return this.useSimpleBounds;
    }

    /**
     * Set whether to use simple bounds.
     *
     * @param useSimpleBounds the useSimpleBounds value to set
     * @return the ReadValuesAtTimesDetailsApiModel object itself.
     */
    public ReadValuesAtTimesDetailsApiModel withUseSimpleBounds(Boolean useSimpleBounds) {
        this.useSimpleBounds = useSimpleBounds;
        return this;
    }

}
