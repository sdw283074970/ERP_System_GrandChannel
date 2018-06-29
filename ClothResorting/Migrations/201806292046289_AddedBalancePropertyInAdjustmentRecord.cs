namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBalancePropertyInAdjustmentRecord : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdjustmentRecords", "Balance", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AdjustmentRecords", "Balance");
        }
    }
}
