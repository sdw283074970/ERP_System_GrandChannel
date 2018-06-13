namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamePurchaseOrderNumberToPurchaseOrderAndDeletedAllSilkIconWords : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.SilkIconCartonDetails", newName: "CartonDetails");
            RenameTable(name: "dbo.SilkIconPackingLists", newName: "PackingLists");
            RenameTable(name: "dbo.SilkIconPreReceiveOrders", newName: "PreReceiveOrders");
            RenameColumn(table: "dbo.CartonBreakDowns", name: "SilkIconCartonDetail_Id", newName: "CartonDetail_Id");
            RenameColumn(table: "dbo.SizeRatios", name: "SilkIconCartonDetail_Id", newName: "CartonDetail_Id");
            RenameColumn(table: "dbo.CartonBreakDowns", name: "SilkIconPackingList_Id", newName: "PackingList_Id");
            RenameColumn(table: "dbo.Measurements", name: "SilkIconPackingList_Id", newName: "PackingList_Id");
            RenameColumn(table: "dbo.PackingLists", name: "SilkIconPreReceiveOrder_Id", newName: "PreReceiveOrder_Id");
            RenameColumn(table: "dbo.CartonDetails", name: "SilkIconPackingList_Id", newName: "PackingList_Id");
            RenameIndex(table: "dbo.CartonBreakDowns", name: "IX_SilkIconCartonDetail_Id", newName: "IX_CartonDetail_Id");
            RenameIndex(table: "dbo.CartonBreakDowns", name: "IX_SilkIconPackingList_Id", newName: "IX_PackingList_Id");
            RenameIndex(table: "dbo.CartonDetails", name: "IX_SilkIconPackingList_Id", newName: "IX_PackingList_Id");
            RenameIndex(table: "dbo.PackingLists", name: "IX_SilkIconPreReceiveOrder_Id", newName: "IX_PreReceiveOrder_Id");
            RenameIndex(table: "dbo.Measurements", name: "IX_SilkIconPackingList_Id", newName: "IX_PackingList_Id");
            RenameIndex(table: "dbo.SizeRatios", name: "IX_SilkIconCartonDetail_Id", newName: "IX_CartonDetail_Id");
            AddColumn("dbo.CartonBreakDowns", "PurchaseOrder", c => c.String());
            AddColumn("dbo.CartonDetails", "PurchaseOrder", c => c.String());
            AddColumn("dbo.PackingLists", "PurchaseOrder", c => c.String());
            AddColumn("dbo.Measurements", "PurchaseOrder", c => c.String());
            AddColumn("dbo.RetrievingRecords", "PurchaseOrder", c => c.String());
            DropColumn("dbo.CartonBreakDowns", "PurchaseNumber");
            DropColumn("dbo.CartonDetails", "PurchaseOrderNumber");
            DropColumn("dbo.PackingLists", "PurchaseOrderNumber");
            DropColumn("dbo.Measurements", "PurchaseOrderNumber");
            DropColumn("dbo.RetrievingRecords", "PurchaseOrderNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RetrievingRecords", "PurchaseOrderNumber", c => c.String());
            AddColumn("dbo.Measurements", "PurchaseOrderNumber", c => c.String());
            AddColumn("dbo.PackingLists", "PurchaseOrderNumber", c => c.String());
            AddColumn("dbo.CartonDetails", "PurchaseOrderNumber", c => c.String());
            AddColumn("dbo.CartonBreakDowns", "PurchaseNumber", c => c.String());
            DropColumn("dbo.RetrievingRecords", "PurchaseOrder");
            DropColumn("dbo.Measurements", "PurchaseOrder");
            DropColumn("dbo.PackingLists", "PurchaseOrder");
            DropColumn("dbo.CartonDetails", "PurchaseOrder");
            DropColumn("dbo.CartonBreakDowns", "PurchaseOrder");
            RenameIndex(table: "dbo.SizeRatios", name: "IX_CartonDetail_Id", newName: "IX_SilkIconCartonDetail_Id");
            RenameIndex(table: "dbo.Measurements", name: "IX_PackingList_Id", newName: "IX_SilkIconPackingList_Id");
            RenameIndex(table: "dbo.PackingLists", name: "IX_PreReceiveOrder_Id", newName: "IX_SilkIconPreReceiveOrder_Id");
            RenameIndex(table: "dbo.CartonDetails", name: "IX_PackingList_Id", newName: "IX_SilkIconPackingList_Id");
            RenameIndex(table: "dbo.CartonBreakDowns", name: "IX_PackingList_Id", newName: "IX_SilkIconPackingList_Id");
            RenameIndex(table: "dbo.CartonBreakDowns", name: "IX_CartonDetail_Id", newName: "IX_SilkIconCartonDetail_Id");
            RenameColumn(table: "dbo.CartonDetails", name: "PackingList_Id", newName: "SilkIconPackingList_Id");
            RenameColumn(table: "dbo.PackingLists", name: "PreReceiveOrder_Id", newName: "SilkIconPreReceiveOrder_Id");
            RenameColumn(table: "dbo.Measurements", name: "PackingList_Id", newName: "SilkIconPackingList_Id");
            RenameColumn(table: "dbo.CartonBreakDowns", name: "PackingList_Id", newName: "SilkIconPackingList_Id");
            RenameColumn(table: "dbo.SizeRatios", name: "CartonDetail_Id", newName: "SilkIconCartonDetail_Id");
            RenameColumn(table: "dbo.CartonBreakDowns", name: "CartonDetail_Id", newName: "SilkIconCartonDetail_Id");
            RenameTable(name: "dbo.PreReceiveOrders", newName: "SilkIconPreReceiveOrders");
            RenameTable(name: "dbo.PackingLists", newName: "SilkIconPackingLists");
            RenameTable(name: "dbo.CartonDetails", newName: "SilkIconCartonDetails");
        }
    }
}
