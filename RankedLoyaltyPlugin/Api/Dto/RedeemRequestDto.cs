using System;

namespace RankedLoyaltyPlugin.Api.Dto
{
    /// <summary>
    /// Redeem request DTO.
    /// </summary>
    public class RedeemRequestDto
    {
        public string CompanyId { get; set; }
        public string RegisterId { get; set; }
        public Guid ClientId { get; set; }
        public Guid OrderId { get; set; }
        public int Points { get; set; }
    }
}
