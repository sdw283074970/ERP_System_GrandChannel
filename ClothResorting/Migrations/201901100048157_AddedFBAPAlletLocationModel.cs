namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFBAPAlletLocationModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBAPalletLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrossWeightPerPlt = c.Single(nullable: false),
                        CBMPerPlt = c.Single(nullable: false),
                        CtnsPerPlt = c.Int(nullable: false),
                        OriginalPlts = c.Int(nullable: false),
                        AvailablePlts = c.Int(nullable: false),
                        PickingPlts = c.Int(nullable: false),
                        ShippedPlts = c.Int(nullable: false),
                        Location = c.String(),
                        GrandNumber = c.String(),
                        Container = c.String(),
                        ShipmentId = c.String(),
                        AmzRefId = c.String(),
                        WarehouseCode = c.String(),
                        ActualCBM = c.Single(nullable: false),
                        ActualGrossWeight = c.Single(nullable: false),
                        ActualQuantity = c.Int(nullable: false),
                        Comment = c.String(),
                        HowToDeliver = c.String(),
                        FBAPallet_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBAPallets", t => t.FBAPallet_Id)
                .Index(t => t.FBAPallet_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBAPalletLocations", "FBAPallet_Id", "dbo.FBAPallets");
            DropIndex("dbo.FBAPalletLocations", new[] { "FBAPallet_Id" });
            DropTable("dbo.FBAPalletLocations");
        }
    }
}
