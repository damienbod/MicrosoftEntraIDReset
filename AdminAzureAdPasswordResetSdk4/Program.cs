using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace AzureAdPasswordReset;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddScoped<UserResetPasswordDelegatedGraphSDK4>();

        string[]? initialScopes = builder.Configuration.GetValue<string>("GraphScopes")?.Split(' ');

        var baseAddress = builder.Configuration["GraphApi:BaseUrl"];
        baseAddress ??= "https://graph.microsoft.com/beta";

        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(baseAddress, "https://graph.microsoft.com/.default")
            .AddDistributedTokenCaches();

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });

        builder.Services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        var app = builder.Build();
        
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        app.Run();
    }
}