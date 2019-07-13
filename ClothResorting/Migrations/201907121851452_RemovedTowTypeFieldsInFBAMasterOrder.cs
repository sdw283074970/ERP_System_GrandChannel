namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedTowTypeFieldsInFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.FBAMasterOrders", "UnloadingType");
            DropColumn("dbo.FBAMasterOrders", "StorageType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBAMasterOrders", "StorageType", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "UnloadingType", c => c.String());
        }
    }
}
