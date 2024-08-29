using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers.Tools
{
    [Authorize]
    public class RequestEmailController : Controller
    {
        // GET: /RequestEmail/

        public ActionResult Index()
        {
            return View();
        }

    }
}
