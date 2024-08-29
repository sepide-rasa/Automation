using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Automation.Controllers.Users;

namespace Automation.BasicInf.Controllers
{
    [Authorize]
    public class SecretarialOrganizationUnitController : Controller
    {
        //
        // GET: /SecretarialOrganizationUnit/

        public ActionResult Index(int id)
        {//بارگذاری صفحه اصلی 
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 55))
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblSecretariatSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                ViewBag.SecretariantId = id;
                ViewBag.SecretariantName = q.fldName;
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
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblSecretariat_OrganizationUnitSelect("fldSecretariatID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            int[] checkedNodes = new int[q.Count];
            for (int i = 0; i < q.Count; i++)
            {
                checkedNodes[i] = Convert.ToInt32(q[i].fldOrganizationUnitID);
            }

            return Json(checkedNodes);
        }

        public JsonResult _OrganizationUnitTree(int? id)
        {
            try
            {
                var p = new Models.AutomationEntities();

                if (id != null)
                {
                    var rols = (from k in p.sp_tblOrganizationUnitSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    hasChildren = p.sp_tblOrganizationUnitSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rols = (from k in p.sp_tblOrganizationUnitSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    hasChildren = p.sp_tblOrganizationUnitSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return null;
            }
        }



        public ActionResult Save(int SecretariatId, List<Models.sp_tblSecretariat_OrganizationUnitSelect> checkedNodes)
        {
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 56))
                {
                    Models.AutomationEntities p = new Models.AutomationEntities();
                    p.sp_tblSecretariat_OrganizationUnitDelete(SecretariatId, 1, "");
                    for (int i = 0; i < checkedNodes.Count(); i++)
                    {
                        p.sp_tblSecretariat_OrganizationUnitInsert(SecretariatId, checkedNodes[i].fldOrganizationUnitID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
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

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblSecretariat_OrganizationUnitSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }
        
    }
}
