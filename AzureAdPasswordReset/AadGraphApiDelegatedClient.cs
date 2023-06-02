using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Security.Cryptography;

namespace AzureAdPasswordReset;

public class AadGraphApiDelegatedClient
{
    private readonly GraphServiceClient _graphServiceClient;

    public AadGraphApiDelegatedClient(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }


    public async Task<User?> ResetPassword(string oid)
    {
        var user = await _graphServiceClient.Users[oid].GetAsync();

        if(user == null)
        {
            throw new ArgumentNullException(nameof(oid));
        }

        user.PasswordProfile = new PasswordProfile
        {
            ForceChangePasswordNextSignIn = true,
            Password = GetRandomString(),
        };

        var result = await _graphServiceClient.Users[oid].PatchAsync(user);

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
