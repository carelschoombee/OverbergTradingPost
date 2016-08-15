using FreeMarket.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    [Authorize]
    [RequireHttps]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var currentUser = await UserManager.FindByIdAsync(userId);
            var unConfirmedEmail = "";
            if (!String.IsNullOrWhiteSpace(currentUser.UnConfirmedEmail))
            {
                unConfirmedEmail = currentUser.UnConfirmedEmail;
            }
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                ConfirmedEmail = currentUser.Email,
                UnConfirmedEmail = unConfirmedEmail
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }

                AuditUser.LogAudit(4, "", user.Id);

                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        public ActionResult ModifyDeliveryDetails()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }

            CustomerAddress address = CustomerAddress.GetCustomerAddress(user.Id);

            ModifyDeliveryDetailsViewModel model = new ModifyDeliveryDetailsViewModel()
            {
                Address = address,
                AddressName = address.AddressName
            };

            foreach (SelectListItem name in model.AdressNameOptions)
            {
                if (name.Value == address.AddressName)
                {
                    name.Selected = true;
                }
            }

            return View(model);
        }

        public ActionResult ModifyDeliveryDetailsByName(string AddressName)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }

            CustomerAddress address = CustomerAddress.GetCustomerAddress(user.Id, AddressName);

            ModifyDeliveryDetailsViewModel model = new ModifyDeliveryDetailsViewModel()
            {
                Address = address,
                AddressName = address.AddressName
            };

            foreach (SelectListItem name in model.AdressNameOptions)
            {
                if (name.Value == AddressName)
                {
                    name.Selected = true;
                }
            }

            return View("ModifyDeliveryDetails", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModifyDeliveryDetails(ModifyDeliveryDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (user == null)
                {
                    return View("Error");
                }

                if (CustomerAddress.AddressExists(user.Id, model.AddressName))
                {
                    FreeMarketResult result = CustomerAddress.UpdateAddress(user.Id, model.AddressName, model.Address.AddressLine1, model.Address.AddressLine2
                           , model.Address.AddressLine3, model.Address.AddressLine4, model.Address.AddressSuburb
                           , model.Address.AddressCity, model.Address.AddressPostalCode);

                    if (result == FreeMarketResult.Success)
                        TempData["message"] = string.Format
                            ("Your {0} address has been updated.",
                            model.AddressName);
                    else
                        TempData["message"] = string.Format
                            ("Sorry, we could not process your request at this time, please try again later.");
                }
                else
                {
                    FreeMarketResult result = CustomerAddress.AddAddress(user.Id, model.AddressName, model.Address.AddressLine1, model.Address.AddressLine2
                           , model.Address.AddressLine3, model.Address.AddressLine4, model.Address.AddressSuburb
                           , model.Address.AddressCity, model.Address.AddressPostalCode);

                    if (result == FreeMarketResult.Success)
                        TempData["message"] = string.Format
                            ("Your {0} address has been added to our system.",
                            model.AddressName);
                    else
                        TempData["message"] = string.Format
                            ("Sorry, we could not process your request at this time, please try again later.");
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult ModifyAccountDetails()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }

            ModifyAccountDetailsViewModel model = new ModifyAccountDetailsViewModel()
            {
                Name = user.Name,
                PrimaryPhoneNumber = user.PhoneNumber,
                SecondaryPhoneNumber = user.SecondaryPhoneNumber,
                PreferredCommunicationMethod = user.PreferredCommunicationMethod,
                DefaultAddress = user.DefaultAddress
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ModifyAccountDetails(ModifyAccountDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (user == null)
                {
                    return View("Error");
                }

                user.Name = model.Name;
                user.PhoneNumber = model.PrimaryPhoneNumber;
                user.SecondaryPhoneNumber = model.SecondaryPhoneNumber;
                user.PreferredCommunicationMethod = model.PreferredCommunicationMethod;
                user.DefaultAddress = model.DefaultAddress;

                var result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                {

                    AuditUser.LogAudit(2, "", user.Id);

                    TempData["message"] = string.Format
                            ("Your account details have been updated.");

                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        public ActionResult ChangeEmail()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var model = new ChangeEmailViewModel()
            {
                ConfirmedEmail = user.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("ChangeEmail", "Manage");
            }

            var user = await UserManager.FindByEmailAsync(model.ConfirmedEmail);
            var userId = user.Id;
            if (user != null)
            {
                //doing a quick swap so we can send the appropriate confirmation email
                user.UnConfirmedEmail = user.Email;
                user.Email = model.UnConfirmedEmail;
                user.EmailConfirmed = false;
                var result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    string callbackUrl =
                    await SendEmailConfirmationTokenAsync(userId, "Confirm your new email");

                    var tempUnconfirmed = user.Email;
                    user.Email = user.UnConfirmedEmail;
                    user.UnConfirmedEmail = tempUnconfirmed;
                    result = await UserManager.UpdateAsync(user);

                    callbackUrl = await SendEmailConfirmationWarningAsync(userId, "You email has been updated to: " + user.UnConfirmedEmail);
                }
            }
            return RedirectToAction("Index", "Manage");
        }

        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var callbackUrl = Url.Action("ConfirmEmail", "Account",
               new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(userID, subject,
               "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return callbackUrl;
        }

        private async Task<string> SendEmailConfirmationWarningAsync(string userID, string body)
        {
            var callbackUrl = Url.Action("ConfirmEmail", "Account",
               new { userId = userID }, protocol: Request.Url.Scheme);

            await UserManager.SendEmailAsync(userID, "Email Updated", body);

            return callbackUrl;
        }

        public async Task<ActionResult> CancelUnconfirmedEmail(string emailOrUserId)
        {
            var user = await UserManager.FindByEmailAsync(emailOrUserId);
            if (user == null)
            {
                user = await UserManager.FindByIdAsync(emailOrUserId);
                if (user != null)
                {
                    user.UnConfirmedEmail = "";
                    user.EmailConfirmed = true;
                    var result = await UserManager.UpdateAsync(user);
                }
            }
            else
            {
                user.UnConfirmedEmail = "";
                user.EmailConfirmed = true;
                var result = await UserManager.UpdateAsync(user);
            }

            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}