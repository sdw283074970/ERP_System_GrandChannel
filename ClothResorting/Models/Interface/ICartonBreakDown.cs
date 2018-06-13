namespace ClothResorting.Models.Interface
{
    public interface ICartonBreakDown
    {
        int? ActualPcs { get; set; }
        int? AvailablePcs { get; set; }
        int? CartonNumberRangeFrom { get; set; }
        int? CartonNumberRangeTo { get; set; }
        string Color { get; set; }
        int? ForecastPcs { get; set; }
        int Id { get; set; }
        string Location { get; set; }
        string PurchaseOrder { get; set; }
        string RunCode { get; set; }
        CartonDetail CartonDetail { get; set; }
        PackingList PackingList { get; set; }
        string Size { get; set; }
        string Style { get; set; }
    }
}