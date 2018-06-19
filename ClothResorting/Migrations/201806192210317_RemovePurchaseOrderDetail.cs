namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovePurchaseOrderDetail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LocationDetails", "PurchaseOrderDetail_Id", "dbo.PurchaseOrderDetails");
            DropForeignKey("dbo.PurchaseOrderDetails", "PackingList_Id", "dbo.PackingLists");
            DropIndex("dbo.PurchaseOrderDetails", new[] { "PackingList_Id" });
            DropIndex("dbo.LocationDetails", new[] { "PurchaseOrderDetail_Id" });
            AddColumn("dbo.LocationDetails", "PackingList_Id", c => c.Int());
            CreateIndex("dbo.LocationDetails", "PackingList_Id");
            AddForeignKey("dbo.LocationDetails", "PackingList_Id", "dbo.PackingLists", "Id");
            DropColumn("dbo.LocationDetails", "PurchaseOrderDetail_Id");
            DropTable("dbo.PurchaseOrderDetails");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.LocationDetails", "PurchaseOrderDetail_Id", c => c.Int());
            DropForeignKey("dbo.LocationDetails", "PackingList_Id", "dbo.PackingLists");
            DropIndex("dbo.LocationDetails", new[] { "PackingList_Id" });
            DropColumn("dbo.LocationDetails", "PackingList_Id");
            CreateIndex("dbo.LocationDetails", "PurchaseOrderDetail_Id");
            CreateIndex("dbo.PurchaseOrderDetails", "PackingList_Id");
            AddForeignKey("dbo.PurchaseOrderDetails", "PackingList_Id", "dbo.PackingLists", "Id");
            AddForeignKey("dbo.LocationDetails", "PurchaseOrderDetail_Id", "dbo.PurchaseOrderDetails", "Id");
        }
    }
}
