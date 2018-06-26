namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedColumnInLocationDetail : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.RegularLocationDetails", name: "PurchaseOrderSummaries_Id", newName: "PurchaseOrderSummary_Id");
            RenameIndex(table: "dbo.RegularLocationDetails", name: "IX_PurchaseOrderSummaries_Id", newName: "IX_PurchaseOrderSummary_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.RegularLocationDetails", name: "IX_PurchaseOrderSummary_Id", newName: "IX_PurchaseOrderSummaries_Id");
            RenameColumn(table: "dbo.RegularLocationDetails", name: "PurchaseOrderSummary_Id", newName: "PurchaseOrderSummaries_Id");
        }
    }
}
