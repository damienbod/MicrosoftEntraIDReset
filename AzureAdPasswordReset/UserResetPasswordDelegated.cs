using Microsoft.Graph.Models;
using System.Security.Cryptography;
using Microsoft.Graph.Users.Item.Authentication.Methods.Item.ResetPassword;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using System.Net.Http.Headers;

namespace AzureAdPasswordReset;

public class UserResetPasswordDelegated
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private GraphServiceClient _graphclient;
    private readonly IHttpClientFactory _clientFactory;

    public UserResetPasswordDelegated(ITokenAcquisition tokenAcquisition,
        IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _tokenAcquisition = tokenAcquisition;
    }

    /// <summary>
    /// User.Read.All and UserAuthenticationMethod.ReadWrite.All permission required
    /// https://learn.microsoft.com/en-us/graph/api/authenticationmethod-resetpassword?view=graph-rest-1.0&tabs=csharp
    /// </summary>
    public async Task<(string? Upn, string? Password)> ResetPassword(string oid)
    {
        _graphclient = await GetGraphClient(new string[] { "User.ReadWrite.All", "UserAuthenticationMethod.ReadWrite.All" });
        var password = GetRandomString();

        var user = await _graphclient.Users[oid].GetAsync();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(oid));
        }

        var methods = await _graphclient
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
            var result = await _graphclient.Users[oid]
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
        _graphclient = await GetGraphClient(new string[] { "User.ReadWrite.All", "UserAuthenticationMethod.ReadWrite.All" });
        var result = await _graphclient.Users.GetAsync((requestConfiguration) =>
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

    private async Task<GraphServiceClient> GetGraphClient(string[] scopes)
    {
        var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri("https://graph.microsoft.com/beta");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

        var graphClient = new GraphServiceClient(client);
        //{
           
        //    AuthenticationProvider = new DelegateAuthenticationProvider((requestMessage) => {
        //        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        //        return Task.CompletedTask;
        //    }),
        //    BaseUrl = "https://graph.microsoft.com/beta"
        //};
        return graphClient;
    }
}
