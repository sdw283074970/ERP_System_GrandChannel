namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedContainerStructureInGDSystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.POSummaries", "ContainerInfo_Id", c => c.Int());
            AddColumn("dbo.Containers", "InboundDate", c => c.DateTime(nullable: false));
            CreateIndex("dbo.POSummaries", "ContainerInfo_Id");
            AddForeignKey("dbo.POSummaries", "ContainerInfo_Id", "dbo.Containers", "Id");
            DropColumn("dbo.FCRegularLocationDetails", "InboundDate");
            DropColumn("dbo.RegularCartonDetails", "InboundDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RegularCartonDetails", "InboundDate", c => c.DateTime());
            AddColumn("dbo.FCRegularLocationDetails", "InboundDate", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.POSummaries", "ContainerInfo_Id", "dbo.Containers");
            DropIndex("dbo.POSummaries", new[] { "ContainerInfo_Id" });
            DropColumn("dbo.Containers", "InboundDate");
            DropColumn("dbo.POSummaries", "ContainerInfo_Id");
        }
    }
}
