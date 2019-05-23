namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUserIpAndRequestBodyInOperationLo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OperationLogs", "UserIp", c => c.String());
            AddColumn("dbo.OperationLogs", "RequestBody", c => c.String());
            DropColumn("dbo.OperationLogs", "Title");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OperationLogs", "Title", c => c.String());
            DropColumn("dbo.OperationLogs", "RequestBody");
            DropColumn("dbo.OperationLogs", "UserIp");
        }
    }
}
