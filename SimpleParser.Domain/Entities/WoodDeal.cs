using System;

namespace SimpleParser.Domain.Entities
{
    public class WoodDeal
    {
        public string DealNumber { get; set; }
        public string SellerName { get; set; }
        public string SellerInn { get; set; }
        public string BuyerName { get; set; }
        public string BuyerInn { get; set; }
        public DateTime DealDate { get; set; }
        public double WoodVolumeSeller { get; set; }
        public double WoodVolumeBuyer { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is WoodDeal deal)) return false;

            return DealNumber == deal.DealNumber &&
                SellerName == deal.SellerName &&
                SellerInn == deal.SellerInn &&
                BuyerName == deal.BuyerName &&
                BuyerInn == deal.BuyerInn &&
                DealDate == deal.DealDate &&
                WoodVolumeSeller == deal.WoodVolumeSeller &&
                WoodVolumeBuyer == deal.WoodVolumeBuyer;
        }
    }
}
