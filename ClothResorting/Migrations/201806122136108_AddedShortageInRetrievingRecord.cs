namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedShortageInRetrievingRecord : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RetrievingRecords", "Shortage", c => c.Int(nullable: false));
            AlterColumn("dbo.RetrievingRecords", "ShoulReturnPcs", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RetrievingRecords", "ShoulReturnPcs", c => c.Int(nullable: false));
            DropColumn("dbo.RetrievingRecords", "Shortage");
        }
    }
}
