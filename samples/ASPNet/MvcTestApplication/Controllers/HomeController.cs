using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;

namespace MvcTestApplication.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Service location just to prove it works :-)
            var container = TinyIoC.TinyIoCContainer.Current;
            var applicationObject = container.Resolve<IApplicationDependency>();
            var requestObject = container.Resolve<IRequestDependency>();
            // Just to the time increments :-)
            Thread.Sleep(2000);
            var requestObject2 = container.Resolve<IRequestDependency>();

            ViewData["Message"] = "Welcome to ASP.NET MVC!";
            ViewData["ApplicationMessage"] = applicationObject.GetContent();
            ViewData["RequestMessage"] = requestObject.GetContent();
            ViewData["RequestMessage2"] = requestObject2.GetContent();
            ViewData["ResultMessage"] = String.Format("Both request dependencies are the same instance: {0}", object.ReferenceEquals(requestObject, requestObject2));

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
