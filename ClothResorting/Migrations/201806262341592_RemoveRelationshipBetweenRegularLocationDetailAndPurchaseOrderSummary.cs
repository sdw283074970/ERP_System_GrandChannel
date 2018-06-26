namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRelationshipBetweenRegularLocationDetailAndPurchaseOrderSummary : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RegularLocationDetails", "PurchaseOrderSummary_Id", "dbo.PurchaseOrderSummaries");
            DropIndex("dbo.RegularLocationDetails", new[] { "PurchaseOrderSummary_Id" });
            DropColumn("dbo.RegularLocationDetails", "PurchaseOrderSummary_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RegularLocationDetails", "PurchaseOrderSummary_Id", c => c.Int());
            CreateIndex("dbo.RegularLocationDetails", "PurchaseOrderSummary_Id");
            AddForeignKey("dbo.RegularLocationDetails", "PurchaseOrderSummary_Id", "dbo.PurchaseOrderSummaries", "Id");
        }
    }
}
