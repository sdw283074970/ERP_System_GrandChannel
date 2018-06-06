namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReAddedAllDbSetsAgain : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DistributionCenters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SilkIconCartonDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CartonNumberRangeFrom = c.String(),
                        CartonNumberRangeTo = c.String(),
                        SumOfCarton = c.String(),
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
                        PcsPerCartons = c.String(),
                        TotalPcs = c.String(),
                        DistributionCenter_Id = c.Int(),
                        SilKIconPODetail_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DistributionCenters", t => t.DistributionCenter_Id)
                .ForeignKey("dbo.SilKIconPODetails", t => t.SilKIconPODetail_Id)
                .Index(t => t.DistributionCenter_Id)
                .Index(t => t.SilKIconPODetail_Id);
            
            CreateTable(
                "dbo.SilkIconPackingListOverViews",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        InvoiceNumber = c.String(),
                        Date = c.DateTime(),
                        TotalCartons = c.String(),
                        TotalGrossWeight = c.String(),
                        TotalNetWeight = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SilkIconPreReceiveOrders", t => t.Id)
                .Index(t => t.Id);
            
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
                        SilkIconPackingListOverView_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SilkIconPackingListOverViews", t => t.SilkIconPackingListOverView_Id)
                .Index(t => t.SilkIconPackingListOverView_Id);
            
            CreateTable(
                "dbo.SilKIconPODetails",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        PurchaseOrder_StyleNumber = c.String(),
                        Color = c.String(),
                        TotalCartons = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SilkIconPackingLists", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.SilkIconPreReceiveOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerName = c.String(),
                        CreatDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SilkIconPackingListOverViews", "Id", "dbo.SilkIconPreReceiveOrders");
            DropForeignKey("dbo.SilKIconPODetails", "Id", "dbo.SilkIconPackingLists");
            DropForeignKey("dbo.SilkIconCartonDetails", "SilKIconPODetail_Id", "dbo.SilKIconPODetails");
            DropForeignKey("dbo.SilkIconPackingLists", "SilkIconPackingListOverView_Id", "dbo.SilkIconPackingListOverViews");
            DropForeignKey("dbo.SilkIconCartonDetails", "DistributionCenter_Id", "dbo.DistributionCenters");
            DropIndex("dbo.SilKIconPODetails", new[] { "Id" });
            DropIndex("dbo.SilkIconPackingLists", new[] { "SilkIconPackingListOverView_Id" });
            DropIndex("dbo.SilkIconPackingListOverViews", new[] { "Id" });
            DropIndex("dbo.SilkIconCartonDetails", new[] { "SilKIconPODetail_Id" });
            DropIndex("dbo.SilkIconCartonDetails", new[] { "DistributionCenter_Id" });
            DropTable("dbo.SilkIconPreReceiveOrders");
            DropTable("dbo.SilKIconPODetails");
            DropTable("dbo.SilkIconPackingLists");
            DropTable("dbo.SilkIconPackingListOverViews");
            DropTable("dbo.SilkIconCartonDetails");
            DropTable("dbo.DistributionCenters");
        }
    }
}
