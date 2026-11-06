using Application.Common.Interfaces;
using Application.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Services.payment
{
    public class ZarinpalPaymentGateway : IPaymentGateway
    {
        private const string Unreachable =
            "ارتباط با درگاه پرداخت برقرار نشد؛ چند لحظه بعد دوباره تلاش کنید.";
        private const string UnreadableAnswer =
            "درگاه پرداخت پاسخ قابل‌فهمی نداد؛ چند لحظه بعد دوباره تلاش کنید.";
        private const string MerchantMissing =
            "شناسهٔ پذیرندهٔ درگاه پرداخت در تنظیمات سرور وارد نشده است.";

        /// <summary>Result codes of Zarinpal v4, in the words a customer or admin can act on.</summary>
        private static readonly Dictionary<int, string> GatewayMessages = new()
        {
            [-9] = "اطلاعات ارسال‌شده به درگاه پرداخت معتبر نیست (مبلغ یا آدرس بازگشت را بررسی کنید).",
            [-10] = "شناسهٔ پذیرنده یا آی‌پی سرور نزد درگاه پرداخت معتبر نیست.",
            [-11] = "شناسهٔ پذیرنده در درگاه پرداخت فعال نیست.",
            [-12] = "تعداد تلاش‌ها برای پرداخت بیش از حد بوده است؛ کمی بعد دوباره تلاش کنید.",
            [-15] = "درگاه پرداخت این فروشگاه به حالت تعلیق درآمده است.",
            [-16] = "سطح تأیید پذیرنده در درگاه پرداخت کافی نیست.",
            [-17] = "پذیرنده در سطح فعلی خود اجازهٔ این نوع پرداخت را ندارد.",
            [-50] = "مبلغ پرداخت‌شده با مبلغ سفارش یکسان نیست.",
            [-51] = "پرداخت در درگاه ناموفق بود؛ اگر مبلغی از حساب شما کم شده باشد، طبق قوانین بانکی برگشت داده می‌شود.",
            [-52] = "درگاه پرداخت با خطای غیرمنتظره روبه‌رو شد؛ با پشتیبانی درگاه تماس بگیرید.",
            [-53] = "این پرداخت متعلق به این فروشگاه نیست.",
            [-54] = "شناسهٔ پرداخت نامعتبر است یا منقضی شده است.",
            [-55] = "تراکنش موردنظر در درگاه پرداخت پیدا نشد.",
        };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZarinpalPaymentGateway> _logger;

        public ZarinpalPaymentGateway(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ZarinpalPaymentGateway> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PaymentRequestResult> RequestPaymentAsync(
            decimal amount,
            string callbackUrl,
            string description)
        {
            var merchantId = _configuration["Zarinpal:MerchantId"];
            if (string.IsNullOrWhiteSpace(merchantId))
                return new PaymentRequestResult { IsSuccess = false, ErrorMessage = MerchantMissing };

            var answer = await PostAsync(
                "https://api.zarinpal.com/pg/v4/payment/request.json",
                new
                {
                    merchant_id = merchantId,
                    amount = (int)amount,
                    callback_url = callbackUrl,
                    description = description
                });

            if (answer.Failure is not null)
                return new PaymentRequestResult { IsSuccess = false, ErrorMessage = answer.Failure };

            var authority = answer.Text("authority");
            if (answer.Code != 100 || string.IsNullOrWhiteSpace(authority))
                return new PaymentRequestResult { IsSuccess = false, ErrorMessage = Describe(answer.Code) };

            return new PaymentRequestResult
            {
                IsSuccess = true,
                Authority = authority,
                PaymentUrl = $"https://www.zarinpal.com/pg/StartPay/{authority}"
            };
        }

        public async Task<PaymentVerifyResult> VerifyPaymentAsync(
            string authority,
            decimal amount)
        {
            var merchantId = _configuration["Zarinpal:MerchantId"];
            if (string.IsNullOrWhiteSpace(merchantId))
                return new PaymentVerifyResult { IsSuccess = false, ErrorMessage = MerchantMissing };

            var answer = await PostAsync(
                "https://api.zarinpal.com/pg/v4/payment/verify.json",
                new
                {
                    merchant_id = merchantId,
                    amount = (int)amount,
                    authority = authority
                });

            if (answer.Failure is not null)
                return new PaymentVerifyResult { IsSuccess = false, ErrorMessage = answer.Failure };

            if (answer.Code is 100 or 101)
            {
                return new PaymentVerifyResult
                {
                    IsSuccess = true,
                    RefId = answer.Text("ref_id") ?? string.Empty
                };
            }

            return new PaymentVerifyResult { IsSuccess = false, ErrorMessage = Describe(answer.Code) };
        }

        private static string Describe(int? code) =>
            code is { } known && GatewayMessages.TryGetValue(known, out var message)
                ? message
                : "درگاه پرداخت این درخواست را نپذیرفت.";

        /// <summary>
        /// Zarinpal puts the outcome in data.code on success and errors.code on failure, and sends
        /// "data": [] and "errors": [] for the empty side, so the answer is read by hand.
        /// </summary>
        private async Task<GatewayAnswer> PostAsync(string url, object body)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(url, body);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Payment gateway unreachable: {Url}", url);
                return new GatewayAnswer(null, null, Unreachable);
            }

            using (response)
            {
                var text = await response.Content.ReadAsStringAsync();
                try
                {
                    using var document = JsonDocument.Parse(text);
                    var root = document.RootElement;
                    var data = root.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.Object
                        ? (JsonElement?)d.Clone()
                        : null;
                    var errors = root.TryGetProperty("errors", out var e) && e.ValueKind == JsonValueKind.Object
                        ? (JsonElement?)e.Clone()
                        : null;

                    var code = CodeOf(data) ?? CodeOf(errors);
                    if (code is null)
                    {
                        _logger.LogWarning("Payment gateway answer has no code ({Status}): {Body}",
                            (int)response.StatusCode, text);
                        return new GatewayAnswer(null, null, UnreadableAnswer);
                    }

                    if (errors is not null)
                        _logger.LogWarning("Payment gateway refused ({Code}): {Body}", code, text);

                    return new GatewayAnswer(code, data, null);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Payment gateway sent no JSON ({Status})", (int)response.StatusCode);
                    return new GatewayAnswer(null, null, UnreadableAnswer);
                }
            }
        }

        private static int? CodeOf(JsonElement? element) =>
            element is { } value
            && value.TryGetProperty("code", out var code)
            && code.TryGetInt32(out var number)
                ? number
                : null;

        private sealed record GatewayAnswer(int? Code, JsonElement? Data, string? Failure)
        {
            public string? Text(string name)
            {
                if (Data is not { } data || !data.TryGetProperty(name, out var value))
                    return null;

                return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
            }
        }
    }
}
