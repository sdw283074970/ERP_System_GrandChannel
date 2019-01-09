namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFBACartonLocationAndFBAPalletLocationModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBACartonLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AvaliableCtns = c.Int(nullable: false),
                        PickingCtns = c.Int(nullable: false),
                        ShippedCtns = c.Int(nullable: false),
                        Location = c.String(),
                        Container = c.String(),
                        ShipmentId = c.String(),
                        AmzRefId = c.String(),
                        WarehouseCode = c.String(),
                        ActualCBM = c.Single(nullable: false),
                        ActualGrossWeight = c.Single(nullable: false),
                        ActualQuantity = c.Single(nullable: false),
                        Comment = c.String(),
                        FBAOrderDetail_Id = c.Int(),
                        FBAPalletLocation_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBAOrderDetails", t => t.FBAOrderDetail_Id)
                .ForeignKey("dbo.FBAPalletLocations", t => t.FBAPalletLocation_Id)
                .Index(t => t.FBAOrderDetail_Id)
                .Index(t => t.FBAPalletLocation_Id);
            
            CreateTable(
                "dbo.FBAPalletLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OriginalPallets = c.Int(nullable: false),
                        AvailablePalltes = c.Int(nullable: false),
                        PickingPallets = c.Int(nullable: false),
                        ShippedPallets = c.Int(nullable: false),
                        Container = c.String(),
                        ShipmentId = c.String(),
                        AmzRefId = c.String(),
                        WarehouseCode = c.String(),
                        ActualCBM = c.Single(nullable: false),
                        ActualGrossWeight = c.Single(nullable: false),
                        ActualQuantity = c.Single(nullable: false),
                        Comment = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBACartonLocations", "FBAPalletLocation_Id", "dbo.FBAPalletLocations");
            DropForeignKey("dbo.FBACartonLocations", "FBAOrderDetail_Id", "dbo.FBAOrderDetails");
            DropIndex("dbo.FBACartonLocations", new[] { "FBAPalletLocation_Id" });
            DropIndex("dbo.FBACartonLocations", new[] { "FBAOrderDetail_Id" });
            DropTable("dbo.FBAPalletLocations");
            DropTable("dbo.FBACartonLocations");
        }
    }
}
