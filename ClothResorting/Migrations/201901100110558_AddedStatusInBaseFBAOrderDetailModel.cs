namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStatusInBaseFBAOrderDetailModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAOrderDetails", "Status", c => c.String());
            AddColumn("dbo.FBACartonLocations", "Status", c => c.String());
            AddColumn("dbo.FBAPallets", "Status", c => c.String());
            AddColumn("dbo.FBAPalletLocations", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPalletLocations", "Status");
            DropColumn("dbo.FBAPallets", "Status");
            DropColumn("dbo.FBACartonLocations", "Status");
            DropColumn("dbo.FBAOrderDetails", "Status");
        }
    }
}
