namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOPOInPickingRecord : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PickingRecords", "OrderPurchaseOrder", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PickingRecords", "OrderPurchaseOrder");
        }
    }
}
