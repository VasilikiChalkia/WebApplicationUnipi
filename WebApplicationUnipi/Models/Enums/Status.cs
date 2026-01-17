using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplicationUnipi.Models.Enums
{
    public enum Status
    {
        [Display(Name = "Ενεργός Χρήστης")]
        Active = 1,
        [Display(Name = "Ανενεργός Χρήστης")]
        InActive = 3
    }
}