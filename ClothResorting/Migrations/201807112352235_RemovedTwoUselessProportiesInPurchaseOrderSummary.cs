namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedTwoUselessProportiesInPurchaseOrderSummary : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseOrderSummaries", "InventoryCtn");
            DropColumn("dbo.PurchaseOrderSummaries", "InventoryPcs");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderSummaries", "InventoryPcs", c => c.Int());
            AddColumn("dbo.PurchaseOrderSummaries", "InventoryCtn", c => c.Int());
        }
    }
}
