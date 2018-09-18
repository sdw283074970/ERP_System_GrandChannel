namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPickingInfoInPullSheetDbSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PullSheets", "PickDate", c => c.String());
            AddColumn("dbo.PullSheets", "PickingMan", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PullSheets", "PickingMan");
            DropColumn("dbo.PullSheets", "PickDate");
        }
    }
}
