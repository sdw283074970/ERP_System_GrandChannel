namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedShouldReturnPcsInRetriveRecord : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RetrievingRecords", "ShoulReturnPcs", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RetrievingRecords", "ShoulReturnPcs");
        }
    }
}
