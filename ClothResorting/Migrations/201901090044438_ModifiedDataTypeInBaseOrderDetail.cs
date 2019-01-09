namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedDataTypeInBaseOrderDetail : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.FBAOrderDetails", "ActualQuantity", c => c.Int(nullable: false));
            AlterColumn("dbo.FBACartonLocations", "ActualQuantity", c => c.Int(nullable: false));
            AlterColumn("dbo.FBAPalletLocations", "ActualQuantity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.FBAPalletLocations", "ActualQuantity", c => c.Single(nullable: false));
            AlterColumn("dbo.FBACartonLocations", "ActualQuantity", c => c.Single(nullable: false));
            AlterColumn("dbo.FBAOrderDetails", "ActualQuantity", c => c.Single(nullable: false));
        }
    }
}
