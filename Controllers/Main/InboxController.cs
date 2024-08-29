using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
namespace Automation.Controllers
{
    [Authorize]
    public class InboxController : Controller
    {
        //
        // GET: /Inbox/

        public ActionResult Index(int id)//id=BoxId
        {
            ViewBag.BoxId = id;
            Session["BoxId"] = id;
            return PartialView();
        }

        public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var t = m.sp_GetDate().FirstOrDefault();
            var time = t.fldDateTime.Date; 
            
            var user = m.sp_tblUserSelect("fldId", Session["UserId"].ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var staff = m.sp_tblStaffSelect("fldId", user.fldStaffID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();

            var q = m.sp_LetterSelectInboxDate("DateDESC", time.AddDays(-(staff.fldLetterLoadNum)), time, (Session["BoxId"]).ToString(),"").ToList().ToDataSourceResult(request);
            Session.Remove("BoxId");
            return Json(q);
        }

        public ActionResult Reload(string Type,int BoxId, string Start, string End)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_LetterSelectInboxDate(Type, MyLib.Shamsi.Shamsi2miladiDateTime(Start), MyLib.Shamsi.Shamsi2miladiDateTime(End), BoxId.ToString(),"").ToList();
            return Json(q, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Details(int id,int ComId)
        {//نمایش اطلاعات جهت رویت کاربر
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                //ویرایش جدول ارجاعات
                Int64 fldIDAssignment = 0;
                Int64 fldLetterIDAssignment = 0;
                string fldAssignmentDateAssignment = "";
                string fldAnswerDateAssignment = "";
                Int64 fldSourceAssIdAssignment = 0;
                string fldDescAssignment = "";
                int fldLetterTypeId = 0;
                string Resivers = "";
                int fldCreator = 0;
                var Assignment = p.sp_tblAssignmentSelect("fldId", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                if (Assignment != null)
                {
                    fldIDAssignment = Assignment.fldID;
                    fldLetterIDAssignment = Assignment.fldLetterID;
                    fldAssignmentDateAssignment = Assignment.fldAssignmentDate;
                    fldAnswerDateAssignment = Assignment.fldAnswerDate;
                    fldSourceAssIdAssignment = Convert.ToInt64(Assignment.fldSourceAssId);
                    fldDescAssignment = Assignment.fldDesc;
                    var q = p.sp_tblLetterSelect("fldId", Assignment.fldLetterID.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                    fldLetterTypeId = q.fldLetterTypeID;
                    fldCreator = q.fldComisionID;
                    var recivers = p.sp_tblInternalAssignmentReceiverSelect("fldAssignmentID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                    foreach (var item in recivers)
                    {
                        var com = p.sp_tblCommisionSelect("fldId", item.fldReceiverComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                        if (com != null)
                            Resivers += com.fldStaffName + "(" + com.fldOrganicRoleName + ")" + "(" + item.fldAssignmentTypeName + ");";
                    }
                    var AssStatus = p.sp_tblInternalAssignmentReceiverSelect("fldAssignmentID", fldIDAssignment.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).Where(h => h.fldReceiverComisionID == ComId).FirstOrDefault();
                    if(AssStatus!=null)
                        if (AssStatus.fldAssignmentStatusID == 1)
                        {
                            p.sp_tblInternalAssignmentReceiverStatusUpdate(AssStatus.fldID, 2, Convert.ToInt32(Session["UserId"]));
                        }
                }
                return Json(new
                {//ویرایش کل نامه
                    //ویرایش جدول ارجاعات
                    fldIDAssignment = fldIDAssignment,
                    fldLetterIDAssignment = fldLetterIDAssignment,
                    fldAssignmentDateAssignment = fldAssignmentDateAssignment,
                    fldAnswerDateAssignment = fldAnswerDateAssignment,
                    fldSourceAssIdAssignment = fldSourceAssIdAssignment,
                    fldDescAssignment = fldDescAssignment,
                    fldLetterTypeId = fldLetterTypeId,
                    fldResivers = Resivers,
                    fldCreator = fldCreator
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return Json(new { data = x.InnerException.Message, state = 1 });
            }
        }
    }
}
