using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class CustomerBase
    {
        [Required]
        [EmailAddress]
        [StringLength(100, ErrorMessage = "The Email field may not contain more than 100 characters.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(100, ErrorMessage = "The Name field may not contain more than 100 characters.")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Primary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]
        [Phone]
        public string PrimaryPhoneNumber { get; set; }

        [Required]
        [Display(Name = "Confirm Primary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]

        [Phone]
        public string ConfirmPrimaryPhoneNumber { get; set; }

        [Required]
        [Display(Name = "Secondary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]
        [Phone]
        public string SecondaryPhoneNumber { get; set; }

        [Required]
        [Display(Name = "Preferred Communication Method")]
        public string PreferredCommunicationMethod { get; set; }

        public List<SelectListItem> CommunicationOptions { get; set; }

        public CustomerBase()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                CommunicationOptions = db.PreferredCommunicationMethods.Select
                    (c => new SelectListItem
                    {
                        Text = c.CommunicationMethod,
                        Value = c.CommunicationMethod
                    }).ToList();
            }
        }
    }

    public class RegisterBase : CustomerBase
    {
        [Required]
        [Display(Name = "Address Line 1")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine1 { get; set; }

        [Required]
        [Display(Name = "Address Line 2")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine2 { get; set; }

        [Display(Name = "Address Line 3")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine3 { get; set; }

        [Display(Name = "Address Line 4")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine4 { get; set; }

        [Required]
        [Display(Name = "Suburb")]
        [StringLength(50, ErrorMessage = "The Suburb field may not contain more than 50 characters.")]
        public string AddressSuburb { get; set; }

        [Required]
        [Display(Name = "City")]
        [StringLength(50, ErrorMessage = "The City field may not contain more than 50 characters.")]
        public string AddressCity { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        [StringLength(50, ErrorMessage = "The Postal Code field may not contain more than 50 characters.")]
        public string AddressPostalCode { get; set; }

        [Required]
        [Display(Name = "Address Name")]
        public string AddressName { get; set; }

        public List<SelectListItem> AdressNameOptions { get; set; }

        public RegisterBase() : base()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                AdressNameOptions = db.AddressNames.Select
                    (c => new SelectListItem
                    {
                        Text = c.AddressName1,
                        Value = c.AddressName1
                    }).ToList();
            }
        }
    }

    public class ExternalLoginConfirmationViewModel : RegisterBase
    {
        [Required]
        [Display(Name = "Email")]
        new public string Email { get; set; }

        public ExternalLoginConfirmationViewModel() : base()
        {

        }

        /* Additional Customer Fields to be added to the Customer table upon registration */
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel : RegisterBase
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /* Additional Customer Fields to be added to the Customer table upon registration */

        public RegisterViewModel() : base()
        {

        }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
