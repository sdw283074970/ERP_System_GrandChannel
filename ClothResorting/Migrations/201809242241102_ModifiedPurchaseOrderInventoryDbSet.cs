namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedPurchaseOrderInventoryDbSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderInventories", "PickingPcs", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseOrderInventories", "ShippedPcs", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderInventories", "ShippedPcs");
            DropColumn("dbo.PurchaseOrderInventories", "PickingPcs");
        }
    }
}
