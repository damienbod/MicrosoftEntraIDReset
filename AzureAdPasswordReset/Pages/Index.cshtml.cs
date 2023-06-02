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
        var usersCollectionResponse = await _graphUsers.FindUsers("f");

        var users = usersCollectionResponse.Value.ToList();
        //CurrentPage.Select(app => new PolicyAssignedApplicationsDto
        //{
        //    Id = app.Id,
        //    DisplayName = app.DisplayName,
        //    AppId = app.AppId,
        //    SignInAudience = app.SignInAudience,
        //    PolicyAssigned = GetFirstTokenLifetimePolicy(app.TokenLifetimePolicies)

        //}).ToList();
    }
}