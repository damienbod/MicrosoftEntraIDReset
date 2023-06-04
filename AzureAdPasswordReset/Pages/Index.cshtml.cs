using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AzureAdPasswordReset.Pages;

public class IndexModel : PageModel
{
    private UserResetPasswordDelegated _graphUsers;
    public string? SearchText { get; set; }

    [BindProperty]
    public string? Upn { get; set; } = null;
    [BindProperty]
    public string? Password { get; set; } = null;

    public IndexModel(UserResetPasswordDelegated graphUsers)
    {
        _graphUsers = graphUsers;
    }

    public void OnGet()
    {
    }

    public async Task<ActionResult> OnGetAutoCompleteSuggest(string term)
    {
        if (term == "*") term = string.Empty;

        var usersCollectionResponse = await _graphUsers.FindUsers(term);

        var users = usersCollectionResponse!.Value!.ToList();

        var usersDisplay = users.Select(user => new
        {
            Id = user.Id,
            UserPrincipalName = user.UserPrincipalName,
            DisplayName = user.DisplayName
        });

        SearchText = term;

        return new JsonResult(usersDisplay);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var id = Request.Form.FirstOrDefault(u => u.Key == "userId").Value.FirstOrDefault();
        var upn = Request.Form.FirstOrDefault(u => u.Key == "userPrincipalName").Value.FirstOrDefault();

        if(!string.IsNullOrEmpty(id))
        {
            var result = await _graphUsers.ResetPassword(id);
            Upn = result.Upn;
            Password = result.Password;
            return Page();
        }

        return Page();
    }
}