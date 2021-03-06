# coding=utf-8
# --------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
#
# Code generated by Microsoft (R) AutoRest Code Generator 2.3.33.0
# Changes may cause incorrect behavior and will be lost if the code is
# regenerated.
# --------------------------------------------------------------------------

from msrest.serialization import Model


class ApplicationRecordListApiModel(Model):
    """Create response.

    :param applications: Applications found
    :type applications:
     list[~azure-iiot-opc-registry.models.ApplicationRecordApiModel]
    :param last_counter_reset_time: Last counter reset
    :type last_counter_reset_time: datetime
    :param next_record_id: Next record id
    :type next_record_id: int
    """

    _validation = {
        'last_counter_reset_time': {'required': True},
        'next_record_id': {'required': True},
    }

    _attribute_map = {
        'applications': {'key': 'applications', 'type': '[ApplicationRecordApiModel]'},
        'last_counter_reset_time': {'key': 'lastCounterResetTime', 'type': 'iso-8601'},
        'next_record_id': {'key': 'nextRecordId', 'type': 'int'},
    }

    def __init__(self, last_counter_reset_time, next_record_id, applications=None):
        super(ApplicationRecordListApiModel, self).__init__()
        self.applications = applications
        self.last_counter_reset_time = last_counter_reset_time
        self.next_record_id = next_record_id
