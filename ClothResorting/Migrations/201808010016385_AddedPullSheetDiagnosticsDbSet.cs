namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPullSheetDiagnosticsDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PullSheetDiagnostics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.String(),
                        DiagnosticDate = c.String(),
                        Description = c.String(),
                        PullSheet_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PullSheets", t => t.PullSheet_Id)
                .Index(t => t.PullSheet_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PullSheetDiagnostics", "PullSheet_Id", "dbo.PullSheets");
            DropIndex("dbo.PullSheetDiagnostics", new[] { "PullSheet_Id" });
            DropTable("dbo.PullSheetDiagnostics");
        }
    }
}
