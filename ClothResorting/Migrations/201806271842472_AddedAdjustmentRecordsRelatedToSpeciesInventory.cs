namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAdjustmentRecordsRelatedToSpeciesInventory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdjustmentRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        Adjustment = c.String(),
                        AdjustDate = c.DateTime(nullable: false),
                        SpeciesInventory_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SpeciesInventories", t => t.SpeciesInventory_Id)
                .Index(t => t.SpeciesInventory_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdjustmentRecords", "SpeciesInventory_Id", "dbo.SpeciesInventories");
            DropIndex("dbo.AdjustmentRecords", new[] { "SpeciesInventory_Id" });
            DropTable("dbo.AdjustmentRecords");
        }
    }
}
