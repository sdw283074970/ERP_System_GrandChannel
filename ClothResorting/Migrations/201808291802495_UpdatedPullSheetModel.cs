namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedPullSheetModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PullSheets", "OrderPurchaseOrder", c => c.String());
            AddColumn("dbo.PullSheets", "Customer", c => c.String());
            AddColumn("dbo.PullSheets", "Address", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PullSheets", "Address");
            DropColumn("dbo.PullSheets", "Customer");
            DropColumn("dbo.PullSheets", "OrderPurchaseOrder");
        }
    }
}
