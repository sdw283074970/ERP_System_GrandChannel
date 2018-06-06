namespace ClothResorting.Migrations
{
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ClothResorting.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ClothResorting.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            //context.SilkIconPreReceiveOrders.AddOrUpdate(
            //    s => s.CustomerName,
            //    new SilkIconPreReceiveOrder
            //    {
            //        CustomerName = "SILK-ICON",
            //        CreatDate = DateTime.Today,
            //        SilkIconPackingListOverView = new SilkIconPackingListOverView
            //        {
            //            Date = DateTime.Today,
            //            InvoiceNumber = "UNKOWN",
            //            TotalCartons = "TOTAL PACKED IN 1661CTNS",
            //            TotalGrossWeight = "TOTAL GROSS WEIGHT:11219.50KGS",
            //            TotalNetWeight = "TOTAL NET WEIGHT:8674.00KGS"
            //        }
            //    }
            //);

            //context.SilkIconPackingLists.AddOrUpdate(
            //    s => s.PurchaseOrderNumber,
            //    new SilkIconPackingList
            //    {
            //        PurchaseOrderNumber = "PO#171119A",
            //        StyleNumber = "STYLE NO.:100006078-CORAL",
            //        Quantity = "2866",
            //        Cartons = "187 cartons",
            //        NetWeight = "721.5",
            //        GrossWeight = "1002.0",
            //        CBM = "127.6",
            //        SilkIconPODetail = new SilKIconPODetail
            //        {
            //            PurchaseOrder_StyleNumber = "100006078-171119A",
            //            TotalCartons = "TOTAL PACKED IN 187CTNS",
            //            Color = "CORAL"
            //        }
            //    }
            //);

            //context.SilkIconCartonDetails.AddOrUpdate(
            //    s => s.CartonNumberRangeFrom,
            //    new SilkIconCartonDetail
            //    {
            //        CartonNumberRangeFrom = "1",
            //        CartonNumberRangeTo = "20",
            //        SumOfCarton = "20",
            //        S = "1",
            //        M = "2",
            //        L = "3",
            //        XL = "4",
            //        XXL = "2",
            //        XXXL = null,
            //        Long = "71",
            //        Width = "58",
            //        Height = "13",
            //        NetWeight = "3.0",
            //        GrossWeight = "4.5",
            //        PcsPerCartons = "12",
            //        TotalPcs = "240",
            //        DistrubutionCenterName = "KA"
            //    }
            //);
        }
    }
}
