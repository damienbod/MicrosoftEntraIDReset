﻿using Microsoft.Graph;
using System.Security.Cryptography;

namespace SelfServiceAzureAdPasswordReset;

public class UserResetPasswordApplicationGraphSDK4
{
    private readonly GraphApplicationClientService _graphApplicationClientService;

    public UserResetPasswordApplicationGraphSDK4(GraphApplicationClientService graphApplicationClientService)
    {
        _graphApplicationClientService = graphApplicationClientService;
    }

    private async Task<string> GetUserIdAsync(string email)
    {
        var filter = $"startswith(userPrincipalName,'{email}')";

        var graphServiceClient = _graphApplicationClientService
            .GetGraphClientWithManagedIdentityOrDevClient();

        var users = await graphServiceClient.Users
            .Request()
            .Filter(filter)
            .GetAsync();

        return users.CurrentPage[0].Id;
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

        await graphServiceClient.Users[userId].Request()
        .UpdateAsync(new User
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
