namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedGeneralLocationSummaryDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GeneralLocationSummaries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Vendor = c.String(),
                        UploadedFileName = c.String(),
                        Operator = c.String(),
                        CreatedDate = c.String(),
                        InboundDate = c.String(),
                        InboundPcs = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ReplenishmentLocationDetails", "GeneralLocationSummary_Id", c => c.Int());
            CreateIndex("dbo.ReplenishmentLocationDetails", "GeneralLocationSummary_Id");
            AddForeignKey("dbo.ReplenishmentLocationDetails", "GeneralLocationSummary_Id", "dbo.GeneralLocationSummaries", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReplenishmentLocationDetails", "GeneralLocationSummary_Id", "dbo.GeneralLocationSummaries");
            DropIndex("dbo.ReplenishmentLocationDetails", new[] { "GeneralLocationSummary_Id" });
            DropColumn("dbo.ReplenishmentLocationDetails", "GeneralLocationSummary_Id");
            DropTable("dbo.GeneralLocationSummaries");
        }
    }
}
