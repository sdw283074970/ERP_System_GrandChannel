using System.Data.Entity;

namespace ClothResorting.Models.Interface
{
    public interface IApplicationDbContext
    {
        DbSet<CartonBreakDown> CartonBreakDowns { get; set; }
        DbSet<Measurement> Measurements { get; set; }
        DbSet<SilkIconCartonDetail> SilkIconCartonDetails { get; set; }
        DbSet<SilkIconPackingList> SilkIconPackingLists { get; set; }
        DbSet<SilkIconPreReceiveOrder> SilkIconPreReceiveOrders { get; set; }
        DbSet<SizeRatio> SizeRatios { get; set; }
    }
}