namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeletedAllDbSets : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SilkIconCartonDetails", "DistributionCenter_Id", "dbo.DistributionCenters");
            DropForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails");
            DropForeignKey("dbo.SilKIconPODetails", "Id", "dbo.SilkIconPackingLists");
            DropForeignKey("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews");
            DropForeignKey("dbo.SilkIconPreReceiveOrders", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews");
            DropIndex("dbo.SilkIconCartonDetails", new[] { "DistributionCenter_Id" });
            DropIndex("dbo.SilkIconCartonDetails", new[] { "SilKIconPODetail_Id" });
            DropIndex("dbo.SilkIconPackingLists", new[] { "SilkIconPackingListOverView_Id" });
            DropIndex("dbo.SilKIconPODetails", new[] { "Id" });
            DropIndex("dbo.SilkIconPreReceiveOrders", new[] { "SilkIconPackingListOverView_Id" });
            DropTable("dbo.DistributionCenters");
            DropTable("dbo.SilkIconCartonDetails");
            DropTable("dbo.SilkIconPackingListOverViews");
            DropTable("dbo.SilkIconPackingLists");
            DropTable("dbo.SilKIconPODetails");
            DropTable("dbo.SilkIconPreReceiveOrders");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SilkIconPreReceiveOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerName = c.String(),
                        CreatDate = c.DateTime(),
                        SilkIconPackingListOverView_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SilKIconPODetails",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        PurchaseOrderNumber = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Total = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SilkIconPackingLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrderNumber = c.String(),
                        StyleNumber = c.String(),
                        Quantity = c.String(),
                        Cartons = c.String(),
                        NetWeight = c.String(),
                        GrossWeight = c.String(),
                        CBM = c.String(),
                        Vol = c.String(),
                        SilkIconPackingListOverView_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SilkIconPackingListOverViews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InvoiceNumber = c.String(),
                        Date = c.DateTime(),
                        TotalCartons = c.String(),
                        Vol = c.String(),
                        TotalGrossWeight = c.String(),
                        TotalNetWeight = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SilkIconCartonDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CartonNumberRangeFrom = c.String(),
                        CartonNumberRangeTo = c.String(),
                        DistrubutionCenterName = c.String(),
                        Long = c.String(),
                        Width = c.String(),
                        Height = c.String(),
                        GrossWeight = c.String(),
                        NetWeight = c.String(),
                        S = c.String(),
                        M = c.String(),
                        L = c.String(),
                        XL = c.String(),
                        XXL = c.String(),
                        XXXL = c.String(),
                        TotalCartons = c.String(),
                        Total = c.String(),
                        DistributionCenter_Id = c.Int(),
                        SilKIconPODetail_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DistributionCenters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.SilkIconPreReceiveOrders", "SilkIconPackingListOverView_Id");
            CreateIndex("dbo.SilKIconPODetails", "Id");
            CreateIndex("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id");
            CreateIndex("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id");
            CreateIndex("dbo.SilkIconCartonDetails", "DistributionCenter_Id");
            AddForeignKey("dbo.SilkIconPreReceiveOrders", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews", "Id");
            AddForeignKey("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews", "Id");
            AddForeignKey("dbo.SilKIconPODetails", "Id", "dbo.SilkIconPackingLists", "Id");
            AddForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails", "Id");
            AddForeignKey("dbo.SilkIconCartonDetails", "DistributionCenter_Id", "dbo.DistributionCenters", "Id");
        }
    }
}
