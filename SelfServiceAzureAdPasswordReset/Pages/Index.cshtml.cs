using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SelfServiceAzureAdPasswordReset.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserResetPasswordApplicationGraphSDK4 _userResetPasswordApp;

        [BindProperty]
        public string? Upn { get; set; } = null;

        public IndexModel(UserResetPasswordApplicationGraphSDK4 userResetPasswordApplicationGraphSDK4)
        {
            _userResetPasswordApp = userResetPasswordApplicationGraphSDK4;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var upn = Request.Form.FirstOrDefault(u => u.Key == "userPrincipalName").Value.FirstOrDefault();

            if (!string.IsNullOrEmpty(upn))
            {
                var result = await _userResetPasswordApp.ResetPassword(upn);
                Upn = result.Upn;
                return Page();
            }

            return Page();
        }
    }
}