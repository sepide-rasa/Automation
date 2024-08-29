using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Automation.Controllers
{
    public class ShowCharkhe_LetterContentController : Controller
    {
        //
        // GET: /ShowCharkhe_LetterContent/

        public ActionResult Index(int LetterId)
        {//بارگذاری صفحه اصلی 
            ViewBag.LetterId = LetterId;

            return PartialView();

        }

        public JsonResult _RolsTree(int? id, int LetterId)
        {
            try
            {
                var p = new Models.AutomationEntities();
                if (id != null)
                {
                    var rols = (from k in p.sp_Charkhe(LetterId, id)
                                select new
                                {
                                    id = k.id,
                                    Name = k.SenderName,
                                    AssId = k.AssId,
                                    Sender=k.sender,
                                    pid=k.pid,
                                    hasChildren = p.sp_Charkhe(LetterId, id).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rols = (from k in p.sp_Charkhe(LetterId, null)
                                select new
                                {
                                    id = k.id,
                                    Name = k.SenderName,
                                    AssId = k.AssId,
                                    Sender = k.sender,
                                    pid = k.pid,
                                    hasChildren = p.sp_Charkhe(LetterId, null).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return null;
            }
        }
        public ActionResult Reload(int field, int Sender, int LetterID)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            if (field == 1)
            {
                var q = m.sp_tblLetter_LogSelect("", LetterID, Sender).ToList();
                return Json(q, JsonRequestBehavior.AllowGet);
            }
            else if (field == 2)
            {
                var q = m.sp_tblContentFile_LogSelect("", LetterID, Sender).ToList();
                return Json(q, JsonRequestBehavior.AllowGet);
            }
            else if (field == 3)
            {
                var q = m.sp_tblContentFileAnnex_LogSelect(LetterID, Sender).ToList();
                return Json(q, JsonRequestBehavior.AllowGet);
            }
            else if (field == 4)
            {
                var q = m.sp_tblHistoryLetter_logSelect(LetterID, Sender).ToList();
                return Json(q, JsonRequestBehavior.AllowGet);
            }
            else if (field == 5)
            {
                var q = m.sp_tblLetterFollow_LogSelect(LetterID, Sender).ToList();
                return Json(q, JsonRequestBehavior.AllowGet);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
    }
}
