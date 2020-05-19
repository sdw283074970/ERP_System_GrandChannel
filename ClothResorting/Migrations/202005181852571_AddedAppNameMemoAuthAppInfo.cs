namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAppNameMemoAuthAppInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthAppInfoes", "AppName", c => c.String());
            AddColumn("dbo.AuthAppInfoes", "Memo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuthAppInfoes", "Memo");
            DropColumn("dbo.AuthAppInfoes", "AppName");
        }
    }
}
