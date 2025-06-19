using Resto.Front.Api.V6;

namespace RankedLoyaltyPlugin.UI
{
    /// <summary>
    /// Helper methods for UI interactions.
    /// </summary>
    public static class UiHelpers
    {
        /// <summary>
        /// Show information popup.
        /// </summary>
        public static void ShowInfo(string text)
        {
            PluginContext.Operations.AddNotificationMessage(text, "Info");
        }

        /// <summary>
        /// Show error popup.
        /// </summary>
        public static void ShowError(string text)
        {
            PluginContext.Operations.AddNotificationMessage(text, "Error");
        }
    }
}
