// =====COPYRIGHT=====
// Copyright 2007 - 2012 Service Repair Solutions, Inc.
// =====COPYRIGHT=====
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using System.Xml.Linq;
using Srs.WebPlatform.Common;

namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// 
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple, Name = "HeartBeatMonitoringServiceBehavior")]
    public sealed partial class HeartBeatService
    {
        /// <summary>
        /// ConvertURLToImage
        /// </summary>
        /// <param name="sessionTokenId">sessionTokenId</param>
        /// <param name="extension">extension</param>
        /// <param name="url">url</param>
        /// <param name="marginLeft">marginLeft</param>
        /// <param name="marginTop">marginTop</param>
        /// <param name="format">format</param>
        /// <param name="page">page</param>
        /// <returns></returns>
        public byte[] ConvertURLToImage(string sessionTokenId, string extension, string url, int marginLeft, int marginTop, string format, int page)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string domainName = string.Format(CultureInfo.InvariantCulture, const_Domainformat,
                                              httpContext.Request.Url.Scheme,
                                              httpContext.Request.Url.Host,
                                              httpContext.Request.Url.Port);

                MemoryStream memoryStream = new MemoryStream();
                string requestUrlForConvert = string.Format(ConfigurationManager.AppSettings[StaticConstants.Const_ConvertURLToImageFormat],
                                                            domainName+ConfigurationManager.AppSettings[StaticConstants.Const_PrinterUrl],
                                                            sessionTokenId,
                                                            HttpUtility.UrlEncode(url),
                                                            marginLeft,
                                                            marginTop,
                                                            format,
                                                            page);
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(requestUrlForConvert);
                webrequest.Method = StaticConstants.Const_Get;
                HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    if (stream != null) stream.CopyTo(memoryStream);
                }
                return memoryStream.ToArray();
            }
            catch (UriFormatException uriFormatException)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, 
                                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                                    Environment.MachineName, 
                                    uriFormatException.StackTrace, 
                                    HttpUtility.UrlEncode(url) + uriFormatException.Message + uriFormatException.StackTrace);
                return null;
            }
            catch (WebException webException)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name,
                                    Assembly.GetExecutingAssembly().GetName().Version.ToString(), 
                                    Environment.MachineName, 
                                    webException.StackTrace, 
                                    HttpUtility.UrlEncode(url) + webException.Message + webException.StackTrace);
                return null;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, 
                                    Assembly.GetExecutingAssembly().GetName().Version.ToString(), 
                                    Environment.MachineName, 
                                    ex.StackTrace, 
                                    HttpUtility.UrlEncode(url) + ex.Message + ex.StackTrace);
                return null;
            }
            
        }

        /// <summary>
        /// GetNumberOfPages
        /// </summary>
        /// <param name="sessionTokenId">sessionTokenId</param>
        /// <param name="extension">extension</param>
        /// <param name="url">url</param>
        /// <param name="marginLeft">marginLeft</param>
        /// <param name="marginTop">marginTop</param>
        /// <returns></returns>
        public int GetNumberOfPages(string sessionTokenId, string extension, string url, int marginLeft, int marginTop)
        {
            try
            {
                HttpContext httpContext = HttpContext.Current;
                string domainName = string.Format(CultureInfo.InvariantCulture, const_Domainformat,
                                              httpContext.Request.Url.Scheme,
                                              httpContext.Request.Url.Host,
                                              httpContext.Request.Url.Port);

                XDocument xmlDocument = null;
                string requestUrl =
                    string.Format(ConfigurationManager.AppSettings[StaticConstants.Const_GetPageNumberFormat],
                                  domainName+ConfigurationManager.AppSettings[StaticConstants.Const_PrinterUrl],
                                  sessionTokenId,
                                  HttpUtility.UrlEncode(url),
                                  marginLeft,
                                  marginTop);

                HttpWebRequest webrequest = (HttpWebRequest) WebRequest.Create(requestUrl);
                webrequest.Method = StaticConstants.Const_Get;
                HttpWebResponse response = (HttpWebResponse) webrequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    string xml = new StreamReader(stream).ReadToEnd();
                    if (!string.IsNullOrEmpty(xml)) xmlDocument = XDocument.Parse(xml);
                }
                return Convert.ToInt32(xmlDocument.Root.Value);
            }
            catch (UriFormatException uriFormatException)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name,
                                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                                    Environment.MachineName,
                                    uriFormatException.StackTrace,
                                    HttpUtility.UrlEncode(url) + uriFormatException.Message + uriFormatException.StackTrace);
                return 0;
            }
            catch (WebException webException)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name,
                                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                                    Environment.MachineName,
                                    webException.StackTrace,
                                    HttpUtility.UrlEncode(url) + webException.Message + webException.StackTrace);
                return 0;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name,
                                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                                    Environment.MachineName,
                                    ex.StackTrace,
                                    HttpUtility.UrlEncode(url) + ex.Message + ex.StackTrace);
                return 0;
            }
        }
    }
}
