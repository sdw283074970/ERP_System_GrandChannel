namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStartedDateInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "StartedBy", c => c.String());
            AddColumn("dbo.FBAShipOrders", "StartedTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "StartedTime");
            DropColumn("dbo.FBAShipOrders", "StartedBy");
        }
    }
}
