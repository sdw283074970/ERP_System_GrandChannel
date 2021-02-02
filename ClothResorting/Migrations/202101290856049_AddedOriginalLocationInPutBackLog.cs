namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOriginalLocationInPutBackLog : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrderPutBackLogs", "OriginalLocation", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrderPutBackLogs", "OriginalLocation");
        }
    }
}
