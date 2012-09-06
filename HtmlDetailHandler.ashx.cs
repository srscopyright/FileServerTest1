using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using Srs.Content.Common;
using Srs.WebPlatform.Common;
using System.Reflection;

namespace Srs.WebPlatform.WebServices.Printer
{

    /// <summary>
    /// Summary description for HtmlDetailHandler
    /// </summary>
    public class HtmlDetailHandler : IHttpHandler
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
                convertUri = context.Request.QueryString.ToString();
                string temp1 = HttpUtility.UrlDecode(convertUri.Substring(4, convertUri.IndexOf(".html") + 1));
                string temp2 = convertUri.Substring(convertUri.IndexOf(".html") + 5);
                convertUri = temp1 + temp2;
                convertUri = HttpUtility.UrlDecode(convertUri);
                string domain = new Uri(convertUri).GetLeftPart(UriPartial.Authority);
                string query = new Uri(convertUri).Query;
                string convertUriDomain = domain;
                if (query.Length > 0)
                    convertUriDomain = convertUri.Replace(
                        new Uri(convertUri).Query == null ? "" : new Uri(convertUri).Query, "");
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
                    string source_url = source.Replace("src='", "").Replace("'", "");
                    string replace = "/HtmlDetailPDFHandler.ashx?URL=" + source_url;
                    res = input.Replace(source_url, replace);

                }
            }
            return res;
        }

        private string RegexReplace(string input, string pattent, string prefix, string domain, string convertUriDomain
                                    )
        {
            Regex regex = new Regex(pattent);
            MatchCollection matchCollection = regex.Matches(input);
            foreach (Match item in matchCollection)
            {

                if (item.Value.Substring(prefix.Length, 7) == "http://" ||
                    item.Value.Substring(prefix.Length, 8) == "https://" ||
                    item.Value.Substring(prefix.Length, 4) == "www." ||
                    item.Value.Substring(prefix.Length, domain.Length) == domain)
                {
                }
                else
                {
                    if (item.Value.IndexOf("../") == 6 || item.Value.IndexOf("../") == 5)
                    {
                        string replace = item.Value.Replace(prefix,
                                                            prefix + convertUriDomain +
                                                            (item.Value.Substring(prefix.Length, 1) == "/" ? "" : "/"));
                        input = input.Replace(item.Value, replace);
                    }
                    else
                    {
                        string replace = item.Value.Replace(prefix,
                                                            prefix + domain +
                                                            (item.Value.Substring(prefix.Length, 1) == "/" ? "" : "/"));
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