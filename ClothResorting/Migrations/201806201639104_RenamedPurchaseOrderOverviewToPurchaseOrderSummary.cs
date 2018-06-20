namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedPurchaseOrderOverviewToPurchaseOrderSummary : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PurchaseOrderOverviews", newName: "PurchaseOrderSummaries");
            RenameColumn(table: "dbo.CartonBreakDowns", name: "PurchaseOrderOverview_Id", newName: "PurchaseOrderSummary_Id");
            RenameColumn(table: "dbo.CartonDetails", name: "PurchaseOrderOverview_Id", newName: "PurchaseOrderSummary_Id");
            RenameColumn(table: "dbo.LocationDetails", name: "PurchaseOrderOverview_Id", newName: "PurchaseOrderSummary_Id");
            RenameColumn(table: "dbo.Measurements", name: "PurchaseOrderOverview_Id", newName: "PurchaseOrderSummary_Id");
            RenameIndex(table: "dbo.CartonBreakDowns", name: "IX_PurchaseOrderOverview_Id", newName: "IX_PurchaseOrderSummary_Id");
            RenameIndex(table: "dbo.CartonDetails", name: "IX_PurchaseOrderOverview_Id", newName: "IX_PurchaseOrderSummary_Id");
            RenameIndex(table: "dbo.LocationDetails", name: "IX_PurchaseOrderOverview_Id", newName: "IX_PurchaseOrderSummary_Id");
            RenameIndex(table: "dbo.Measurements", name: "IX_PurchaseOrderOverview_Id", newName: "IX_PurchaseOrderSummary_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Measurements", name: "IX_PurchaseOrderSummary_Id", newName: "IX_PurchaseOrderOverview_Id");
            RenameIndex(table: "dbo.LocationDetails", name: "IX_PurchaseOrderSummary_Id", newName: "IX_PurchaseOrderOverview_Id");
            RenameIndex(table: "dbo.CartonDetails", name: "IX_PurchaseOrderSummary_Id", newName: "IX_PurchaseOrderOverview_Id");
            RenameIndex(table: "dbo.CartonBreakDowns", name: "IX_PurchaseOrderSummary_Id", newName: "IX_PurchaseOrderOverview_Id");
            RenameColumn(table: "dbo.Measurements", name: "PurchaseOrderSummary_Id", newName: "PurchaseOrderOverview_Id");
            RenameColumn(table: "dbo.LocationDetails", name: "PurchaseOrderSummary_Id", newName: "PurchaseOrderOverview_Id");
            RenameColumn(table: "dbo.CartonDetails", name: "PurchaseOrderSummary_Id", newName: "PurchaseOrderOverview_Id");
            RenameColumn(table: "dbo.CartonBreakDowns", name: "PurchaseOrderSummary_Id", newName: "PurchaseOrderOverview_Id");
            RenameTable(name: "dbo.PurchaseOrderSummaries", newName: "PurchaseOrderOverviews");
        }
    }
}
