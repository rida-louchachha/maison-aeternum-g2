using System.Net;

namespace MaisonAeternum.IntegrationTests;

[Collection("Integration")]
public class AuthenticationTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthenticationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ThenLogin_SignsTheApprenticeInWithoutEmailVerification()
    {
        var client = _factory.CreateHttpsClient();
        var email = $"itest.{Guid.NewGuid():N}@example.com";
        const string password = "Guild#Bench4471";

        var getRegister = await client.GetAsync("/Account/Register");
        var token = await getRegister.ExtractAntiForgeryTokenAsync();

        var registerForm = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FirstName"] = "Integration",
            ["LastName"] = "Tester",
            ["Email"] = email,
            ["Password"] = password,
            ["ConfirmPassword"] = password,
            ["AcceptTerms"] = "true"
        };

        var registerResponse = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(registerForm));

        // Registration signs the learner in immediately (no email confirmation step) and redirects home.
        Assert.Equal(HttpStatusCode.Redirect, registerResponse.StatusCode);
        Assert.Equal("/", registerResponse.Headers.Location!.ToString());

        // A brand-new client (fresh cookie jar) can now log in with the same credentials.
        var loginClient = _factory.CreateHttpsClient();
        var getLogin = await loginClient.GetAsync("/Account/Login");
        var loginToken = await getLogin.ExtractAntiForgeryTokenAsync();

        var loginForm = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = loginToken,
            ["Email"] = email,
            ["Password"] = password
        };

        var loginResponse = await loginClient.PostAsync("/Account/Login", new FormUrlEncodedContent(loginForm));

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReRendersFormWithError()
    {
        var client = _factory.CreateHttpsClient();
        var getLogin = await client.GetAsync("/Account/Login");
        var token = await getLogin.ExtractAntiForgeryTokenAsync();

        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = "nonexistent.user@example.com",
            ["Password"] = "WrongPassword!123"
        };

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(form));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // re-renders the form, does not redirect
        Assert.Contains("Incorrect email or password", body);
    }

    [Fact]
    public async Task Register_WithPasswordContainingOwnName_IsRejected()
    {
        var client = _factory.CreateHttpsClient();
        var getRegister = await client.GetAsync("/Account/Register");
        var token = await getRegister.ExtractAntiForgeryTokenAsync();

        var form = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FirstName"] = "Camille",
            ["LastName"] = "Bertrand",
            ["Email"] = $"camille.{Guid.NewGuid():N}@example.com",
            ["Password"] = "Camille#Strong99",
            ["ConfirmPassword"] = "Camille#Strong99",
            ["AcceptTerms"] = "true"
        };

        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(form));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // re-rendered with a validation error, not redirected
        Assert.Contains("should not contain your own name", body);
    }
}
