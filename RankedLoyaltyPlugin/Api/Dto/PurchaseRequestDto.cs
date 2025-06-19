using System;

namespace RankedLoyaltyPlugin.Api.Dto
{
    /// <summary>
    /// Purchase request DTO.
    /// </summary>
    public class PurchaseRequestDto
    {
        public string CompanyId { get; set; }
        public string RegisterId { get; set; }
        public Guid ClientId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Sum { get; set; }
        public int PointsRedeemed { get; set; }
    }
}
