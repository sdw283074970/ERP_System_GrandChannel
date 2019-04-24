namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTotalCtnsAndPltsInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "TotalCtns", c => c.Int(nullable: false));
            AddColumn("dbo.FBAShipOrders", "TotalPlts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "TotalPlts");
            DropColumn("dbo.FBAShipOrders", "TotalCtns");
        }
    }
}
