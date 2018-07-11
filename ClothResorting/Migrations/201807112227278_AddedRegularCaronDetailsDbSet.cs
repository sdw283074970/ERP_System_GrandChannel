namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedRegularCaronDetailsDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.POSummaries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        PoLine = c.Int(nullable: false),
                        Customer = c.String(),
                        Quantity = c.Int(nullable: false),
                        Cartons = c.Int(nullable: false),
                        CBM = c.Double(nullable: false),
                        GrossWeight = c.Double(nullable: false),
                        NetWeight = c.Double(nullable: false),
                        NNetWeight = c.Double(nullable: false),
                        Container = c.String(),
                        PreReceiveOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PreReceiveOrders", t => t.PreReceiveOrder_Id)
                .Index(t => t.PreReceiveOrder_Id);
            
            CreateTable(
                "dbo.RegularCartonDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Customer = c.String(),
                        CartonRange = c.String(),
                        Dimension = c.String(),
                        GrossWeight = c.Double(nullable: false),
                        NetWeight = c.Double(nullable: false),
                        SizeBundle = c.String(),
                        PcsBundle = c.String(),
                        PcsPerCarton = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        POSumary_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.POSummaries", t => t.POSumary_Id)
                .Index(t => t.POSumary_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RegularCartonDetails", "POSumary_Id", "dbo.POSummaries");
            DropForeignKey("dbo.POSummaries", "PreReceiveOrder_Id", "dbo.PreReceiveOrders");
            DropIndex("dbo.RegularCartonDetails", new[] { "POSumary_Id" });
            DropIndex("dbo.POSummaries", new[] { "PreReceiveOrder_Id" });
            DropTable("dbo.RegularCartonDetails");
            DropTable("dbo.POSummaries");
        }
    }
}
