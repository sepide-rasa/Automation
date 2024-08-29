using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers.Letter
{
    [Authorize]
    public class CheckReciverAndSenderController : Controller
    {
        //
        // GET: /CheckReciverAndSender/

        public ActionResult Index(int id, Models.Recive_send Re_Se)
        {
            ViewBag.State = id;
            ViewBag.Reciver_sender = Re_Se.Reciver_sender;
            ViewBag.Reciver_senderId = Re_Se.Reciver_senderId;
            Session["State"] = id;
            return PartialView();
        }

        public ActionResult ReloadGride(Models.Recive_send Re_Se)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            List<Models.InternalAssignment> groups = new List<Models.InternalAssignment>();
            var K = Re_Se.Reciver_sender.Split(';');
            var S = Re_Se.Reciver_senderId.Split(';');
            for (byte i = 0; i < K.Length - 1; i++)
            {
                Models.InternalAssignment V = new Models.InternalAssignment();
                if (Convert.ToInt32(Session["State"]) == 1 || Convert.ToInt32(Session["State"]) == 3 || 
                    Convert.ToInt32(Session["State"]) == 4 || Convert.ToInt32(Session["State"]) == 7)
                {
                    var H = S[i].Split('|');
                    V.fldID = Convert.ToInt32(H[0]);
                    V.AssignmentId = Convert.ToInt32(H[1]);
                }
                else if (Convert.ToInt32(Session["State"]) == 2)
                {
                    V.fldID = Convert.ToInt32(S[i]);
                }
                V.fldDesc = K[i];
                groups.Add(V);
            }
            Session.Remove("State");
            return Json(groups, JsonRequestBehavior.AllowGet);
        }
    }
}
