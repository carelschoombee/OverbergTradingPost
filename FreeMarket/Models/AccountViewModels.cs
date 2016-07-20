using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /* Additional Customer Fields to be added to the Customer table upon registration */

        [Required]
        [Display(Name = "Name")]
        [StringLength(100, ErrorMessage = "The Name field may not contain more than 100 characters.")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Surname")]
        [StringLength(100, ErrorMessage = "The Surname field may not contain more than 100 characters.")]
        public string Surname { get; set; }

        [Display(Name = "Primary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]
        [Phone]
        public string PrimaryPhoneNumber { get; set; }

        [Display(Name = "Confirm Primary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]
        [System.ComponentModel.DataAnnotations.Compare("PrimaryPhoneNumber", ErrorMessage = "The phone numbers not match.")]
        [Phone]
        public string ConfirmPrimaryPhoneNumber { get; set; }

        [Display(Name = "Secondary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]
        [Phone]
        public string SecondaryPhoneNumber { get; set; }

        [Display(Name = "Preferred Communication Method")]
        public string PreferredCommunicationMethod { get; set; }

        public List<SelectListItem> CommunicationOptions { get; set; }

        [Display(Name = "Address Line 1")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine2 { get; set; }

        [Display(Name = "Address Line 3")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine3 { get; set; }

        [Display(Name = "Address Line 4")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine4 { get; set; }

        [Display(Name = "Suburb")]
        [StringLength(50, ErrorMessage = "The Suburb field may not contain more than 50 characters.")]
        public string AddressSuburb { get; set; }

        [Display(Name = "City")]
        [StringLength(50, ErrorMessage = "The City field may not contain more than 50 characters.")]
        public string AddressCity { get; set; }

        [Display(Name = "Postal Code")]
        [StringLength(50, ErrorMessage = "The Postal Code field may not contain more than 50 characters.")]
        public string AddressPostalCode { get; set; }

        public ExternalLoginConfirmationViewModel()
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

    public class RegisterViewModel
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

        /* Additional Customer Fields to be added to the Customer table upon registration */

        [Required]
        [Display(Name = "Name")]
        [StringLength(100, ErrorMessage = "The Name field may not contain more than 100 characters.")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Surname")]
        [StringLength(100, ErrorMessage = "The Surname field may not contain more than 100 characters.")]
        public string Surname { get; set; }

        [Display(Name = "Primary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]
        [Phone]
        public string PrimaryPhoneNumber { get; set; }

        [Display(Name = "Confirm Primary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]
        [System.ComponentModel.DataAnnotations.Compare("PrimaryPhoneNumber", ErrorMessage = "The phone numbers not match.")]
        [Phone]
        public string ConfirmPrimaryPhoneNumber { get; set; }

        [Display(Name = "Secondary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]
        [Phone]
        public string SecondaryPhoneNumber { get; set; }

        [Display(Name = "Preferred Communication Method")]
        public string PreferredCommunicationMethod { get; set; }

        public List<SelectListItem> CommunicationOptions { get; set; }

        [Display(Name = "Address Line 1")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine2 { get; set; }

        [Display(Name = "Address Line 3")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine3 { get; set; }

        [Display(Name = "Address Line 4")]
        [StringLength(250, ErrorMessage = "The Address field may not contain more than 250 characters.")]
        public string AddressLine4 { get; set; }

        [Display(Name = "Suburb")]
        [StringLength(50, ErrorMessage = "The Suburb field may not contain more than 50 characters.")]
        public string AddressSuburb { get; set; }

        [Display(Name = "City")]
        [StringLength(50, ErrorMessage = "The City field may not contain more than 50 characters.")]
        public string AddressCity { get; set; }

        [Display(Name = "Postal Code")]
        [StringLength(50, ErrorMessage = "The Postal Code field may not contain more than 50 characters.")]
        public string AddressPostalCode { get; set; }

        public RegisterViewModel()
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
