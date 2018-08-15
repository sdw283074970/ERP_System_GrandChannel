namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderTyperInPOsummary : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.POSummaries", "OrderType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.POSummaries", "OrderType");
        }
    }
}
