using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.Data.WcfLinq.Helpers;
using DevExpress.Web.Mvc;
using WebApplicationUnipi.Models;
using WebApplicationUnipi.Models.AnnualLeaves;
using WebApplicationUnipi.Models.Enums;
using WebApplicationUnipi.Models.MyApplicationUnipi;
using WebApplicationUnipi.Services.Factories;
using MvcSiteMapProvider.Web.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WebApplicationUnipi.Controllers
{
    public class AllEmployeesViewController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            UserAuth.Check(Session, filterContext, controllerName);
        }



        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // GET: AllEmployeesView
        public ActionResult Index()
            {
            using (var db = new MyApplicationUnipiEntities())
            {
                var usrID = (int)Session["UserId"];
             
               return View();
            }
        }

        MyApplicationUnipiEntities db = new MyApplicationUnipiEntities();

        [HttpGet]
        public ActionResult CompanyEmployees()
        {
         
            return View();
        }

        #region First level GridView Employee methods

        [ValidateInput(false)]
        public async Task<ActionResult> GridViewIndexPartial()
        {
         
            var empls = await GetEmployees();

            return PartialView("_GridViewIndexPartial", empls.ToList());
        }


        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> GridViewIndexPartialUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] All_Employees_Custom item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    HR_Employee modelItem = db.HR_Employee.FirstOrDefault(it => it.Id == item.Id);

                    if (modelItem != null)
                    {
                        // Update name fields if changed
                        if (!string.IsNullOrEmpty(item.Employee_Fname) && modelItem.Employee_Fname != item.Employee_Fname)
                            modelItem.Employee_Fname = item.Employee_Fname;

                        if (!string.IsNullOrEmpty(item.Employee_Lname) && modelItem.Employee_Lname != item.Employee_Lname)
                            modelItem.Employee_Lname = item.Employee_Lname;

                        if (modelItem.Department_Id != item.Department_Id)
                            modelItem.Department_Id = item.Department_Id;

                        if (modelItem.Position_Id != item.Position_Id)
                            modelItem.Position_Id = item.Position_Id;

                     
                        if (modelItem.Privileges_Num != item.Privileges_Num)
                            modelItem.Privileges_Num = item.Privileges_Num;

                        if (modelItem.Status != item.Status)
                            modelItem.Status = item.Status;

                        db.SaveChanges();
                    }
                    else
                    {
                        // Create new employee
                        HR_Employee modelItem1 = new HR_Employee
                        {
                            Id = item.Id,
                            Employee_Fname = item.Employee_Fname,
                            Employee_Lname = item.Employee_Lname,
                            Department_Id = item.Department_Id,
                            Position_Id = item.Position_Id,
                            Status = item.Status > 0 ? item.Status : 1, // default to Active
                            Privileges_Num = item.Privileges_Num
                        };

                        db.HR_Employee.Add(modelItem1);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
            {
                ViewData["EditError"] = "Please, correct all errors.";
            }

            // Return updated grid
            var empls = await GetEmployees();
            return PartialView("_GridViewIndexPartial", empls);
        }



        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> GridViewIndexPartialDelete([ModelBinder(typeof(DevExpressEditorsBinder))] System.Int32? Id)
        {
            
                try
                {
                    var item = db.HR_Employee.Where(it => it.Id == Id).FirstOrDefault();
                    if (item != null)
                    {
                       
                        item.Status = 3;
                       
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }

                GetAllComponentPrivileges();
            
            var empls = await GetEmployees();
          
            return PartialView("_GridViewIndexPartial", empls);
        }

        private async Task<List<All_Employees_Custom>> GetEmployees()
        {
          
            IEnumerable<All_Employees_Custom> customia;
            List<All_Employees_View> model = new List<All_Employees_View>();

           
           
                // Get the data from the DB View
                var userId = int.Parse(Session["UserId"].ToString());
               
           
                model = db.All_Employees_View.AsNoTracking().Where(i => i.Status != 3).ToList();
        
            //await users reference task to continue


            customia = model.GroupBy(g => g.Id).Select(s => new All_Employees_Custom
            {
                Id = s.Select(s1 => s1.Id).FirstOrDefault(),
                Department_Id = s.Select(s1 => s1.Department_Id).FirstOrDefault(),
                Position_Id = s.Select(s1 => s1.Position_Id).FirstOrDefault(),
                Employee_Fname = s.Select(s1 => s1.Employee_Fname).FirstOrDefault(),
                Employee_Lname = s.Select(s1 => s1.Employee_Lname).FirstOrDefault(),
                Password = s.Select(s1 => s1.Password).FirstOrDefault(),
                Status = s.Select(s1 => s1.Status).FirstOrDefault(),
                Available = MyProfileController.GetAvailableDays(s.Select(s1 => s1.Id).FirstOrDefault(), (byte)LeaveTypes.Annual),
                Privileges_Num = s.Select(s1 => s1.Privileges_Num).FirstOrDefault()

            });

            return customia.OrderBy(o => o.Employee_Lname).ToList();
        }

        #endregion

     


        // MORE DETAILS ON SPECIFIC EMPLOYEE
        [HttpGet]
        [SiteMapTitle("usrID")]
        public ActionResult MoreDetailsEmployee(System.Int32? usrID, string status, String errorMsg = "")
        {
            ViewBag.Id = usrID;
          

            // there are cases where the grids redirect to this action, with that if an error message in a view data will be lost, so pass as param and set again here
            if (!string.IsNullOrEmpty(errorMsg))
            {
                ViewData["EditError"] = errorMsg;
                TempData["returnMsg"] = "True";
            }


            if (Session["username"] != null)
            {
                int findId = SearchID(usrID);
                var user = db.Employee_Details.Where(x => x.Id == findId).FirstOrDefault();

                var hrUser = db.HR_Employee.FirstOrDefault(x => x.Id == user.Id);

              

               
                return View(user);
            }
            else
                return RedirectToAction("Index", "Login");
        }

        // UTILITY
        private void GetAllComponentPrivileges()
        {
            ViewBag.AllPrivileges = db.Attributes
                .Where(x => x.Status == (int)Status.Active)
                .OrderBy(o => o.Name).ToList();
        }

       

        #region Contracts TAB

        [ValidateInput(false)]
        public ActionResult GridViewPartialContracts(System.Int32? Id)
        {
            int findId = SearchID(Id);

            //ViewBag.contractTypesList = db.Contract_Types.Where(w => w.Status != 3).ToList();

            var model = db.Employee_Contracts.AsNoTracking().
                Where(
                i => (i.Employee_Status == 1 || i.Employee_Status == 3) &&
                    i.Id == findId &&
                    i.Expr1 != null &&
                    i.Contract_Status != 3
                ).Distinct().
                ToList();

            // Create user handle to find the users key
            var HRuser = db.HR_Employee.FirstOrDefault(x => x.Id == findId);

            // Get Users Key from references


            return PartialView("_GridViewPartialContracts", model);
        }
        public int SearchID(int? toSearchId)
        {
            // On cancel the ID is NOT passed as a paremeter to each method,
            // so we either get the new provided ID by the method or get the last known ID from the session
            int returnId;
            if (toSearchId != null && (int)toSearchId != 0) // for nullable int (int?) and cast null int to int to check against 0
            {
                Session["queryId"] = (int)toSearchId;
                returnId = int.Parse(Session["queryId"].ToString());
            }
            else
            {
                returnId = int.Parse(Session["queryId"].ToString());
            }
            return returnId;
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialContractsAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] WebApplicationUnipi.Models.MyApplicationUnipi.Employee_Contracts item)
        {
            int usersId = SearchID(item.Id); // get the users ID as a foreign key to the new Contract
            if (ModelState.IsValid)
            {
                try
                {
                    HR_Employee_Contract _contract = new HR_Employee_Contract();
                    {
                        _contract.Employee_Id = usersId;
                        _contract.Description = item.Description;
                        _contract.Start_Date = item.Start_Date;
                        _contract.End_Date = item.End_Date;
                        _contract.Daily_Working_Hours = item.Daily_Working_Hours;
                        _contract.Role = item.Role;
                        _contract.Status = item.Contract_Status;
                        db.HR_Employee_Contract.Add(_contract);
                        db.SaveChanges();
                    };
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";

            var model = db.Employee_Contracts.AsNoTracking().Where(i => i.Employee_Status != 3 && i.Contract_Status != 3 && i.Id == usersId).ToList();

            // Create user handle to find the users key
            var HRuser = db.HR_Employee.FirstOrDefault(x => x.Id == usersId);

           
            return PartialView("_GridViewPartialContracts", model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialContractsUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] WebApplicationUnipi.Models.MyApplicationUnipi.Employee_Contracts item)
        {
            int usersId = SearchID(item.Id); // get the users ID as a foreign key to the new Contract
            if (ModelState.IsValid)
            {
                try
                {
                    HR_Employee_Contract _contractitem = db.HR_Employee_Contract.FirstOrDefault(it => it.Id == item.Expr1);
                    if (_contractitem != null)
                    {
                        _contractitem.Description = item.Description;
                        _contractitem.Start_Date = item.Start_Date;
                        _contractitem.End_Date = item.End_Date;
                        _contractitem.Daily_Working_Hours = item.Daily_Working_Hours;
                        _contractitem.Role = item.Role;
                        _contractitem.Status = item.Contract_Status;

                        db.SaveChanges();
                    }
                    else
                    {
                        HR_Employee_Contract newcontract = new HR_Employee_Contract();
                        newcontract.Id = usersId;
                        newcontract.Employee_Id = item.Id;
                        newcontract.Description = item.Description;
                        newcontract.Start_Date = item.Start_Date;
                        newcontract.End_Date = item.End_Date;
                        newcontract.Daily_Working_Hours = item.Daily_Working_Hours;
                        newcontract.Role = item.Role;
                        newcontract.Status = 1;
                        db.HR_Employee_Contract.Add(newcontract);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";

            var model = db.Employee_Contracts.AsNoTracking().Where(i => i.Employee_Status != 3 && i.Contract_Status != 3 && i.Id == usersId).ToList();

           

            return PartialView("_GridViewPartialContracts", model);
        }
        //[HttpPost, ValidateInput(false)]
        //public ActionResult GridViewPartialContractsDelete([ModelBinder(typeof(DevExpressEditorsBinder))] WebApplicationUnipi.Models.MyApplicationUnipi.Employee_Contracts item)
        //{
        //    var model = db.Employee_Contracts.AsNoTracking();
        //    HR_Employee_Contract ToDeleteContruct = db.HR_Employee_Contract.FirstOrDefault(contract => contract.Id == item.Expr1);
        //    if (ToDeleteContruct != null)
        //    {
        //        try
        //        {
        //            var ContractModel = db.HR_Employee_Contract;
        //            ContractModel.Remove(ToDeleteContruct);
        //            db.SaveChanges();
        //        }
        //        catch (Exception e)
        //        {
        //            ViewData["EditError"] = e.Message;
        //        }
        //    }
        //    return PartialView("_GridViewPartialContracts", model.Where(i => i.Employee_Status != 3 && i.Id == item.Id).ToList());
        //}
        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialContractsDelete([ModelBinder(typeof(DevExpressEditorsBinder))] System.Int32? Expr1, System.Int32? Id)
        {
            int usersId = SearchID(Id); // get the users ID as a foreign key to the new Contract
            if (Expr1 >= 0)
            {
                try
                {
                    var item = db.HR_Employee_Contract.Where(it => it.Id == Expr1).FirstOrDefault();
                    if (item != null)
                        item.Status = 3;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }

            var model = db.Employee_Contracts.AsNoTracking().Where(i => i.Employee_Status != 3 && i.Contract_Status != 3 && i.Id == usersId).ToList();
            // Create user handle to find the users key
            var HRuser = db.HR_Employee.FirstOrDefault(x => x.Id == usersId);


            return PartialView("_GridViewPartialContracts", model);
        }

        #endregion

        #region Annual Leaves per Year TAB
        [ValidateInput(false)]
        public ActionResult ApprovedLeavesPerYear(int? Id)
        {
            if (Id == null || Id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            ViewBag.Id = Id;
            return PartialView("_ApprovedLeavesPerYear");


        }

        [ValidateInput(false)]
        public ActionResult ApprovedLeavesGridViewPartial(int? Id, int? year)
        {

            if (Id == null || Id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (year == null || year == 0)
            {
                year = DateTime.UtcNow.Year;
            }

            ViewBag.Id = Id;
            ViewBag.Year = year;

            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                var model = db.LeavesRequests.Where(w => w.Status == (byte)Models.Enums.LeavesRequestStatus.Approved
                                                      && w.EmployeeId == Id
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

                // *** In this case the GridView will show data for the same user, so the available days can be calculated outside the Select below (saves time/resources (maybe?))
                //var availDays = MyProfileController.GetAvailableDays(Id);

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
                    //Available = MyProfileController.GetAvailableDays(Id, (byte)s.TypeOfLeave) // ***
                }).OrderByDescending(o => o.DateCreated).ToList();

                return PartialView("_ApprovedLeavesGridViewPartial", c);
            }
        }

        //public ActionResult ApprovedLeavesGridViewPartialAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] WebApplicationUnipi.Models.MyApplicationUnipi.Employee_Contracts item , int? Id)
        //{
        //    if (Id == null || Id <= 0)
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        //    ViewBag.Id = Id;

        //    using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
        //    {
        //        var model = db.LeavesRequests.Where(w => w.Status == (byte)Models.Enums.LeavesRequestStatus.Approved
        //                                              && w.EmployeeId == Id
        //                                              && (w.DateFrom.Value.Year == DateTime.UtcNow.Year || w.DateTo.Value.Year == DateTime.UtcNow.Year))
        //                                     .Include(hr => hr.HR_Employee).ToList();

        //        // CONVERT DATETIMES TO GR TIME !!
        //        var GreeceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
        //        model.ForEach(f =>
        //        {
        //            f.DateCreated = TimeZoneInfo.ConvertTimeFromUtc(f.DateCreated.Value, GreeceTimeZone);
        //        });
        //        UpdateModel(model);

        //        // *** In this case the GridView will show data for the same user, so the availabe days can be calculated outside the Select below (saves time/resources (maybe?))
        //        var availDays = MyProfileController.GetAvailableDays(Id);

        //        var c = model.Select(s => new
        //        {
        //            s.Id,
        //            Employee_Fname = s.HR_Employee.FetchEmployeeName().Split(' ')[0],
        //            Employee_Lname = s.HR_Employee.FetchEmployeeName().Split(' ')[1],
        //            s.DateCreated,
        //            s.DateFrom,
        //            s.DateTo,
        //            s.TypeOfLeave,
        //            s.NumOfDays,
        //            s.Comments,
        //            s.Status,
        //            Available = availDays // ***
        //        }).OrderByDescending(o => o.DateCreated).ToList();

        //        return PartialView("_ApprovedLeavesGridViewPartial", c);
        //    }
        //}




        //[HttpPost, ValidateInput(false)]
        //public ActionResult ApprovedLeavesGridViewPartialUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] WebApplicationUnipi.Models.MyApplicationUnipi.Employee_Contracts item)
        //{
        //    int usersId = SearchID(item.Id); // get the users ID as a foreign key to the new Contract
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            HR_Employee_Contract _contractitem = db.HR_Employee_Contract.FirstOrDefault(it => it.Id == item.Expr1);
        //            if (_contractitem != null)
        //            {
        //                _contractitem.Description = item.Description;
        //                _contractitem.Start_Date = item.Start_Date;
        //                _contractitem.End_Date = item.End_Date;
        //                _contractitem.Daily_Working_Hours = item.Daily_Working_Hours;
        //                _contractitem.Role = item.Role;
        //                _contractitem.Status = item.Contract_Status;

        //                db.SaveChanges();
        //            }
        //            else
        //            {
        //                HR_Employee_Contract newcontract = new HR_Employee_Contract();
        //                newcontract.Id = usersId;
        //                newcontract.Employee_Id = item.Id;
        //                newcontract.Description = item.Description;
        //                newcontract.Start_Date = item.Start_Date;
        //                newcontract.End_Date = item.End_Date;
        //                newcontract.Daily_Working_Hours = item.Daily_Working_Hours;
        //                newcontract.Role = item.Role;
        //                newcontract.Status = 1;
        //                db.HR_Employee_Contract.Add(newcontract);
        //                db.SaveChanges();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            ViewData["EditError"] = e.Message;
        //        }
        //    }
        //    else
        //        ViewData["EditError"] = "Please, correct all errors.";

        //    var model = db.Employee_Contracts.AsNoTracking().Where(i => i.Employee_Status != 3 && i.Contract_Status != 3 && i.Id == usersId).ToList();

        //    // Create user handle to find the users key
        //    var HRuser = db.HR_Employee.FirstOrDefault(x => x.Id == usersId);

        //    // Get Users Key from references
        //    var userHandle = CryptoServices.getUserHandle(HRuser.Username, Convert.FromBase64String(HRuser.Salt));

        //    var usrKey = db.UserReferences.FirstOrDefault(x => x.UserReference == userHandle);

        //    // Decrypt the needed users info for this partial view
        //    model = CryptoServices.genericDecryptor(
        //        usrs: model,
        //        keys: new List<UserReferences>() { usrKey },
        //        wantedProps: new List<string> { nameof(HR_Employee.Employee_Fname), nameof(HR_Employee.Employee_Lname), }
        //    );

        //    return PartialView("_ApprovedLeavesGridViewPartial", model);
        //}

        [ValidateInput(false)]
        public ActionResult ApprovedLeavesPerEmployeeCancel(int Id, int? year)
        {
            if (Id > 0)
            {
                if (year == null)
                {
                    year = DateTime.UtcNow.Year;
                }

                ViewBag.Year = year;
                ViewBag.Id = Id;

                using (var db = new MyApplicationUnipiEntities())
                {
                    var reqToDelete = db.LeavesRequests.SingleOrDefault(s => s.Id == Id);

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

                    var model = db.LeavesRequests.Where(w => w.Status == (byte)Models.Enums.LeavesRequestStatus.Approved
                                                      && w.EmployeeId == reqToDelete.EmployeeId
                                                      && (w.DateFrom.Value.Year == year || w.DateTo.Value.Year == year))
                                             .Include(hr => hr.HR_Employee).ToList();

                    // CONVERT DATETIMES TO GR TIME !!
                    var GreeceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
                    model.ForEach(f =>
                    {
                        f.DateCreated = TimeZoneInfo.ConvertTimeFromUtc(f.DateCreated.Value, GreeceTimeZone);
                    });
                    UpdateModel(model);

                    // *** In this case the GridView will show data for the same user, so the availabe days can be calculated outside the Select below (saves time/resources (maybe?))
                    //var availDays = MyProfileController.GetAvailableDays(Id);

                    var c = model.Select(s => new
                    {
                        s.Id,
                        Employee_Fname = s.HR_Employee.Employee_Fname.Split(' ')[1],
                        Employee_Lname = s.HR_Employee.Employee_Lname.Split(' ')[0],
                        s.DateCreated,
                        s.DateFrom,
                        s.DateTo,
                        s.TypeOfLeave,
                        s.AmountRequested,
                        s.Comments,
                        s.Status,
                        Available = MyProfileController.GetAvailableDays(Id, (byte)s.TypeOfLeave) // ***
                    }).OrderByDescending(o => o.DateCreated).ToList();

                    return PartialView("_ApprovedLeavesGridViewPartial", c);
                }
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        #endregion



        #region Personal Details tab

        public ActionResult FetchPersonalInfo(int Id)
        {
            int findId = SearchID(Id);
            using (var db = new MyApplicationUnipiEntities())
            {
                var emplModel = db.HR_Employee.FirstOrDefault(i => i.Id == findId);



                return PartialView("_FetchPersonalInfo", emplModel);
            }
        }


        [HttpPost]
        public ActionResult SavePersonalInfo(HR_Employee empl)
        {
            var usrId = empl.Id;
            if (empl != null && usrId > 0 && ModelState.IsValid)
            {
                using (var db = new MyApplicationUnipiEntities())
                {
                    var usrModel = db.HR_Employee.SingleOrDefault(s => s.Id == usrId);



                    usrModel.Personal_Email = empl.Personal_Email;
                    usrModel.Home_Address = empl.Home_Address;
                    usrModel.Personal_Mobile_Number = empl.Personal_Mobile_Number;
                    usrModel.Birth_Date = empl.Birth_Date;
                    usrModel.Transportation = empl.Transportation;
                    db.SaveChanges();



                    return Json(empl);
                }
            }
            else
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        }

        /*[ValidateInput(false)]
        //public ActionResult CardViewPartialMoreDetails(System.Int32? Id)
        //{
        //    int findId = SearchID(Id);
        //    var model = db.HR_Employee.FirstOrDefault(i => i.Id == findId);

        //    return PartialView("_CardViewPartialMoreDetails", new List<HR_Employee>() { model.FetchClearObject() });
        }*/

        //[HttpPost, ValidateInput(false)]
        //public ActionResult CardViewPartialMoreDetailsUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] WebApplicationUnipi.Models.MyApplicationUnipi.HR_Employee item)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            HR_Employee ditemp = db.HR_Employee.FirstOrDefault(it => it.Id == item.Id);

        //            var userHandle = CryptoServices.getUserHandle(ditemp.Username, Convert.FromBase64String(ditemp.Salt));

        //            // Get Users Key from references
        //            var usrKey = db.UserReferences.FirstOrDefault(x => x.UserReference == userHandle).Hash;

        //            if (ditemp != null)
        //            {
        //                // Decrypt database values and compare the values, if different change them
        //                if (string.IsNullOrEmpty(ditemp.Personal_Email) || CryptoServices.decryptSingleUserData(ditemp.Personal_Email, Convert.FromBase64String(usrKey)) != item.Personal_Email)
        //                    ditemp.Personal_Email = CryptoServices.encyptSingleUserData(item.Personal_Email, Convert.FromBase64String(usrKey));

        //                if (string.IsNullOrEmpty(ditemp.Home_Address) || CryptoServices.decryptSingleUserData(ditemp.Home_Address, Convert.FromBase64String(usrKey)) != item.Home_Address)
        //                    ditemp.Home_Address = CryptoServices.encyptSingleUserData(item.Home_Address, Convert.FromBase64String(usrKey));

        //                if (string.IsNullOrEmpty(ditemp.Personal_Mobile_Number) || CryptoServices.decryptSingleUserData(ditemp.Personal_Mobile_Number, Convert.FromBase64String(usrKey)) != item.Personal_Mobile_Number)
        //                    ditemp.Personal_Mobile_Number = CryptoServices.encyptSingleUserData(item.Personal_Mobile_Number, Convert.FromBase64String(usrKey));

        //                if (string.IsNullOrEmpty(ditemp.Transportation) || CryptoServices.decryptSingleUserData(ditemp.Transportation, Convert.FromBase64String(usrKey)) != item.Transportation)
        //                    ditemp.Transportation = CryptoServices.encyptSingleUserData(item.Transportation, Convert.FromBase64String(usrKey));

        //                if (!string.IsNullOrEmpty(item.Birth_Date))
        //                {
        //                    // below 3 lines parse the date 
        //                    if (DateTime.TryParse(item.Birth_Date, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out var dateResult))
        //                    {                         
        //                        dateResult = new DateTime(dateResult.Year, dateResult.Month, dateResult.Day);
        //                        if (string.IsNullOrEmpty(ditemp.Birth_Date))
        //                        {
        //                            ditemp.Birth_Date = CryptoServices.encyptSingleUserData(dateResult.ToString("dd/MM/yyyy"), Convert.FromBase64String(usrKey));
        //                        }
        //                        else
        //                        {
        //                            ditemp.Birth_Date = CryptoServices.encyptSingleUserData(dateResult.ToString("dd/MM/yyyy"), Convert.FromBase64String(usrKey));
        //                        }
        //                        item.Birth_Date = dateResult.ToString("dd/MM/yyyy");
        //                    }

        //                }
        //                db.SaveChanges();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            ViewData["EditError"] = e.Message;
        //        }
        //    }
        //    else
        //        ViewData["EditError"] = "Please, correct all errors.";
        //    return PartialView("_CardViewPartialMoreDetails", new List<HR_Employee>() { item});
        //}
        /*[HttpPost, ValidateInput(false)]
        public ActionResult CardViewPartialMoreDetailsDelete(System.Int32 Id)
        {
            var model = db.All_Employees_View;
            if (Id >= 0)
            {
                try
                {
                    var item = model.FirstOrDefault(it => it.Id == Id);
                    if (item != null)
                        model.Remove(item);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("_CardViewPartialMoreDetails", model.ToList());
        }*/

        #endregion

        #region Annual Leaves per Contract TAB

        // Annual Leaves Tab  //
        [ValidateInput(false)]
        public ActionResult GridViewPartialAnnualLeaves(System.Int32? Id)
        {
            int findId = SearchID(Id);
            var modelDb = db.AnnualLeavesContractsView.AsNoTracking().Where(w => w.Employee_Id == findId && w.Status != 3).OrderBy(o => o.Year).ToList();
            return PartialView("_GridViewPartialAnnualLeaves", GetModel(modelDb));
        }

        public List<CustomLeaves> GetModel(List<AnnualLeavesContractsView> modelFromDb)
        {
            var model = new List<CustomLeaves> { };
            modelFromDb.ForEach(f =>
            {
                var item = new CustomLeaves(f);
                model.Add(item);
            });
            return model;
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialAnnualLeavesAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] CustomLeaves item)
        {
            int uId = int.Parse(Session["queryId"].ToString());
            int userId = SearchID(uId);
            var model = db.AnnualLeavesContractsView.AsNoTracking();

            if (ModelState.IsValid)
            {
                try
                {
                    int infoToId;
                    bool isInt = int.TryParse(item.contractInfo, out infoToId);
                    //int infoToId = Convert.ToInt32(item.contractInfo);
                    HR_Employee_Contract contract = db.HR_Employee_Contract.Where(w => w.Employee_Id == userId && w.Id == infoToId).SingleOrDefault();
                    bool akyro = db.AnnualLeaves.Any(w => w.ContractId == infoToId && w.Year == item.Year && w.LeaveType == item.LeaveType && w.Status != 3);
                    if (contract != null)
                    {
                        if (!akyro)
                        {
                            AnnualLeaves leaveObj = new AnnualLeaves();
                            {
                                leaveObj.HR_Employee_Contract = contract;
                                leaveObj.Year = item.Year;
                                leaveObj.AnnualSum = item.AnnualSum;
                                leaveObj.Available = item.Available;
                                leaveObj.LeaveType = item.LeaveType;
                                leaveObj.Status = 1;
                                db.AnnualLeaves.Add(leaveObj);
                                db.SaveChanges();
                            };
                        }
                        else
                            ViewData["EditError"] = "Entries for this contract and specific Year already exist. Please use the Edit button to edit an item.";
                    }
                    else
                        ViewData["EditError"] = "Contract item returned null.";
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";

            return PartialView("_GridViewPartialAnnualLeaves", GetModel(model.Where(w => w.Employee_Id == userId && w.Status != 3).OrderBy(o => o.Year).ToList()));
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialAnnualLeavesUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] CustomLeaves item)
        {
            int uId = int.Parse(Session["queryId"].ToString());
            int userId = SearchID(uId);
            var model = db.AnnualLeavesContractsView.AsNoTracking();

            if (ModelState.IsValid)
            {
                try
                {
                    int infoToId;
                    bool isInt = int.TryParse(item.contractInfo, out infoToId);
                    //int infoToId = Convert.ToInt32(item.contractInfo);                    
                    AnnualLeaves leavesItem = db.AnnualLeaves.FirstOrDefault(f => f.Id == item.AnnualLeavesId);

                    if (leavesItem != null)
                    {
                        if (isInt)
                        {
                            HR_Employee_Contract contract = db.HR_Employee_Contract.Where(w => w.Employee_Id == userId && w.Id == infoToId).SingleOrDefault();
                            leavesItem.HR_Employee_Contract = contract;
                        }
                        leavesItem.AnnualSum = item.AnnualSum;
                        leavesItem.LeaveType = item.LeaveType;
                        leavesItem.Available = item.Available;
                        leavesItem.Year = item.Year;
                        db.SaveChanges();
                    }
                    else
                        ViewData["EditError"] = "Null item Id";
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";

            return PartialView("_GridViewPartialAnnualLeaves", GetModel(model.Where(w => w.Employee_Id == userId && w.Status != 3).OrderBy(o => o.Year).ToList()));
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialAnnualLeavesDelete([ModelBinder(typeof(DevExpressEditorsBinder))] System.Int32? AnnualLeavesId)
        {
            int uId = int.Parse(Session["queryId"].ToString());
            int userId = SearchID(uId);
            var model = db.AnnualLeavesContractsView.AsNoTracking();

            if (AnnualLeavesId >= 0)
            {
                try
                {
                    var item = db.AnnualLeaves.Where(w => w.Id == AnnualLeavesId).SingleOrDefault();
                    if (item != null)
                        item.Status = 3;  // ENUM ??
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("_GridViewPartialAnnualLeaves", GetModel(model.Where(w => w.Employee_Id == userId && w.Status != 3).OrderBy(o => o.Year).ToList()));
        }

        #endregion



     
        // Dropdown "More Details" Request Responses
        [ValidateInput(false)]
        public ActionResult LeavesRequestsMoreDetails(int? Id)
        {
            // Get all the user references


            if (string.IsNullOrEmpty((string)Session["Username"]))
                return RedirectToAction("Index", "Login");

            if (Id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
                var model = db.LeavesRequestsResponse.Where(w => w.LeavesReqId == Id && w.Status != (byte)Models.Enums.LeavesRequestStatus.Deleted).Include(hr => hr.HR_Employee).ToList();

                // CONVERT DATETIMES TO GR TIME !!
                var GreeceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
                model.ForEach(f =>
                {
                    f.DateOfResponse = TimeZoneInfo.ConvertTimeFromUtc(f.DateOfResponse.Value, GreeceTimeZone);
                });
                UpdateModel(model);



                var cryptonite = model.Select(s => s.HR_Employee).ToList();


                var c = model.Select(s => new
                {
                    s.Id,
                    s.LeavesReqId,
                    s.EmployeeId,
                    FullName = cryptonite.Where(w => w.Id == s.EmployeeId).Select(o => o.Employee_Fname).FirstOrDefault() + " " + cryptonite.Where(w => w.Id == s.EmployeeId).Select(o => o.Employee_Lname).FirstOrDefault(),
                    s.DateOfResponse,
                    s.Status
                });
                ViewBag.Id = Id;

                return PartialView("_ApprovedAnnualLeavesMoreDetails", c.OrderByDescending(o => o.DateOfResponse).ToList());
            }
        }

       
        public ActionResult LeavesPerEmployee()
        {
            int usrId = (int)Session["UserId"];

            using (var db = new WebApplicationUnipi.Models.MyApplicationUnipi.MyApplicationUnipiEntities())
            {
             
                var users = db.HR_Employee.Where(w => w.Status == 1).ToList();

                ViewData["empList"] = users.Select(x => new SelectListItem
                {
                    Text = x.Employee_Lname + " | " + x.HR_Position.Position_Name,
                    Value = x.Id.ToString(),
                    //Selected = (selectedUser != null || selectedUser != 0) && selectedUser == x.Id ? true : false
                }).ToList();
            }

            return PartialView("_AnnualLeavesPerEmployee");
        }


       



    }
}