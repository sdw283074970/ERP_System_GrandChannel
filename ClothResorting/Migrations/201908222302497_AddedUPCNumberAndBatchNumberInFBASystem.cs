namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUPCNumberAndBatchNumberInFBASystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "BatchNumber", c => c.String());
            AddColumn("dbo.FBAOrderDetails", "UPCNumber", c => c.String());
            AddColumn("dbo.FBACartonLocations", "UPCNumber", c => c.String());
            AddColumn("dbo.FBAPickDetails", "UPCNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPickDetails", "UPCNumber");
            DropColumn("dbo.FBACartonLocations", "UPCNumber");
            DropColumn("dbo.FBAOrderDetails", "UPCNumber");
            DropColumn("dbo.FBAShipOrders", "BatchNumber");
        }
    }
}
