namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPermanentSKU : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PermanentSKUs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.String(),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        Quantity = c.Int(nullable: false),
                        AvailablePcs = c.Int(nullable: false),
                        PickingPcs = c.Int(nullable: false),
                        ShippedPcs = c.Int(nullable: false),
                        Location = c.String(),
                        Vendor = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.PickDetails", "PermanentSKU_Id", c => c.Int());
            AddColumn("dbo.RegularCartonDetails", "PermanentSKU_Id", c => c.Int());
            CreateIndex("dbo.PickDetails", "PermanentSKU_Id");
            CreateIndex("dbo.RegularCartonDetails", "PermanentSKU_Id");
            AddForeignKey("dbo.RegularCartonDetails", "PermanentSKU_Id", "dbo.PermanentSKUs", "Id");
            AddForeignKey("dbo.PickDetails", "PermanentSKU_Id", "dbo.PermanentSKUs", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PickDetails", "PermanentSKU_Id", "dbo.PermanentSKUs");
            DropForeignKey("dbo.RegularCartonDetails", "PermanentSKU_Id", "dbo.PermanentSKUs");
            DropIndex("dbo.RegularCartonDetails", new[] { "PermanentSKU_Id" });
            DropIndex("dbo.PickDetails", new[] { "PermanentSKU_Id" });
            DropColumn("dbo.RegularCartonDetails", "PermanentSKU_Id");
            DropColumn("dbo.PickDetails", "PermanentSKU_Id");
            DropTable("dbo.PermanentSKUs");
        }
    }
}
