namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedRegularLocationDetailsDbSet : DbMigration
    {
        public override void Up()
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
                        OrgNumberOfCartons = c.Int(nullable: false),
                        InvNumberOfCartons = c.Int(nullable: false),
                        OrgPcs = c.Int(nullable: false),
                        InvPcs = c.Int(nullable: false),
                        Location = c.String(),
                        InboundDate = c.DateTime(nullable: false),
                        PurchaseOrderSummary_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PurchaseOrderSummaries", t => t.PurchaseOrderSummary_Id)
                .Index(t => t.PurchaseOrderSummary_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RegularLocationDetails", "PurchaseOrderSummary_Id", "dbo.PurchaseOrderSummaries");
            DropIndex("dbo.RegularLocationDetails", new[] { "PurchaseOrderSummary_Id" });
            DropTable("dbo.RegularLocationDetails");
        }
    }
}
