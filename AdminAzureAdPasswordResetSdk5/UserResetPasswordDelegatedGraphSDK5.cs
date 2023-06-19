using System.Security.Cryptography;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace AzureAdPasswordReset;

public class UserResetPasswordDelegatedGraphSDK5
{
    private readonly GraphServiceClient _graphServiceClient;

    public UserResetPasswordDelegatedGraphSDK5(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    /// <summary>
    /// Directory.AccessAsUser.All User.ReadWrite.All UserAuthenticationMethod.ReadWrite.All
    /// </summary>
    public async Task<(string? Upn, string? Password)> ResetPassword(string oid)
    {
        var user = await _graphServiceClient
            .Users[oid]
            .GetAsync();

        if (user == null)
            throw new ArgumentNullException(nameof(oid));

        var password = GetRandomString();

        await _graphServiceClient.Users[oid].PatchAsync(
            new User
            {
                PasswordProfile = new PasswordProfile
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = true
                }
            });

        return (user.UserPrincipalName, password);
    }

    public async Task<UserCollectionResponse?> FindUsers(string search)
    {
        var result = await _graphServiceClient.Users.GetAsync((requestConfiguration) =>
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
