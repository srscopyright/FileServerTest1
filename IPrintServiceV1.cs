// =====COPYRIGHT=====
// github http copyright text
// =====COPYRIGHT=====
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Collections;
using Srs.WebPlatform.Common;

namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// WCF interface for the printer
    /// </summary>
    [ServiceContract(Namespace = "Srs.WebPlatform.WebServices.Printer")]
    public interface IPrintServiceV1
    {
        /// <summary>
        /// Returns a PDF stream that rendered from an URL
        /// The url need is a html page (.aspx, .html, .htm, .asp, .jsp...)
        /// </summary>
        /// <param name="sessionTokenId">SessionTokenId given by user to authorize</param>
        /// <param name="url">The URL of a page that will be rendered</param>
        /// <param name="marginLeft">Value in integer that indicates the margin in the left of pdf file</param>
        /// <param name="marginTop">Value in integer that indicates the margin in the top of pdf file</param>
        /// <returns>Returns a PDF byte[]</returns>
        [OperationContract, WebGet, WebServiceMethodAttributeV1("e03453ef-e965-48ca-bc25-7494732ab129")]
        Stream ConvertUrlToPdf(string sessionTokenId, string url, int marginLeft, int marginTop);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionTokenId"></param>
        /// <param name="url"></param>
        /// <param name="marginLeft"></param>
        /// <param name="marginTop"></param>
        /// <returns></returns>
        [OperationContract, WebGet, WebServiceMethodAttributeV1("e20f2190-66b6-42a8-a707-a50406416be0")]
        int GetNumberOfPages(string sessionTokenId, string url, int marginLeft, int marginTop);
        
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
        /// <param name="page">The specific page that return to user. The total number of pages can be get by call method GetNumberOfPages</param>
        /// <returns>Return an image stream</returns>
        [OperationContract, WebGet, WebServiceMethodAttributeV1("12200f23-3475-4193-9cda-e0ec7e1c3b5e")]
        Stream ConvertUrlToImage(string sessionTokenId, string url, int marginLeft, int marginTop, string format, int page);

    }
}
