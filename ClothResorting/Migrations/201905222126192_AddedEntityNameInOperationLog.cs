namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedEntityNameInOperationLog : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OperationLogs", "EntityName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OperationLogs", "EntityName");
        }
    }
}
