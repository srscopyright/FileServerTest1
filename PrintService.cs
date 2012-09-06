// =====COPYRIGHT=====
// github http copyright text
// =====COPYRIGHT=====
using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel.Activation;
using System.ServiceModel;
using Srs.WebPlatform.Common;
using System.Collections;
using WebSupergoo.ABCpdf8;
using System.Net;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// Implementation of the PrintService.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple, Name = "SRSPrinterServiceBehavior")]
    public sealed partial class PrintService : ContextBoundObject
    {
        //private TraceLogger _logger = new TraceLogger();
        private const string constDomainFiltering = "DomainFiltering";
        private const string constFilterdDomain = "FilteredDomains";
        private const string constThreadPool = "ThreadPool";
        private const string constInThread = "InThread";
        private const string constPdf = "pdf";
        private const string constLicenseKey = "LicenseKey";
        private const string constCacheDuration = "CacheDuration";
        private const string constLog = "Log";
        private string licenseKey = string.Empty;
        private const int constDotPerInches = 150;
        private const int constImageQuality = 100;
        private const int constSaveQuality = 100;
        private const string PNG = "png";
        private const string JPG = "jpg";
        private const string GIF = "gif";
        private const string TIFF = "tif";
        private const string PDF = "pdf";
        private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static readonly Dictionary<string, string> mineTypeLookup = new Dictionary<string, string> { 
                                                                                        {"PNG", "image/png"},        
                                                                                        {"JPG","image/jpeg"},
                                                                                        {"JPEG","image/jpeg"},
                                                                                        {"JPE","image/jpeg"},
                                                                                        {"PDF","application/pdf"},
                                                                                        {"BMP","image/x-ms-bmp"},
                                                                                        {"GIF","image/gif"}
                                                                                                            };
        private const bool EnableHostWebBrowser = false;
        private const int ContentCount = 80;
        private const int CachingDuration = 86400; // One day

        private volatile int margineLeft = 0;
        private volatile int margineTop = 0;

        private volatile string url = string.Empty;

        //private Dictionary<string, Thread> threadPool = new Dictionary<string, Thread>();

        //Manage the Thread
        //Each thread will be started when a new url requested. then we terminate the thread
        //When the new url comes, check if there is a thread is being started for this url
        //Start the thread for the new url
        //Keep this thread until the generateimages is not finished
        //When the generateimages is finished
        //Terminate the thread


        //private string licenseKey = RoleEnvironment.GetConfigurationSettingValue(constLicenseKey);
        /// <summary>
        /// 
        /// </summary>
        public PrintService()
        {       
            licenseKey = System.Web.Configuration.WebConfigurationManager.AppSettings[constLicenseKey];

        }
        /// <summary>
        /// Check if an url is qualified to generate a pdf or images
        /// </summary>
        /// <param name="url">Input url</param>
        /// <returns>Returns a boolean value to indicate if an url is qulified or not. True is qualified, False is not qualified.</returns>
        public bool IsQualifiedUrl(string url)
        {
            bool isContained = false;
            // Get the qualified domain from web.config
            string domainFiltering = System.Web.Configuration.WebConfigurationManager.AppSettings[constFilterdDomain];
            try
            {
                //string domainFiltering = RoleEnvironment.GetConfigurationSettingValue(constFilterdDomain);

                // Split the string into an array
                string[] filteredDomains = domainFiltering.Split(new char[] { ',' });

                // Loop the array of domain to check if the input url contains one element in the filtered domains
                // if the input url contains one element in the filtered domains, that is the url has been qualified to generate a pdf file
                // after that we will break the loop and return the value.
                // else we will return false value. That means the url is not qualified to generate a pdf file.
                foreach (string domain in filteredDomains)
                {
                    int indexOfStar = domain.IndexOf("*");

                    string subdomain = string.Empty;
                    if (indexOfStar >= 0)
                    {
                        subdomain = domain.Substring(indexOfStar + 2);
                    }
                    else
                    {
                        subdomain = domain;
                    }
                    if (url.Contains(subdomain))
                    {
                        isContained = true;
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
            }

            return isContained;
        }

        private Stream ConvertUrlToImage(string url, int marginLeft, int marginTop, int page)
        {
            //bool isLogged = Boolean.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings[constLog]);

            MemoryStream memoryStream = null;
            byte[] imageBytes = null;
            //return the coresponding image here for the first page
            XSettings.InstallRedistributionLicense(
                System.Web.Configuration.WebConfigurationManager.AppSettings[constLicenseKey]);

            //Create a new Doc
            using (Doc theDoc = new Doc())
            {
                theDoc.HtmlOptions.UseScript = true; // Enable javascript at the startup time
                theDoc.HtmlOptions.UseActiveX = true; // Enable SVG
                theDoc.HtmlOptions.UseVideo = true; // Enable Video
                theDoc.HtmlOptions.Timeout = 120000;
                // Time out is 2 minutes for ABCPDF .NET Doc calls the url to get the html
                theDoc.HtmlOptions.PageCacheEnabled = true; // Enable ABCPDF .NET page cache                    
                theDoc.Rect.Inset(marginLeft, marginTop); // Insert the margin left and margin top
                theDoc.Page = theDoc.AddPage();

                int theID = theDoc.AddImageUrl(url.ToString());
                // Now chain subsequent pages together. Stop when reach a page which wasn't truncated.
                while (true)
                {
                    if (!theDoc.Chainable(theID))
                        break;
                    theDoc.Page = theDoc.AddPage();
                    theDoc.Rendering.DotsPerInch = constDotPerInches; // set DPI = 150
                    theDoc.Rendering.SaveQuality = constSaveQuality; // set Quality = 100
                    theID = theDoc.AddImageToChain(theID);
                }
                theDoc.PageNumber = page;
                theDoc.Rendering.DotsPerInch = constDotPerInches;
                theDoc.Rendering.SaveQuality = constImageQuality;
                theDoc.Flatten();
                imageBytes = theDoc.Rendering.GetData("abc.png");
                theDoc.Clear();
            }
            memoryStream = new MemoryStream(imageBytes);

            return memoryStream;

        }
        private Stream ConvertPdfToImage(string url, int marginLeft, int marginTop, int page)
        {
            Stream returnStream = null;

            byte[] buffer = new byte[4096];
            int count = 0;
            byte[] pdfBytes = null;
            XSettings.InstallRedistributionLicense(System.Web.Configuration.WebConfigurationManager.AppSettings[constLicenseKey]);


            using (MemoryStream memoryStream = new MemoryStream())
            {
                WebRequest webRequest = WebRequest.Create(url);
                WebResponse webResponse = webRequest.GetResponse();

                Stream stream = webResponse.GetResponseStream();
                do
                {
                    count = stream.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, count);

                } while (count != 0);

                pdfBytes = memoryStream.ToArray();
            }

            using (Doc theDoc = new Doc())
            {
                theDoc.HtmlOptions.UseScript = true;
                theDoc.HtmlOptions.UseActiveX = true; // Enalbe SVG
                theDoc.HtmlOptions.UseVideo = true;
                theDoc.HtmlOptions.PageCacheEnabled = true; //Enable cache
                theDoc.HtmlOptions.Timeout = 120000;
                theDoc.Rect.Inset(marginLeft, marginTop);
                theDoc.Read(pdfBytes);
                theDoc.PageNumber = page;
                theDoc.Rendering.DotsPerInch = constDotPerInches;
                theDoc.Rendering.SaveQuality = constSaveQuality;
                theDoc.Flatten();

                //Each page of pdf will be returned as a byte array

                byte[] images = theDoc.Rendering.GetData("abc.png");
                returnStream = new MemoryStream(images);
                theDoc.Clear();
            }
            return returnStream;
        }

        /// <summary>
        /// This method to add log to SRS Logger to monitor the CPU hang 100%
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <param name="detail"></param>
        private void LogMonitorCPU(string stackTrace, string detail)
        {
            LoggerHelperV1.Warn(AssemblyName, AssemblyVersion, Environment.MachineName, stackTrace, detail);
        }

        /// <summary>
        /// This method check the url that is a pdf url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool IsPdfUrl(string url)
        {
            bool isPdf = false;

            string fullUrl = url.ToString();

            int index = fullUrl.LastIndexOf(".");
            string extension = fullUrl.Substring(index + 1);

            if (constPdf == extension.ToLower())
            {
                isPdf = true;
            }
            return isPdf;
        }

        private string GetExtension(string url)
        {
            string fullUrl = url.ToString();
            int index = fullUrl.LastIndexOf(".");
            string extension = fullUrl.Substring(index + 1);
            string[] queryString = extension.Split(new char[] { '?', '#', '&' });
            if (queryString.Length > 1)
            {
                extension = queryString[0];
            }

            return extension;
        }

        private int GetNumberOfPages(string url, string extension, int marginLeft, int marginTop)
        {
            this.margineLeft = marginLeft;
            this.margineTop = marginTop;
            this.url = url;
            int PageCount = 0;
            bool isLogged = Boolean.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings[constLog]);
            // This code line sets the license key to the Doc object
            XSettings.InstallRedistributionLicense(System.Web.Configuration.WebConfigurationManager.AppSettings[constLicenseKey]);

            //Create a new Doc

            switch (extension)
            {
                //If the url is a pdf file
                case PDF:
                    byte[] buffer = new byte[4096];
                    int count = 0;
                    byte[] pdfBytes = null;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        WebRequest webRequest = WebRequest.Create(url);
                        WebResponse webResponse = webRequest.GetResponse();
                        Stream stream = webResponse.GetResponseStream();
                        do
                        {
                            count = stream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);

                        } while (count != 0);

                        pdfBytes = memoryStream.ToArray();

                    }
                    using (Doc theDoc = new Doc())
                    {
                        theDoc.HtmlOptions.UseScript = true;
                        theDoc.HtmlOptions.UseActiveX = true;
                        theDoc.HtmlOptions.UseVideo = true;
                        theDoc.HtmlOptions.PageCacheEnabled = true;
                        theDoc.HtmlOptions.Timeout = 120000;
                        theDoc.Rect.Inset(marginLeft, marginTop);
                        theDoc.Read(pdfBytes);
                        PageCount = theDoc.PageCount;
                    }
                    return PageCount;
                //break;
                default:
                    using (Doc htmlDoc = new Doc())
                    {
                        //Start new thread to generate images when user call GetNumberOfPages method
                        htmlDoc.HtmlOptions.UseScript = true; // Enable javascript at the startup time
                        htmlDoc.HtmlOptions.UseActiveX = true; // Enable SVG
                        htmlDoc.HtmlOptions.UseVideo = true; // Enable Video
                        htmlDoc.HtmlOptions.Timeout = 120000;
                        // Time out is 2 minutes for ABCPDF .NET Doc calls the url to get the html
                        htmlDoc.HtmlOptions.PageCacheEnabled = true; // Enable ABCPDF .NET page cache                    
                        htmlDoc.Rect.Inset(marginLeft, marginTop); // Insert the margin left and margin top
                        htmlDoc.Page = htmlDoc.AddPage();

                        int theID = htmlDoc.AddImageUrl(url.ToString());
                        // Now chain subsequent pages together. Stop when reach a page which wasn't truncated.
                        while (true)
                        {
                            if (!htmlDoc.Chainable(theID))
                                break;
                            htmlDoc.Page = htmlDoc.AddPage();
                            htmlDoc.Rendering.DotsPerInch = constDotPerInches; // set DPI = 150
                            htmlDoc.Rendering.SaveQuality = constSaveQuality; // set Quality = 100
                            theID = htmlDoc.AddImageToChain(theID);
                        }
                        if (htmlDoc != null)
                            return htmlDoc.PageCount;
                    }
                    break;
            }
            return 0;

        }

        private static string GetMineType(string extension)
        {
            extension = extension.ToUpper(CultureInfo.InvariantCulture);
            string mineType;
            mineTypeLookup.TryGetValue(extension, out mineType);

            return mineType;
        }
    }
}
