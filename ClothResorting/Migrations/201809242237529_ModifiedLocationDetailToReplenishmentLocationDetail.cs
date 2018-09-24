namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedLocationDetailToReplenishmentLocationDetail : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.LocationDetails", newName: "ReplenishmentLocationDetails");
            AddColumn("dbo.SpeciesInventories", "AvailablePcs", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseOrderInventories", "AvailablePcs", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseOrderInventories", "AvailableCtns", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "Cartons", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "AvailableCtns", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "PickingCtns", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "ShippedCtns", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "Quantity", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "AvailablePcs", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "PickingPcs", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "ShippedPcs", c => c.Int(nullable: false));
            DropColumn("dbo.SpeciesInventories", "InvPcs");
            DropColumn("dbo.PurchaseOrderInventories", "InvPcs");
            DropColumn("dbo.PurchaseOrderInventories", "InvCtns");
            DropColumn("dbo.ReplenishmentLocationDetails", "OrgNumberOfCartons");
            DropColumn("dbo.ReplenishmentLocationDetails", "InvNumberOfCartons");
            DropColumn("dbo.ReplenishmentLocationDetails", "OrgPcs");
            DropColumn("dbo.ReplenishmentLocationDetails", "InvPcs");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReplenishmentLocationDetails", "InvPcs", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "OrgPcs", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "InvNumberOfCartons", c => c.Int(nullable: false));
            AddColumn("dbo.ReplenishmentLocationDetails", "OrgNumberOfCartons", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseOrderInventories", "InvCtns", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseOrderInventories", "InvPcs", c => c.Int(nullable: false));
            AddColumn("dbo.SpeciesInventories", "InvPcs", c => c.Int(nullable: false));
            DropColumn("dbo.ReplenishmentLocationDetails", "ShippedPcs");
            DropColumn("dbo.ReplenishmentLocationDetails", "PickingPcs");
            DropColumn("dbo.ReplenishmentLocationDetails", "AvailablePcs");
            DropColumn("dbo.ReplenishmentLocationDetails", "Quantity");
            DropColumn("dbo.ReplenishmentLocationDetails", "ShippedCtns");
            DropColumn("dbo.ReplenishmentLocationDetails", "PickingCtns");
            DropColumn("dbo.ReplenishmentLocationDetails", "AvailableCtns");
            DropColumn("dbo.ReplenishmentLocationDetails", "Cartons");
            DropColumn("dbo.PurchaseOrderInventories", "AvailableCtns");
            DropColumn("dbo.PurchaseOrderInventories", "AvailablePcs");
            DropColumn("dbo.SpeciesInventories", "AvailablePcs");
            RenameTable(name: "dbo.ReplenishmentLocationDetails", newName: "LocationDetails");
        }
    }
}
