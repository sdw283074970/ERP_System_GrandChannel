namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedRelationshipAmongPackingListAndPurchaseOrderDetailAndLocationDetail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LocationDetails", "CartonBreakdown_Id", "dbo.CartonBreakDowns");
            DropIndex("dbo.LocationDetails", new[] { "CartonBreakdown_Id" });
            AddColumn("dbo.LocationDetails", "PurchaseOrderDetail_Id", c => c.Int());
            CreateIndex("dbo.LocationDetails", "PurchaseOrderDetail_Id");
            AddForeignKey("dbo.LocationDetails", "PurchaseOrderDetail_Id", "dbo.PurchaseOrderDetails", "Id");
            DropColumn("dbo.LocationDetails", "CartonBreakdown_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LocationDetails", "CartonBreakdown_Id", c => c.Int());
            DropForeignKey("dbo.LocationDetails", "PurchaseOrderDetail_Id", "dbo.PurchaseOrderDetails");
            DropIndex("dbo.LocationDetails", new[] { "PurchaseOrderDetail_Id" });
            DropColumn("dbo.LocationDetails", "PurchaseOrderDetail_Id");
            CreateIndex("dbo.LocationDetails", "CartonBreakdown_Id");
            AddForeignKey("dbo.LocationDetails", "CartonBreakdown_Id", "dbo.CartonBreakDowns", "Id");
        }
    }
}
