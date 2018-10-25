namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOAuthInfoDbSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OAuthInfoes", "AccessToken", c => c.String());
            AddColumn("dbo.OAuthInfoes", "refreshToken", c => c.String());
            DropColumn("dbo.OAuthInfoes", "Code");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OAuthInfoes", "Code", c => c.String());
            DropColumn("dbo.OAuthInfoes", "refreshToken");
            DropColumn("dbo.OAuthInfoes", "AccessToken");
        }
    }
}
