using System.Text.RegularExpressions;

namespace MaisonAeternum.IntegrationTests;

internal static class HtmlHelpers
{
    private static readonly Regex TokenRegex = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.Compiled);

    public static async Task<string> ExtractAntiForgeryTokenAsync(this HttpResponseMessage response)
    {
        var html = await response.Content.ReadAsStringAsync();
        var match = TokenRegex.Match(html);
        if (!match.Success)
            throw new InvalidOperationException("No antiforgery token found in the response HTML.");

        return match.Groups[1].Value;
    }
}
