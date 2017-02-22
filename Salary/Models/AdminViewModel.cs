using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace IdentitySample.Models
{
    //ViewModel for RolesAdmin/Index
    public class RoleViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
    //ViewModel for UserAdmin/Edit
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        //Adding properties to user: 
        //EmployeeName, EmployeeSurname, EmployeeNettoWage, EmployeePhoto

        [Display(Name = "Name")]
        [Required]
        public string EmployeeName { get; set; }

        [Display(Name = "Surname")]
        [Required]
        public string EmployeeSurname { get; set; }

        [Display(Name = "Salary (net)")]
        [RegularExpression("^[0-9]{1,10}([.]{1}[0-9]{1,2})?$", ErrorMessage = "Only numbers separated with dot!")]
        public double EmployeeNettoWage { get; set; }
        
        [Display(Name = "Photo")]
        public byte[] EmployeePhoto { get; set; }

        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}