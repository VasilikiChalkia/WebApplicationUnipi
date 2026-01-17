using System.ComponentModel.DataAnnotations;

namespace WebApplicationUnipi.Models.Enums
{   
    public enum Contracts
    {   
        [Display(Name = "Σύμβαση Αορίστου, Full Time")]
        IndefiniteFullTime = 1,
        [Display(Name = "Σύμβαση Αορίστου, Part Time")]
        IndefinitePartTime = 2,
        [Display(Name = "Σύμβαση Ορισμένου, Full Time")]
        DefiniteFullTime = 3,
        [Display(Name = "Σύμβαση Ορισμένου, Part Time")]
        DefinitePartTime = 4,
        [Display(Name = "Εξωτερικός Συνεργάτης")]
        Freelance = 5,
    }
}