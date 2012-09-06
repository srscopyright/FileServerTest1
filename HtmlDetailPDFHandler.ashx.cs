using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Reflection;
using Srs.WebPlatform.Common;

namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// Summary description for HtmlDetailPDFHandler
    /// </summary>
    public class HtmlDetailPDFHandler : IHttpHandler
    {
        private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {


                string convertUri = "";
                convertUri = (context.Request.QueryString.ToString()).Replace("URL=", "");
                string temp = HttpUtility.UrlDecode(convertUri.Substring(0, convertUri.IndexOf("POX%2fGet") + 9));
                convertUri = temp + convertUri.Substring(convertUri.IndexOf("POX%2fGet") + 9);
                string domain = new Uri(convertUri).GetLeftPart(UriPartial.Authority);
                domain = "http://www.identifix.com";
                string query = new Uri(convertUri).Query;
                string convertUriDomain = domain;
                if (query.Length > 0)
                    convertUriDomain = convertUri.Replace(new Uri(convertUri).Query == null ? "" : new Uri(convertUri).Query, "");
                else
                {
                    convertUriDomain = convertUri;
                }
                if (convertUriDomain.Substring(convertUriDomain.Length - 1) == "/")
                {
                    convertUriDomain = convertUriDomain.Substring(0, convertUriDomain.Length - 1);
                }

                string respone = GetHtmlText(convertUri);

                respone = RegexReplace(respone, "href=\"(.)+([.]css)(.)+(^<)?(/>)", "href=\"", domain, domain);

                respone = RegexReplace(respone, "href=\"(.)+<[^.css]+?>", "href=\"", domain, convertUriDomain);

                respone = RegexReplace(respone, "href='(.)+<[^.css]+?>'", "href='", domain, convertUriDomain);

                respone = RegexReplace(respone, "src=\"(.)+\"", "src=\"", domain, convertUriDomain);

                respone = RegexReplace(respone, "src='(.)+'", "src='", domain, convertUriDomain);

                respone = RegexReplace(respone, "url(.)+\"", "url(", domain, convertUriDomain);

                respone = ReplaceIframe(respone);

                context.Response.ContentType = "text/HTML";
                context.Response.Write(respone);
            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
            }
        }
        private string GetHtmlText(string Url)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(Url);
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();
            return result;

        }
 
        private string ReplaceIframe(string input)
        {
            string res = input;

            Regex regex = new Regex("<[iI][fF][rR][aA][mM][eE](.)+</[iI][fF][rR][aA][mM][eE]>");
            MatchCollection matchCollection = regex.Matches(input);
            foreach (Match match in matchCollection)
            {
                if (match.Value.IndexOf("Srs.WebPlatform.WebServices.FileManager.svc") != -1)
                {
                    string temp = "";
                    if (match.Value.IndexOf("src='") > 0)
                    {
                        int i = match.Value.IndexOf("src='");
                        temp = match.Value.Substring(i);
                        int j = 0;
                        int count = 0;
                        while (true)
                        {
                            j = temp.IndexOf("'", j);
                            j++;
                            count++;
                            if (j != -1 && count == 2)
                            {
                                temp = temp.Substring(0, j);
                                break;
                            }
                            else if (j == -1)
                            {
                                break;
                            }

                        }

                    }
                    else if (match.Value.IndexOf("src=\"") > 0)
                    {
                        int i = match.Value.IndexOf("src=\"");
                        temp = match.Value.Substring(i);
                        int j = 0;
                        int count = 0;
                        while (true)
                        {
                            j = temp.IndexOf("\"", j);
                            j++;
                            count++;
                            if (j != -1 && count == 2)
                            {
                                temp = temp.Substring(0, j);
                                break;
                            }
                            else if (j == -1)
                            {
                                break;
                            }

                        }
                    }

                    string source = temp;
                    string source_url = source.Replace("src=\"", "").Replace("\"", "");

                    source_url = source_url.Replace("&amp;", "&");

                    int start = source_url.IndexOf("sessionTokenId=");
                    string tokenid = source_url.Substring(start);
                    int end = 0;
                    int counthit = 0;
                    while (true)
                    {
                        end = tokenid.IndexOf("&", end);
                        end++;
                        counthit++;
                        if (end != -1 && counthit == 1)
                        {
                            tokenid = tokenid.Substring(15, end - 16);
                            break;
                        }
                        else if (end == -1)
                        {
                            break;
                        }

                    }
                    string sessionTokenId = tokenid;
                    string attribute =
                        "http://printer.qatestinglogger.mpifix.com/WebServices/Srs.WebPlatform.WebServices.Printer.svc/POX/IPrintServiceV1/ConvertUrlToImage?sessionTokenId=" + sessionTokenId + "&url=" +
                        HttpUtility.UrlEncode(source_url) +
                        HttpUtility.UrlEncode("&marginLeft=0&marginTop=0&format=png&page=1") +
                        "&marginLeft=0&marginTop=0&format=png&page=1";
                    source_url = source_url.Replace("&", "&amp;");
                    res = input.Replace(source_url, attribute);

                }
            }
            return res;
        }
        private string RegexReplace(string input, string pattent, string prefix, string domain, string convertUriDomain)
        {
            Regex regex = new Regex(pattent);
            MatchCollection matchCollection = regex.Matches(input);
            foreach (Match item in matchCollection)
            {

                if (item.Value.Substring(prefix.Length, 7) == "http://" || item.Value.Substring(prefix.Length, 8) == "https://" || item.Value.Substring(prefix.Length, 4) == "www." || item.Value.Substring(prefix.Length, domain.Length) == domain)
                {
                }
                else
                {
                    if (item.Value.IndexOf("../") == 6 || item.Value.IndexOf("../") == 5)
                    {
                        string replace = item.Value.Replace(prefix, prefix + convertUriDomain + (item.Value.Substring(prefix.Length, 1) == "/" ? "" : "/"));
                        input = input.Replace(item.Value, replace);
                    }
                    else
                    {
                        string replace = item.Value.Replace(prefix, prefix + domain + (item.Value.Substring(prefix.Length, 1) == "/" ? "" : "/"));
                        input = input.Replace(item.Value, replace);
                    }

                }
            }
            return input;
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsReusable
        {
            get { throw new NotImplementedException(); }
        }
    }
}