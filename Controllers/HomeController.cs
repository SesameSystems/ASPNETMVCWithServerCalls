using ASPNETMVCWithServerCalls.Codes;
using Forge.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASPNETMVCWithServerCalls.Controllers
{
    public class HomeController : Controller
    {

        private static ILog LOGGER = LogManager.GetLogger(typeof(HomeController));

        public ActionResult Index()
        {
            ComProxy proxy = (ComProxy)Session[Consts.SESSION_COMPROXY];
            // query database(s)
            DatabaseResponse dbResponse = proxy.GetDatabases();
            foreach (KeyAndValueItem item in dbResponse.Items)
            {
                LOGGER.Info($"DbId: {item.Id}, Name: {item.Name}");
                // query each db details
                SPDatabaseDetailsResponse dbDetails = proxy.GetDatabaseDetails(new SPDatabaseDetailsRequest() { DatabaseId = item.Id });
                LOGGER.Info($"Currency: {dbDetails.Currency}");
                LOGGER.Info($"Population: {dbDetails.Population.ToString()}");
                LOGGER.Info($"Sample: {dbDetails.Sample.ToString()}");
                LOGGER.Info("Languages:");
                foreach (KeyAndValueItem langItem in dbDetails.Languages)
                {
                    LOGGER.Info($"{langItem.Id}, name: {langItem.Name}");
                }
                LOGGER.Info("-----------");
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}