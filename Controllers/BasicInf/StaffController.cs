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
    public class StaffController : Controller
    {
        //
        // GET: /Staff/


        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 15))
            {
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblStaffSelect("", "", 30, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().ToDataSourceResult(request);
            return Json(q);
        }

        public ActionResult Save(Models.Staff Staff)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();

                byte[] image1 = null, image2 = null;

                if (Staff.fldStaffPicture != null)
                    image1 = Automation.Helper.ClsCommon.Base64ToImage(Staff.fldStaffPicture);

                if (Staff.fldSignPicture != null)
                    image2 = Automation.Helper.ClsCommon.Base64ToImage(Staff.fldSignPicture);

                if (Staff.fldDesc == null)
                    Staff.fldDesc = "";
                if (Staff.fldEmailAddress == null)
                    Staff.fldEmailAddress = "";
                if (Staff.fldMobile == null)
                    Staff.fldMobile = "";
                if (Staff.fldAddress == null)
                    Staff.fldAddress = "";
                if (Staff.fldId == 0)
                {//ثبت رکورد جدید
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 16))
                    {

                        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));

                        p.sp_tblStaffInsert(_id, Staff.fldName, Staff.fldFamily, Staff.fldMelliCode, Staff.fldNameFather, MyLib.Shamsi.Shamsi2miladiDateTime(Staff.fldBirthDate), Staff.fldEmailAddress, Staff.fldMobile, Staff.fldAddress, Staff.fldSign,true,7 ,Convert.ToInt32(Session["UserId"]), Staff.fldDesc,Session["UserPass"].ToString());
                        p.sp_tblPictureInsert(Convert.ToInt32(_id.Value), image1, image2, Convert.ToInt32(Session["UserId"]), Staff.fldDesc, Session["UserPass"].ToString());
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
                    if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 17))
                    {
                        p.sp_tblStaffUpdate(Staff.fldId, Staff.fldName, Staff.fldFamily, Staff.fldMelliCode, Staff.fldNameFather, MyLib.Shamsi.Shamsi2miladiDateTime(Staff.fldBirthDate), Staff.fldEmailAddress, Staff.fldMobile, Staff.fldAddress, Staff.fldSign, Staff.fldNotify,Staff.fldLetterLoadNum, Convert.ToInt32(Session["UserId"]), Staff.fldDesc, Session["UserPass"].ToString());
                        var k = p.sp_tblPictureSelect("fldStaffID", Staff.fldId.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        p.sp_tblPictureUpdate(k.fldID, Staff.fldId, image1, image2, Convert.ToInt32(Session["UserId"]), Staff.fldDesc, Session["UserPass"].ToString());
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
            string[] _fiald = new string[] { "fldFamily", "fldName", "fldMeliCode" };
            string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
            string searchtext = string.Format(searchType[searchtype], value);
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblStaffSelect(_fiald[Convert.ToInt32(field)], searchtext, top, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 18))
                {
                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    var k = Car.sp_tblPictureSelect("fldStaffID", id, 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    Car.sp_tblPictureDelete(k.fldID, 1, "");
                    Car.sp_tblStaffDelete(Convert.ToInt32(id), 1, "");
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
                var q = p.sp_tblStaffSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var k = p.sp_tblPictureSelect("fldStaffID", q.fldID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                var picId = "";
                if (k != null)
                    picId = k.fldID.ToString();
                return Json(new
                {
                    fldID = q.fldID,
                    fldName=q.fldName,
                    fldFamily=q.fldFamily,
                    fldNameFather = q.fldNameFather,
                    fldMeliCode=q.fldMelliCode,
                    fldBirthDate = q.fldBirthDate,
                    fldEmailAddress = q.fldEmailAddress,
                    fldMobile = q.fldMobile,
                    fldAddress = q.fldAddress,
                    fldSign = q.fldSign,
                    fldDesc = q.fldDesc,
                    fldNotify=q.fldNotify,
                    fldLetterLoadNum=q.fldLetterLoadNum,
                    fldpicId = picId                   
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public FileContentResult SignImage(int id)
        {//برگرداندن عکس 
            Models.AutomationEntities p = new Models.AutomationEntities();

            var pic = p.sp_tblPictureSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            if (pic != null)
            {
                if (pic.fldSignPicture != null)
                {
                    return File((byte[])pic.fldSignPicture, "jpg");
                }

            }
            return null;

        }
        public FileContentResult StaffImage(int id)
        {//برگرداندن عکس 
            Models.AutomationEntities p = new Models.AutomationEntities();

            var pic = p.sp_tblPictureSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            if (pic != null)
            {
                if (pic.fldStaffPicture != null)
                {
                    return File((byte[])pic.fldStaffPicture, "jpg");
                }

            }
            return null;

        }

    }
}
