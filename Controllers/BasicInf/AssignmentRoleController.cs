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
    public class AssignmentRoleController : Controller
    {
        //
        // GET: /AssegnmentRole/

        public ActionResult Index(int idComision)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("logon", "Account");
            if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 35))
            {
                Models.AutomationEntities m = new Models.AutomationEntities();
                var q = m.sp_tblCommisionSelect("fldID", idComision.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                ViewBag.StaffName = q.fldStaffName;
                ViewBag.OrganicRoleName = q.fldOrganicRoleName;
                ViewBag.Comisionid = idComision;
                Session["SenderComId"] = idComision;
                return PartialView();
            }
            else
            {
                Session["ER"] = "شما مجاز به دسترسی نمی باشید.";
                return RedirectToAction("error", "Metro");
            }
        }
        public JsonResult _UnitTree(string id)
        {
            try
            {
                var p = new Models.AutomationEntities();

                var Date = p.sp_GetDate().FirstOrDefault();
                if (id != null )
                {
                    var idd = id.Split('|')[0];
                    var rols = (from k in p.sp_SelectCommisionTree("fldPID", idd.ToString(), 0, Date.fldDateTime)//.Where(h => h.ReceiverId != Convert.ToInt32(Session["SenderComId"]))
                                //دلیل: با این کار اگر شخص انتخاب شده را نشان ندهیم زیرشاخه ها هم نشان داده نمیشوند.
                                select new
                                {
                                    id = k.fldID.ToString() + '|' + k.ReceiverId.ToString(),
                                    //id = k.ReceiverId,
                                    Name = k.KarmandName,
                                    Reciver = k.ReceiverId,
                                    hasChildren = p.sp_SelectCommisionTree("fldPID", idd.ToString(), 0, Date.fldDateTime)//.Where(h => h.ReceiverId != Convert.ToInt32(Session["SenderComId"])).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var rols = (from k in p.sp_SelectCommisionTree("", "", 0, Date.fldDateTime)//.Where(h => h.ReceiverId != Convert.ToInt32(Session["SenderComId"]))
                                select new
                                {
                                    id = k.fldID.ToString() + '|' + k.ReceiverId.ToString(),
                                    //id = k.ReceiverId,
                                    Name = k.KarmandName,
                                    Reciver = k.ReceiverId,
                                    hasChildren = p.sp_SelectCommisionTree("", "", 0, Date.fldDateTime)//.Where(h => h.ReceiverId != Convert.ToInt32(Session["SenderComId"])).Any()

                                });
                    return Json(rols, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception x)
            {
                return null;
            }
        }

        //public ActionResult Reload(string field, string value, int top, int searchtype)
        //{//جستجو
        //    string[] _fiald = new string[] { "fldStaffName" };
        //    string[] searchType = new string[] { "%{0}%", "{0}%", "{0}" };
        //    string searchtext = string.Format(searchType[searchtype], value);
        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblCommisionSelect(_fiald[Convert.ToInt32(field)], searchtext, top, 1, "").ToList();
        //    return Json(q, JsonRequestBehavior.AllowGet);
        //}

      
        //public ActionResult Fill([DataSourceRequest] DataSourceRequest request)
        //{
        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblAssignmentTypeSelect("", "", 30, 1, "").ToList().ToDataSourceResult(request);
        //    return Json(q);
        //} 

        //public JsonResult GetInf(int idComision)
        //{

        //    Models.AutomationEntities m = new Models.AutomationEntities();
        //    var q = m.sp_tblCommisionSelect("fldID", idComision.ToString(), 1, 1, "").FirstOrDefault();
        //    return Json(new
        //    {
                
        //        StaffName = q.fldStaffName + '(' + q.fldOrganicRoleName + ')'
        //    }, JsonRequestBehavior.AllowGet);

        //}


        public JsonResult DefualtcheckBox(int id)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var comission = m.sp_tblCommisionSelect("fldid", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var orgUnit = m.sp_tblOrganicRoleSelect("fldid", comission.fldOrganicRoleID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var orgUnit1 = m.sp_tblOrganicRoleSelect("fldid", orgUnit.fldPID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
            var q = m.sp_tblCommisionSelect("fldOrganicRoleID", orgUnit1.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            ArrayList ar = new ArrayList();
            
            for (int i = 0; i < q.Count; i++)
            {
                ar.Add(Convert.ToInt32(q[i].fldID));
            }

            var q1 = m.sp_tblOrganicRoleSelect("fldPID", orgUnit.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
            int count = q.Count;
            foreach (var item in q1)
            {
                var q2 = m.sp_tblCommisionSelect("fldOrganicRoleID", item.fldID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();
                count += q2.Count;
                for (int i = 0; i < q2.Count; i++)
                {
                    ar.Add(Convert.ToInt32(q2[i].fldID));
                }
            }
            int[] checkedNodes = new int[count];
            for (int i = 0; i < ar.Count; i++)
            {
                checkedNodes[i] = Convert.ToInt32(ar[i]);
            }
            return Json(checkedNodes);
        }

        public JsonResult checkBox(int id)
        {
            Models.AutomationEntities m = new Models.AutomationEntities();
            var q = m.sp_tblAssignmentRoleSelect("fldSenderComisionID", id.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).ToList();

            string[] checkedNodes = new string[q.Count];
            for (int i = 0; i < q.Count; i++)
            {
                var c = m.sp_tblCommisionSelect("fldId", q[i].fldReceiverComisionID.ToString(), 0, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                //checkedNodes[i] = Convert.ToInt32(q[i].fldReceiverComisionID);
                checkedNodes[i] = c.fldOrganicRoleID.ToString() + '|' + q[i].fldReceiverComisionID.ToString();
            } 
            int[] checkedNodesId = new int[q.Count];
            for (int i = 0; i < q.Count; i++)
            {
                checkedNodesId[i] = Convert.ToInt32(q[i].fldID);
            }

           // return Json(checkedNodes);
            return Json(new
            {
                checkedNodes = checkedNodes,
                checkedNodesId = checkedNodesId
            }, JsonRequestBehavior.AllowGet);
        }


        //public JsonResult AssignmentPosition(int id)
        //{
        //    Models.AutomationEntities car = new Models.AutomationEntities();
        //    var nodes = car.sp_tblOrganizationUnitSelect("fldID", id.ToString(), 1, 1, "").FirstOrDefault();

        //    return Json(new { Position = nodes.fldName }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult Save(int fldSenderComisionID, List<Models.AssignmentRol> checkedNodes)
        {
            try
            {
                Models.AutomationEntities p = new Models.AutomationEntities();
                 var Date = p.sp_GetDate().FirstOrDefault();
                p.sp_tblAssinmentRoleDelete(fldSenderComisionID, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 40))
                {
                    for (int i = 0; i < checkedNodes.Count(); i++)
                    {
                        //var k=p.sp_SelectCommisionTree("fldOrganicRoleID", checkedNodes[i].fldReceiverComisionID.ToString(), 0, Date.fldDateTime).FirstOrDefault();
                        //checkedNodes[i].fldReceiverComisionID = k.ReceiverId;
                        int ComId = Convert.ToInt32(checkedNodes[i].fldReceiverComisionID.Split('|')[1]);

                        if (ComId != 0 && ComId != fldSenderComisionID)
                            p.sp_tblAssinmentRoleInsert(fldSenderComisionID, ComId, Convert.ToInt32(Session["UserId"]), "", Session["UserPass"].ToString());
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
        public ActionResult Delete(string id)
        {//حذف یک رکورد
            try
            {
                if (Permossions.haveAccess(Convert.ToInt32(Session["UserId"]), 42))
                {
                    Models.AutomationEntities Car = new Models.AutomationEntities();
                    if (Convert.ToInt32(id) != 0)
                    {
                        Car.sp_tblAssignmentTypeDelete(Convert.ToInt32(id), Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString());
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
                var q = p.sp_tblAssinmentRoleSelect("fldID", id.ToString(), 1, Convert.ToInt32(Session["UserId"]), Session["UserPass"].ToString()).FirstOrDefault();
                return Json(new
                {
                   fldSenderComisionID=q.fldSenderComisionID,
                   fldId = q.fldID,
                   fldDesc = q.fldDesc
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                return null;
            }
        }

    }
}
