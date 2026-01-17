using DevExpress.Web.Mvc;
using WebApplicationUnipi.Models.Enums;
using WebApplicationUnipi.Models.MyApplicationUnipi;
using WebApplicationUnipi.Services.Factories;
using System;
using System.Linq;
using System.Web.Mvc;


namespace WebApplicationUnipi.Controllers
{
    public class EntryButtonsController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            UserAuth.Check(Session, filterContext, controllerName);
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // GET: EntryButtons
        public ActionResult Index()
        {
            return View();
        }

        MyApplicationUnipiEntities db = new MyApplicationUnipiEntities();

        #region Position Tab

        [ValidateInput(false)]
        public ActionResult GridViewPartial()
        {
            var model = db.HR_Position;

            return PartialView("_GridViewPartial", model.Where(i => i.Status != 3 && i.Type == 1).OrderBy(x => x.Position_Name).ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] HR_Position item)
        {
            var model = db.HR_Position;
            if (ModelState.IsValid)
            {
                try
                {
                    HR_Position mdPosition = new HR_Position();
                    {
                        mdPosition.Id = item.Id;
                        mdPosition.Position_Name = item.Position_Name;
                        mdPosition.Type = 1;
                        mdPosition.Status = 1;
                        db.HR_Position.Add(mdPosition);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return PartialView("_GridViewPartial", model.Where(i => i.Status != 3 && i.Type == 1).ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] HR_Position item)
        {
            var model = db.HR_Position;
            if (ModelState.IsValid)
            {
                try
                {
                    HR_Position mdPosition = db.HR_Position.FirstOrDefault(it => it.Id == item.Id);
                    if (mdPosition != null)
                    {
                        mdPosition.Position_Name = item.Position_Name;
                        mdPosition.Type = 1;
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
            return PartialView("_GridViewPartial", model.Where(i => i.Status != 3 && i.Type == 1).ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialDelete(System.Int32 Id)
        {
            var model = db.HR_Position;
            if (Id >= 0)
            {
                try
                {
                    var item = db.HR_Position.Where(it => it.Id == Id).FirstOrDefault();
                    if (item != null)
                        item.Status = 3;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("_GridViewPartial", model.Where(i => i.Status != 3 && i.Type == 1).ToList());
        }

        #endregion

        #region Department Tab

        [ValidateInput(false)]
        public ActionResult GridView1Partial()
        {
            if (Session["username"] != null)
            {
                var model = db.HR_Department;
            return PartialView("_GridView1Partial", model.Where(i => i.Status != 3).OrderBy(x => x.Department_Name).ToList());
            }
            else
                return RedirectToAction("Index", "Login");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridView1PartialAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] HR_Department item)
        {
            var model = db.HR_Department;
            if (ModelState.IsValid)
            {
                try
                {
                    HR_Department mdDepartment = new HR_Department();
                    {
                        mdDepartment.Id = item.Id;
                        mdDepartment.Department_Name = item.Department_Name;
                        mdDepartment.Status = 1;
                        db.HR_Department.Add(mdDepartment);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return PartialView("_GridView1Partial", model.Where(i => i.Status != 3).ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridView1PartialUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] HR_Department item)
        {
            var model = db.HR_Department;
            if (ModelState.IsValid)
            {
                try
                {
                    HR_Department mdDepartment = db.HR_Department.FirstOrDefault(it => it.Id == item.Id);
                    if (mdDepartment != null)
                    {
                        mdDepartment.Department_Name = item.Department_Name;
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
            return PartialView("_GridView1Partial", model.Where(i => i.Status != 3).ToList());
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult GridView1PartialDelete(System.Int32 Id)
        {
            var model = db.HR_Department;
            if (Id >= 0)
            {
                try
                {
                    var item = db.HR_Department.Where(it => it.Id == Id).FirstOrDefault();
                    if (item != null)
                        item.Status = 3;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("_GridView1Partial", model.Where(i => i.Status != 3).ToList());
        }

        #endregion

     

    

        #region DaysOff Tab

        [ValidateInput(false)]
        public ActionResult DaysOffGridViewPartial()
        {
            var model = db.Daysoff;

            return PartialView("_DaysOffGridViewPartial", model.Where(i => i.Status != 3).OrderByDescending(o => o.Date).ToList());
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult DaysOffGridViewPartialAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] Daysoff item)
        {
            var model = db.Daysoff;
            if (ModelState.IsValid)
            {
                try
                {
                    Daysoff mdBuilding = new Daysoff();
                    {
                        mdBuilding.Id = item.Id;
                        mdBuilding.Date = item.Date;
                        mdBuilding.Desc = item.Desc;
                        mdBuilding.Status = 1;
                        db.Daysoff.Add(mdBuilding);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return PartialView("_DaysOffGridViewPartial", model.Where(i => i.Status != 3).OrderByDescending(o => o.Date).ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult DaysOffGridViewPartialUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Daysoff item)
        {
            var model = db.Daysoff;
            if (ModelState.IsValid)
            {
                try
                {
                    Daysoff mdBuilding = db.Daysoff.FirstOrDefault(it => it.Id == item.Id);
                    if (mdBuilding != null)
                    {
                        mdBuilding.Date = item.Date;
                        mdBuilding.Desc = item.Desc;
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
            return PartialView("_DaysOffGridViewPartial", model.Where(i => i.Status != 3).OrderByDescending(o => o.Date).ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult DaysOffGridViewPartialDelete([ModelBinder(typeof(DevExpressEditorsBinder))] System.Int32? Id)
        {
            var model = db.Daysoff;
            if (Id >= 0)
            {
                try
                {
                    var item = db.Daysoff.Where(it => it.Id == Id).FirstOrDefault();
                    if (item != null)
                        item.Status = 3;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("_DaysOffGridViewPartial", model.Where(i => i.Status != 3).OrderByDescending(o => o.Date).ToList());
        }

        #endregion

        #region Specialties Tab

        [ValidateInput(false)]
        public ActionResult SpecialtiesGridViewPartial()
        {
            var model = db.HR_Position;

                return PartialView("_SpecialtiesGridViewPartial", model.Where(i => i.Status != 3 && i.Type == 2).OrderBy(x => x.Position_Name).ToList());
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult SpecialtiesGridViewPartialAddNew([ModelBinder(typeof(DevExpressEditorsBinder))] HR_Position item)
        {
            var model = db.HR_Position;
            if (ModelState.IsValid)
            {
                try
                {
                    HR_Position mdPosition = new HR_Position();
                    {
                        mdPosition.Id = item.Id;
                        mdPosition.Position_Name = item.Position_Name;
                        mdPosition.Status = 1;
                        mdPosition.Type = 2;
                        db.HR_Position.Add(mdPosition);
                    }
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";
            return PartialView("_SpecialtiesGridViewPartial", model.Where(i => i.Status != 3 && i.Type == 2).ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult SpecialtiesGridViewPartialUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] HR_Position item)
        {
            var model = db.HR_Position;
            if (ModelState.IsValid)
            {
                try
                {
                    HR_Position mdPosition = db.HR_Position.FirstOrDefault(it => it.Id == item.Id);
                    if (mdPosition != null)
                    {
                        mdPosition.Position_Name = item.Position_Name;
                        mdPosition.Type = 2;
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
            return PartialView("_SpecialtiesGridViewPartial", model.Where(i => i.Status != 3 && i.Type == 2).ToList());
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult SpecialtiesGridViewPartialDelete(System.Int32 Id)
        {
            var model = db.HR_Position;
            if (Id >= 0)
            {
                try
                {
                    var item = db.HR_Position.Where(it => it.Id == Id).FirstOrDefault();
                    if (item != null)
                        item.Status = 3;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return PartialView("_SpecialtiesGridViewPartial", model.Where(i => i.Status != 3 && i.Type == 2).ToList());
        }

        #endregion

        #region Roles Tab

        [ValidateInput(false)]
        public ActionResult RolesGridViewPartial()
        {
            var model = db.Attributes;

            return PartialView("_RolesGridViewPartial", model.Where(i => i.Status != (int)Status.InActive).OrderBy(x => x.Name).ToList());
        }       

        [ValidateInput(false)]
        public ActionResult RolesGridViewPartialUpdate([ModelBinder(typeof(DevExpressEditorsBinder))] Attributes item)
        {
            var model = db.Attributes;

            if (ModelState.IsValid)
            {
                try
                {
                    Attributes attributes = db.Attributes.FirstOrDefault(it => it.Id == item.Id);
                    if (attributes != null)
                    {
                        attributes.Name = item.Name;
                        attributes.Description = item.Description;

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
            return PartialView("_RolesGridViewPartial", model.Where(i => i.Status != (int)Status.InActive).ToList());
        }

        #endregion

      
    }
}