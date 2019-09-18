namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedEFileStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EFiles", "Status", c => c.String());
            AddColumn("dbo.EFiles", "DiscardBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EFiles", "DiscardBy");
            DropColumn("dbo.EFiles", "Status");
        }
    }
}
