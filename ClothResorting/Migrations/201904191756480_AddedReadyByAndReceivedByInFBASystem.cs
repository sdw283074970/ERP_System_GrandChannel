namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedReadyByAndReceivedByInFBASystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "ReadyBy", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "ReceivedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "ReceivedBy");
            DropColumn("dbo.FBAShipOrders", "ReadyBy");
        }
    }
}
