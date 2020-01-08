using ASPNETMVCWithServerCalls.Codes;
using Forge.Logging;
using Forge.Logging.Log4net;
using Sesame.Communication.Data.Indentification;
using Sesame.Communication.External.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;

namespace ASPNETMVCWithServerCalls
{
    public class MvcApplication : System.Web.HttpApplication
    {

        private static ILog LOGGER = null;

        private readonly AutoResetEvent mFaultHandlerEvent = new AutoResetEvent(false);
        private Thread mFaultHandlerThread = null;
        private AutoResetEvent mFaultHandlerStopEvent = null;
        private bool mFaultHandlerRunning = true;

        static MvcApplication()
        {
            Log4NetManager.InitializeFromAppConfig();
            LOGGER = LogManager.GetLogger(typeof(MvcApplication));
            if (LOGGER.IsDebugEnabled) LOGGER.Debug(string.Format("GLOBAL_ASAX, Process private memory consumption: {0} byte(s)", Process.GetCurrentProcess().PrivateMemorySize64.ToString("### ### ### ###")));
            LogUtils.LogAll();
            if (LOGGER.IsDebugEnabled) LOGGER.Debug(string.Format("GLOBAL_ASAX, Process private memory consumption: {0} byte(s)", Process.GetCurrentProcess().PrivateMemorySize64.ToString("### ### ### ###")));
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ClientProxyBase.SourceId = ClientIdGenerator.GenerateId(ClientTypeEnum.External);
            ClientProxyBase.ConfigureClientProxyForCallback(new ConfigurationForCallback("TcpEndpointForCallbackMode"));
            ClientProxyBase.Faulted += ClientProxyBase_Faulted;
            try
            {
                ClientProxyBase.Open();
            }
            catch (Exception ex)
            {
                LOGGER.Error(string.Format("Failed to open connection. Reason: {0}", ex.Message));
                mFaultHandlerEvent.Set();
            }
            mFaultHandlerThread = new Thread(new ThreadStart(FaultHandlerThreadMain));
            mFaultHandlerThread.Name = "FaultHandlerThread";
            mFaultHandlerThread.Start();
        }

        protected void Application_End()
        {
            ClientProxyBase.Faulted -= ClientProxyBase_Faulted;

            mFaultHandlerStopEvent = new AutoResetEvent(false);
            mFaultHandlerRunning = false;
            mFaultHandlerEvent.Set();
            mFaultHandlerStopEvent.WaitOne();
            mFaultHandlerStopEvent.Dispose();
            mFaultHandlerEvent.Dispose();

            ClientProxyBase.Close();

            if (LOGGER.IsInfoEnabled) LOGGER.Info("GLOBAL_ASAX, application stop.");
        }

        private void ClientProxyBase_Faulted(object sender, EventArgs e)
        {
            mFaultHandlerEvent.Set();
        }

        private void FaultHandlerThreadMain()
        {
            while (mFaultHandlerRunning)
            {
                mFaultHandlerEvent.WaitOne();
                if (mFaultHandlerRunning)
                {
                    try
                    {
                        ClientProxyBase.Open();
                    }
                    catch (Exception ex)
                    {
                        LOGGER.Error(string.Format("Failed to open connection. Reason: {0}", ex.Message));
                        Thread.Sleep(1000);
                    }
                }
            }
            mFaultHandlerStopEvent.Set();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (LOGGER.IsInfoEnabled) LOGGER.Info(string.Format("GLOBAL_ASAX, starting new user session. SessionId: {0}", Session.SessionID));
            // initialize session to keep the same id and session object
            ComProxy proxy = null;
            HttpSessionState session = GetSession();
            session[Consts.SESSION_COMPROXY] = proxy = new ComProxy();
        }

        public HttpSessionState GetSession()
        {
            // Check if current context exists
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Session;
            }
            else
            {
                return this.Session;
            }
        }

    }
}
