using System.Net;

namespace MaisonAeternum.IntegrationTests;

[Collection("Integration")]
public class HomePageTests
{
    private readonly CustomWebApplicationFactory _factory;

    public HomePageTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHome_ReturnsSuccess_AndRendersTheLandingPage()
    {
        var client = _factory.CreateHttpsClient();

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Maison Aeternum", body);
    }

    [Fact]
    public async Task GetLogin_ReturnsSuccess()
    {
        var client = _factory.CreateHttpsClient();

        var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/Admin/Categories")]
    [InlineData("/Learner/Dashboard")]
    public async Task GetProtectedPage_Anonymous_RedirectsToLogin(string path)
    {
        var client = _factory.CreateHttpsClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
    }
}
