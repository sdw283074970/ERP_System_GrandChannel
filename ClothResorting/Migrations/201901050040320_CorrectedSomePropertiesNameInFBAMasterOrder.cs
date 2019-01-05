namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CorrectedSomePropertiesNameInFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "ActualPlts", c => c.Int(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "Status", c => c.String());
            DropColumn("dbo.FBAMasterOrders", "AcutalPlts");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBAMasterOrders", "AcutalPlts", c => c.Int(nullable: false));
            DropColumn("dbo.FBAMasterOrders", "Status");
            DropColumn("dbo.FBAMasterOrders", "ActualPlts");
        }
    }
}
