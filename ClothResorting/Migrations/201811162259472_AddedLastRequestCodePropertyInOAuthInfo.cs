namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLastRequestCodePropertyInOAuthInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OAuthInfoes", "LastRequestCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OAuthInfoes", "LastRequestCode");
        }
    }
}
