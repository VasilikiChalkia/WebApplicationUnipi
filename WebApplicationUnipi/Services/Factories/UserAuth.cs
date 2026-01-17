using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplicationUnipi.Services.Factories
{
    public static class UserAuth
    {
        public static void Check(HttpSessionStateBase session, ActionExecutingContext filterContext, string controllerName)
        {
            if (session["Username"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "Index" }));
                return;
            }
            if (session["ControllerName"] != null)
            {
                if (session["ControllerName"].ToString() == controllerName && (bool)session["HasAuth"])
                    return;
            }
            PrimeListCheck(session, filterContext, controllerName);
            return;
        }

        private static void PrimeListCheck(HttpSessionStateBase session, ActionExecutingContext filterContext, string controllerName)
        {
            var PrimeList = (List<int>)session["UserRights"];
            var homeItems = DirGet.LoadJson();
            if (homeItems != null)
            {
                var thisController = homeItems.Where(w => w.controller == controllerName).SingleOrDefault();                
                if (thisController != null)
                {
                    var controllerRights = thisController.rights;
                    if (controllerRights.Count == 0)
                    {
                        session["HasAuth"] = true;
                        session["ControllerName"] = controllerName;
                        return;
                    }

                    //Check if at least one ControllerPrimeNumber exists in PrimeList
                    var atLeast1CNumIncluded = PrimeList.Any(a => controllerRights.Contains(a));

                    if (!atLeast1CNumIncluded)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "Unauthorized" }));
                        return;
                    }
                    else
                    {
                        session["HasAuth"] = true;
                        session["ControllerName"] = controllerName;
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "Error" }));
            return; 
        }
    }
}