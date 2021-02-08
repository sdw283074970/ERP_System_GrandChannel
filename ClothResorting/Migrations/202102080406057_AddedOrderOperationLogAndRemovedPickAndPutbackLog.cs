namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrderOperationLogAndRemovedPickAndPutbackLog : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FBAShipOrderPickLogs", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropForeignKey("dbo.FBAShipOrderPutBackLogs", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropIndex("dbo.FBAShipOrderPickLogs", new[] { "FBAShipOrder_Id" });
            DropIndex("dbo.FBAShipOrderPutBackLogs", new[] { "FBAShipOrder_Id" });
            CreateTable(
                "dbo.OrderOperationLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        Type = c.String(),
                        OperationDate = c.DateTime(nullable: false),
                        Operator = c.String(),
                        FBAShipOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBAShipOrders", t => t.FBAShipOrder_Id)
                .Index(t => t.FBAShipOrder_Id);
            
            DropTable("dbo.FBAShipOrderPickLogs");
            DropTable("dbo.FBAShipOrderPutBackLogs");
        }
        
        public override void Down()
        {
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
                        OriginalLocation = c.String(),
                        NewLocation = c.String(),
                        FBAShipOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
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
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.OrderOperationLogs", "FBAShipOrder_Id", "dbo.FBAShipOrders");
            DropIndex("dbo.OrderOperationLogs", new[] { "FBAShipOrder_Id" });
            DropTable("dbo.OrderOperationLogs");
            CreateIndex("dbo.FBAShipOrderPutBackLogs", "FBAShipOrder_Id");
            CreateIndex("dbo.FBAShipOrderPickLogs", "FBAShipOrder_Id");
            AddForeignKey("dbo.FBAShipOrderPutBackLogs", "FBAShipOrder_Id", "dbo.FBAShipOrders", "Id");
            AddForeignKey("dbo.FBAShipOrderPickLogs", "FBAShipOrder_Id", "dbo.FBAShipOrders", "Id");
        }
    }
}
