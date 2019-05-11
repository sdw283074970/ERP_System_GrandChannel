namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInstructionTemplateDbSet : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ChargingItemDetails", "Customer_Id", "dbo.UpperVendors");
            DropIndex("dbo.ChargingItemDetails", new[] { "Customer_Id" });
            CreateTable(
                "dbo.InstructionTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        CreateBy = c.String(),
                        Customer_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UpperVendors", t => t.Customer_Id)
                .Index(t => t.Customer_Id);
            
            DropColumn("dbo.ChargingItemDetails", "Customer_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChargingItemDetails", "Customer_Id", c => c.Int());
            DropForeignKey("dbo.InstructionTemplates", "Customer_Id", "dbo.UpperVendors");
            DropIndex("dbo.InstructionTemplates", new[] { "Customer_Id" });
            DropTable("dbo.InstructionTemplates");
            CreateIndex("dbo.ChargingItemDetails", "Customer_Id");
            AddForeignKey("dbo.ChargingItemDetails", "Customer_Id", "dbo.UpperVendors", "Id");
        }
    }
}
