using System;

namespace RankedLoyaltyPlugin.Licensing
{
    /// <summary>
    /// License information model.
    /// </summary>
    public class LicenseModel
    {
        public string CompanyId { get; set; }
        public string RegisterId { get; set; }
        public DateTime Expires { get; set; }
        public string Signature { get; set; }
    }
}
