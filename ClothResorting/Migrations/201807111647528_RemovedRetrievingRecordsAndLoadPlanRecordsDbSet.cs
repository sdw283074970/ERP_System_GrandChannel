namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedRetrievingRecordsAndLoadPlanRecordsDbSet : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RetrievingRecords", "LoadPlanRecord_Id", "dbo.LoadPlanRecords");
            DropIndex("dbo.RetrievingRecords", new[] { "LoadPlanRecord_Id" });
            DropTable("dbo.LoadPlanRecords");
            DropTable("dbo.RetrievingRecords");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.RetrievingRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Location = c.String(),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        RetrivedPcs = c.Int(),
                        TargetPcs = c.Int(),
                        NumberOfCartons = c.Int(),
                        TotalOfCartons = c.Int(),
                        IsOpened = c.Boolean(nullable: false),
                        IfOpen = c.Boolean(nullable: false),
                        Shortage = c.Int(nullable: false),
                        RetrievedDate = c.DateTime(),
                        LoadPlanRecord_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
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
            
            CreateIndex("dbo.RetrievingRecords", "LoadPlanRecord_Id");
            AddForeignKey("dbo.RetrievingRecords", "LoadPlanRecord_Id", "dbo.LoadPlanRecords", "Id");
        }
    }
}
