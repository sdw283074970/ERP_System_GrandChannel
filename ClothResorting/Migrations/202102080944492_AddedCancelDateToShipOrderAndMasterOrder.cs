namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCancelDateToShipOrderAndMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "CancelDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "CancelDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "CancelDate");
            DropColumn("dbo.FBAShipOrders", "CancelDate");
        }
    }
}
