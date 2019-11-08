namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPODStatusInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "PODStatus", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "PODStatus");
        }
    }
}
