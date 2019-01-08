namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFBAOrderDetailsModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBAOrderDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LotSize = c.String(),
                        HowToDeliver = c.String(),
                        GrossWeight = c.Single(nullable: false),
                        CBM = c.Single(nullable: false),
                        Quantity = c.Int(nullable: false),
                        Remark = c.String(),
                        ComsumedQuantity = c.Int(nullable: false),
                        Comment = c.String(),
                        Container = c.String(),
                        ShipmentId = c.String(),
                        AmzRefId = c.String(),
                        WarehouseCode = c.String(),
                        ActualCBM = c.Single(nullable: false),
                        ActualGrossWeight = c.Single(nullable: false),
                        ActualQuantity = c.Single(nullable: false),
                        FBAMasterOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBAMasterOrders", t => t.FBAMasterOrder_Id)
                .Index(t => t.FBAMasterOrder_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBAOrderDetails", "FBAMasterOrder_Id", "dbo.FBAMasterOrders");
            DropIndex("dbo.FBAOrderDetails", new[] { "FBAMasterOrder_Id" });
            DropTable("dbo.FBAOrderDetails");
        }
    }
}
