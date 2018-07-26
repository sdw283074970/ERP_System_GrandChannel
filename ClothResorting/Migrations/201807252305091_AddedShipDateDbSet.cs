namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedShipDateDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ShipOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderPurchaseOrder = c.String(),
                        Customer = c.String(),
                        Address_1 = c.String(),
                        Address_2 = c.String(),
                        ShipDate = c.String(),
                        PickTicketsRange = c.String(),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.PickingRecords", "ShipOrder_Id", c => c.Int());
            CreateIndex("dbo.PickingRecords", "ShipOrder_Id");
            AddForeignKey("dbo.PickingRecords", "ShipOrder_Id", "dbo.ShipOrders", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PickingRecords", "ShipOrder_Id", "dbo.ShipOrders");
            DropIndex("dbo.PickingRecords", new[] { "ShipOrder_Id" });
            DropColumn("dbo.PickingRecords", "ShipOrder_Id");
            DropTable("dbo.ShipOrders");
        }
    }
}
