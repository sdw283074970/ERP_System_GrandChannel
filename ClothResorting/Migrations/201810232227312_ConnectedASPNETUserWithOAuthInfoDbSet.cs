namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectedASPNETUserWithOAuthInfoDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OAuthInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PlatformName = c.String(),
                        Code = c.String(),
                        RealmId = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OAuthInfoes", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.OAuthInfoes", new[] { "ApplicationUser_Id" });
            DropTable("dbo.OAuthInfoes");
        }
    }
}
