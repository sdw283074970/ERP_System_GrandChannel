namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedThreeNewPropertiesInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "BOLNumber", c => c.String());
            AddColumn("dbo.FBAShipOrders", "Carrier", c => c.String());
            AddColumn("dbo.FBAShipOrders", "ETS", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "ETS");
            DropColumn("dbo.FBAShipOrders", "Carrier");
            DropColumn("dbo.FBAShipOrders", "BOLNumber");
        }
    }
}
