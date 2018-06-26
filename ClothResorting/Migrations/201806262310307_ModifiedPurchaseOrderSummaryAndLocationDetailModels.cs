namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedPurchaseOrderSummaryAndLocationDetailModels : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.LocationDetails", name: "PurchaseOrderSummaries_Id", newName: "PurchaseOrderSummary_Id");
            RenameIndex(table: "dbo.LocationDetails", name: "IX_PurchaseOrderSummaries_Id", newName: "IX_PurchaseOrderSummary_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.LocationDetails", name: "IX_PurchaseOrderSummary_Id", newName: "IX_PurchaseOrderSummaries_Id");
            RenameColumn(table: "dbo.LocationDetails", name: "PurchaseOrderSummary_Id", newName: "PurchaseOrderSummaries_Id");
        }
    }
}
