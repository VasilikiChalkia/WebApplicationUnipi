using WebApplicationUnipi.Controllers;
using WebApplicationUnipi.Models.Enums;
using WebApplicationUnipi.Models.MyApplicationUnipi;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace WebApplicationUnipi.Services.Factories
{
    public class LeaveSystemService
    {
        public static Logger logger = LogManager.GetLogger("logger");



        public static List<CustomModelLeaveRequests> PrepareModelLeaveRequests(List<LeavesRequests> model)
        {
            using (var db = new MyApplicationUnipiEntities())
            {
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

                var typesWithFiles = new List<byte> {(byte)LeaveTypes.Sick, (byte)LeaveTypes.Election };

                // Check if the model contains items of Requested status, so we can include the computation of available days.
                // Available days not needed to be shown in cases of other tabs like (Approved and Rejected requests).
                bool eimasteRequested = false;
                if (model.All(a => a.Status == (byte)LeavesRequestStatus.Requested))
                    eimasteRequested = true;

                var tempList = eimasteRequested ? model.GroupBy(g => new { g.EmployeeId, g.TypeOfLeave }).Select(s => new
                {
                    EmployeeId = s.Key,
                    Available = db.AnnualLeaves.Where(w => w.HR_Employee_Contract.Employee_Id == s.Key.EmployeeId && w.Status != 3 && w.LeaveType == s.Key.TypeOfLeave)
                                               .Select(i => (int?)i.Available).ToList().Sum()
                }).ToList() : null;

                var c = model.Select(s => new CustomModelLeaveRequests
                {
                    Id = s.Id,
                    Employee_Fname = s.HR_Employee.Employee_Fname,
                    Employee_Lname = s.HR_Employee.Employee_Lname,
                    DateCreated = s.DateCreated,
                    DateFrom = s.DateFrom,
                    DateTo = s.DateTo,
                    TypeOfLeave = s.TypeOfLeave,
                    AmountRequested = s.AmountRequested,
                    Comments = s.Comments,
                    Status = s.Status,
                    Available = (eimasteRequested) ? tempList.FirstOrDefault(f => f.EmployeeId.EmployeeId == s.EmployeeId && f.EmployeeId.TypeOfLeave == s.TypeOfLeave).Available : null,                  
                    // Available = db.AnnualLeaves.Where(w => w.HR_Employee_Contract.Employee_Id == s.HR_Employee.Id && w.Status != 3).Select(i => (int?)i.Available).ToList().Sum()
                    Files = typesWithFiles.Contains(s.TypeOfLeave.Value) ? 
                            string.Join(",", s.LeaveRequestsFiles.Where(w => w.Status == (int)Status.Active).Select(o => o.FileName).ToList()) : "",
                }).OrderByDescending(o => o.DateCreated).ToList();

                return c; 
            }
        }

        public static List<CustomModelLeaveResponses> PrepareModelLeaveResponses(List<LeavesRequestsResponse> model)
        {
            // CONVERT DATETIMES TO GR TIME !!
            var GreeceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
            model.ForEach(f =>
            {
                f.DateOfResponse = TimeZoneInfo.ConvertTimeFromUtc(f.DateOfResponse.Value, GreeceTimeZone);
            });




            var cryptonite = model.Select(s => s.HR_Employee).ToList();



            var c = model.Select(s => new CustomModelLeaveResponses
            {
                Id = s.Id,
                LeavesReqId = s.LeavesReqId,
                EmployeeId = s.EmployeeId,
                FullName = cryptonite.Where(w => w.Id == s.EmployeeId).Select(o => o.Employee_Fname).FirstOrDefault() + " " + cryptonite.Where(w => w.Id == s.EmployeeId).Select(o => o.Employee_Lname).FirstOrDefault(),
                DateOfResponse = s.DateOfResponse,
                Status = s.Status
            }).OrderByDescending(o => o.DateOfResponse).ToList();

            return c;
        }

        public static string ApproveRequest(int reqId, int respondeeId)
        {
            using (var db = new MyApplicationUnipiEntities())
            {
               
                if (reqId > 0)
                {
                  
                    LeavesRequests request = db.LeavesRequests.Where(w => w.Id == reqId).FirstOrDefault();
                    List<LeavesRequestsResponse> responses = db.LeavesRequestsResponse.Where(w => w.LeavesReqId == reqId && w.Status != (byte)LeavesRequestStatus.Deleted).ToList();
              

                    int availableDays = 0;
                    var amountToRemove = request.AmountRequested;

                    var criticalLeaveTypes = new List<byte> { (byte)LeaveTypes.Annual, (byte)LeaveTypes.Student };

                    if (criticalLeaveTypes.Contains((byte)request.TypeOfLeave))
                    {
                        availableDays = MyProfileController.GetAvailableDays(request.HR_Employee.Id, (byte)request.TypeOfLeave);

                        if (amountToRemove > availableDays)
                            return "Error, not enough available days for the request approval";
                    }

                    // Checking (again) if the user has already responded to this request
                    bool ksanaApantisa = responses.Any(a => a.EmployeeId == respondeeId);
                    if (!ksanaApantisa) // Double check. The button leading here doesn't show if user has already responded.
                    {
                        // Add a response entry in LeavesRequestsResponse table
                        LeavesRequestsResponse responseItem = new LeavesRequestsResponse();
                        {
                            responseItem.LeavesReqId = reqId;
                            responseItem.EmployeeId = respondeeId;
                            responseItem.DateOfResponse = DateTime.UtcNow;
                            responseItem.Status = (byte)Models.Enums.LeavesRequestStatus.Approved;
                            db.LeavesRequestsResponse.Add(responseItem);
                            db.SaveChanges();
                        };
                    }
                    responses = db.LeavesRequestsResponse.Include(x => x.HR_Employee).Where(w => w.LeavesReqId == reqId && w.Status != (byte)LeavesRequestStatus.Deleted).ToList();

                    var fullName = request.HR_Employee.Employee_Lname;

                 

                    // Now fix the available days.Available days are not affected if Type of leave is not "Annual" or "Student".
                    if (criticalLeaveTypes.Contains((byte)request.TypeOfLeave))
                    {
                        List<AnnualLeaves> annualLeaves = db.AnnualLeaves.Where(w => w.HR_Employee_Contract.Employee_Id == request.EmployeeId && w.Status != 3 && w.Available != 0 && w.LeaveType == request.TypeOfLeave).OrderBy(o => o.Year).ThenBy(o => o.HR_Employee_Contract.Start_Date).ToList();

                        List<byte?> tempList = new List<byte?> { };
                        annualLeaves.ForEach(f => { tempList.Add(f.Available); });

                        for (int i = 0; i < tempList.Count; i++)
                        {
                            if (tempList[i] >= amountToRemove)
                            {
                                tempList[i] -= amountToRemove;
                                amountToRemove = 0;
                                break;
                            }
                            else
                            {
                                amountToRemove -= tempList[i];
                                tempList[i] = 0;
                            }
                        }

                        int x = 0;
                        annualLeaves.ForEach(f =>
                        {
                            var item = new LeavesDayChanges
                            {
                                AnnualLeavesID = f.Id,
                                LeavesRequestID = request.Id,
                                AmountRemoved = (byte)(f.Available - tempList[x]),
                                Date = DateTime.Now,
                                Status = 1
                            };
                            db.LeavesDayChanges.Add(item);

                            f.Available = tempList[x];
                            x++;
                        });
                        db.SaveChanges();
                    }

                    string dateToShow = string.Empty;
                    
                        dateToShow = request.DateFrom.Value.Date.ToString("dd/MM/yy") + " - " + request.DateTo.Value.Date.ToString("dd/MM/yy");

                    
                        request.Status = (byte)LeavesRequestStatus.Approved;


                        db.SaveChanges();

                       

                    return $"{fullName} request is approved!";
                }
                
                else
                    return "Error: Invalid Id value";
            }
        }


        public static string RejectRequest(int reqId, int respondeeId)
        {
            using (var db = new MyApplicationUnipiEntities())
            {
                string respondeeMail = db.HR_Employee.SingleOrDefault(s => s.Id == respondeeId).Personal_Email;

                LeavesRequests request = db.LeavesRequests.Where(w => w.Id == reqId).FirstOrDefault();
                List<LeavesRequestsResponse> responses = db.LeavesRequestsResponse.Where(w => w.LeavesReqId == reqId && w.Status != (byte)LeavesRequestStatus.Deleted).ToList();

                // Checking (again) if the user has already responded to this request
                // Not necessary check. The button leading here doesn't show if user has already responded.
                bool ksanaApantisa = responses.Any(a => a.EmployeeId == respondeeId);
                if (!ksanaApantisa)
                {
                    // Add a response entry in LeavesRequestsResponse table
                    LeavesRequestsResponse responseItem = new LeavesRequestsResponse();
                    {
                        responseItem.LeavesReqId = reqId;
                        responseItem.EmployeeId = respondeeId;
                        responseItem.DateOfResponse = DateTime.UtcNow;
                        responseItem.Status = (byte)Models.Enums.LeavesRequestStatus.Rejected;
                        db.LeavesRequestsResponse.Add(responseItem);
                        db.SaveChanges();
                    };

                    responses = db.LeavesRequestsResponse.Include(x => x.HR_Employee).Where(w => w.LeavesReqId == reqId && w.Status != (byte)LeavesRequestStatus.Deleted).ToList();
                    var requestEmail = request.HR_Employee.Personal_Email;
                    var fullName = request.HR_Employee.Employee_Lname;
                    int availableDays = MyProfileController.GetAvailableDays(request.HR_Employee.Id, (byte)request.TypeOfLeave);

                    request.Status = (byte)LeavesRequestStatus.Rejected;
                    db.SaveChanges();

                    string dateToShow = string.Empty;                   
                        dateToShow = request.DateFrom.Value.Date.ToString("dd/MM/yy") + " - " + request.DateTo.Value.Date.ToString("dd/MM/yy");


                    return $"{fullName} request is rejected.";
                }
                else
                    return "Error: A response from this user already exists.";
            }

        }


        public static void CancelRequest(int reqId)
        {
            using (var db = new MyApplicationUnipiEntities())
            {
                var reqToDelete = db.LeavesRequests.SingleOrDefault(s => s.Id == reqId);
                bool wasItApproved = (reqToDelete.Status == (byte)LeavesRequestStatus.Approved);

                reqToDelete.Status = (byte)LeavesRequestStatus.Cancelled;

                var criticalLeaveTypes = new List<byte> { (byte)LeaveTypes.Annual, (byte)LeaveTypes.Student };

                // If the request was of Annual or Student  Leave type we have to restore the Available days that were removed on approval, to the proper contracts.
                if (criticalLeaveTypes.Contains((byte)reqToDelete.TypeOfLeave))
                {
                    var amountToRestore = reqToDelete.AmountRequested;
                    var LeavesDaysItems = db.LeavesDayChanges.Where(w => w.LeavesRequestID == reqToDelete.Id && w.Status == (byte)Status.Active).ToList();

                    if (LeavesDaysItems.Count == 1 && amountToRestore == LeavesDaysItems[0].AmountRemoved)
                    {
                        var item = LeavesDaysItems.First();
                        var theContract = db.AnnualLeaves.SingleOrDefault(s => s.Id == item.AnnualLeavesID);
                        theContract.Available += LeavesDaysItems[0].AmountRemoved;
                        LeavesDaysItems[0].Status = (byte)Status.InActive;
                    }
                    else if (LeavesDaysItems.Count > 1)
                    {
                        // Check if there is any newer, approved request than the request to delete that would cause issue with the available days.
                        //var ohFck = db.LeavesRequests.Where(a => a.Status == (byte)LeavesRequestStatus.Approved && a.DateCreated > reqToDelete.DateCreated).ToList();
                        LeavesDaysItems.ForEach(f => {
                            db.AnnualLeaves.SingleOrDefault(s => s.Id == f.AnnualLeavesID).Available += f.AmountRemoved;
                            f.Status = (byte)Status.InActive;
                        });                        
                    }
                }
              
                db.SaveChanges();
            }
        }


      


        public class CustomModelLeaveRequests
        {
            public int Id { get; set; }
            public string Employee_Fname { get; set; }
            public string Employee_Lname { get; set; }
            public DateTime? DateCreated { get; set; }
            public DateTime? DateFrom { get; set; }
            public DateTime? DateTo { get; set; }
            public byte? TypeOfLeave { get; set; }
            public byte? AmountRequested { get; set; }
            public string Comments { get; set; }



            public byte? Status { get; set; }
            public int? Available { get; set; }

            public string Files { get; set; }
        }

        public class CustomModelLeaveResponses
        {
            public int Id { get; set; }
            public int? LeavesReqId { get; set; }
            public int? EmployeeId { get; set; }
            public string FullName { get; set; }
            public DateTime? DateOfResponse { get; set; }
            public byte? Status { get; set; }

        }
    }
}