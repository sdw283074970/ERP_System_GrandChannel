namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMemoInAdjustmentRecord : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdjustmentRecords", "Memo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AdjustmentRecords", "Memo");
        }
    }
}
