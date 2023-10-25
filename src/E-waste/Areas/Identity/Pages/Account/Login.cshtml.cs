// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using E_waste.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static E_waste.Areas.Identity.Pages.Account.RegisterModel;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace E_waste.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, IHttpClientFactory httpClientFactory)
        {
            _signInManager = signInManager;
            _logger = logger;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            public string username { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>

        }

        public class InputModel2
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public string userID { get; set; }
            public string email { get; set; }
            public string name { get; set; }
            public string address { get; set; }
            public string phoneNo { get; set; }
            
            public string token { get; set; }

            public string expiration { get; set; }
        }



        public class ApiResponse
        {
            public string status { get; set; }
            public string message { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
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
            var jsonUserData = System.Text.Json.JsonSerializer.Serialize(Input);

            // Create an instance of HttpClient using the factory
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                // Set the API endpoint URL
                //var apiUrl = "https://localhost:7010/api/authentication/login";
                var apiUrl = "http://accountservice.ap-southeast-1.elasticbeanstalk.com/api/Authentication/login";

                // Create a StringContent with JSON data
                var content = new StringContent(jsonUserData, Encoding.UTF8, "application/json");

                // Make the POST request and get the response
                var response = await httpClient.PostAsync(apiUrl, content);

                // Check if the request was successful (status code 2xx)
                if (response.IsSuccessStatusCode)
                {
                    // Optionally, you can handle the success response
                    
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<InputModel2>(errorResponse);

                    var values = new Dictionary<string, string>
                    {
                        { "UserID", apiResponse.userID},
                        { "Email", apiResponse.email},
                        { "Address", apiResponse.address },
                        { "Name",  apiResponse.name},
                        { "PhoneNo",  apiResponse.phoneNo}
                    };

                    // Serialize the values to JSON
                    string json = JsonConvert.SerializeObject(values);

                    // Store the JSON string in the session
                    HttpContext.Session.SetString("Userid", json);

                    // Process the API response as needed
                    return LocalRedirect("/Products/Index"); // Redirect to a success page
                }
                else
                {
                    // Optionally, handle the error response
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    // Process the error response as needed
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // COMMENTED OUT
            //public async Task<IActionResult> OnPostAsync(string returnUrl = null)
            //{
            //    returnUrl ??= Url.Content("~/");

            //    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            //    if (ModelState.IsValid)
            //    {
            //        // This doesn't count login failures towards account lockout
            //        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            //        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            //        if (result.Succeeded)
            //        {
            //            _logger.LogInformation("User logged in.");
            //            returnUrl = "/Products/Index";
            //            return LocalRedirect(returnUrl);
            //        }
            //        if (result.RequiresTwoFactor)
            //        {
            //            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            //        }
            //        if (result.IsLockedOut)
            //        {
            //            _logger.LogWarning("User account locked out.");
            //            return RedirectToPage("./Lockout");
            //        }
            //        else
            //        {
            //            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            //            return Page();
            //        }
            //    }

            //    // If we got this far, something failed, redisplay form
            //    return Page();
            //}
        }
    }
}
