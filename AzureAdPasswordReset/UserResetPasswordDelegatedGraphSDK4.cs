using System.Security.Cryptography;
using Microsoft.Graph;

namespace AzureAdPasswordReset;

public class UserResetPasswordDelegatedGraphSDK4
{
    private readonly GraphServiceClient _graphServiceClient;

    public UserResetPasswordDelegatedGraphSDK4(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    /// <summary>
    /// Directory.AccessAsUser.All User.ReadWrite.All UserAuthenticationMethod.ReadWrite.All
    /// </summary>
    public async Task<(string? Upn, string? Password)> ResetPassword(string oid)
    {
        var password = GetRandomString();

        var user = await _graphServiceClient.Users[oid]
            .Request().GetAsync();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(oid));
        }

        await _graphServiceClient.Users[oid].Request()
            .UpdateAsync(new User
            {
                PasswordProfile = new PasswordProfile
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = true
                }
            });

        return (user.UserPrincipalName, password);
    }

    public async Task<IGraphServiceUsersCollectionPage?> FindUsers(string search)
    {
        var users = await _graphServiceClient.Users.Request()
            .Filter("userType eq 'Member'")
            //.Filter($"displayName/any(c:startswith(c/value, '{search}'))")
            // ("accountEnabled eq true") // onPremisesSyncEnabled eq false
            .Select(u => new
            {
                u.Id,
                u.GivenName,
                u.Surname,
                u.DisplayName,
                u.Mail,
                u.EmployeeId,
                u.EmployeeType,
                u.BusinessPhones,
                u.MobilePhone,
                u.AccountEnabled,
                u.UserPrincipalName
            })
            .GetAsync();

        return users;
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
