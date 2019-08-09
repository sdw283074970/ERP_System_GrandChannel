namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTwoTypesAndCreatedByInMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "CreatedBy", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "UnloadingType", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "StorageType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "StorageType");
            DropColumn("dbo.FBAMasterOrders", "UnloadingType");
            DropColumn("dbo.FBAMasterOrders", "CreatedBy");
        }
    }
}
