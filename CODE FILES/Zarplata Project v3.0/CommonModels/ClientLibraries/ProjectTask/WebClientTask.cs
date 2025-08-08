using AngleSharp;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.CheckTask.Models;
using CommonModels.ProjectTask.ProxyCombiner.TaskType.ParseTask.Models;
using CommonModels.UserAgentClasses;
using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace CommonModels.ClientLibraries.ProjectTask
{
    public abstract class WebClientTask
    {
        protected HttpClient? workHttpConnection { get; private set; } = null;
        protected WebProxy? webProxy { get; private set; } = null;
        protected CheckedProxy? proxy { get; set; } = null;
        protected CookieContainer? cookieContainer { get; private set; } = null;
        protected BrowsingContext? htmlParseContext { get; private set; } = null;
        protected Engine? jsExecuteContextEngine { get; private set; } = null;
        protected TesseractEngine? tesseractExecuteContextEngine { get; private set; } = null;
        protected UserAgent? contextUserAgent { get; private set; } = null;
        protected Dictionary<string, List<string>>? contextHeaders { get; private set; } = null;

        protected bool SetWebContextData(
            HttpClient? httpConn = null, 
            CookieContainer? cookieC = null, 
            WebProxy? webP = null, 
            BrowsingContext? htmlParseC = null,
            Engine? jsExecuteC = null,
            TesseractEngine? tesseractExecuteC = null,
            UserAgent? contextUserA = null, 
            Dictionary<string, 
            List<string>>? contextH = null)
        {
            try
            {
                if (httpConn != null)
                    workHttpConnection = httpConn;
                if (cookieC != null)
                    cookieContainer = cookieC;
                if (webP != null)
                    webProxy = webP;
                if (htmlParseC != null)
                    htmlParseContext = htmlParseC;
                if(jsExecuteC != null)
                    jsExecuteContextEngine = jsExecuteC;
                if (tesseractExecuteC != null)
                    tesseractExecuteContextEngine = tesseractExecuteC;
                if (contextUserA != null)
                    contextUserAgent = contextUserA;
                if (contextH != null)
                    contextHeaders = contextH;
            }
            catch
            {
                return false;
            }

            return true;
        }
        
        protected abstract UserAgent? CreateNewRandomUserAgent();
        protected abstract CookieCollection? CreateNewRandomCookieCollection();
        protected abstract Dictionary<string, List<string>>? CreateNewRandomHeaderCollection(UserAgent userAgent);

        protected void DisposeData()
        {
            try
            {
                if (workHttpConnection != null) workHttpConnection.Dispose();
                if (webProxy != null) webProxy = null;
                if (proxy != null) proxy = null;
                if (cookieContainer != null) cookieContainer = null;

                if (htmlParseContext != null)
                {
                    if (htmlParseContext.Active != null) htmlParseContext.Active.Dispose();
                    if (htmlParseContext.Current != null) htmlParseContext.Current.Dispose();
                    if (htmlParseContext.Parent != null) htmlParseContext.Parent.Dispose();
                }

                if(jsExecuteContextEngine != null)
                {
                    jsExecuteContextEngine = null;
                }

                if (tesseractExecuteContextEngine != null)
                {
                    if (!tesseractExecuteContextEngine.IsDisposed)
                    {
                        tesseractExecuteContextEngine.Dispose();
                    }

                    tesseractExecuteContextEngine = null;
                }

                if (contextUserAgent != null) contextUserAgent = null;
                if (contextHeaders != null) contextHeaders = null;
            }
            catch { }
        }
    }
}
