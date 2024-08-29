using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Automation.Controllers.Users;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class OrgUnitController : Controller
    {
        //
        // GET: /OrgUnit/

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 3))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public JsonResult _RolsTree(int? id)
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
            catch (Exception x) {
                return null;
            }
        }

        public ActionResult Move(int Source,int Destination)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var des = p.sp_tblOrganizationUnitSelect("fldid", Destination.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var source=p.sp_tblOrganizationUnitSelect("fldid", Source.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (des.fldPID != Source && Source != Destination)
                {
                    p.sp_tblOrganizationUnitUpdate(source.fldID, source.fldName, des.fldID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                    return Json(new { data = "انتقال با موفقیت انجام شد.", state = 0 });
                }
                return Json(new { data = "انتقال با موفقیت انجام نشد.", state = 1 });
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }


        public ActionResult Save(Models.sp_tblOrganizationUnitSelect OrgUnit,bool isDabirkhane)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (OrgUnit.fldDesc == null)
                    OrgUnit.fldDesc = "";
                if (OrgUnit.fldID == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 4))
                    {
                        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldId", typeof(int));
                        p.sp_tblOrganizationUnitInsert(_id, OrgUnit.fldName, OrgUnit.fldPID, Convert.ToInt32(Session["UserId"]), OrgUnit.fldDesc, Session["UserPass"].ToString());
                        if (isDabirkhane)
                            p.sp_tblSecretariatInsert(Convert.ToInt32(_id.Value), 1, "", "");
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                    }
                    else
                    {
                        Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                        return RedirectToAction("error", "Metro");
                    }
                }
                else
                {//ویرایش رکورد ارسالی
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 5))
                    {
                        p.sp_tblOrganizationUnitUpdate(OrgUnit.fldID, OrgUnit.fldName, OrgUnit.fldPID, Convert.ToInt32(Session["UserId"]), OrgUnit.fldDesc, Session["UserPass"].ToString());
                        if (isDabirkhane)
                        {
                            var dabir = p.sp_tblSecretariatSelect("fldOrgUnitId", OrgUnit.fldID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                            if (dabir == null)
                                p.sp_tblSecretariatInsert(Convert.ToInt32(OrgUnit.fldID), Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                            else
                                p.sp_tblSecretariatUpdate(dabir.fldID, Convert.ToInt32(OrgUnit.fldID), Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        }
                        else
                        {
                            var dabir = p.sp_tblSecretariatSelect("fldOrgUnitId", OrgUnit.fldID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                            if (dabir != null)
                                p.sp_tblSecretariatDelete(dabir.fldID, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                        }
                        return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });
                    }
                    else
                    {
                        Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                        return RedirectToAction("error", "Metro");
                    }
                }
            }
            
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                 if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 6))
                {
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblOrganizationUnitDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                    return Json(new { data = "حذف با موفقیت انجام شد.", state = 0 });
                }
                else
                {
                    return Json(new { data = "رکوردی برای حذف انتخاب نشده است.", state = 1 });
                }
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
        public JsonResult Details(int id)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblOrganizationUnitSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var dabir = p.sp_tblSecretariatSelect("fldOrgUnitId", q.fldID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                bool isDabirkhane = false;
                if (dabir != null)
                    isDabirkhane = true;
                return Json(new
                {
                    fldName = q.fldName,
                    fldId = q.fldID,
                    fldPId = q.fldPID,
                    isDabirkhane = isDabirkhane
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return null;
            }
        }
    }
}
