// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Ecomm_project_1.DataAccess.Repository.IRepository;
using Ecomm_project_1.Models;
using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Ecomm_project_1.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            //add custom columns
            [Required]
            public string Name { get; set; }
            [Display(Name = "Street Address")]
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            [Display(Name = "Postal Code")]
            public string PostalCode { get; set; }
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
            [Display(Name = "Company")]
            public int? CompanyID { get; set; }
            [ForeignKey("CompanyID")]
            public Company company { get; set; }
            [NotMapped]
            public string Role { get; set; }
            //Add Company And Roles
            public IEnumerable<SelectListItem> CompanyList { get; set; }
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            Input = new InputModel
            {
                CompanyList = _unitOfWork.company.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.id.ToString()
                }),
                RoleList = _roleManager.Roles.Where(r => r.Name != SD.Role_Individual).Select(r => r.Name)
                .Select(rl => new SelectListItem()
                {
                    Text = rl,
                    Value = rl
                })
            };
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //var user = CreateUser();
                var user = new ApplicationUser()
                {
                    Name = Input.Name,
                    UserName = Input.Email,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    StreetAddress = Input.StreetAddress,
                    City = Input.City,
                    State = Input.State,
                    PostalCode = Input.PostalCode,
                    CompanyID = Input.CompanyID,
                    Role = Input.Role
                };

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.PhoneNumber = Input.PhoneNumber; 
                user.TwoFactorEnabled = true;
                user.PhoneNumberConfirmed = true;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    }
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Individual))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Individual));
                    }
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Employee))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
                    }
                    if (!await _roleManager.RoleExistsAsync(SD.Role_Company))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Company));
                    }
                    // Add Admin Role To First User
                    // await _userManager.AddToRoleAsync(user, SD.Role_Admin);
                    if (Input.Role == null && Input.CompanyID == null)
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_Individual);
                    }
                    else
                    {
                        if (Input.CompanyID > 0)
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_Company);
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(user, Input.Role);
                        }
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string emailBody = $@"
                <div style='font-family: Arial, sans-serif; text-align: center; padding: 40px;'>
                        <h1 style='font-size: 24px; color: #111;'>Thank you for using Book Shopping app</h1>
                        <p style='font-size: 16px; color: #333;'>Please take a moment to make sure we've got your email address right.</p>
                   <div style='margin-top: 30px;'>
                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                        style='background-color: #e60023; color: white; padding: 15px 25px; text-decoration: none; border-radius: 30px; font-weight: bold; display: inline-block;'>
                         Confirm your email address
                        </a>
                   </div>
                </div>";

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email", emailBody);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        if (Input.Role == null && Input.CompanyID == null)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "User", new { area = "Admin" });
                        }
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
