namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSeveralFieldsInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "TotalPltsFromInventory", c => c.Int(nullable: false));
            AddColumn("dbo.FBAShipOrders", "TotalNewPlts", c => c.Int(nullable: false));
            AddColumn("dbo.FBAShipOrders", "Instructor", c => c.String());
            AddColumn("dbo.FBAShipOrders", "Lot", c => c.String());
            AddColumn("dbo.FBAShipOrders", "Comment", c => c.String());
            DropColumn("dbo.FBAShipOrders", "PickDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBAShipOrders", "PickDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.FBAShipOrders", "Comment");
            DropColumn("dbo.FBAShipOrders", "Lot");
            DropColumn("dbo.FBAShipOrders", "Instructor");
            DropColumn("dbo.FBAShipOrders", "TotalNewPlts");
            DropColumn("dbo.FBAShipOrders", "TotalPltsFromInventory");
        }
    }
}
