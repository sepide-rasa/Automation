using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.IO;

namespace Automation.Controllers.BasicInf
{
    [Authorize]
    public class LetterAttachmentController : Controller
    {
        //
        // GET: /LetterAttachment/

        public ActionResult Index()
        {//بارگذاری صفحه اصلی 
            return PartialView();
        }
        public ActionResult Upload()
        {
            var file = Request.Files["Filedata"];
            string savePath = Server.MapPath(@"~\Uploaded\" + file.FileName);
            file.SaveAs(savePath);
            Session["savePath"] = savePath;
            return Content(Url.Content(@"~\Uploaded\" + file.FileName));
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities q = new Models.AutomationEntities();
            var m = q.sp_tblLetterAttachmentSelect("fldLetterID_Attach", "", 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList().ToDataSourceResult(request);
            return Json(m);

        }
        //public ActionResult Reload(string field, string value, int top, int searchtype)
        //{//جستجو
        //    string[] _fiald = new string[] { "fldName" };
        //    string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
        //    string searchtext = string.Format(searchType[searchtype], value);
        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblLetterAttachmentSelect(_fiald[Convert.ToInt32(field)], searchtext, top).ToList();
        //    return Json(q, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {

                Models.AutomationEntities Car = new Models.AutomationEntities();
                if (Convert.ToInt32(id) != 0)
                {
                    Car.sp_tblLetterAttachmentDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                    return Json(new { data = "حذف با موفقیت انجام شد.", state = 0 });
                }
                else
                {
                    return Json(new { data = "رکوردی برای حذف انتخاب نشده است.", state = 1 });
                }

            }
            catch (Exception x)
            {

                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        public ActionResult Save(Models.LetterAssign LetterAttachment)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                if (LetterAttachment.fldDesc == null)
                    LetterAttachment.fldDesc = "";
                if (LetterAttachment.fldID == 0)
                {//ثبت رکورد جدید                    
                    byte[] _file = null;
                    if (Session["savePath"] != null)
                    {
                        MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(Session["savePath"].ToString()));                        
                        string filename =Path.GetFileName(Session["savePath"].ToString());
                        System.IO.File.Delete(Session["savePath"].ToString());
                        _file = stream.ToArray();
                        System.Data.Objects.ObjectParameter _id = new System.Data.Objects.ObjectParameter("fldID", typeof(int));
                        p.sp_tblContentFileInsert(_id, filename, _file, null, null, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString(), "");
                        p.sp_tblLetterAttachmentInsert(1, "", Convert.ToInt64(_id.Value), Convert.ToInt32(Session["UserId"]), LetterAttachment.fldDesc, Session["UserPass"].ToString());
                        Session.Remove("savePath");
                        return Json(new { data = "ذخیره با موفقیت انجام شد.", state = 0 });
                    }
                    else
                        return Json(new { data = "لطفا فایل را وارد کنید.", state = 1 });

                }
                else
                {//ویرایش رکورد ارسالی
                    byte[] report_file = null;
                    if (Session["savePath"] != null)
                    {
                        //var q = p.sp_tblReportsSelect("fldOlgoLetterId", LetterAttachment.fldID.ToString(), 1).FirstOrDefault();
                        //MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(Session["savePath"].ToString()));
                        //System.IO.File.Delete(Session["savePath"].ToString());
                        //Session.Remove("savePath");
                        //report_file = stream.ToArray();
                        //p.sp_tblOlgoGharardadUpdate(LetterAttachment.fldID, LetterAttachment.fldName, 1, LetterAttachment.fldDesc);
                        //p.sp_tblReportsUpdate(q.fldId, report_file, null, LetterAttachment.fldID, 1, LetterAttachment.fldDesc);
                        return Json(new { data = "ویرایش با موفقیت انجام شد.", state = 0 });
                    }
                    else
                        return Json(new { data = "لطفا فایل گزارش را وارد کنید.", state = 1 });
                }
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }

        ////public JsonResult Details(int id)
        ////{//نمایش اطلاعات جهت رویت کاربر
        //    try
        //    {
        //        Models.AutomationEntities Car = new Models.AutomationEntities();
        //        var q = Car.sp_tblOlgoGharardadSelect("fldID", id.ToString(), 1).FirstOrDefault();
        //        return Json(new
        //        {
        //            fldId = q.fldID,
        //            fldName = q.fldName,
        //            fldDesc = q.fldDesc
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception x)
        //    {
        //        return Json(new { data = x.InnerException.Message, state = 1 });
        //    }
        //}

    }
}
