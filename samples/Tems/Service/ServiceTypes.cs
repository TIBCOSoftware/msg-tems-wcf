/*
 * Copyright © 2021. TIBCO Software Inc.
 * This file is subject to the license terms contained
 * in the license file that is distributed with this file.
 * 
 */

namespace com.tibco.sample.service
{
    // These classes allow multiple Service Endpoints to be defined using the same basic
    // service. Each Service element name must be unique. Otherwise the following
    // exception is thrown:
    //     A child element named 'service' with same key already exists at the same
    //     configuration scope. Collection elements must be unique within the same
    //     configuration scope (e.g. the same application.config file).
    //     Duplicate key value: 'com.tibco.sample.service.Service[...]Type'.

    public class ServiceRequestReplyTypeHttp : ServiceRequestReplyType { }
    public class ServiceRequestReplySessionTypePipe : ServiceRequestReplySessionType { }
    public class ServiceDatagramSessionTypePipe : ServiceDatagramSessionType { }
    public class ServiceDatagramSessionTypeTcp : ServiceDatagramSessionType { }
}
