namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedRequestIdInOperationLog : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OperationLogs", "RequestId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OperationLogs", "RequestId");
        }
    }
}
