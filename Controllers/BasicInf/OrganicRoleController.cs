using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Collections;
using Automation.Controllers.Users;
namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class OrganicRoleController : Controller
    {
        //
        // GET: /OrganicRole/

        public ActionResult Index()
        {//بارگذاری صفحه اصلی 
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 7))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public JsonResult _UnitTree(int? id)
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
        public ActionResult Move(int Source, int Destination)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var des = p.sp_tblOrganicRoleSelect("fldid", Destination.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var source = p.sp_tblOrganicRoleSelect("fldid", Source.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (des.fldPID != Source&&Source!=Destination)
                {
                    p.sp_tblOrganicRoleUpdate(source.fldID, source.fldName, source.fldOrganizationUnitID, des.fldID, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                    return Json(new { data = "انتقال با موفقیت انجام شد.", state = 0 });
                }
                return Json(new { data = "انتقال با موفقیت انجام نشد.", state = 1 });
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
        public JsonResult _RoleTree(int? id)
        {
            try
            {
                var p = new Models.AutomationEntities();

                if (id != null)
                {
                    var rols = (from k in p.sp_tblOrganicRoleSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    hasChildren = p.sp_tblOrganicRoleSelect("fldPID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rols = (from k in p.sp_tblOrganicRoleSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString())
                                select new
                                {
                                    id = k.fldID,
                                    Name = k.fldName,
                                    hasChildren = p.sp_tblOrganicRoleSelect("", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return null;
            }
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblAssignmentTypeSelect("", "", 30, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().ToDataSourceResult(request);
            return Json(q);
        }

        public JsonResult OrgUnitPosition(int id)
        {
            Models.AutomationEntities car = new Models.AutomationEntities();
            var nodes = car.sp_tblOrganizationUnitSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();            

            return Json(new { Position = nodes.fldName },JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(Models.sp_tblOrganicRoleSelect OrganicRole)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (OrganicRole.fldDesc == null)
                    OrganicRole.fldDesc = "";
                if (OrganicRole.fldID == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 8))
                    {

                        p.sp_tblOrganicRoleInsert(OrganicRole.fldName, OrganicRole.fldOrganizationUnitID, OrganicRole.fldPID, Convert.ToInt32(Session["UserId"]), OrganicRole.fldDesc, Session["UserPass"].ToString());

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
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 6))
                    {

                        p.sp_tblOrganicRoleUpdate(OrganicRole.fldID, OrganicRole.fldName, OrganicRole.fldOrganizationUnitID, OrganicRole.fldPID, Convert.ToInt32(Session["UserId"]), OrganicRole.fldDesc, Session["UserPass"].ToString());
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

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldType" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblAssignmentTypeSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
           
            try
            { 
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 10))
                {
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblOrganicRoleDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
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
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                var q = p.sp_tblOrganicRoleSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                return Json(new
                {
                    fldID = q.fldID,
                    fldName = q.fldName,
                    fldPID=q.fldPID,
                    fldDesc = q.fldDesc,
                    O_UnitId=q.fldOrganizationUnitID
                    
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

    }
}
