using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Automation.Controllers.Users;

namespace Automation.Controllers.Tools
{
    public class DashburdController : Controller
    {
        //
        // GET: /Dashburd/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 99))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public ActionResult Reload(string Start, string End)
        {//جستجو            
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_ManagerVaziatYekMahe(MyLib.Shamsi.Shamsi2miladiDateTime(Start), MyLib.Shamsi.Shamsi2miladiDateTime(End)).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
    }
}
