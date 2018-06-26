namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSpeciesInventoryDbSET : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SpeciesInventories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        Quantity = c.Int(nullable: false),
                        PurchaseOrderInventory_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PurchaseOrderInventories", t => t.PurchaseOrderInventory_Id)
                .Index(t => t.PurchaseOrderInventory_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SpeciesInventories", "PurchaseOrderInventory_Id", "dbo.PurchaseOrderInventories");
            DropIndex("dbo.SpeciesInventories", new[] { "PurchaseOrderInventory_Id" });
            DropTable("dbo.SpeciesInventories");
        }
    }
}
