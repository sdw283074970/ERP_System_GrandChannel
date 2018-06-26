namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPurchaseOrderInventoriesDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PurchaseOrderInventories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        OrderType = c.String(),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.PurchaseOrderSummaries", "Style", c => c.String());
            DropColumn("dbo.PurchaseOrderSummaries", "StyleNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseOrderSummaries", "StyleNumber", c => c.String());
            DropColumn("dbo.PurchaseOrderSummaries", "Style");
            DropTable("dbo.PurchaseOrderInventories");
        }
    }
}
