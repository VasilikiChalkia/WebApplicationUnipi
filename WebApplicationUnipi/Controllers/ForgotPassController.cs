using WebApplicationUnipi.Models.MyApplicationUnipi;
using System.Web.Mvc;
using System.Linq;
using WebApplicationUnipi.Services.Factories;
using System;
using System.Data.Entity;


namespace WebApplicationUnipi.Controllers
{
    public class ForgotPassController : Controller
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        MyApplicationUnipiEntities ctx = new MyApplicationUnipiEntities();

        // GET: ForgotPass
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        [HttpPost]
        public ActionResult SubmitValuesForgot(string Employee_Fname, string Employee_Lname, string Birth_Date, string Personal_Email, string Personal_Mobile_Number)
        {
            try
            {
                // Find employee matching all entered info
                var userData = ctx.HR_Employee
                    .Where(x => x.Status == 1 &&
                                x.Employee_Fname == Employee_Fname &&
                                x.Employee_Lname == Employee_Lname &&
                                x.Birth_Date == Birth_Date &&
                                x.Personal_Mobile_Number == Personal_Mobile_Number)
                    .FirstOrDefault();

                if (userData != null)
                {
                    MyLogger.getInstance().Info($"User {Employee_Fname} {Employee_Lname} verified for password reset.");

                    // Store user email in session for the reset step
                    Session["user"] = userData.Personal_Email;

                    // Redirect to ResetPassword page
                    return RedirectToAction("ResetPassword", new { user = userData.Personal_Email });
                }
                else
                {
                    ViewData["Message"] = "Details do not match our records. Please try again.";
                    return View("ForgotPassword");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error verifying user details for password reset.");
                ViewData["Message"] = "An error occurred. Please try again later.";
                return View("ForgotPassword");
            }
        }


        public ActionResult ResetPassword(string user)
        {
            var userID = user;
         
            Session["user"] = user;

            if (userID != null)
            {
                
                    return View("ResetPassword");
                             
            }
            else
            {
                ViewData["Message"] = "Failed to reset.";
                return View("ErrorPage");
            }
        }


        public ActionResult submitNewPass(string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != null && ConfirmPassword != null && NewPassword != "" && ConfirmPassword != "")
            {
             
                var userID = (string)HttpContext.Session["user"];
                var resetmodel = ctx.HR_Employee.Where(x => x.Status == 1 && x.Personal_Email == userID).FirstOrDefault();


                if (resetmodel != null )
                {
                    if (NewPassword == ConfirmPassword)
                    {
                        resetmodel.Password = NewPassword;
                        
                        ctx.HR_Employee.Attach(resetmodel);
                        ctx.Entry(resetmodel).State = EntityState.Modified;
                        ctx.SaveChanges();

                        TempData["Message"] = "Your password changed successfully!";
                        return RedirectToAction("Index", "Login");
                    }
                    else
                        ViewData["Message"] = "New Password and Confirmation don't match! Try again.";
                    return View("ResetPassword");
                }
            }
            ViewData["Message"] = "Something went wrong!";
            return View("ResetPassword");
        }


    }
}

