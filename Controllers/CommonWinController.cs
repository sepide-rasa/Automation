using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers
{
    public class CommonWinController : Controller
    {
        //
        // GET: /CommonWin/

        public ActionResult Index()
        {
            return PartialView();
        }

    }
}
