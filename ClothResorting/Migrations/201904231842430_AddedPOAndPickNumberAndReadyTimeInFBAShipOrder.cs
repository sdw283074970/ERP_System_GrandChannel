namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPOAndPickNumberAndReadyTimeInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "ReadyTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.FBAShipOrders", "PurchaseOrderNumber", c => c.String());
            AddColumn("dbo.FBAShipOrders", "PickNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "PickNumber");
            DropColumn("dbo.FBAShipOrders", "PurchaseOrderNumber");
            DropColumn("dbo.FBAShipOrders", "ReadyTime");
        }
    }
}
