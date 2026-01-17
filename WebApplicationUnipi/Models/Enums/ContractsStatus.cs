using System.ComponentModel.DataAnnotations;

namespace WebApplicationUnipi.Models.Enums
{
    public enum ContractsStatus
    {
        [Display(Name = "Ενεργή")]
        Active = 1,
        [Display(Name = "Ανενεργή")]
        Inactive = 2,
        [Display(Name = "Διαγραμμένη")]
        Deleted = 3
    }
}