namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPurchaseOrderDetailDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PurchaseOrderDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        ForecastPcs = c.String(),
                        ActualPcs = c.Int(nullable: false),
                        AvailablePcs = c.Int(nullable: false),
                        PackingList_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PackingLists", t => t.PackingList_Id)
                .Index(t => t.PackingList_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PurchaseOrderDetails", "PackingList_Id", "dbo.PackingLists");
            DropIndex("dbo.PurchaseOrderDetails", new[] { "PackingList_Id" });
            DropTable("dbo.PurchaseOrderDetails");
        }
    }
}
