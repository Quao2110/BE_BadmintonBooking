using System.Security.Cryptography;
using System.Text;
using Domain.Const;
using Xunit;

namespace Domain.Tests;

public class VnPaySignatureTests
{
    [Fact]
    public void BuildPaymentUrl_ShouldMatchDemoHelperHash_WithSpecialCharacters()
    {
        // Arrange
        var amountVnd = 498000;
        var txnRef = Guid.NewGuid().ToString();
        var orderInfo = "Thanh toan don hang test A&B / 123";
        var ipAddr = "127.0.0.1";

        // Act
        var url = VnPay.BuildPaymentUrl(amountVnd, orderInfo, txnRef, ipAddr);
        var uri = new Uri(url);
        var queryParams = ParseQuery(uri.Query);

        // Assert
        Assert.True(queryParams.ContainsKey("vnp_SecureHash"));

        var signParams = queryParams
            .Where(kvp => kvp.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase)
                          && !string.Equals(kvp.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                          && !string.Equals(kvp.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(k => k.Key, v => v.Value, StringComparer.Ordinal);

        var expectedHashData = BuildHashDataLikeDemo(signParams);
        var expectedHash = HmacSha512(VnPay.HashSecret, expectedHashData);

        Assert.Equal(expectedHash, queryParams["vnp_SecureHash"], ignoreCase: true);
    }

    [Fact]
    public void VerifySignature_ShouldReturnTrue_ForUrlGeneratedByBuildPaymentUrl()
    {
        // Arrange
        var url = VnPay.BuildPaymentUrl(
            amountVnd: 50000,
            orderInfo: "Thanh toan don hang test",
            txnRef: Guid.NewGuid().ToString(),
            ipAddress: "127.0.0.1");

        var uri = new Uri(url);
        var queryParams = ParseQuery(uri.Query)
            .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);

        // Act
        var isValid = VnPay.VerifySignature(queryParams);

        // Assert
        Assert.True(isValid);
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var trimmed = query.TrimStart('?');

        if (string.IsNullOrWhiteSpace(trimmed))
            return result;

        foreach (var pair in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = pair.IndexOf('=');
            if (idx < 0)
            {
                result[Uri.UnescapeDataString(pair)] = string.Empty;
                continue;
            }

            var key = Uri.UnescapeDataString(pair[..idx]);
            var value = Uri.UnescapeDataString(pair[(idx + 1)..]);
            result[key] = value;
        }

        return result;
    }

    // Matches demo_vnpay/lib/vnpay/vnpay_helper.dart:_buildHashData
    private static string BuildHashDataLikeDemo(Dictionary<string, string> parameters)
    {
        var keys = parameters.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
        var items = new List<string>();

        foreach (var key in keys)
        {
            var value = parameters[key] ?? string.Empty;
            if (string.IsNullOrEmpty(value))
                continue;

            items.Add($"{key}={Uri.EscapeDataString(value)}");
        }

        return string.Join("&", items);
    }

    private static string HmacSha512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
