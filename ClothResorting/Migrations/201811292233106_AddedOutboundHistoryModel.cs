namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOutboundHistoryModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OutboundHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OutboundPcs = c.Int(nullable: false),
                        OutboundDate = c.DateTime(nullable: false),
                        FromLocation = c.String(),
                        OrderPurchaseOrder = c.String(),
                        SpeciesInventory_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SpeciesInventories", t => t.SpeciesInventory_Id)
                .Index(t => t.SpeciesInventory_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OutboundHistories", "SpeciesInventory_Id", "dbo.SpeciesInventories");
            DropIndex("dbo.OutboundHistories", new[] { "SpeciesInventory_Id" });
            DropTable("dbo.OutboundHistories");
        }
    }
}
