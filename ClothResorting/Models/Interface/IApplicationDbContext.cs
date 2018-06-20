using System.Data.Entity;

namespace ClothResorting.Models.Interface
{
    public interface IApplicationDbContext
    {
        DbSet<CartonBreakDown> CartonBreakDowns { get; set; }
        DbSet<Measurement> Measurements { get; set; }
        DbSet<CartonDetail> CartonDetails { get; set; }
        DbSet<PurchaseOrderOverview> PurchaseOrderOverview { get; set; }
        DbSet<PreReceiveOrder> PreReceiveOrders { get; set; }
        DbSet<SizeRatio> SizeRatios { get; set; }
    }
}