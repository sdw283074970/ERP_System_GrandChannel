namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLastUpdateTimeInUpperVendor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UpperVendors", "LastUpdatedTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UpperVendors", "LastUpdatedTime");
        }
    }
}
