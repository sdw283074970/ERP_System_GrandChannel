namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectVendorsToUsers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UpperVendors", "ApplicationUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.UpperVendors", "ApplicationUser_Id");
            AddForeignKey("dbo.UpperVendors", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UpperVendors", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.UpperVendors", new[] { "ApplicationUser_Id" });
            DropColumn("dbo.UpperVendors", "ApplicationUser_Id");
        }
    }
}
