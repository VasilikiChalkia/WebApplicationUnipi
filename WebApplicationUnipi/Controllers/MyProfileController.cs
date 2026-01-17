using DevExpress.Web.Mvc;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplicationUnipi.Models.Enums;
using WebApplicationUnipi.Models.MyApplicationUnipi;
using WebApplicationUnipi.Services;
using WebApplicationUnipi.Services.Factories;

namespace WebApplicationUnipi.Controllers
{
    public class MyProfileController : Controller
    {
        public static Logger logger = LogManager.GetLogger("logger");
        private static MyApplicationUnipiEntities context = new MyApplicationUnipiEntities();

       
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["Username"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "Index" }));
                return;
            }
            base.OnActionExecuting(filterContext);
        }
        //#endif

        // GET: MyProfile
        public ActionResult Index()
        {
            var usrId = (int)Session["UserId"];
            using (var db = new MyApplicationUnipiEntities())
            {
                var emplModel = db.HR_Employee.SingleOrDefault(s => s.Id == usrId);



              

                var model = new Tuple<HR_Employee, string, string>(emplModel, emplModel.Personal_Email, emplModel.Personal_Mobile_Number);

                return View("Index", model);
            }
        }

        [HttpPost]
        public ActionResult SaveEmployeePersonalData(HR_Employee empl)
        {
           
            var usrId = (int)Session["UserId"];
         
            if (empl != null && usrId > 0 && ModelState.IsValid)
            {
                using (var db = new MyApplicationUnipiEntities())
                {
                   
                    var usrModel = db.HR_Employee.SingleOrDefault(s => s.Id == usrId);

                    //usrModel = empl;
                    //usrModel.Employee_Fname = empl.Employee_Fname;
                    //usrModel.Employee_Lname = empl.Employee_Lname;
                    usrModel.Personal_Email = empl.Personal_Email;
                    usrModel.Home_Address = empl.Home_Address;
                    usrModel.Personal_Mobile_Number = empl.Personal_Mobile_Number;
                    usrModel.Birth_Date = empl.Birth_Date;
                    usrModel.Transportation = empl.Transportation;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

      


       
       
        #region Annual Leave part

        public ActionResult MyAnnualLeave()
        {
            var userId = (int)Session["userId"];
            var availableDays = GetAvailableDays(userId, (byte)LeaveTypes.Annual);
            ViewData["AvailableDays"] = availableDays;

            return View();
        }

        // Create Leave Request -- GET
        public ActionResult NewLeaveRequestPartial()
        {
            return PartialView("NewLeaveRequestPartial");
        }

        public ActionResult CancelledLeaveRequestPartial()
        {
            return PartialView("MyCancelledLeaveRequestsGridViewPartial");
        }


        public ActionResult MyRejectedLeavesGridViewPartial()
        {
            using (var db = new MyApplicationUnipiEntities())
            {
                var userId = (int)Session["userId"];
                var requests = db.LeavesRequests.Where(w => w.EmployeeId == userId && w.Status == (byte)Models.Enums.LeavesRequestStatus.Rejected).OrderBy(o => o.DateCreated).ToList();

                ConvertToGRTime(requests);

                var model = requests.Select(s => new
                {
                    s.Id,
                    s.TypeOfLeave,
                    s.DateCreated,
                    DateFrom = s.DateFrom.Value.Date,
                    DateTo = s.DateTo.Value.Date,
                    s.AmountRequested,
                    s.Comments,
                    s.Status
                });

                return PartialView("MyRejectedLeavesGridViewPartial", model.OrderByDescending(o => o.DateCreated).ToList());
            }
        }

        // Create Leave Request -- POST
        [HttpPost]
        public ActionResult NewLeaveRequestPartial(byte? typeOfLeave, string dateFrom, string dateTo, byte? numOfDays, string comments)
        {
            var db = new MyApplicationUnipiEntities();
            var userId = (int)Session["userId"];
            var availableDays = GetAvailableDays(userId, (byte)typeOfLeave);

            DateTime dFrom;
            DateTime dTo;
            var boolFrom = DateTime.TryParseExact(dateFrom, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDateFrom);
            var boolTo = DateTime.TryParseExact(dateTo, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDateTo);

            var limitedLeaveTypes = new List<byte> { (byte)LeaveTypes.Annual, (byte)LeaveTypes.Student};
            byte? amountRequested = default;
            string dateToShow = default;
            
                dFrom = parsedDateFrom.Date;
                dTo = parsedDateTo.Date;
                amountRequested = numOfDays;
                dateToShow = dFrom.Date.ToString("dd/MM/yy") + " - " + dTo.Date.ToString("dd/MM/yy");            

            if ((!boolFrom && !boolTo) || typeOfLeave == null || amountRequested == null || dFrom > dTo)
            {
                TempData["returnMsg"] = "False";
                return RedirectToAction("MyAnnualLeave");
            }
            if (amountRequested > availableDays && limitedLeaveTypes.Contains((byte)typeOfLeave))
            {
                var messageToRtrn = $"You have selected more days than you have available. Please try again.";
                TempData["returnMsg"] = "True";
                TempData["msgToRtrn"] = messageToRtrn;

                return RedirectToAction("MyAnnualLeave");
            }           
            if (string.IsNullOrEmpty(comments))
                comments = null;

            HR_Employee theEmployee = db.HR_Employee.FirstOrDefault(w => w.Id == userId && w.Status != (byte)Status.InActive);

            if (theEmployee != null)
            {
                LeavesRequests newRequestItem = new LeavesRequests
                {
                    HR_Employee = theEmployee,
                    EmployeeId = theEmployee.Id,
                    TypeOfLeave = typeOfLeave,
                    DateFrom = dFrom,
                    DateTo = dTo,
                    AmountRequested = amountRequested,
                    Comments = comments,
                    DateCreated = DateTime.UtcNow,
                    Status = (byte)LeavesRequestStatus.Requested
                };
                db.LeavesRequests.Add(newRequestItem);
                db.SaveChanges();
            }
            return RedirectToAction("MyAnnualLeave");
        }


    
        // My Requests tab
        public ActionResult MyLeaveRequestsGridViewPartial()
        {
            using (var db = new MyApplicationUnipiEntities())
            {
                var userId = (int)Session["userId"];
                var requests = db.LeavesRequests.Where(w => w.EmployeeId == userId && w.Status == (byte)LeavesRequestStatus.Requested).OrderBy(o => o.DateCreated).ToList();

                var typesWithFiles = new List<byte> { (byte)LeaveTypes.Sick, (byte)LeaveTypes.Election };

                ConvertToGRTime(requests);
                var model = requests.Select(s => new
                {
                    s.Id,
                    s.TypeOfLeave,
                    s.DateCreated,
                    DateFrom =  s.DateFrom.Value.Date,
                    DateTo = s.DateTo.Value.Date,
                    s.AmountRequested,
                    s.Comments,
                    s.Status,
                    Files = typesWithFiles.Contains(s.TypeOfLeave.Value) ?
                            string.Join(",", s.LeaveRequestsFiles.Where(w => w.Status == (int)Status.Active).Select(x => x.FileName).ToList()) : "",
                });
                return PartialView("MyLeaveRequestsGridViewPartial", model.OrderByDescending(o => o.DateCreated).ToList());
            }
        }

        // Delete a request
        public ActionResult DeleteLeaveRequest([ModelBinder(typeof(DevExpressEditorsBinder))] int? Id)
        {
            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                var userId = (int)Session["userId"]; // not needed exactly...
                var request = db.LeavesRequests.FirstOrDefault(f => f.Id == Id && f.EmployeeId == userId);

                if (!Id.HasValue || request == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                request.Status = (byte)LeavesRequestStatus.Deleted;
                db.SaveChanges();

                return RedirectToAction("MyAnnualLeave");
            }
        }


        // Approved requests tab
        public ActionResult MyApprovedLeavesGridViewPartial()
        {
            using (var db = new MyApplicationUnipiEntities())
            {
                var userId = (int)Session["userId"];
                var requests = db.LeavesRequests.Where(w => w.EmployeeId == userId && w.Status == (byte)Models.Enums.LeavesRequestStatus.Approved).OrderBy(o => o.DateCreated).ToList();

                var typesWithFiles = new List<byte> { (byte)LeaveTypes.Sick, (byte)LeaveTypes.Election };

                ConvertToGRTime(requests);
                var model = requests.Select(s => new
                {
                    s.Id,
                    s.TypeOfLeave,
                    s.DateCreated,
                    DateFrom =  s.DateFrom.Value.Date,
                    DateTo =  s.DateTo.Value.Date,
                    s.AmountRequested,
                    s.Comments,
                    s.Status,
                    Files = typesWithFiles.Contains(s.TypeOfLeave.Value) ?
                            string.Join(",", s.LeaveRequestsFiles.Where(w => w.Status == (int)Status.Active).Select(x => x.FileName).ToList()) : "",
                });

                return PartialView("MyApprovedLeavesGridViewPartial", model.OrderByDescending(o => o.DateCreated).ToList());
            }
        }

        //Cancel an Approved Request
        public ActionResult MyApprovedLeavesGridViewPartialCancel(int? Id)
        {
            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                var request = db.LeavesRequests.SingleOrDefault(f => f.Id == Id);

                if (!Id.HasValue || request == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                var userId = (int)Session["userId"];
                LeavesRequestsResponse responseItem = new LeavesRequestsResponse();
                {
                    responseItem.LeavesReqId = Id;
                    responseItem.EmployeeId = userId;
                    responseItem.DateOfResponse = DateTime.UtcNow;
                    responseItem.Status = (byte)Models.Enums.LeavesRequestStatus.Cancelled;
                    db.LeavesRequestsResponse.Add(responseItem);
                    db.SaveChanges();
                };

                // Call the leave service to Cancel the request
                LeaveSystemService.CancelRequest((int)Id);

                var requests = db.LeavesRequests.Where(w => w.EmployeeId == userId && w.Status == (byte)LeavesRequestStatus.Approved).OrderBy(o => o.DateCreated).ToList();

                ConvertToGRTime(requests);

                var model = requests.Select(s => new
                {
                    s.Id,
                    s.TypeOfLeave,
                    s.DateCreated,
                    DateFrom = s.DateFrom.Value.Date,
                    DateTo = s.DateTo.Value.Date,
                    s.AmountRequested,
                    s.Comments,
                    s.Status
                });
                return PartialView("MyApprovedLeavesGridViewPartial", model.OrderByDescending(o => o.DateCreated).ToList());
            }

        }

        // Cancelled Requests Tab
        public ActionResult MyCancelledLeaveRequestsGridViewPartial()
        {
            using (var db = new MyApplicationUnipiEntities())
            {
                var userId = (int)Session["userId"];
                var modelFromDb = db.LeavesRequests.Where(w => w.Status == (byte)LeavesRequestStatus.Cancelled && w.EmployeeId == userId).OrderBy(o => o.DateCreated).ToList();

                var typesWithFiles = new List<byte> { (byte)LeaveTypes.Sick,  (byte)LeaveTypes.Election };

                ConvertToGRTime(modelFromDb);
                if (userId <= 0)
                {
                    ViewData["EditError"] = "Employee not found.";
                }

                var leaveRequestFromDb = modelFromDb.Select(s => new
                {
                    s.Id,
                    s.TypeOfLeave,
                    s.DateCreated,
                    DateFrom =  s.DateFrom.Value.Date,
                    DateTo =  s.DateTo.Value.Date,
                    s.AmountRequested,
                    s.Comments,
                    s.Status,
                    Files = typesWithFiles.Contains(s.TypeOfLeave.Value) ?
                            string.Join(",", s.LeaveRequestsFiles.Where(w => w.Status == (int)Status.Active).Select(x => x.FileName).ToList()) : "",
                });
                return PartialView("MyCancelledLeaveRequestGridViewPartial", leaveRequestFromDb.OrderByDescending(o => o.DateCreated).ToList());
            }
        }


        public ActionResult UploadLeaveRequestFile(int? Id)
        {
            using (var db = new MyApplicationUnipiEntities())
            {
                var theLeaveRequest = db.LeavesRequests.Include(x => x.LeaveRequestsFiles).FirstOrDefault(f => f.Id == Id);

                return View(theLeaveRequest);
            }
        }

        [HttpPost]
        public ActionResult UploadLeaveRequestFilePost(int requestId)
        {
            var incoming = Request.Files;

            if (incoming != null & incoming.Count > 0)
            {
                using (var db = new MyApplicationUnipiEntities())
                {
                    for (int i = 0; i < incoming.Count; i++)
                    {
                        HttpPostedFileBase file = incoming[i];

                        var newItem = new LeaveRequestsFiles();
                        byte[] data;
                        using (BinaryReader br = new BinaryReader(file.InputStream))
                        {
                            data = br.ReadBytes(file.ContentLength);
                        }

                        newItem.LeaveRequest_Id = requestId;
                        newItem.FileName = file.FileName;

                        var split = file.FileName.Split('.');
                        newItem.FileExtension = split.Last();
                        newItem.BinaryData = data;
                        newItem.DateUploaded = DateTime.Now;
                        newItem.Status = (int)Status.Active;
                        db.LeaveRequestsFiles.Add(newItem);
                    }
                    db.SaveChanges();

                }
            }
            return RedirectToAction("UploadLeaveRequestFile", new { Id = requestId });
        }
        public ActionResult DownloadLeavesFile(int requestId, string fileName)
        {

            using (var db = new MyApplicationUnipiEntities())
            {
                var file = db.LeaveRequestsFiles.Where(w => w.LeaveRequest_Id == requestId && w.FileName == fileName && w.Status == (int)Status.Active).FirstOrDefault();

                return File(file.BinaryData, file.FileExtension, file.FileName);
            }
            //return File(file.BinaryData, file.FileExtention, file.FileName);
            //return null;
        }
        [HttpPost]
        public ActionResult RemoveFile(int requestId, long fileId)
        {
            if (requestId > 0 && fileId > 0)
            {
                using (var db = new MyApplicationUnipiEntities())
                {
                    var theFile = db.LeaveRequestsFiles.FirstOrDefault(f => f.Id == fileId);
                    theFile.Status = (int)Status.InActive;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("UploadLeaveRequestFile", new { Id = requestId });
        }

        // Utility methods
        public void ConvertToGRTime(List<LeavesRequests> reqs)
        {
            // CONVERT DATETIMES TO GR TIME !!
            var GreeceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
            reqs.ForEach(f =>
            {
                f.DateCreated = TimeZoneInfo.ConvertTimeFromUtc(f.DateCreated.Value, GreeceTimeZone);
                f.DateFrom = TimeZoneInfo.ConvertTimeFromUtc(f.DateFrom.Value, GreeceTimeZone);
                f.DateTo = TimeZoneInfo.ConvertTimeFromUtc(f.DateTo.Value, GreeceTimeZone);
            });
            UpdateModel(reqs);
        }

        public static int GetAvailableDays(int? userId, byte typeOfLeave)
        {
            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {

                var availableDays = db.AnnualLeaves.Where(w => w.HR_Employee_Contract.Employee_Id == userId && w.Status != (byte)Status.InActive && w.LeaveType == typeOfLeave)
                    .Select(i => (int)((i.Available != null) ? i.Available : 0)).ToList().Sum();

                return availableDays;

            }
        }

        // Public Holiday tab
        [ValidateInput(false)]
        public ActionResult DaysOffGridViewPartial()
        {
            MyApplicationUnipiEntities db = new MyApplicationUnipiEntities();
            var model = db.Daysoff;
            int currentYear = DateTime.Now.Year;
            return PartialView("_DaysOffGridViewPartial", model.Where(i => i.Status != (byte)WebApplicationUnipi.Models.Enums.Status.InActive && i.Date.HasValue && i.Date.Value.Year == currentYear).OrderByDescending(o => o.Date).ToList());
        }

        #endregion

    }

}
