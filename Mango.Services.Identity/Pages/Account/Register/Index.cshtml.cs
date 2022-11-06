using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IdentityModel;
using Mango.Services.Identity.Models;
using MangoRestaurent.Pages;
using MangoRestaurent.Pages.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Mango.Services.Identity.Pages.Account.Register
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public RegisterViewModel Input { get; set; }

        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IIdentityProviderStore _identityProviderStore;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(
            IIdentityServerInteractionService interaction,
            IAuthenticationSchemeProvider schemeProvider,
            IIdentityProviderStore identityProviderStore,
            IEventService events,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _interaction = interaction;
            _schemeProvider = schemeProvider;
            _identityProviderStore = identityProviderStore;
            _events = events;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> OnGet(string returnUrl)
        {
            await BuildRegisterModelAsync(returnUrl);

            if (Input.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToPage("/ExternalLogin/Challenge", new { scheme = Input.ExternalLoginScheme, returnUrl });
            }
            return Page();
        }

        public async Task<IActionResult> OnPost(string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    EmailConfirmed = true,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                };
                //var hasher = new PasswordHasher<ApplicationUser>();
                //user.PasswordHash = hasher.HashPassword(user, Input.Password);
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(Input.RoleName).GetAwaiter().GetResult())
                    {
                        var userRole = new IdentityRole
                        {
                            Name = Input.RoleName,
                            NormalizedName = Input.RoleName,
                        };
                        await _roleManager.CreateAsync(userRole);
                    }
                    await _userManager.AddToRoleAsync(user, Input.RoleName);

                    await _userManager.AddClaimsAsync(user, new Claim[] {
                        new Claim(JwtClaimTypes.Name, Input.Username),
                        new Claim(JwtClaimTypes.Email, Input.Email),
                        new Claim(JwtClaimTypes.FamilyName, Input.FirstName),
                        new Claim(JwtClaimTypes.GivenName, Input.LastName),
                            new Claim(JwtClaimTypes.WebSite, "http://"+Input.Username+".com"),
                            new Claim(JwtClaimTypes.Role,"User")
                    });

                    // check if we are in the context of an authorization request
                    var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

                    //var signedUser = await _userManager.FindByEmailAsync(Input.Email);

                    var loginresult = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, false, lockoutOnFailure: true);

                    if (loginresult.Succeeded)
                    {
                        var checkuser = await _userManager.FindByNameAsync(Input.Username);
                        await _events.RaiseAsync(new UserLoginSuccessEvent(checkuser.UserName, checkuser.Id, checkuser.UserName, clientId: context?.Client.ClientId));

                        if (context != null)
                        {
                            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                            if (context.IsNativeClient())
                            {
                                // The client is native, so this change in how to
                                // return the response is for better UX for the end user.
                                return this.LoadingPage(Input.ReturnUrl);
                            }

                            return Redirect(Input.ReturnUrl);
                        }

                        if (Url.IsLocalUrl(Input.ReturnUrl))
                        {
                            return Redirect(Input.ReturnUrl);
                        }
                        else if (string.IsNullOrEmpty(Input.ReturnUrl))
                        {
                            return Redirect("~/");
                        }
                        else
                        {
                            throw new Exception("invalid return URL");
                        }
                    }

                }
            }
            BuildRegisterModelAsync(Input.ReturnUrl);
            return Page();
        }

        private async Task BuildRegisterModelAsync(string returnUrl)
        {
            Input = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };

            List<string> roles = new() {
                SD.Admin,
                SD.Customer
            };
            ViewData["roles_message"] = roles;

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                //Input = new RegisterViewModel
                //{
                //    EnableLocalLogin = local,
                //};

                Input.EnableLocalLogin = local;
                Input.Username = context?.LoginHint;

                if (!local)
                {
                    Input.ExternalProviders = new[] { new RegisterViewModel.ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new RegisterViewModel.ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var dyanmicSchemes = (await _identityProviderStore.GetAllSchemeNamesAsync())
                .Where(x => x.Enabled)
                .Select(x => new RegisterViewModel.ExternalProvider
                {
                    AuthenticationScheme = x.Scheme,
                    DisplayName = x.DisplayName
                });
            providers.AddRange(dyanmicSchemes);


            var allowLocal = true;
            var client = context?.Client;
            if (client != null)
            {
                allowLocal = client.EnableLocalLogin;
                if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                {
                    providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                }
            }

            Input = new RegisterViewModel
            {
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                AllowRememberLogin = LoginOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
                ExternalProviders = providers.ToArray()
            };

        }
    }

}
