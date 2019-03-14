namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedChargingItemDetailsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChargingItemDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        Status = c.String(),
                        FBAMasterOrder_Id = c.Int(),
                        FBAShipOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBAMasterOrders", t => t.FBAMasterOrder_Id)
                .ForeignKey("dbo.FBAShipOrders", t => t.FBAShipOrder_Id)
                .Index(t => t.FBAMasterOrder_Id)
                .Index(t => t.FBAShipOrder_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChargingItemDetails", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropForeignKey("dbo.ChargingItemDetails", "FBAMasterOrder_Id", "dbo.FBAMasterOrders");
            DropIndex("dbo.ChargingItemDetails", new[] { "FBAShipOrder_Id" });
            DropIndex("dbo.ChargingItemDetails", new[] { "FBAMasterOrder_Id" });
            DropTable("dbo.ChargingItemDetails");
        }
    }
}
