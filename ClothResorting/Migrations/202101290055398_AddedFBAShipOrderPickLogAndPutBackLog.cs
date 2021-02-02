namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFBAShipOrderPickLogAndPutBackLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBAShipOrderPickLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CartonId = c.Int(nullable: false),
                        Container = c.String(),
                        ShipmentId = c.String(),
                        AmzRefId = c.String(),
                        WarehouseCode = c.String(),
                        PickQuantity = c.String(),
                        QuantityBeforPick = c.String(),
                        QuantityAfterPick = c.String(),
                        FromLocation = c.String(),
                        FBAShipOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBAShipOrders", t => t.FBAShipOrder_Id)
                .Index(t => t.FBAShipOrder_Id);
            
            CreateTable(
                "dbo.FBAShipOrderPutBackLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CartonId = c.Int(nullable: false),
                        Container = c.String(),
                        ShipmentId = c.String(),
                        AmzRefId = c.String(),
                        WarehouseCode = c.String(),
                        PutBackQuantity = c.String(),
                        QuantityBeforePutBack = c.String(),
                        QuantityAfterPutBack = c.String(),
                        NewLocation = c.String(),
                        FBAShipOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBAShipOrders", t => t.FBAShipOrder_Id)
                .Index(t => t.FBAShipOrder_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBAShipOrderPutBackLogs", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropForeignKey("dbo.FBAShipOrderPickLogs", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropIndex("dbo.FBAShipOrderPutBackLogs", new[] { "FBAShipOrder_Id" });
            DropIndex("dbo.FBAShipOrderPickLogs", new[] { "FBAShipOrder_Id" });
            DropTable("dbo.FBAShipOrderPutBackLogs");
            DropTable("dbo.FBAShipOrderPickLogs");
        }
    }
}
