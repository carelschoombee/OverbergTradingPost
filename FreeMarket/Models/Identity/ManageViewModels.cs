using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    public enum FreeMarketResult
    {
        Success,
        Exception,
        Failure
    }

    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }

        public string ConfirmedEmail { get; set; }
        public string UnConfirmedEmail { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ModifyAccountDetailsViewModel
    {
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
        [Display(Name = "Secondary Phone Number")]
        [StringLength(50, ErrorMessage = "The Phone Number field may not contain more than 50 characters.")]
        [Phone]
        public string SecondaryPhoneNumber { get; set; }

        [Required]
        [Display(Name = "Preferred Communication Method")]
        public string PreferredCommunicationMethod { get; set; }

        public List<SelectListItem> CommunicationOptions { get; set; }

        public ModifyAccountDetailsViewModel()
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

    public class ModifyDeliveryDetailsViewModel
    {
        public CustomerAddress Address { get; set; }

        [Display(Name = "Address Name")]
        public string AddressName { get; set; }

        public List<SelectListItem> AdressNameOptions { get; set; }

        public ModifyDeliveryDetailsViewModel()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                AdressNameOptions = db.AddressNames.Select
                    (c => new SelectListItem
                    {
                        Text = c.AddressName1,
                        Value = c.AddressName1,
                    }).ToList();

                AdressNameOptions[0].Selected = true;
            }
        }
    }

    public class ChangeEmailViewModel
    {
        public string ConfirmedEmail { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string UnConfirmedEmail { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}