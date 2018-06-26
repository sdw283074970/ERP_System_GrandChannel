namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamePackingListToPurchaseOrderSummary : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PackingLists", newName: "PurchaseOrderSummaries");
            RenameColumn(table: "dbo.Measurements", name: "PackingList_Id", newName: "PurchaseOrderSummary_Id");
            RenameIndex(table: "dbo.Measurements", name: "IX_PackingList_Id", newName: "IX_PurchaseOrderSummary_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Measurements", name: "IX_PurchaseOrderSummary_Id", newName: "IX_PackingList_Id");
            RenameColumn(table: "dbo.Measurements", name: "PurchaseOrderSummary_Id", newName: "PackingList_Id");
            RenameTable(name: "dbo.PurchaseOrderSummaries", newName: "PackingLists");
        }
    }
}
