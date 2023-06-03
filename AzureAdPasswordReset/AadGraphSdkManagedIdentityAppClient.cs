using Microsoft.Graph.Models;
using System.Security.Cryptography;
using Microsoft.Graph.Users.Item.Authentication.Methods.Item.ResetPassword;

namespace AzureAdPasswordReset;

public class AadGraphSdkManagedIdentityAppClient
{
    private readonly GraphApplicationClientService _graphService;

    public AadGraphSdkManagedIdentityAppClient(GraphApplicationClientService graphService)
    {
        _graphService = graphService;
    }

    /// <summary>
    /// User.Read.All and UserAuthenticationMethod.ReadWrite.All permission required
    /// https://learn.microsoft.com/en-us/graph/api/authenticationmethod-resetpassword?view=graph-rest-1.0&tabs=csharp
    /// </summary>
    public async Task<(string? Upn, string? Password)> ResetPassword(string oid)
    {
        var password = GetRandomString();
        var graphServiceClient = _graphService.GetGraphClientWithManagedIdentityOrDevClient();

        var user = await graphServiceClient.Users[oid].GetAsync();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(oid));
        }

        var methods = await graphServiceClient
            .Users[oid].Authentication.Methods.GetAsync();

        // "28c10230-6103-485e-b985-444c60001490" == password
        if (!methods!.Value!.Exists(au => au.Id == "28c10230-6103-485e-b985-444c60001490"))
        {
            throw new ArgumentNullException(nameof(oid));
        }

        var requestBody = new ResetPasswordPostRequestBody
        {
            NewPassword = password,
        };

        try {
            var result = await graphServiceClient.Users[oid]
              .Authentication
              .Methods["28c10230-6103-485e-b985-444c60001490"]
              .ResetPassword
              .PostAsync(requestBody);

            return (user.UserPrincipalName, result!.NewPassword);
        }
        catch(Exception ex)
        {
            var sss = ex.Message;
        }
      


        return (null, null);
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
