using Microsoft.Graph;

namespace AzureAdPasswordReset;

public class AadGraphApiDelegatedClient
{
    private readonly GraphServiceClient _graphServiceClient;

    public AadGraphApiDelegatedClient(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }


    public async Task<bool> ResetPassword(string oid)
    {
        var user = await _graphServiceClient.Users[oid].GetAsync();

        var updated = await _graphServiceClient.Users[oid].PatchAsync(user);

        return true;
    }


}
