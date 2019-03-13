namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTotalAmountForFBAMAsterOrderAndShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "TotalAmount", c => c.Single(nullable: false));
            AddColumn("dbo.FBAShipOrders", "TotalAmount", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "TotalAmount");
            DropColumn("dbo.FBAMasterOrders", "TotalAmount");
        }
    }
}
