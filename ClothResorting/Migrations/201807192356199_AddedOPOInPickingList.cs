namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOPOInPickingList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PickingLists", "OrderPurchaseOrder", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PickingLists", "OrderPurchaseOrder");
        }
    }
}
