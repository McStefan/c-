using System;

namespace RankedLoyaltyPlugin.Api.Dto
{
    /// <summary>
    /// Client info DTO.
    /// </summary>
    public class ClientInfoDto
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public int Balance { get; set; }
        public int Redeemable { get; set; }
    }
}
