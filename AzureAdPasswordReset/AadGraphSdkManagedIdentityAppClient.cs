using Microsoft.Graph.Models;
using System.Security.Cryptography;
using Microsoft.Graph.Users;
using Microsoft.Kiota.Abstractions;

namespace AzureAdPasswordReset;

public class AadGraphSdkManagedIdentityAppClient
{
    private readonly GraphApplicationClientService _graphService;

    public AadGraphSdkManagedIdentityAppClient(GraphApplicationClientService graphService)
    {
        _graphService = graphService;
    }

    /// <summary>
    /// User.ReadWrite.All permission required
    /// </summary>
    public async Task<User?> ResetPassword(string oid)
    {
        var graphServiceClient = _graphService.GetGraphClientWithManagedIdentityOrDevClient();

        var user = await graphServiceClient.Users[oid].GetAsync();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(oid));
        }

        user.PasswordProfile = new PasswordProfile
        {
            ForceChangePasswordNextSignIn = true,
            Password = GetRandomString(),
        };

        var result = await graphServiceClient.Users[oid].PatchAsync(user);

        return result;
    }

    public async Task<UserCollectionResponse?> FindUsers(string search)
    {
        var graphServiceClient = _graphService.GetGraphClientWithManagedIdentityOrDevClient();


        var result = await graphServiceClient.Users.GetAsync((requestConfiguration) =>
        {
            requestConfiguration.QueryParameters.Top = 10;
            if (!string.IsNullOrEmpty(search))
            {
                requestConfiguration.QueryParameters.Search = $"\"displayName:{search}\"";
            }
            requestConfiguration.QueryParameters.Orderby = new string[] { "displayName" };
            requestConfiguration.QueryParameters.Count = true;
            requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName", "userPrincipalName", "userType" };
            requestConfiguration.QueryParameters.Filter = "userType eq 'Member'"; // onPremisesSyncEnabled eq false
            requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
        });

        return result;
    }

    private static string GetRandomString()
    {
        var random = $"{GenerateRandom()}{GenerateRandom()}{GenerateRandom()}{GenerateRandom()}-AC";
        return random;
    }

    private static int GenerateRandom()
    {
        return RandomNumberGenerator.GetInt32(100000000, int.MaxValue);
    }

}
