namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedShipDateInShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShipOrders", "ShipDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShipOrders", "ShipDate");
        }
    }
}
