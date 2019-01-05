namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedMasterOrderModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "ActualCBM", c => c.Single(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "ActualCtns", c => c.Int(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "AcutalPlts", c => c.Int(nullable: false));
            AlterColumn("dbo.FBAMasterOrders", "TotalCBM", c => c.Single(nullable: false));
            AlterColumn("dbo.FBAMasterOrders", "TotalCtns", c => c.Int(nullable: false));
            DropColumn("dbo.FBAMasterOrders", "TotalPlts");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBAMasterOrders", "TotalPlts", c => c.String());
            AlterColumn("dbo.FBAMasterOrders", "TotalCtns", c => c.String());
            AlterColumn("dbo.FBAMasterOrders", "TotalCBM", c => c.String());
            DropColumn("dbo.FBAMasterOrders", "AcutalPlts");
            DropColumn("dbo.FBAMasterOrders", "ActualCtns");
            DropColumn("dbo.FBAMasterOrders", "ActualCBM");
        }
    }
}
