using DevExpress.Web.Mvc;
using WebApplicationUnipi.Models.Enums;
using WebApplicationUnipi.Models.MyApplicationUnipi;
using WebApplicationUnipi.Services.Factories;
using MvcSiteMapProvider.Web.Mvc.Filters;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace WebApplicationUnipi.Controllers
{
    public class AdminAnnualLeavesController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            UserAuth.Check(Session, filterContext, controllerName);
        }

        public static Logger logger = LogManager.GetLogger("logger");



        // GET: AdminAnnualLeaves
        public ActionResult Index()
        {
            using (var db = new MyApplicationUnipiEntities())
            {
                int usrId = (int)Session["UserId"];
            
                return View();
            }
        }

        public ActionResult AdminLeaves()
        {
           

            return View();
        }

        public ActionResult AnnualLeaves()
        {
            using (var db = new MyApplicationUnipiEntities())
            {
                int usrId = (int)Session["UserId"];
         
                return View();
            }            
         }

        // Requests Tab (Default)
        [ValidateInput(false)]

        
        public ActionResult LeavesRequestsGridViewPartial(int? userID)
        {

            ViewBag.userID = userID;
           
            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {                
                var model = db.LeavesRequests.Include(x => x.LeaveRequestsFiles).Where(w => w.Status == (byte)LeavesRequestStatus.Requested).Include(hr => hr.HR_Employee).ToList();

                
                model = model.Where(w => w.HR_Employee.Status == (int)Status.Active).ToList();
             
                var c = LeaveSystemService.PrepareModelLeaveRequests(model);


                return PartialView("_LeavesRequestsGridViewPartial", c);
            }
        }

        [HttpPost]
        public async Task<ActionResult> ApproveLeaveRequest(int? Id)
        {
            if (!Id.HasValue || Id <= 0)
                return Json(new { success = false, message = "Invalid request" });

            int respondeeId = (int)Session["UserId"];
            string msgResult = "Oops, something went wrong!";

            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                if (respondeeId > 0)
                    msgResult = LeaveSystemService.ApproveRequest(Id.Value, respondeeId);
            }

            logger.Info(msgResult);
            return Json(new { success = true, message = msgResult });
        }

        [HttpPost]
        public async Task<ActionResult> RejectLeaveRequest(int? Id)
        {
            if (!Id.HasValue || Id <= 0)
                return Json(new { success = false, message = "Invalid request" });

            int respondeeId = (int)Session["UserId"];
            string msgResult = "Oops, something went wrong!";

            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                if (respondeeId > 0)
                    msgResult = LeaveSystemService.RejectRequest(Id.Value, respondeeId);
            }

            logger.Info(msgResult);
            return Json(new { success = true, message = msgResult });
        }
        [HttpGet]
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



        //Cancelled Leaves Tab

        public ActionResult CancelledLeaves()
        {
            return PartialView("_CancelledLeaves");
        }
        
        public ActionResult CancelledLeavesGridPartial(int? year)
        {
            if (year == null)
                year = DateTime.UtcNow.Year;
            
            ViewBag.year = year;

            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                var model = db.LeavesRequests.Where(w => w.Status == (byte)LeavesRequestStatus.Cancelled 
                                                      && (w.DateFrom.Value.Year == year || w.DateTo.Value.Year == year)
                                                   ).Include(hr => hr.HR_Employee).OrderBy(o => o.DateCreated).ToList();

                // CONVERT DATETIMES TO GR TIME !!
                var GreeceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
                model.ForEach(f =>
                {
                    f.DateCreated = TimeZoneInfo.ConvertTimeFromUtc(f.DateCreated.Value, GreeceTimeZone);
                    f.DateFrom = TimeZoneInfo.ConvertTimeFromUtc(f.DateFrom.Value, GreeceTimeZone);
                    f.DateTo = TimeZoneInfo.ConvertTimeFromUtc(f.DateTo.Value, GreeceTimeZone);
                   
                    
                    f.DateFrom = f.DateFrom.Value.Date;
                    f.DateTo = f.DateTo.Value.Date;
                    
                });

              
                 model = model.Where(w => w.HR_Employee.Status == (int)Status.Active).ToList();
              
                //var requests = db.LeavesRequests.Where(w => w.Status == (byte)Models.Enums.LeavesRequestStatus.Cancelled).Include(hr => hr.HR_Employee).OrderBy(o => o.DateCreated).ToList();
                var typesWithFiles = new List<byte> { (byte)LeaveTypes.Sick, (byte)LeaveTypes.Election };

                var c = model.Select(s => new
                {
                    s.Id,
                    Employee_Fname = s.HR_Employee.Employee_Fname.Split(' ')[1],
                    Employee_Lname = s.HR_Employee. Employee_Lname.Split(' ')[0],
                    s.TypeOfLeave,
                    s.DateCreated,
                    s.DateFrom,
                    s.DateTo,
                    s.AmountRequested,
                    s.Comments,
                    s.Status,
                    Files = typesWithFiles.Contains(s.TypeOfLeave.Value) ?
                            string.Join(",", s.LeaveRequestsFiles.Where(w => w.Status == (int)Status.Active).Select(o => o.FileName).ToList()) : "",
                   
                });
                return PartialView("CancelledLeavesGridViewPartial" , c.OrderByDescending(o => o.DateCreated).ToList());
            }            
        }


    


        // Dropdown "More Details" Request Responses

        [ValidateInput(false)]
        public async Task<ActionResult> LeavesRequestsMoreDetails(int? Id)
        {
            if (string.IsNullOrEmpty((string)Session["Username"]))
                return RedirectToAction("Index", "Login");

            if (Id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                var model = db.LeavesRequestsResponse.Where(w => w.LeavesReqId == Id && w.Status != (byte)Models.Enums.LeavesRequestStatus.Deleted).Include(hr => hr.HR_Employee).ToList();

                var c = LeaveSystemService.PrepareModelLeaveResponses(model);

                ViewBag.Id = Id;
                return PartialView("_LeavesRequestsMoreDetails", c);
            }
        }


        // IT DELETES A RESPONSE AND REVERTS THE STATUS OF THE REQUEST IF IT WAS CHANGED.
        public ActionResult DeleteRequestResponse(int? Id)
        {            
         
            using (var db = new MyApplicationUnipiEntities())
            {
                var theResponse = db.LeavesRequestsResponse.Where(w => w.Id == Id).FirstOrDefault();
                var theRequest = (theResponse != null) ? theResponse.LeavesRequests : null;

                if (!Id.HasValue || theResponse == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

          
                var responses = db.LeavesRequestsResponse.Where(w => w.LeavesReqId == theResponse.LeavesReqId && w.Status != (byte)LeavesRequestStatus.Deleted).ToList();

                if (theRequest.Status == (byte)LeavesRequestStatus.Approved)
                {
                    // Diadikasia epistrofis diathesimwn imerwn pou aferethikan kata to approve tou request.
                    var criticalLeaveTypes = new List<byte> { (byte)LeaveTypes.Annual, (byte)LeaveTypes.Student};
                                        
                    if (criticalLeaveTypes.Contains((byte)theRequest.TypeOfLeave))
                    {
                        var daysToRestore = theRequest.AmountRequested;
                        var LeavesDaysItems = db.LeavesDayChanges.Where(w => w.LeavesRequestID == theRequest.Id && w.Status == (byte)Status.Active).ToList();

                        if (LeavesDaysItems.Count == 1 && daysToRestore == LeavesDaysItems[0].AmountRemoved)
                        {
                            var item = LeavesDaysItems.First();
                            var theContract = db.AnnualLeaves.SingleOrDefault(s => s.Id == item.AnnualLeavesID);
                            theContract.Available += LeavesDaysItems[0].AmountRemoved;
                            LeavesDaysItems[0].Status = (byte)Status.InActive;
                        }
                        else if (LeavesDaysItems.Count > 1)
                        {                            
                            LeavesDaysItems.ForEach(f => {
                                db.AnnualLeaves.SingleOrDefault(s => s.Id == f.AnnualLeavesID).Available += f.AmountRemoved;
                                f.Status = (byte)Status.InActive;
                            });
                        }
                    }

                    
                    
                }

                theRequest.Status = (byte)LeavesRequestStatus.Requested;
                theResponse.Status = (byte)LeavesRequestStatus.Deleted;
                db.SaveChanges();
            }
            return RedirectToAction("AdminLeaves");
        }


      


        // Approved & Rejected Leaves (per month) Tabs

        public ActionResult LeavesPerMonth()
        {
            
            ViewData["monthPick"] = DateTime.UtcNow.ToString("MMMM yyyy");
            return PartialView("_LeavesPerMonth");
        }

        public ActionResult RejectedLeavesPerMonth()
        {
           
            ViewData["monthPick"] = DateTime.UtcNow.ToString("MMMM yyyy");
            return PartialView("_RejectedLeavesPerMonth");
        }
    

        [ValidateInput(false)]
        public ActionResult ApprovedLeavesRequestsGridViewPartial(DateTime? dateFrom, int? Id)
        {
            var userId = (int)Session["UserId"];
          
            if (dateFrom == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ViewData["monthPick"] = dateFrom;

            using (var db = new MyApplicationUnipiEntities())
            {
                var model = db.LeavesRequests.Where(w => w.Status == (byte)LeavesRequestStatus.Approved 
                                                         && ((w.DateFrom.Value.Month == dateFrom.Value.Month && w.DateFrom.Value.Year == dateFrom.Value.Year) 
                                                            || (w.DateTo.Value.Month == dateFrom.Value.Month && w.DateTo.Value.Year == dateFrom.Value.Year))
                                                   ).Include(hr => hr.HR_Employee).ToList();


                var c = LeaveSystemService.PrepareModelLeaveRequests(model);

                return PartialView("ApprovedLeavesRequestsGridViewPartial", c);
            }
        }

        [ValidateInput(false)]
        public ActionResult ApprovedLeavesPerMonthCancel(int? Id, DateTime? dateFrom)
        {
            if (Id > 0 && dateFrom != null)
            {                
                ViewData["monthPick"] = dateFrom;
            
                var PriveList = (List<int>)Session["UserRights"];
          
                using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
                {
                    int respondeeId = (int)Session["UserId"];
                    LeavesRequestsResponse responseItem = new LeavesRequestsResponse();
                    {
                        responseItem.LeavesReqId = Id;
                        responseItem.EmployeeId = respondeeId;
                        responseItem.DateOfResponse = DateTime.UtcNow;
                        responseItem.Status = (byte)LeavesRequestStatus.Cancelled;
                        db.LeavesRequestsResponse.Add(responseItem);
                        db.SaveChanges();
                    };

                    // Call the leave service to Cancel the request
                    LeaveSystemService.CancelRequest((int)Id);

                    var model = db.LeavesRequests.Where(w => w.Status == (byte)LeavesRequestStatus.Approved
                                                           && ((w.DateFrom.Value.Month == dateFrom.Value.Month && w.DateFrom.Value.Year == dateFrom.Value.Year)
                                                                || (w.DateTo.Value.Month == dateFrom.Value.Month && w.DateTo.Value.Year == dateFrom.Value.Year))
                                                       ).Include(hr => hr.HR_Employee).ToList();

                
                        model = model.Where(w => w.HR_Employee.Status == (int)Status.Active).ToList();

                    var c = LeaveSystemService.PrepareModelLeaveRequests(model);

                    return PartialView("ApprovedLeavesRequestsGridViewPartial", c);
                } 
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);                        
        }


        [ValidateInput(false)]
        public ActionResult RejectedLeavesRequestsGridViewPartial(DateTime? dateFrom)
        {            
            var PriveList = (List<int>)Session["UserRights"];
          
            if (dateFrom == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ViewData["monthPick"] = dateFrom;

            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                var model = db.LeavesRequests.Where(w => w.Status == (byte)Models.Enums.LeavesRequestStatus.Rejected
                                                             && ((w.DateFrom.Value.Month == dateFrom.Value.Month && w.DateFrom.Value.Year == dateFrom.Value.Year)
                                                                || (w.DateTo.Value.Month == dateFrom.Value.Month && w.DateTo.Value.Year == dateFrom.Value.Year))
                                                   ).Include(hr => hr.HR_Employee).ToList();

                
                var c = LeaveSystemService.PrepareModelLeaveRequests(model);

                return PartialView("_RejectedLeavesRequestsGridViewPartial", c);
            }
        }


        [ValidateInput(false)]
        public ActionResult RejectedLeavesPerMonthCancel(int? Id, DateTime? dateFrom)
        {
            if (Id > 0 && dateFrom != null)
            {
                ViewData["monthPick"] = dateFrom;
           
                var PriveList = (List<int>)Session["UserRights"];
               
                using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
                {
                    int respondeeId = (int)Session["UserId"];
                    LeavesRequestsResponse responseItem = new LeavesRequestsResponse();
                    {
                        responseItem.LeavesReqId = Id;
                        responseItem.EmployeeId = respondeeId;
                        responseItem.DateOfResponse = DateTime.UtcNow;
                        responseItem.Status = (byte)Models.Enums.LeavesRequestStatus.Cancelled;
                        //responseItem.Points = 0;
                        db.LeavesRequestsResponse.Add(responseItem);
                        db.SaveChanges();
                    };
                                        
                    var reqToDelete = db.LeavesRequests.SingleOrDefault(s => s.Id == Id);
                    reqToDelete.Status = (byte)LeavesRequestStatus.Cancelled;
                    db.SaveChanges();

                    var model = db.LeavesRequests.Where(w => w.Status == (byte)Models.Enums.LeavesRequestStatus.Rejected
                                                             && ((w.DateFrom.Value.Month == dateFrom.Value.Month && w.DateFrom.Value.Year == dateFrom.Value.Year)
                                                                || (w.DateTo.Value.Month == dateFrom.Value.Month && w.DateTo.Value.Year == dateFrom.Value.Year))
                                                       ).Include(hr => hr.HR_Employee).ToList();

                   
                        model = model.Where(w => w.HR_Employee.Status == (int)Status.Active).ToList();

                    var c = LeaveSystemService.PrepareModelLeaveRequests(model);

                    return PartialView("_RejectedLeavesRequestsGridViewPartial", c);
                }               
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }



        // Approved Leaves per employee Tab (for current Year)
        public ActionResult LeavesPerEmployee()
        {
            using (var db = new MyApplicationUnipiEntities())
            {
           
                var users = db.HR_Employee.Where(w => w.Status == (byte)Status.Active).ToList();
               
                ViewData["empList"] = users.Select(x => new SelectListItem
                {
                    Text = x.Employee_Lname + " | " + x.HR_Position.Position_Name,
                    Value = x.Id.ToString(),
                    //Selected = (selectedUser != null || selectedUser != 0) && selectedUser == x.Id ? true : false
                }).ToList();
            }
            return PartialView("_LeavesPerEmployee");
        }

        [ValidateInput(false)]
        public ActionResult ApprovedLeavesPerEmployeeGridViewPartial(int? userId, int? year)
        {            
            if ( userId <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            if (year == null)
                year = DateTime.UtcNow.Year;
            
            ViewBag.userId = userId;
            ViewBag.year = year;

            var PriveList = (List<int>)Session["UserRights"];
            
            var typesWithFiles = new List<byte> { (byte)LeaveTypes.Sick,  (byte)LeaveTypes.Election };

            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                var model = db.LeavesRequests.Where(w => w.Status == (byte)Models.Enums.LeavesRequestStatus.Approved
                                                      && w.EmployeeId == userId
                                                      && (w.DateFrom.Value.Year == year || w.DateTo.Value.Year == year))
                                             .Include(hr => hr.HR_Employee).ToList();


                // CONVERT DATETIMES TO GR TIME !!
                var GreeceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
                model.ForEach(f =>
                {
                    f.DateCreated = TimeZoneInfo.ConvertTimeFromUtc(f.DateCreated.Value, GreeceTimeZone);
                    f.DateFrom = TimeZoneInfo.ConvertTimeFromUtc(f.DateFrom.Value, GreeceTimeZone);
                    f.DateTo = TimeZoneInfo.ConvertTimeFromUtc(f.DateTo.Value, GreeceTimeZone);

                   
                     f.DateFrom = f.DateFrom.Value.Date;
                     f.DateTo = f.DateTo.Value.Date;
                    
                });
                UpdateModel(model);                               

                var c = model.Select(s => new
                {                    
                    s.Id,
                    Employee_Fname = s.HR_Employee.Employee_Fname,
                    Employee_Lname = s.HR_Employee.Employee_Lname,                    
                    s.DateCreated,
                    s.DateFrom,
                    s.DateTo,
                    s.TypeOfLeave,
                    s.AmountRequested,
                    s.Comments,
                    s.Status,
                    Available = MyProfileController.GetAvailableDays(userId, (byte)s.TypeOfLeave),// ***
                    Files = typesWithFiles.Contains(s.TypeOfLeave.Value) ?
                            string.Join(",", s.LeaveRequestsFiles.Where(w => w.Status == (int)Status.Active).Select(x => x.FileName).ToList()) : "",
                }).OrderByDescending(o => o.DateCreated).ToList();

                return PartialView("_ApprovedLeavesPerEmployeeGridViewPartial", c);
            }
        }

        [ValidateInput(false)]
        public ActionResult ApprovedLeavesPerEmployeeCancel(int? Id, int? userId , int? year)
        {
            if (Id > 0 && userId > 0)
            {
                if (year == null)
                {
                    year = DateTime.UtcNow.Year;
                }

                ViewBag.userId = userId;
                ViewBag.year = year;

                using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
                {
                    int respondeeId = (int)Session["UserId"];
                    LeavesRequestsResponse responseItem = new LeavesRequestsResponse();
                    {
                        responseItem.LeavesReqId = Id;
                        responseItem.EmployeeId = respondeeId;
                        responseItem.DateOfResponse = DateTime.UtcNow;
                        responseItem.Status = (byte)Models.Enums.LeavesRequestStatus.Cancelled;
                        db.LeavesRequestsResponse.Add(responseItem);
                        db.SaveChanges();
                    };

                    // Call the leave service to Cancel the request
                    LeaveSystemService.CancelRequest((int)Id);

                    var model = db.LeavesRequests.Where(w => w.Status == (byte)Models.Enums.LeavesRequestStatus.Approved
                                                                           && w.EmployeeId == userId
                                                                           && (w.DateFrom.Value.Year == year || w.DateTo.Value.Year == year))
                                                                  .Include(hr => hr.HR_Employee).ToList();

                    // CONVERT DATETIMES TO GR TIME !!
                    var GreeceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
                    model.ForEach(f =>
                    {
                        f.DateCreated = TimeZoneInfo.ConvertTimeFromUtc(f.DateCreated.Value, GreeceTimeZone);
                        f.DateFrom = TimeZoneInfo.ConvertTimeFromUtc(f.DateFrom.Value, GreeceTimeZone);
                        f.DateTo = TimeZoneInfo.ConvertTimeFromUtc(f.DateTo.Value, GreeceTimeZone);
                    });
                    UpdateModel(model);                                       

                    var c = model.Select(s => new
                    {
                        s.Id,
                        Employee_Fname = s.HR_Employee.Employee_Fname,
                        Employee_Lname = s.HR_Employee.Employee_Lname,
                        s.DateCreated,
                        s.DateFrom,
                        s.DateTo,
                        s.TypeOfLeave,
                        s.AmountRequested,
                        s.Comments,
                        s.Status,
                        Available = MyProfileController.GetAvailableDays(userId, (byte)s.TypeOfLeave)
                    }).OrderByDescending(o => o.DateCreated).ToList();

                    return PartialView("_ApprovedLeavesPerEmployeeGridViewPartial", c);
                }                
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            
        }
    }
}