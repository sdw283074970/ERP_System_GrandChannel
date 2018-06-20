namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedPackingListToPurchaseOrderOverview : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PackingLists", newName: "PurchaseOrderOverviews");
            RenameColumn(table: "dbo.CartonBreakDowns", name: "PackingList_Id", newName: "PurchaseOrderOverview_Id");
            RenameColumn(table: "dbo.CartonDetails", name: "PackingList_Id", newName: "PurchaseOrderOverview_Id");
            RenameColumn(table: "dbo.LocationDetails", name: "PackingList_Id", newName: "PurchaseOrderOverview_Id");
            RenameColumn(table: "dbo.Measurements", name: "PackingList_Id", newName: "PurchaseOrderOverview_Id");
            RenameIndex(table: "dbo.CartonBreakDowns", name: "IX_PackingList_Id", newName: "IX_PurchaseOrderOverview_Id");
            RenameIndex(table: "dbo.CartonDetails", name: "IX_PackingList_Id", newName: "IX_PurchaseOrderOverview_Id");
            RenameIndex(table: "dbo.LocationDetails", name: "IX_PackingList_Id", newName: "IX_PurchaseOrderOverview_Id");
            RenameIndex(table: "dbo.Measurements", name: "IX_PackingList_Id", newName: "IX_PurchaseOrderOverview_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Measurements", name: "IX_PurchaseOrderOverview_Id", newName: "IX_PackingList_Id");
            RenameIndex(table: "dbo.LocationDetails", name: "IX_PurchaseOrderOverview_Id", newName: "IX_PackingList_Id");
            RenameIndex(table: "dbo.CartonDetails", name: "IX_PurchaseOrderOverview_Id", newName: "IX_PackingList_Id");
            RenameIndex(table: "dbo.CartonBreakDowns", name: "IX_PurchaseOrderOverview_Id", newName: "IX_PackingList_Id");
            RenameColumn(table: "dbo.Measurements", name: "PurchaseOrderOverview_Id", newName: "PackingList_Id");
            RenameColumn(table: "dbo.LocationDetails", name: "PurchaseOrderOverview_Id", newName: "PackingList_Id");
            RenameColumn(table: "dbo.CartonDetails", name: "PurchaseOrderOverview_Id", newName: "PackingList_Id");
            RenameColumn(table: "dbo.CartonBreakDowns", name: "PurchaseOrderOverview_Id", newName: "PackingList_Id");
            RenameTable(name: "dbo.PurchaseOrderOverviews", newName: "PackingLists");
        }
    }
}
