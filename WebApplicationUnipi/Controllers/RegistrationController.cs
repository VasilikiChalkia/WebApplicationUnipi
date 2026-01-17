using NLog;
using System;
using System.Linq;
using System.Web.Mvc;
using WebApplicationUnipi.Models.Enums;
using WebApplicationUnipi.Models.MyApplicationUnipi;
using WebApplicationUnipi.Services.Factories;

namespace WebApplicationUnipi.Controllers
{
    public class RegistrationController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private MyApplicationUnipiEntities ctx = new MyApplicationUnipiEntities();

        // GET: Registration/UserRegister
        public ActionResult UserRegister()
        {
           
            var departments = ctx.HR_Department
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Department_Name
                })
                .ToList();

            var positions = ctx.HR_Position
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Position_Name
                })
                .ToList();

            var privileges = Enum.GetValues(typeof(Privileges))
         .Cast<Privileges>()
         .Select(p => new SelectListItem
         {
             Value = ((int)p).ToString(),
             Text = p.GetDisplayAttributeFrom()  
         })
         .ToList();


           
            ViewData["Departments"] = departments;
            ViewData["Positions"] = positions;
            ViewData["Privilege"] = privileges;

            return View();
        }

        // POST: Registration/SubmitRegistration
        [HttpPost]
        public ActionResult SubmitRegistration(
            string FirstName, string LastName,  string Password, string ConfirmPassword,
            int? Department_Id, int? Position_Id, string Privileges_Num, string Personal_Email,
            string Home_Address, string Personal_Mobile_Number, string Birth_Date, string Transportation)
        {

            // Repopulate dropdowns for the view
            var departments = ctx.HR_Department
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Department_Name
                })
                .ToList();

            var positions = ctx.HR_Position
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Position_Name
                })
                .ToList();

            var privileges = ctx.Attributes
               .Select(p => new SelectListItem
               {
                   Value = p.Id.ToString(),
                   Text = p.Name
               })
               .ToList();

            ViewData["Departments"] = departments;
            ViewData["Positions"] = positions;
            ViewData["Privilege"] = privileges;

            // Validate required fields
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) ||
                string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(ConfirmPassword))
            {
                ViewData["Message"] = "All fields are required.";
                return View("UserRegister");
            }

            // Validate password match
            if (Password != ConfirmPassword)
            {
                ViewData["Message"] = "Passwords do not match.";
                return View("UserRegister");
            }

            // Check if email is already registered
            var existingUser = ctx.HR_Employee.FirstOrDefault(x => x.Personal_Email == Personal_Email);
            if (existingUser != null)
            {
                ViewData["Message"] = "Email already registered.";
                return View("UserRegister");
            }

            try
            {
                
                var newUser = new HR_Employee
                {
                    Employee_Fname = FirstName,
                    Employee_Lname = LastName,
                    Personal_Email = Personal_Email,
                    Password = Password,
                    Department_Id = Department_Id,
                    Position_Id = Position_Id,
                    Privileges_Num = Privileges_Num,
                    Home_Address = Home_Address,
                    Personal_Mobile_Number = Personal_Mobile_Number,
                    Birth_Date = Birth_Date,
                    Transportation = Transportation,
                    Status = 1
                };

               
                ctx.HR_Employee.Add(newUser);
                ctx.SaveChanges();

                var privilegeValue = new Attributes_Values
                {
                    User_Id = newUser.Id,
                    Attribute_Id = int.Parse(Privileges_Num), 
                    Status = 1            
                };

                ctx.Attributes_Values.Add(privilegeValue);
                ctx.SaveChanges();

               
                TempData["Success"] = "Registration successful. You may now log in.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception e)
            {
              
                Logger.Error(e.Message);
                ViewData["Message"] = "An error occurred. Please try again later.";

              

                return View("UserRegister");
            }
        }
    }
}