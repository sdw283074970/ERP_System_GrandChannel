namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOldAndNewValueInOperationLog : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OperationLogs", "OldValue", c => c.String());
            AddColumn("dbo.OperationLogs", "NewValue", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OperationLogs", "NewValue");
            DropColumn("dbo.OperationLogs", "OldValue");
        }
    }
}
