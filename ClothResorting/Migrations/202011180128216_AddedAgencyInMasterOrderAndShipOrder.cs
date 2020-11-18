namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAgencyInMasterOrderAndShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "Agency", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "Agency", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "Agency");
            DropColumn("dbo.FBAShipOrders", "Agency");
        }
    }
}
