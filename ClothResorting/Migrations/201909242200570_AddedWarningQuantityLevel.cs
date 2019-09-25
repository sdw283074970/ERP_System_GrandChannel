namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedWarningQuantityLevel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UpperVendors", "WarningQuantityLevel", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UpperVendors", "WarningQuantityLevel");
        }
    }
}
