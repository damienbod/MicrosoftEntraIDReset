using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SelfServiceAzureAdPasswordReset.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserResetPasswordApplicationGraphSDK4 _userResetPasswordApp;

        [BindProperty]
        public string Upn { get; set; } = string.Empty;

        [BindProperty]
        public string? Password { get; set; } = string.Empty;

        public IndexModel(UserResetPasswordApplicationGraphSDK4 userResetPasswordApplicationGraphSDK4)
        {
            _userResetPasswordApp = userResetPasswordApplicationGraphSDK4;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Password = await _userResetPasswordApp.ResetPassword(Upn);
            
            return Page();
        }
    }
}