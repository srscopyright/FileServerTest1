// =====COPYRIGHT=====
// github http copyright text
// =====COPYRIGHT=====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Srs.WebPlatform.Common;
namespace Srs.WebPlatform.WebServices.Printer
{
    /// <summary>
    /// Global.asax class
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// Application_Error event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Arguments</param>
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception baseException = Server.GetLastError().GetBaseException();
            // Log the exception
            LoggerHelperV1.Error(Assembly.GetExecutingAssembly().GetName().Name,
                            Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                            Environment.MachineName,
                            baseException.StackTrace,
                            baseException.Message+ baseException.StackTrace);
        }


    }
}
