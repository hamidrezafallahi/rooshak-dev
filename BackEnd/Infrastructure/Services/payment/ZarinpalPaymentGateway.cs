using Application.Common.Interfaces;
using Application.Dtos;
using Infrastructure.Services.payment.models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.Services.payment
{
    public class ZarinpalPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ZarinpalPaymentGateway(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PaymentRequestResult> RequestPaymentAsync(
            decimal amount,
            string callbackUrl,
            string description)
        {
            var merchantId = _configuration["Zarinpal:MerchantId"];

            var request = new
            {
                merchant_id = merchantId,
                amount = (int)amount,
                callback_url = callbackUrl,
                description = description
            };

            var response = await _httpClient
                .PostAsJsonAsync("https://api.zarinpal.com/pg/v4/payment/request.json", request);

            if (!response.IsSuccessStatusCode)
                return new PaymentRequestResult { IsSuccess = false };

            var result = await response.Content.ReadFromJsonAsync<ZarinpalRequestResponse>();

            if (result?.data?.authority == null)
                return new PaymentRequestResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Gateway error"
                };

            return new PaymentRequestResult
            {
                IsSuccess = true,
                Authority = result.data.authority,
                PaymentUrl = $"https://www.zarinpal.com/pg/StartPay/{result.data.authority}"
            };
        }

        public async Task<PaymentVerifyResult> VerifyPaymentAsync(
            string authority,
            decimal amount)
        {
            var merchantId = _configuration["Zarinpal:MerchantId"];

            var request = new
            {
                merchant_id = merchantId,
                amount = (int)amount,
                authority = authority
            };

            var response = await _httpClient
                .PostAsJsonAsync("https://api.zarinpal.com/pg/v4/payment/verify.json", request);

            var result = await response.Content.ReadFromJsonAsync<ZarinpalVerifyResponse>();

            if (result?.data?.code == 100 || result?.data?.code == 101)
            {
                return new PaymentVerifyResult
                {
                    IsSuccess = true,
                    RefId = result.data.ref_id.ToString()
                };
            }

            return new PaymentVerifyResult
            {
                IsSuccess = false,
                ErrorMessage = "Payment failed"
            };
        }
    }
}
