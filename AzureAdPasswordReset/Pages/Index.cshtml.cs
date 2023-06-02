using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AzureAdPasswordReset.Pages;

public class IndexModel : PageModel
{
    private AadGraphSdkManagedIdentityAppClient _graphUsers;

    public IndexModel(AadGraphSdkManagedIdentityAppClient graphUsers)
    {
        _graphUsers = graphUsers;
    }
    public async Task OnGetAsync()
    {
        var usersCollectionResponse = await _graphUsers.FindUsers("li");

        var users = usersCollectionResponse.Value!.ToList();
    }
}