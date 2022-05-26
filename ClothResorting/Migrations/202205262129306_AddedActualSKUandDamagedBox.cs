namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedActualSKUandDamagedBox : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "DamagedBox", c => c.Int(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "ActualSKU", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "ActualSKU");
            DropColumn("dbo.FBAMasterOrders", "DamagedBox");
        }
    }
}
