using System.Net;

namespace MaisonAeternum.IntegrationTests;

[Collection("Integration")]
public class AuthorizationTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Learner_CannotReachTheAdminArea()
    {
        var client = _factory.CreateHttpsClient();
        await RegisterAndSignInAsLearnerAsync(client);

        var response = await client.GetAsync("/Admin/Categories");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task SeededAdmin_CanReachTheAdminArea()
    {
        var client = _factory.CreateHttpsClient();

        var getLogin = await client.GetAsync("/Account/Login");
        var token = await getLogin.ExtractAntiForgeryTokenAsync();

        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = "admin@maisonaeternum.com",
            ["Password"] = "MaisonAdmin!2026"
        };
        await client.PostAsync("/Account/Login", new FormUrlEncodedContent(form));

        var response = await client.GetAsync("/Admin/Categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task RegisterAndSignInAsLearnerAsync(HttpClient client)
    {
        var email = $"itest.learner.{Guid.NewGuid():N}@example.com";
        const string password = "Guild#Bench4471";

        var getRegister = await client.GetAsync("/Account/Register");
        var token = await getRegister.ExtractAntiForgeryTokenAsync();

        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FirstName"] = "Learner",
            ["LastName"] = "OnlyAccess",
            ["Email"] = email,
            ["Password"] = password,
            ["ConfirmPassword"] = password,
            ["AcceptTerms"] = "true"
        };

        await client.PostAsync("/Account/Register", new FormUrlEncodedContent(form));
    }
}
