namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedSomeNameAndDateDataTyple : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PickDetails", "LocationDetailId", c => c.Int(nullable: false));
            AddColumn("dbo.PickDetails", "PickDate", c => c.String());
            AddColumn("dbo.PullSheets", "CreateDate", c => c.String());
            DropColumn("dbo.PickDetails", "InboundDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PickDetails", "InboundDate", c => c.DateTime());
            DropColumn("dbo.PullSheets", "CreateDate");
            DropColumn("dbo.PickDetails", "PickDate");
            DropColumn("dbo.PickDetails", "LocationDetailId");
        }
    }
}
