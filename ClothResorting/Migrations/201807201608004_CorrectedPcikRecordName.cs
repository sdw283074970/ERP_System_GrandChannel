namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CorrectedPcikRecordName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PickingRecords", "PcsPerCarton", c => c.Int(nullable: false));
            DropColumn("dbo.PickingRecords", "PcsPerCaron");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PickingRecords", "PcsPerCaron", c => c.Int(nullable: false));
            DropColumn("dbo.PickingRecords", "PcsPerCarton");
        }
    }
}
