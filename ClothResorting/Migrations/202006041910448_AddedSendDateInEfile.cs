namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSendDateInEfile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EFiles", "SendDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EFiles", "SendDate");
        }
    }
}
