using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplicationUnipi.Models.Enums
{
    public enum Privileges
    {

        [Display(Name = "Admin Access")]
        HR_Admin = 24,
        [Display(Name = "Employee Access")]
        HR_Employee = 25
    }
}