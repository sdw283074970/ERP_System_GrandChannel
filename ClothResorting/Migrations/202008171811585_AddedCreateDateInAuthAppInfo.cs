namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCreateDateInAuthAppInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthAppInfoes", "CreateDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.AuthAppInfoes", "ApplicationUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.AuthAppInfoes", "ApplicationUser_Id");
            AddForeignKey("dbo.AuthAppInfoes", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AuthAppInfoes", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.AuthAppInfoes", new[] { "ApplicationUser_Id" });
            DropColumn("dbo.AuthAppInfoes", "ApplicationUser_Id");
            DropColumn("dbo.AuthAppInfoes", "CreateDate");
        }
    }
}
