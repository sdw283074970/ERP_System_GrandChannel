namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrderTypePropertyInPurchaseOrderSummary : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderSummaries", "OrderType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderSummaries", "OrderType");
        }
    }
}
