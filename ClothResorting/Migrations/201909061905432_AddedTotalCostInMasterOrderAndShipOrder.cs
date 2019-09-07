namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTotalCostInMasterOrderAndShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "TotalCost", c => c.Single(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "TotalCost", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "TotalCost");
            DropColumn("dbo.FBAShipOrders", "TotalCost");
        }
    }
}
