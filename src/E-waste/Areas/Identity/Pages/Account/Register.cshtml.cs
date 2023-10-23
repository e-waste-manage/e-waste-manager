// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using E_waste.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using Microsoft.CodeAnalysis.Scripting;
using static System.Net.Mime.MediaTypeNames;

namespace E_waste.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHttpClientFactory _httpClientFactory;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
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
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Name")]
            public string Name { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Address")]
            public string Address { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Username")]
            public string Username { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
            [DataType(DataType.PhoneNumber)]
            [Display(Name = "Phone number")]
            public string Phonenumber { get; set; }

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
        }

        public bool ShowRegistrationAlert { get; set; }




        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            // Gather user registration data from the form or other sources
            //var userData = new
            //{
            //    Username = Input.Username,
            //    Email = "user@example.com",
            //    Password = "examplePassword"
            //    // Add other registration fields as needed
            //};

            // Convert user data to JSON
            var jsonUserData = JsonSerializer.Serialize(Input);
            var role = "Admin";
            // Create an instance of HttpClient using the factory
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                // Set the API endpoint URL
                var apiUrl = "https://localhost:7010/api/Authentication/Register";

                // Create a StringContent with JSON data
                var content = new StringContent(jsonUserData, Encoding.UTF8, "application/json");

                // Make the POST request and get the response
                var response = await httpClient.PostAsync($"{apiUrl}?role={role}", content);

                // Check if the request was successful (status code 2xx)
                if (response.IsSuccessStatusCode)
                {
                    //Response.Write("<script>alert('Successfully Registered, Please verify your Email')</script>"); 
                    ShowRegistrationAlert = true;
                    // Optionally, you can handle the success response
                    var apiResponse = await response.Content.ReadAsStringAsync();
                    //ScriptManager.RegisterStartupScript(this, GetType(), "displayalertmessage", "Showalert();", true);
                    // Process the API response as needed
                    //return LocalRedirect("/Identity/Account/Login"); // Redirect to a success page
                    return Page();
                }
                else
                {
                    // Optionally, handle the error response
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    // Process the error response as needed
                    ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                    return Page();
                }
            }
        }

    // COMMENTED OUT
    //public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    //{
    //    returnUrl ??= Url.Content("~/");
    //    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    //    if (ModelState.IsValid)
    //    {
    //        var user = CreateUser();


    //        user.Name = Input.Name;
    //        user.Address = Input.Address;
    //        user.UserName = Input.Username;
    //        user.Email = Input.Email;
    //        user.PhoneNumber = Input.Phonenumber;

    //        await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
    //        await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
    //        var result = await _userManager.CreateAsync(user, Input.Password);

    //        if (result.Succeeded)
    //        {
    //            _logger.LogInformation("User created a new account with password.");

    //            var userId = await _userManager.GetUserIdAsync(user);
    //            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    //            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    //            var callbackUrl = Url.Page(
    //                "/Account/ConfirmEmail",
    //                pageHandler: null,
    //                values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
    //                protocol: Request.Scheme);

    //            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
    //                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

    //            if (_userManager.Options.SignIn.RequireConfirmedAccount)
    //            {
    //                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
    //            }
    //            else
    //            {
    //                await _signInManager.SignInAsync(user, isPersistent: false);
    //                return LocalRedirect(returnUrl);
    //            }
    //        }
    //        foreach (var error in result.Errors)
    //        {
    //            ModelState.AddModelError(string.Empty, error.Description);
    //        }
    //    }

        // If we got this far, something failed, redisplay form
    //    return Page();
    //}

    private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
