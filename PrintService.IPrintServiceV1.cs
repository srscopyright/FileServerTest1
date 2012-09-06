// =====COPYRIGHT=====
// github http copyright text
// =====COPYRIGHT=====
using System;
using System.Linq;
using System.IO;
using System.Web;
using WebSupergoo.ABCpdf8;
using Srs.WebPlatform.Common;
using System.Web.Configuration;
using System.Collections;
using System.ServiceModel.Web;
using System.Net;
using System.Globalization;

namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// This is that actual implementation of the PrintService
    /// </summary>
    partial class PrintService : IPrintServiceV1
    {
        // Put license key to Azure setting.

        #region IPrintServiceV1 Members

        /// <summary>
        /// Convert the HTML code from the specified URL to a PDF document and send the 
        /// document as an attachment to the browser
        /// </summary>
        /// <param name="sessionTokenId">SessionTokenId given by user to authorize</param>
        /// <param name="url">The URL of a page that will be rendered</param>
        /// <param name="marginLeft"></param>
        /// <param name="marginTop"></param>
        /// <returns>a byte array that rendered as a PDF, will be null if error</returns>
        Stream IPrintServiceV1.ConvertUrlToPdf(string sessionTokenId, string url, int marginLeft, int marginTop)
        {
            Stream returnStream = null;

            if (marginLeft < 10000 && marginTop < 10000)
            {
                try
                {
                    bool isQualified = IsQualifiedUrl(url.ToString());
                    if (isQualified)
                    {
                        // Create a Doc object
                        XSettings.InstallRedistributionLicense(licenseKey);

                        using (Doc theDoc = new Doc())
                        {
                            //theDoc.SetInfo(0, "RenderDelay", "1000");
                            theDoc.HtmlOptions.UseScript = true;
                            theDoc.HtmlOptions.UseActiveX = true;
                            theDoc.HtmlOptions.UseVideo = true;
                            theDoc.HtmlOptions.PageCacheEnabled = true;
                            theDoc.HtmlOptions.Timeout = 120000; // 120 seconds
                            theDoc.Rect.Inset(marginLeft, marginTop); // add margin

                            // Add the first page of HTML. Save the returned ID as this will be used to add subsequent pages                    
                            theDoc.Page = theDoc.AddPage();

                            int theID = theDoc.AddImageUrl(url.ToString());

                            // Now chain subsequent pages together. Stop when reach a page which wasn't truncated.
                            while (true)
                            {
                                if (!theDoc.Chainable(theID))
                                    break;
                                theDoc.Page = theDoc.AddPage();
                                theDoc.Rendering.DotsPerInch = constDotPerInches; // DPI
                                theDoc.Rendering.SaveQuality = constSaveQuality; // Quality                       
                                theID = theDoc.AddImageToChain(theID);
                            }

                            // After adding the pages we can flatten them. We can't do this until after the pages have been added 
                            // because flattening will invalidate our previous ID and break the chain.
                            for (int i = 1; i <= theDoc.PageCount; i++)
                            {
                                theDoc.PageNumber = i;
                                theDoc.Flatten();
                            }
                            // Get pdf data from the Doc object
                            returnStream = new MemoryStream(theDoc.GetData());
                            //returnByte = theDoc.GetData();

                            theDoc.Clear();
                        }
                    }

                    //TO-DO: Add the HTTP Status Code 403 (Forbidden) when the url is not in supported list
                }
                catch (UriFormatException uriFormatException)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                    LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName,
                                         uriFormatException.StackTrace,
                                         HttpUtility.UrlEncode(url.ToString()) + uriFormatException.Message +
                                         uriFormatException.StackTrace);
                    returnStream = null;
                }
                catch (WebException webException)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                    LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, webException.StackTrace,
                                         HttpUtility.UrlEncode(url.ToString()) + webException.Message +
                                         webException.StackTrace);
                    returnStream = null;
                }
                catch (Exception ex)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                    LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace,
                                         HttpUtility.UrlEncode(url.ToString()) + ex.Message + ex.StackTrace);
                    returnStream = null;
                }
                if (WebOperationContext.Current != null)
                {
                    WebOperationContext.Current.OutgoingResponse.ContentType = "application/pdf";
                    HttpResponseHeader cacheHeader = HttpResponseHeader.CacheControl;
                    WebOperationContext.Current.OutgoingResponse.Headers.Add(cacheHeader,
                                                                             string.Format(CultureInfo.InvariantCulture,
                                                                                           "max-age={0}, must-revalidate",
                                                                                           CachingDuration));
                    //Add one day caching
                }
            } 
            return returnStream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionTokenId"></param>
        /// <param name="url"></param>
        /// <param name="marginLeft"></param>
        /// <param name="marginTop"></param>
        /// <returns></returns>
        int IPrintServiceV1.GetNumberOfPages(string sessionTokenId, string url, int marginLeft, int marginTop)
        {
                string extension = string.Empty;
                extension = GetExtension(url);
                extension = extension.ToLower(CultureInfo.InvariantCulture);
                int numberOfPages = 0;
                if (marginLeft < 10000 && marginTop < 10000)
                {
                    try
                    {
                        bool isQualified = IsQualifiedUrl(url.ToString());
                        if (isQualified)
                        {
                            numberOfPages = GetNumberOfPages(url, extension, marginLeft, marginTop);
                        }
                    }
                    catch (WebException webEx)
                    {
                        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                        LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, webEx.StackTrace,
                                             webEx.Message + webEx.StackTrace + HttpUtility.UrlEncode(url.ToString()));

                    }
                    catch (Exception ex)
                    {
                        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                        LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace,
                                             ex.Message + ex.StackTrace + HttpUtility.UrlEncode(url.ToString()));

                    }
                }
            return numberOfPages;
        }

        /// <summary>
        /// Return an image the of a html page
        /// A html file may be generated to a punk of images
        /// This method return a single image that specific in pageNumber parameter
        /// </summary>
        /// <param name="sessionTokenId">SessionTokenId given by user to authorize</param>
        /// <param name="url">The URL of a page that will be rendered</param>
        /// <param name="marginLeft">Value in integer that indicates the margin in the left of pdf file</param>
        /// <param name="marginTop">Value in integer that indicates the margin in the top of pdf file</param>
        /// <param name="format">Format of return image, currently we support BMP, PNG, GIF, JPG, JPEG</param>
        /// <param name="page">The specific page that return to user. The total number of pages can be got by call method GetNumberOfPages</param>
        /// <returns>Return an image stream</returns>
        Stream IPrintServiceV1.ConvertUrlToImage(string sessionTokenId, string url, int marginLeft, int marginTop, string format, int page)
        {
            Stream returnStream = null;
            format = format.ToUpper(CultureInfo.InvariantCulture);
            if (format == "PNG" || format == "BMP" || format == "GIF" || format == "JPG" || format == "JPEG")
            {
                if (marginLeft < 10000 && marginTop < 10000)
                {
                    string extension = string.Empty;
                    extension = GetExtension(url);
                    extension = extension.ToLower(CultureInfo.InvariantCulture);
                    try
                    {
                        bool isQualified = IsQualifiedUrl(url.ToString());
                        if (isQualified)
                        {
                            switch (extension)
                            {
                                case PDF:
                                    returnStream = ConvertPdfToImage(url, marginLeft, marginTop, page);
                                    break;
                                default:
                                    returnStream = ConvertUrlToImage(url, marginLeft, marginTop, page);
                                    break;
                            }
                        }
                    }
                    catch (UriFormatException uriFormatException)
                    {
                        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                        LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName,
                                             uriFormatException.StackTrace,
                                             HttpUtility.UrlEncode(url.ToString()) + uriFormatException.Message +
                                             uriFormatException.StackTrace);

                    }
                    catch (WebException webException)
                    {
                        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                        LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName,
                                             webException.StackTrace,
                                             HttpUtility.UrlEncode(url.ToString()) + webException.Message +
                                             webException.StackTrace);

                    }
                    catch (Exception ex)
                    {

                        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                        LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace,
                                             HttpUtility.UrlEncode(url.ToString()) + ex.Message + ex.StackTrace);

                    }
                    if (WebOperationContext.Current != null)
                    {
                        WebOperationContext.Current.OutgoingResponse.ContentType = GetMineType(format);
                        HttpResponseHeader cacheHeader = HttpResponseHeader.CacheControl;
                        WebOperationContext.Current.OutgoingResponse.Headers.Add(cacheHeader,
                                                                                 string.Format(
                                                                                     CultureInfo.InvariantCulture,
                                                                                     "max-age={0}, must-revalidate",
                                                                                     CachingDuration));
                    }
                }
            }
            return returnStream;
        }
        #endregion
    }
}
