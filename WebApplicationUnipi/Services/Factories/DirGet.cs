using WebApplicationUnipi.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace WebApplicationUnipi.Services.Factories
{
    public class DirGet
    {
       

        public static List<HomeItems> LoadJson()
        {
            //try
            //{
            //    var jsonFile = File.ReadAllText(HostingEnvironment.ApplicationPhysicalPath + "/Content/js/homeList.json");
            //    var response = JsonConvert.DeserializeObject<ResponseJson>(jsonFile);

            //    return response.home.items.ToList();
            //}
            //catch (Exception ex)
            //{
            //    return null;
            //}

            using (StreamReader r = new StreamReader(HostingEnvironment.ApplicationPhysicalPath + "/Content/js/homeList.json"))
            {
                string json = r.ReadToEnd();
                var items = JsonConvert.DeserializeObject<ResponseJson>(json);
                return items.home;
            }
        }
    }
    
    public class ResponseJson
    {
        public List<HomeItems> home { get; set; }
    }
    public class HomeItems
    {
        public int? order;
        public string text;
        public byte? status;
        public string view;
        public string controller;
        public List<int> rights;
        public string icon;
    }
    
}