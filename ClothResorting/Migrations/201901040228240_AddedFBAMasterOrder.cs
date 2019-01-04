namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBAMasterOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GrandNumber = c.String(),
                        TotalCBM = c.String(),
                        TotalPlts = c.String(),
                        TotalCtns = c.String(),
                        InboundDate = c.DateTime(nullable: false),
                        Customer_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UpperVendors", t => t.Customer_Id)
                .Index(t => t.Customer_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBAMasterOrders", "Customer_Id", "dbo.UpperVendors");
            DropIndex("dbo.FBAMasterOrders", new[] { "Customer_Id" });
            DropTable("dbo.FBAMasterOrders");
        }
    }
}
