namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedWarehouseLocationInMasterOrderAndShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "WarehouseLocation", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "WarehouseLocation", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "WarehouseLocation");
            DropColumn("dbo.FBAShipOrders", "WarehouseLocation");
        }
    }
}
