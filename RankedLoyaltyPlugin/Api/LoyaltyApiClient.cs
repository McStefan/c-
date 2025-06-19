using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RankedLoyaltyPlugin.Api.Dto;

namespace RankedLoyaltyPlugin.Api
{
    /// <summary>
    /// Client for loyalty API.
    /// </summary>
    public class LoyaltyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _registerId;

        /// <summary>
        /// Initializes new API client.
        /// </summary>
        public LoyaltyApiClient(string baseUrl, string apiKey, string registerId)
        {
            _registerId = registerId;
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Api-Key", apiKey);
        }

        /// <summary>
        /// Get client info.
        /// </summary>
        public async Task<ClientInfoDto> GetClientInfoAsync(string companyId, string qr)
        {
            var resp = await _httpClient.GetAsync($"/api/v1/loyalty/client?qr={qr}&companyId={companyId}");
            resp.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<ClientInfoDto>(await resp.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Redeem points.
        /// </summary>
        public async Task<RedeemResponseDto> RedeemAsync(RedeemRequestDto dto)
        {
            var resp = await _httpClient.PostAsync("/api/v1/loyalty/redeem", Serialize(dto));
            resp.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<RedeemResponseDto>(await resp.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Rollback redeem.
        /// </summary>
        public async Task RollbackRedeemAsync(string transactionId)
        {
            await _httpClient.PostAsync("/api/v1/loyalty/redeem/rollback", Serialize(new { transactionId }));
        }

        /// <summary>
        /// Purchase complete.
        /// </summary>
        public async Task<PurchaseResponseDto> PurchaseAsync(PurchaseRequestDto dto)
        {
            var resp = await _httpClient.PostAsync("/api/v1/loyalty/purchase", Serialize(dto));
            resp.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<PurchaseResponseDto>(await resp.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Cancel purchase.
        /// </summary>
        public async Task CancelPurchaseAsync(Guid orderId)
        {
            await _httpClient.PostAsync("/api/v1/loyalty/purchase/cancel", Serialize(new { originalOrderId = orderId }));
        }

        private StringContent Serialize(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }
    }
}
