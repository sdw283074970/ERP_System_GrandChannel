namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInboundDateInRegularLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocations", "InboundDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FCRegularLocations", "InboundDate");
        }
    }
}
