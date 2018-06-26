namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedSomeOldModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LocationDetails", "PurchaseOrderInventory_Id", c => c.Int());
            AddColumn("dbo.RegularLocationDetails", "PurchaseOrderInventory_Id", c => c.Int());
            AddColumn("dbo.PurchaseOrderInventories", "InvPcs", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseOrderInventories", "InvCtns", c => c.Int(nullable: false));
            CreateIndex("dbo.LocationDetails", "PurchaseOrderInventory_Id");
            CreateIndex("dbo.RegularLocationDetails", "PurchaseOrderInventory_Id");
            AddForeignKey("dbo.LocationDetails", "PurchaseOrderInventory_Id", "dbo.PurchaseOrderInventories", "Id");
            AddForeignKey("dbo.RegularLocationDetails", "PurchaseOrderInventory_Id", "dbo.PurchaseOrderInventories", "Id");
            DropColumn("dbo.PurchaseOrderInventories", "Quantity");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderInventories", "Quantity", c => c.Int(nullable: false));
            DropForeignKey("dbo.RegularLocationDetails", "PurchaseOrderInventory_Id", "dbo.PurchaseOrderInventories");
            DropForeignKey("dbo.LocationDetails", "PurchaseOrderInventory_Id", "dbo.PurchaseOrderInventories");
            DropIndex("dbo.RegularLocationDetails", new[] { "PurchaseOrderInventory_Id" });
            DropIndex("dbo.LocationDetails", new[] { "PurchaseOrderInventory_Id" });
            DropColumn("dbo.PurchaseOrderInventories", "InvCtns");
            DropColumn("dbo.PurchaseOrderInventories", "InvPcs");
            DropColumn("dbo.RegularLocationDetails", "PurchaseOrderInventory_Id");
            DropColumn("dbo.LocationDetails", "PurchaseOrderInventory_Id");
        }
    }
}
