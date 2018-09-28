namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrderTypeInShipOrderDbSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShipOrders", "OrderType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShipOrders", "OrderType");
        }
    }
}
