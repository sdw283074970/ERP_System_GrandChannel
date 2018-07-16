namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFCRegularLocationsDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FCRegularLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        CustomerCode = c.String(),
                        SizeBundle = c.String(),
                        PcsBundle = c.String(),
                        Cartons = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        Location = c.String(),
                        PreReceiveOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PreReceiveOrders", t => t.PreReceiveOrder_Id)
                .Index(t => t.PreReceiveOrder_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FCRegularLocations", "PreReceiveOrder_Id", "dbo.PreReceiveOrders");
            DropIndex("dbo.FCRegularLocations", new[] { "PreReceiveOrder_Id" });
            DropTable("dbo.FCRegularLocations");
        }
    }
}
