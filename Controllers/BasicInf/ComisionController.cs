using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Automation.Controllers.Users;
using System.Collections;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class ComisionController : Controller
    {
        //
        // GET: /Comision/

        public ActionResult Index(int idStaff,int S)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 19))
            {
                ViewBag.Staffid = idStaff;
                Session["StaffId"] = idStaff;
                ViewBag.Step = S;
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }

        public ActionResult Reload(string field, string value, int top, int searchtype)
        {//جستجو
            string[] _fiald = new string[] { "fldStaffName", "fldStaffID" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblCommisionSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _RolsTree(int? id)
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
        public JsonResult OrgUnitPosition(int id)
        {
            Models.AutomationEntities car = new Models.AutomationEntities();
            var nodes = car.sp_tblOrganicRoleSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

            return Json(new { Position = nodes.fldName }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblCommisionSelect("fldStaffID", Session["StaffId"].ToString(), 30, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().ToDataSourceResult(request);
            Session.Remove("StaffId");
            return Json(q);
        }

        public JsonResult GetInf(int idStaff)
        {

            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblStaffSelect("fldId", idStaff.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            return Json(new
            {
                StaffName = q.fldName + " " + q.fldFamily,
            }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult OrgRolePosition(int id)
        {
            Models.AutomationEntities car = new Models.AutomationEntities();
            var nodes = car.sp_tblOrganicRoleSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

            return Json(new { Position = nodes.fldName }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Save(Models.Comision Comision)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (Comision.fldDesc == null)
                    Comision.fldDesc = "";
                if (Comision.fldID == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 20))
                    {
                        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldid", typeof(int));
                        p.sp_tblCommisionInsert(_id, Comision.fldStaffID, Comision.fldOrganicRoleID, MyLib.Shamsi.Shamsi2miladiDateTime(Comision.fldStartDate),
                            MyLib.Shamsi.Shamsi2miladiDateTime(Comision.fldEndDate), Comision.fldOraganicNumber, Convert.ToInt32(Session["UserId"]), Comision.fldDesc, Session["UserPass"].ToString());

                        var comission = p.sp_tblCommisionSelect("fldid", _id.Value.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        var orgUnit = p.sp_tblOrganicRoleSelect("fldid", comission.fldOrganicRoleID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        var orgUnit1 = p.sp_tblOrganicRoleSelect("fldid", orgUnit.fldPID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        var q = p.sp_tblCommisionSelect("fldOrganicRoleID", orgUnit1.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                        ArrayList ar = new ArrayList();

                        for (int i = 0; i < q.Count; i++)
                        {
                            ar.Add(Convert.ToInt32(q[i].fldID));
                        }

                        var q1 = p.sp_tblOrganicRoleSelect("fldPID", orgUnit.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                        
                        foreach (var item in q1)
                        {
                            var q2 = p.sp_tblCommisionSelect("fldOrganicRoleID", item.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                            
                            for (int i = 0; i < q2.Count; i++)
                            {
                                ar.Add(Convert.ToInt32(q2[i].fldID));
                            }
                        }
                        for (int i = 0; i < ar.Count; i++)
                        {
                            p.sp_tblAssignmentRoleInsert(Convert.ToInt32(_id.Value), Convert.ToInt32(ar[i]), Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
                        }
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
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 21))
                    {
                        p.sp_tblCommisionUpdate(Comision.fldID, Comision.fldStaffID, Comision.fldOrganicRoleID, MyLib.Shamsi.Shamsi2miladiDateTime(Comision.fldStartDate), MyLib.Shamsi.Shamsi2miladiDateTime(Comision.fldEndDate), Comision.fldOraganicNumber, Convert.ToInt32(Session["UserId"]), Comision.fldDesc, Session["UserPass"].ToString());
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
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 22))
                {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        Car.sp_tblCommisionDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
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
                var q = p.sp_tblCommisionSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                return Json(new
                {
                    fldID= q.fldID,
                    fldStaffID = q.fldStaffID,
                    fldOrganicRoleID=q.fldOrganicRoleID,
                    fldStartDate=q.fldStartDate,
                    fldEndDate=q.fldEndDate,
                    fldOraganicNumber=q.fldOraganicNumber,
                    fldDesc=q.fldDesc,
                    fldOrganicRoleName=q.fldOrganicRoleName
                   
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return null;
            }
        }

    }
}
