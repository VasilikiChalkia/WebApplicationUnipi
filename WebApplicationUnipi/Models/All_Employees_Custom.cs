using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationUnipi.Models
{
    public class All_Employees_Custom
    {
        public All_Employees_Custom()
        {

        }

        public All_Employees_Custom(All_Employees_Custom item)
        {
            Id = item.Id;
            Department_Id = item.Department_Id;
            Position_Id = item.Position_Id;
            Employee_Fname = item.Employee_Fname;
            Employee_Lname = item.Employee_Lname;
            Privileges_Num = item.Privileges_Num;
            Status = item.Status;
          
   
        }

        public int Id { get; set; }
        public Nullable<int> Department_Id { get; set; }
        public Nullable<int> Position_Id { get; set; }
        public string Employee_Fname { get; set; }
        public string Employee_Lname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Privileges_Num { get; set; }
        public Nullable<int> Status { get; set; }
   
        public int Available { get; set; }
  
    }
}