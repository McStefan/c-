using System;
using System.Threading.Tasks;
using Resto.Front.Api.V6;
using Resto.Front.Api.V6.Data.Payments;
using Resto.Front.Api.V6.Events;
using RankedLoyaltyPlugin.Api;

namespace RankedLoyaltyPlugin
{
    /// <summary>
    /// Payment processor for loyalty points.
    /// </summary>
    public class LoyaltyPointsPaymentProcessor : IExternalPaymentProcessor
    {
        private readonly LoyaltyApiClient _apiClient;

        /// <summary>
        /// Initializes new processor.
        /// </summary>
        public LoyaltyPointsPaymentProcessor(LoyaltyApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public string Code => "LOYALTY_POINTS";

        /// <inheritdoc />
        public string Name => "Loyalty Points";

        /// <inheritdoc />
        public Task<IPaymentResult> Pay(IPaymentDataContext context)
        {
            PluginContext.Log.Info($"Paying loyalty points {context.Sum}");
            return Task.FromResult<IPaymentResult>(PluginContext.CreatePaymentResult("OK"));
        }

        /// <inheritdoc />
        public async Task<IPaymentResult> ReturnPayment(IPaymentDataContext context)
        {
            try
            {
                var txId = context.GetRollbackData<string>();
                if (!string.IsNullOrEmpty(txId))
                    await _apiClient.RollbackRedeemAsync(txId);
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error(ex);
            }
            return PluginContext.CreatePaymentResult("OK");
        }
    }
}
