// =====COPYRIGHT=====
// github http copyright text
// =====COPYRIGHT=====
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Configuration;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Drawing;
using Srs.WebPlatform.Common;
using System.Drawing.Imaging;
using System.Reflection;
namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// Public Default class
    /// </summary>
    public partial class Default : System.Web.UI.Page
    {
        private const string printerUrl = "PrinterUrl";
        private const string _schemaInstanceURI = "http://www.w3.org/2001/XMLSchema-instance";
        private const string _schemaSytemIO = "http://schemas.datacontract.org/2004/07/System.IO";
        private const string _schemaXML = "http://www.w3.org/2001/XMLSchema";
        private const string _schemaBase64 = "base64Binary";
        private const string _arrayofAnnyType = "ArrayOfanyType";
        private const string _dummySessionToken = "8984a4d1-014e-4793-afa6-18ff7b332bd0";
        private const string _errorMessage = "Domain is not in the supported list";
        /// <summary>
        /// Page_Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }
        /// <summary>
        /// Button Export Image action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ButtonExportImage_Click(object sender, EventArgs e)
        {
            try
            {
                literalViewer.Text = string.Empty;
                literalHttpStatusMessage.Text = string.Empty;
                StringBuilder imageText = new StringBuilder();

                string webservicePath = ConfigurationManager.AppSettings[printerUrl];

                Uri requestURL = Request.Url;
                string original = requestURL.OriginalString;

                string pathAndQuery = requestURL.PathAndQuery;
                original = original.Replace(pathAndQuery, string.Empty);

                string url = original + webservicePath;

                string strUrl = textBoxUrl.Text;
                int marginLeft = 0;
                if (!string.IsNullOrEmpty(textBoxMarginLeft.Text))
                {
                    marginLeft = Int32.Parse(textBoxMarginLeft.Text, CultureInfo.InvariantCulture);
                }
                int marginTop = 0;
                if (!string.IsNullOrEmpty(textBoxMarginTop.Text))
                {
                    marginTop = Int32.Parse(textBoxMarginTop.Text, CultureInfo.InvariantCulture);
                }
                string format = dropDownListFormat.SelectedValue;

                
                string requestUrl = string.Format(CultureInfo.InvariantCulture,"{0}/POX/IPrintServiceV1/GetNumberOfPages?sessionTokenId={1}&url={2}&marginLeft={3}&marginTop={4}", url, _dummySessionToken, HttpUtility.UrlEncode(strUrl), marginLeft, marginTop);

                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(new Uri(requestUrl));

                webrequest.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();

                    string xml = new StreamReader(stream).ReadToEnd();

                    if (!string.IsNullOrEmpty(xml))
                    {
                        XDocument xmlDocument = XDocument.Parse(xml);
                        RemoveAttributes(xmlDocument.Root);

                        Type[] extraTypes = new Type[1];
                        extraTypes[0] = typeof (byte[]);

                        string xmlString = xmlDocument.ToString();

                        xmlString = xmlString.Replace("ArrayOfanyType", "ArrayOfAnyType");

                        XmlSerializer xmlSerializer = new XmlSerializer(typeof (int));
                        using (StringReader reader = new StringReader(xmlString))
                        {

                            int numberOfPages = (int) xmlSerializer.Deserialize(reader);
                            if (numberOfPages != 0)
                            {
                                for (int i = 1; i <= numberOfPages; i++)
                                {
                                    imageText.AppendLine("<img src='" +
                                                         string.Format(CultureInfo.InvariantCulture,
                                                             "{0}/POX/IPrintServiceV1/ConvertUrlToImage?sessionTokenId={1}&url={2}&marginLeft={3}&marginTop={4}&format={5}&page={6}",
                                                             url, _dummySessionToken, HttpUtility.UrlEncode(strUrl),
                                                             marginLeft, marginTop, format, i) +
                                                         "'/><button onclick=\"OnPrint('" +
                                                         string.Format(CultureInfo.InvariantCulture,
                                                             "{0}/POX/IPrintServiceV1/ConvertUrlToImage?sessionTokenId={1}&url={2}&marginLeft={3}&marginTop={4}&format={5}&page={6}",
                                                             url, _dummySessionToken, HttpUtility.UrlEncode(strUrl),
                                                             marginLeft, marginTop, format, i) +
                                                         "')\">Print</button><br/>");
                                }
                            }
                            literalHttpStatusMessage.Text = imageText.ToString();
                        }
                    }
                    else
                    {
                        literalHttpStatusMessage.Text = _errorMessage;
                    }
                }
                else
                {
                    literalHttpStatusMessage.Text = response.StatusDescription;
                }
            }
            catch (TimeoutException ex)
            {
                literalHttpStatusMessage.Text = ex.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
            }
            catch (FileNotFoundException fileNotFoundEx)
            {
                literalHttpStatusMessage.Text = fileNotFoundEx.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, fileNotFoundEx.StackTrace, fileNotFoundEx.Message + fileNotFoundEx.StackTrace);
            }
            catch(WebException webEx)
            {
                literalHttpStatusMessage.Text = webEx.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, webEx.StackTrace, webEx.Message + webEx.StackTrace);
                
            }
            catch (HttpException httpException)
            {
                literalHttpStatusMessage.Text = httpException.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, httpException.StackTrace, httpException.Message + httpException.StackTrace);

            }
            catch (UriFormatException uriFormatException)
            {
                literalHttpStatusMessage.Text = uriFormatException.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, uriFormatException.StackTrace, uriFormatException.Message + uriFormatException.StackTrace);

            }
        }
        /// <summary>
        /// Button Export PDF Action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ButtonExportPdf_Click(object sender, EventArgs e)
        {
            try
            {
                literalViewer.Text = string.Empty;
                literalHttpStatusMessage.Text = string.Empty;
                int marginLeft = 0;
                if (!string.IsNullOrEmpty(textBoxMarginLeft.Text))
                {
                    marginLeft = Int32.Parse(textBoxMarginLeft.Text,CultureInfo.InvariantCulture);
                }
                int marginTop = 0;
                if (!string.IsNullOrEmpty(textBoxMarginTop.Text))
                {
                    marginTop = Int32.Parse(textBoxMarginTop.Text, CultureInfo.InvariantCulture);
                }
                string webservicePath = ConfigurationManager.AppSettings[printerUrl];
                Uri requestURL = Request.Url;
                string original = requestURL.OriginalString;

                string pathAndQuery = requestURL.PathAndQuery;
                original = original.Replace(pathAndQuery, string.Empty);

                string webserviceUrl = original + webservicePath;

                string url = string.Format(CultureInfo.InvariantCulture,"{0}/POX/IPrintServiceV1/ConvertUrlToPdf?sessionTokenId={1}&url={2}&marginLeft={3}&marginTop={4}", webserviceUrl, _dummySessionToken, HttpUtility.UrlEncode(textBoxUrl.Text), marginLeft, marginTop);
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
                HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse();
                literalHttpStatusMessage.Text = string.Empty;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    string xml = new StreamReader(stream).ReadToEnd();
                    if (string.IsNullOrEmpty(xml))
                    {
                        literalHttpStatusMessage.Text = _errorMessage;
                    }
                }
                else
                {
                    literalHttpStatusMessage.Text = response.StatusDescription;
                }
                literalViewer.Text = "<embed id='pdfViewer' runat='server' width='100%' height='100%' src=" + url + "></embed>";

            }
            catch (TimeoutException ex)
            {
                literalHttpStatusMessage.Text = ex.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
            }
            catch (FileNotFoundException fileNotFoundEx)
            {
                literalHttpStatusMessage.Text = fileNotFoundEx.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, fileNotFoundEx.StackTrace, fileNotFoundEx.Message + fileNotFoundEx.StackTrace);
            }
            catch (WebException webEx)
            {
                literalHttpStatusMessage.Text = webEx.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, webEx.StackTrace, webEx.Message + webEx.StackTrace);

            }
            catch (HttpException httpException)
            {
                literalHttpStatusMessage.Text = httpException.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, httpException.StackTrace, httpException.Message + httpException.StackTrace);

            }
            catch (UriFormatException uriFormatException)
            {
                literalHttpStatusMessage.Text = uriFormatException.Message;
                LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), Environment.MachineName, uriFormatException.StackTrace, uriFormatException.Message + uriFormatException.StackTrace);

            }
            
        }

        private static void RemoveAttributes(XElement element)
        {
            IEnumerable<XElement> xElements = element.Elements();

            foreach (XAttribute attribute in element.Attributes())
            {
                if (string.Compare(attribute.Value,_schemaInstanceURI,StringComparison.OrdinalIgnoreCase) !=0
                    && string.Compare(attribute.Value, _schemaXML, StringComparison.OrdinalIgnoreCase) != 0
                    && string.Compare(attribute.Name.NamespaceName, _schemaXML, StringComparison.OrdinalIgnoreCase) != 0
                    && !attribute.Value.Contains(_schemaBase64))
                {
                    attribute.Remove();
                }
            }
            element.Name = string.Empty + element.Name.LocalName;

            foreach (XElement anotherElement in xElements)
            {
                RemoveAttributes(anotherElement);
            }
        }

        private static ImageFormat GetImageFormat(string format)
        {
            ImageFormat imageFormat;
            switch (format)
            {
                case "png":
                    imageFormat = ImageFormat.Png;
                    break;
                case "gif":
                    imageFormat = ImageFormat.Gif;
                    break;
                case "bmp":
                    imageFormat = ImageFormat.Bmp;
                    break;
                case "jpg":
                    imageFormat = ImageFormat.Jpeg;
                    break;
                default:
                    imageFormat = ImageFormat.Bmp;
                    break;
            }
            return imageFormat;
        }
    }
}
