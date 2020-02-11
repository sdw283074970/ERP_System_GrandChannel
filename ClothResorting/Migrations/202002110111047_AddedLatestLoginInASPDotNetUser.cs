namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLatestLoginInASPDotNetUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "LatestLogin", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "LatestLogin");
        }
    }
}
