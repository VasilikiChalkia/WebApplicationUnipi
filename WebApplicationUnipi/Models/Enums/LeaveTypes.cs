using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplicationUnipi.Models.Enums
{ 
    public enum LeaveTypes
    {
        [Display(Name = "Ετήσια κανονική άδεια | Annual Leave")]
        Annual = 1,
        [Display(Name = "Φοιτητική | Study Leave")]
        Student = 2,
        [Display(Name = "Αναρρωτική | Sick Leave")]
        Sick = 3,
        [Display(Name = "Άνευ αποδοχών | Unpaid Leave")]
        Unpaid = 4,
        [Display(Name = "Γονική άδεια | Parental Leave")]
        Parental = 5,      
        [Display(Name = "Άδεια αιμοδοσίας | Blood donation Leave")]
        BloodDonation = 6,
        [Display(Name = "Εκλογική άδεια | Election Leave")]
        Election = 7,
        [Display(Name = "Άλλο | Other")]
        Other = 8
    }
}