namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCloseDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "CloseDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.FBAShipOrders", "CloseDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "CloseDate");
            DropColumn("dbo.FBAMasterOrders", "CloseDate");
        }
    }
}
