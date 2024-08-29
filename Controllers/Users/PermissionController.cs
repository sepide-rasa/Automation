using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Automation.Controllers.Users;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class PermissionController : Controller
    {
        //
        // GET: /Premission/


        public ActionResult Index()
        {//بارگذاری صفحه اصلی 
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 63))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public JsonResult checkBox(int id)
        {
            Models.AutomationEntities q = new Models.AutomationEntities();
            var q1 = q.sp_tblPermissionSelect("fldGroupId", id, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            int[] checkedNodes = new int[q1.Count];
            for (int i = 0; i < q1.Count; i++)
            {
                checkedNodes[i] = Convert.ToInt32(q1[i].fldApplicationPartID);
            }

            return Json(checkedNodes);
        }

        public JsonResult GetCascadeGroup()
        {
            Models.AutomationEntities q = new Models.AutomationEntities();
            var w = q.sp_tblUserGroupSelect("", "", 0).Select(c => new { fldID = c.fldID, fldName = c.fldTitle });
            return Json(w, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _Rol(int? id)
        {
            var p = new Models.AutomationEntities();
            if (id != null)
            {
                var rols = (from k in p.sp_tblApplicationPartSelect("fldPID", id.ToString(), 0)
                            select new
                            {
                                id = k.fldID,
                                Name = k.fldTitle,
                                hasChildren = p.sp_tblApplicationPartSelect("fldPID", id.ToString(), 0).Any()

                            });
                return Json(rols, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var rols = (from k in p.sp_tblApplicationPartSelect("", "", 0)
                            select new
                            {
                                id = k.fldID,
                                Name = k.fldTitle,
                                hasChildren = p.sp_tblApplicationPartSelect("", "", 0).Any()

                            });
                return Json(rols, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Save(int GroupId, List<Models.Premissions> checkedNodes)
        {
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 64))
                {
                    Models.AutomationEntities q = new Models.AutomationEntities();
                    q.sp_tblPermissionDelete(GroupId, 1);
                    for (int i = 0; i < checkedNodes.Count(); i++)
                    {
                        q.sp_tblPermissionInsert(checkedNodes[i].fldUserGroupID, checkedNodes[i].fldApplicationPartID, Convert.ToInt32(Session["UserId"]), "");
                    }
                    return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                }
                else
                {
                    Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                    return RedirectToAction("error", "Metro");
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

    }
}
