using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AzureAdPasswordReset.Pages;

public class IndexModel : PageModel
{
    private AadGraphSdkManagedIdentityAppClient _graphUsers;
    public string? SearchText { get; set; }

    public IndexModel(AadGraphSdkManagedIdentityAppClient graphUsers)
    {
        _graphUsers = graphUsers;
    }
    public async Task OnGetAsync()
    {
        var usersCollectionResponse = await _graphUsers.FindUsers("li");

        var users = usersCollectionResponse.Value!.ToList();
    }

    public async Task<ActionResult> OnGetAutoCompleteSuggest(string term)
    {
        var usersCollectionResponse = await _graphUsers.FindUsers(term);

        var users = usersCollectionResponse!.Value!.ToList();

        var usersDisplay = users.Select(user => new
        {
            Id = user.Id,
            Upn = user.UserPrincipalName,
            DisplayName = user.DisplayName,
        });

        SearchText = term;

        return new JsonResult(usersDisplay);
    }
}