using System;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;
using Resto.Front.Api.V6;
using Resto.Front.Api.V6.Editors;
using Resto.Front.Api.V6.Events;
using RankedLoyaltyPlugin.Api;
using RankedLoyaltyPlugin.Api.Dto;
using RankedLoyaltyPlugin.Licensing;
using RankedLoyaltyPlugin.UI;

namespace RankedLoyaltyPlugin
{
    /// <summary>
    /// Main plugin class implementing IFrontPlugin.
    /// </summary>
    public sealed class RankedLoyaltyPlugin : IFrontPlugin
    {
        private readonly LoyaltyApiClient _apiClient;
        private readonly LicenseValidator _licenseValidator;
        private readonly string _companyId;
        private readonly string _registerId;
        private Guid? _clientId;
        private int _redeemable;
        private IOperationService _operations;
        private IViewManager _viewManager;
        private Guid _orderId;
        private const string PaySystemName = "Loyalty Points";

        /// <summary>
        /// Initializes plugin and subscribes to events.
        /// </summary>
        public RankedLoyaltyPlugin()
        {
            PluginContext.Log.Info("RankedLoyaltyPlugin starting");

            _operations = PluginContext.Operations;
            _viewManager = PluginContext.ViewManager;

            _companyId = ConfigurationManager.AppSettings["CompanyId"];

            string licensePath = ConfigurationManager.AppSettings["PluginLicensePath"];
            _licenseValidator = new LicenseValidator(licensePath);
            if (!_licenseValidator.Validate())
            {
                UiHelpers.ShowError("License validation failed");
                return;
            }

            _registerId = _licenseValidator.License.RegisterId;
            string apiBase = ConfigurationManager.AppSettings["ApiBaseUrl"];
            string apiKey = ConfigurationManager.AppSettings["ApiKey"];
            _apiClient = new LoyaltyApiClient(apiBase, apiKey, _registerId);

            _operations.RegisterPaymentSystem(new LoyaltyPointsPaymentProcessor(_apiClient));
            _viewManager.AddButtonToOrderEditScreen("Scan QR", OnScanQrClicked);
            _viewManager.AddButtonToPaymentScreen("Redeem Points", OnRedeemClicked, false);

            PluginContext.Notifications.OrderEditBarcodeScanned += OnBarcodeScanned;
            PluginContext.Notifications.ServiceChequePrinted += OnServiceChequePrinted;
            PluginContext.Notifications.OrderStorned += OnOrderStorned;
        }

        private void OnScanQrClicked()
        {
            if (!_licenseValidator.IsValid)
                return;
            var result = _viewManager.ShowExtendedKeyboardDialog("Scan QR", string.Empty, true);
            if (!result.Success)
                return;
            ProcessQr(result.Text);
        }

        private async void ProcessQr(string qr)
        {
            try
            {
                var client = await _apiClient.GetClientInfoAsync(_companyId, qr);
                _clientId = client.ClientId;
                _redeemable = client.Redeemable;
                UiHelpers.ShowInfo($"Balance {client.Balance} pts, available {client.Redeemable} pts");
                _viewManager.UpdatePaymentScreenButtonState("Redeem Points", _redeemable > 0);
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error(ex);                UiHelpers.ShowError("Failed to get client info");
            }
        }

        private void OnRedeemClicked()
        {
            if (!_clientId.HasValue)
                return;
            var max = _redeemable;
            var dialog = _viewManager.ShowExtendedKeyboardDialog("Redeem", max.ToString(), false);
            if (!dialog.Success)
                return;
            if (!int.TryParse(dialog.Text, out int points) || points <= 0)
                return;
            Redeem(points);
        }

        private async void Redeem(int points)
        {
            try
            {
                var order = PluginContext.Operations.GetOrders().FirstOrDefault(o => o.State == OrderState.Open);
                if (order == null)
                    return;
                _orderId = order.Id;

                var response = await _apiClient.RedeemAsync(new RedeemRequestDto
                {
                    CompanyId = _companyId,
                    RegisterId = _registerId,
                    ClientId = _clientId.Value,
                    OrderId = _orderId,
                    Points = points
                });

                _operations.AddExternalPayment(order, PaySystemName, points);
                _operations.SetPaymentSystemRollbackData(order.Id, PaySystemName, response.TransactionId);
                _viewManager.UpdatePaymentScreenButtonState("Redeem Points", false);
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error(ex);
                UiHelpers.ShowError("Redeem failed");
            }
        }

        private void OnBarcodeScanned(BarcodeScannedEventArgs args)
        {
            OnBarcodeReceived(args.Barcode);        }

        private void OnBarcodeReceived(string barcode)
        {
            ProcessQr(barcode);
        }

        private async void OnServiceChequePrinted(ServiceChequePrintedEventArgs args)
        {
            if (!_clientId.HasValue)
                return;
            try
            {
                await _apiClient.PurchaseAsync(new PurchaseRequestDto
                {
                    CompanyId = _companyId,
                    RegisterId = _registerId,
                    ClientId = _clientId.Value,
                    OrderId = args.OrderId,
                    Sum = args.Sums.Sum,
                    PointsRedeemed = args.Payments
                        .Where(p => p.PaymentType.Name == PaySystemName)
                        .Sum(p => (int)p.Sum)
                });
                UiHelpers.ShowInfo("Points accrued");
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error(ex);
            }
        }

        private async void OnOrderStorned(OrderStornedEventArgs args)
        {
            try
            {
                await _apiClient.CancelPurchaseAsync(args.OrderId);
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error(ex);
            }
        }

        /// <summary>
        /// Plugin cleanup.
        /// </summary>
        public void Dispose()
        {
            PluginContext.Notifications.OrderEditBarcodeScanned -= OnBarcodeScanned;
            PluginContext.Notifications.ServiceChequePrinted -= OnServiceChequePrinted;
            PluginContext.Notifications.OrderStorned -= OnOrderStorned;
        }
    }
}
