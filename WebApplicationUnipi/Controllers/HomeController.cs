using System.Web.Mvc;


namespace WebApplicationUnipi.Controllers
{
  

    public class HomeController : Controller
    {

        [Route("Error")]
        public ActionResult Error()
        {
            return View();
        }

        [Route("Maintenance")]
        public ActionResult Maintenance()
        {
            return View();
        }


        [Route("NoAccess")]
        public ActionResult NoAccess()
        {
            return View();
        }

        [Route("Unauthorized")]

        public ActionResult Unauthorized()
        {
            return new HttpUnauthorizedResult();
        }

        public ActionResult Index()
        {
            //using (var db = new MyApplicationUnipiEntities())
            //{
            //    var latestUpdate = db.Updates.OrderByDescending(u => u.ReleaseDate).FirstOrDefault();
            //    if (latestUpdate != null && latestUpdate.ReleaseDate >= DateTime.UtcNow.AddDays(-1))
            //    {
            //        ViewBag.LatestUpdate = latestUpdate;
            //    }
            //}
            if (Session["username"] != null)
                return View();
            else
                return RedirectToAction("Index", "Login");
        }



        //[Route("Updates")]
        //public ActionResult Updates()
        //{
            
        //    var updates = UpdatePage(); 
        //    return View(updates);
        //}

        //private List<UpdateModel> UpdatePage()
        //{
            
        //    using (var db = new MyApplicationUnipiEntities())
        //    {
        //        var updates = db.Updates.ToList();
        //        var updateModels = updates.Select(u => new UpdateModel
        //        {
        //            Id = u.Id,
        //            Version = u.Version,
        //            Title = u.Title, 
        //            Description = u.Description,
        //            Type = u.Type,
        //            ReleaseDate = (DateTime)u.ReleaseDate
        //        }).ToList();
        //        return updateModels;

        //    }
        //}


        public ActionResult DownloadFile(string resFullName, string fileName)
        {
            System.IO.File.Move(resFullName, resFullName.Replace(fileName, "~" + fileName));
            resFullName = resFullName.Replace(fileName, "~" + fileName);
            return File(System.IO.File.ReadAllBytes(resFullName), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}