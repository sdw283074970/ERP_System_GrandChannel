namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInboundDateInFBAPickDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPickDetails", "InboundDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPickDetails", "InboundDate");
        }
    }
}
