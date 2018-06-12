namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLoadPlanRecordsDbSet : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RetrievingRecords", "PackingList_Id", "dbo.SilkIconPackingLists");
            DropIndex("dbo.RetrievingRecords", new[] { "PackingList_Id" });
            CreateTable(
                "dbo.LoadPlanRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        OutBoundDate = c.DateTime(nullable: false),
                        OutBoundCtns = c.Int(nullable: false),
                        OutBoundPcs = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.RetrievingRecords", "LoadPlanRecord_Id", c => c.Int());
            CreateIndex("dbo.RetrievingRecords", "LoadPlanRecord_Id");
            AddForeignKey("dbo.RetrievingRecords", "LoadPlanRecord_Id", "dbo.LoadPlanRecords", "Id");
            DropColumn("dbo.RetrievingRecords", "PackingList_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RetrievingRecords", "PackingList_Id", c => c.Int());
            DropForeignKey("dbo.RetrievingRecords", "LoadPlanRecord_Id", "dbo.LoadPlanRecords");
            DropIndex("dbo.RetrievingRecords", new[] { "LoadPlanRecord_Id" });
            DropColumn("dbo.RetrievingRecords", "LoadPlanRecord_Id");
            DropTable("dbo.LoadPlanRecords");
            CreateIndex("dbo.RetrievingRecords", "PackingList_Id");
            AddForeignKey("dbo.RetrievingRecords", "PackingList_Id", "dbo.SilkIconPackingLists", "Id");
        }
    }
}
