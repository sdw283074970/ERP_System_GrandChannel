namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatedFBAPickDetailModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBAPickDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Size = c.String(),
                        CtnsPerPlt = c.Int(nullable: false),
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
                        Status = c.String(),
                        FBACartonLocation_Id = c.Int(),
                        FBAPalletLocation_Id = c.Int(),
                        FBAShipOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBACartonLocations", t => t.FBACartonLocation_Id)
                .ForeignKey("dbo.FBAPalletLocations", t => t.FBAPalletLocation_Id)
                .ForeignKey("dbo.FBAShipOrders", t => t.FBAShipOrder_Id)
                .Index(t => t.FBACartonLocation_Id)
                .Index(t => t.FBAPalletLocation_Id)
                .Index(t => t.FBAShipOrder_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBAPickDetails", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropForeignKey("dbo.FBAPickDetails", "FBAPalletLocation_Id", "dbo.FBAPalletLocations");
            DropForeignKey("dbo.FBAPickDetails", "FBACartonLocation_Id", "dbo.FBACartonLocations");
            DropIndex("dbo.FBAPickDetails", new[] { "FBAShipOrder_Id" });
            DropIndex("dbo.FBAPickDetails", new[] { "FBAPalletLocation_Id" });
            DropIndex("dbo.FBAPickDetails", new[] { "FBACartonLocation_Id" });
            DropTable("dbo.FBAPickDetails");
        }
    }
}
