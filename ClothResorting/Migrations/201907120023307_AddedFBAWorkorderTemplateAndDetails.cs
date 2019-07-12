namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFBAWorkorderTemplateAndDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FBAWorkOrderDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        FBAWorkOrderTemplate_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FBAWorkOrderTemplates", t => t.FBAWorkOrderTemplate_Id)
                .Index(t => t.FBAWorkOrderTemplate_Id);
            
            CreateTable(
                "dbo.FBAWorkOrderTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TemplateName = c.String(),
                        CustomerCode = c.String(),
                        WorkOrderType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.FBAMasterOrders", "UnloadingType", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "StorageType", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FBAWorkOrderDetails", "FBAWorkOrderTemplate_Id", "dbo.FBAWorkOrderTemplates");
            DropIndex("dbo.FBAWorkOrderDetails", new[] { "FBAWorkOrderTemplate_Id" });
            DropColumn("dbo.FBAMasterOrders", "StorageType");
            DropColumn("dbo.FBAMasterOrders", "UnloadingType");
            DropTable("dbo.FBAWorkOrderTemplates");
            DropTable("dbo.FBAWorkOrderDetails");
        }
    }
}
