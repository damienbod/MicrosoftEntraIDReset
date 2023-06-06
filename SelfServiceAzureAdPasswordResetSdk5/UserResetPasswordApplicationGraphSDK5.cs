using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Security.Cryptography;

namespace SelfServiceAzureAdPasswordReset;

public class UserResetPasswordApplicationGraphSDK5
{
    private readonly GraphApplicationClientService _graphApplicationClientService;

    public UserResetPasswordApplicationGraphSDK5(GraphApplicationClientService graphApplicationClientService)
    {
        _graphApplicationClientService = graphApplicationClientService;
    }

    private async Task<string?> GetUserIdAsync(string email)
    {
        var filter = $"startswith(userPrincipalName,'{email}')";

        var graphServiceClient = _graphApplicationClientService
            .GetGraphClientWithManagedIdentityOrDevClient();

        var result = await graphServiceClient.Users.GetAsync((requestConfiguration) =>
        {
            requestConfiguration.QueryParameters.Top = 10;
            if (!string.IsNullOrEmpty(email))
            {
                requestConfiguration.QueryParameters.Search = $"\"userPrincipalName:{email}\"";
            }
            requestConfiguration.QueryParameters.Orderby = new string[] { "displayName" };
            requestConfiguration.QueryParameters.Count = true;
            requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName", "userPrincipalName", "userType" };
            requestConfiguration.QueryParameters.Filter = "userType eq 'Member'"; // onPremisesSyncEnabled eq false
            requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
        });

        return result!.Value!.FirstOrDefault()!.Id;
    }

    public async Task<string?> ResetPassword(string email)
    {
        var graphServiceClient = _graphApplicationClientService
            .GetGraphClientWithManagedIdentityOrDevClient();

        var userId = await GetUserIdAsync(email);

        if (userId == null)
        {
            throw new ArgumentNullException(nameof(email));
        }

        var password = GetRandomString();

        await graphServiceClient.Users[userId].PatchAsync(
            new User
            {
                PasswordProfile = new PasswordProfile
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = true
                }
            });

        return password;
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
