namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrderTypeFieldInFBAShipOrderModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "OrderType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "OrderType");
        }
    }
}
