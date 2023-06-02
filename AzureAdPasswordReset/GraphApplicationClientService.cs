﻿using Azure.Identity;
using Microsoft.Graph;

namespace AzureAdPasswordReset;

public class GraphApplicationClientService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private GraphServiceClient? _graphServiceClient;

    public GraphApplicationClientService(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// gets a singleton instance of the GraphServiceClient
    /// </summary>
    public GraphServiceClient GetGraphClientWithManagedIdentityOrDevClient()
    {
        if (_graphServiceClient != null)
            return _graphServiceClient;

        string[] scopes = new[] { "https://graph.microsoft.com/.default" };

        var chainedTokenCredential = GetChainedTokenCredentials();
        _graphServiceClient = new GraphServiceClient(chainedTokenCredential, scopes);

        return _graphServiceClient;
    }

    private ChainedTokenCredential GetChainedTokenCredentials()
    {
        if (!_environment.IsDevelopment())
        {
            // You could also use a certificate here
            return new ChainedTokenCredential(new ManagedIdentityCredential());
        }
        else // dev env
        {
            var tenantId = _configuration["AzureAdGraph:TenantId"];
            var clientId = _configuration.GetValue<string>("AzureAdGraph:ClientId");
            var clientSecret = _configuration.GetValue<string>("AzureAdGraph:ClientSecret");

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var devClientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            var chainedTokenCredential = new ChainedTokenCredential(devClientSecretCredential);

            return chainedTokenCredential;
        }
    }
}