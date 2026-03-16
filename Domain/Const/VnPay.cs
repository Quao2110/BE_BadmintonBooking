using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Domain.Const
{
    public static class VnPay
    {
        public static string TmnCode = "CMIIO4M7";
        public static string HashSecret = "B6Q7RJXN0JW4L9742WTYG67A07CCMPH9";
        public static string BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public static string ReturnUrl = "myapp://payment-result";
        public static string Version = "2.1.0";
        public static string Command = "pay";
        public static string Locale = "vn";
        public static string CurrencyCode = "VND";
        public static string DateFormat = "yyyyMMddHHmmss";

        /// <summary>
        /// Tính toán chuỗi băm HMACSHA512 (Hex lowercase)
        /// </summary>
        public static string ComputeHash(string data)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(HashSecret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                // Convert sang Hex và viết thường để khớp với .toString() của Flutter
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static string BuildPaymentUrl(int amountVnd, string orderInfo, string txnRef, string? ipAddress = null)
        {
            var safeIp = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress!;
            var queryParams = new Dictionary<string, string>
            {
                { "vnp_Version", Version },
                { "vnp_Command", Command },
                { "vnp_TmnCode", TmnCode },
                { "vnp_Amount", (amountVnd * 100).ToString() },
                { "vnp_CurrCode", CurrencyCode },
                { "vnp_TxnRef", txnRef },
                { "vnp_ReturnUrl", ReturnUrl },
                { "vnp_Locale", Locale },
                { "vnp_OrderInfo", orderInfo },
                { "vnp_OrderType", "other" },
                { "vnp_IpAddr", safeIp },
                { "vnp_CreateDate", DateTime.Now.ToString(DateFormat)},
                { "vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString(DateFormat)},
            };

            // Sắp xếp theo bảng chữ cái (Ordinal) giống Flutter .sort()
            var sortedParams = queryParams
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .ToList();

            var queryString = BuildQueryString(sortedParams);
            var hashData = BuildHashData(sortedParams);
            var secureHash = ComputeHash(hashData);

            return $"{BaseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        public static bool VerifySignature(IReadOnlyDictionary<string, string> queryParams)
        {
            if (!queryParams.TryGetValue("vnp_SecureHash", out var secureHash) || string.IsNullOrWhiteSpace(secureHash))
            {
                return false;
            }

            var signData = BuildSignData(queryParams);
            var calculatedHash = ComputeHash(signData);

            return string.Equals(calculatedHash, secureHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildSignData(IReadOnlyDictionary<string, string> queryParams)
        {
            var sortedParams = queryParams
                .Where(kvp => kvp.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase)
                              && !string.IsNullOrWhiteSpace(kvp.Value)
                              && !string.Equals(kvp.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase)
                              && !string.Equals(kvp.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .ToList();

            return BuildHashData(sortedParams);
        }

        /// <summary>
        /// Logic này được ép theo Flutter: Encode cả Value khi băm
        /// </summary>
        private static string BuildHashData(IEnumerable<KeyValuePair<string, string>> sortedParams)
        {
            // Dùng Uri.EscapeDataString để khoảng trắng ra %20, dấu / ra %2F... 
            // Giống hệt Uri.encodeQueryComponent của Flutter
            return string.Join("&", sortedParams
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        }

        private static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> sortedParams)
        {
            // Encode cả Key và Value cho chuỗi URL
            return string.Join("&", sortedParams
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        }
    }
}