namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedRegularLocationDetails : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RegularLocationDetails", "PurchaseOrderInventory_Id", "dbo.PurchaseOrderInventories");
            DropIndex("dbo.RegularLocationDetails", new[] { "PurchaseOrderInventory_Id" });
            DropTable("dbo.RegularLocationDetails");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.RegularLocationDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        RunCode = c.String(),
                        SizeCombine = c.String(),
                        PcsCombine = c.String(),
                        OrgNumberOfCartons = c.Int(nullable: false),
                        InvNumberOfCartons = c.Int(nullable: false),
                        OrgPcs = c.Int(nullable: false),
                        InvPcs = c.Int(nullable: false),
                        Location = c.String(),
                        InboundDate = c.DateTime(nullable: false),
                        PurchaseOrderInventory_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.RegularLocationDetails", "PurchaseOrderInventory_Id");
            AddForeignKey("dbo.RegularLocationDetails", "PurchaseOrderInventory_Id", "dbo.PurchaseOrderInventories", "Id");
        }
    }
}
