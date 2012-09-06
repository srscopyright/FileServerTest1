// =====COPYRIGHT=====
// Copyright 2007 - 2012 Service Repair Solutions, Inc.
// =====COPYRIGHT=====
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Srs.WebPlatform.Common;

namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// 
    /// </summary>
    [ServiceContract(Namespace = "Srs.WebPlatform.WebServices.Printer")]
    public interface IHeartBeatMonitoringServiceV1
    {
        /// <summary>
        /// Check all method from Service Printer such as
        ///     + GetNumberOfPage()
        ///     + ConvertURLToImage
        ///       - ConvertImageURLToImage
        ///       - ConvertPDFURLToImage
        ///     + ConvertURLToPDF
        ///       - ConvertImageURLToPDF
        ///       - ConvertPDFURLToPDF
        /// </summary>
        /// <returns>
        ///     True  : if all of them are pass
        ///     False : if one of them is fail
        /// </returns>
        [OperationContract, WebGet, WebServiceMethodAttributeV1("978b5787-e78f-4646-ba66-254dffae88cc")]
        bool HeartBeat();
    }
}
