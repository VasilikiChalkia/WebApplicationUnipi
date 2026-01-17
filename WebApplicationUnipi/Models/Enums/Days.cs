using System.ComponentModel.DataAnnotations;

namespace WebApplicationUnipi.Models.Enums
{
    public enum Days
    {
        [Display(Name = "Δευτέρα")]
        Monday = 1,
        [Display(Name = "Τρίτη")]
        Tuesday = 2,
        [Display(Name = "Τετάρτη")]
        Wednesday = 3,
        [Display(Name = "Πέμπτη")]
        Thursday = 4,
        [Display(Name = "Παρασκευή")]
        Friday = 5,
        [Display(Name = "Σάββατο")]
        Saturday = 6,
        [Display(Name = "Κυριακή")]
        Sunday = 7,
    }
}