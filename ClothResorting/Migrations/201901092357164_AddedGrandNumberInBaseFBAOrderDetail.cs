namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedGrandNumberInBaseFBAOrderDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAOrderDetails", "GrandNumber", c => c.String());
            AddColumn("dbo.FBACartonLocations", "GrandNumber", c => c.String());
            AddColumn("dbo.FBAPallets", "GrandNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPallets", "GrandNumber");
            DropColumn("dbo.FBACartonLocations", "GrandNumber");
            DropColumn("dbo.FBAOrderDetails", "GrandNumber");
        }
    }
}
