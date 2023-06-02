using Microsoft.Graph.Models;
using System.Security.Cryptography;

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
