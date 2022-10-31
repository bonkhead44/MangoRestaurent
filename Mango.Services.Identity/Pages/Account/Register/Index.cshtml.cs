using Mango.Services.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mango.Services.Identity.Pages.Account.Register
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public RegisterViewModel Input { get; set; }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> OnGet(string returnUrl)
        {
            List<string> roles = new() {
                SD.Admin,
                SD.Customer
            };
            ViewData["roels_message"] = roles;
            Input = new RegisterViewModel {
                ReturnUrl =returnUrl
            };
            return Page();
        }
    }
}
