namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOperationLogsDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OperationLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        User = c.String(),
                        OperationType = c.String(),
                        Description = c.String(),
                        RequestUri = c.String(),
                        Title = c.String(),
                        Exception = c.String(),
                        OperationDate = c.DateTime(nullable: false),
                        Level = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OperationLogs");
        }
    }
}
