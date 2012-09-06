// =====COPYRIGHT=====
// github http copyright text
// =====COPYRIGHT=====
using System;
using System.Configuration;
using System.Globalization;
using System.Web;

namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// 
    /// </summary>
    partial class HeartBeatService:IHeartBeatMonitoringServiceV1
    {
        private const string const_Domainformat = "{0}://{1}:{2}";

        /// <summary>
        /// Make sure all of servive methods are pass
        /// </summary>
        /// <returns>
        ///     True  : all Passed
        ///     False : fail on one of them
        /// </returns>
        bool IHeartBeatMonitoringServiceV1.HeartBeat()
        {
            bool checkHeartBeatResult;

            HttpContext httpContext = HttpContext.Current;
            string domainName = string.Format(CultureInfo.InvariantCulture, const_Domainformat,
                                              httpContext.Request.Url.Scheme, 
                                              httpContext.Request.Url.Host,
                                              httpContext.Request.Url.Port);

          

            // Check GetNumberOfPages of Image
            if (GetNumberOfPages(StaticConstants.Const_DummySessionToken,
                                string.Empty,
                                domainName + ConfigurationManager.AppSettings[StaticConstants.Const_ImageURLForTest],
                                Convert.ToInt32(ConfigurationManager.AppSettings[StaticConstants.Const_ExpectedMarginLeft]),
                                Convert.ToInt32(ConfigurationManager.AppSettings[StaticConstants.Const_ExpectedMarginTop])) > 0)
                checkHeartBeatResult = true;
            else
                return false;

            // Check ConvertURLToImage with Image URL
            if (ConvertURLToImage(StaticConstants.Const_DummySessionToken,
                                    string.Empty,
                                    domainName +  ConfigurationManager.AppSettings[StaticConstants.Const_ImageURLForTest],
                                    Convert.ToInt32(ConfigurationManager.AppSettings[StaticConstants.Const_ExpectedMarginLeft]),
                                    Convert.ToInt32(ConfigurationManager.AppSettings[StaticConstants.Const_ExpectedMarginTop]),
                                    ConfigurationManager.AppSettings[StaticConstants.Const_ExpectedImageFormat],
                                    Convert.ToInt32(ConfigurationManager.AppSettings[StaticConstants.Const_PageExpectToPrint])).Length > 0)
                checkHeartBeatResult = true;
            else
                return false;

          

         

            return checkHeartBeatResult;
        }
    }
}
