namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMemoToInventoryRelatedModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBACartonLocations", "Memo", c => c.String());
            AddColumn("dbo.FBAPallets", "Memo", c => c.String());
            AddColumn("dbo.FBAPalletLocations", "Memo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPalletLocations", "Memo");
            DropColumn("dbo.FBAPallets", "Memo");
            DropColumn("dbo.FBACartonLocations", "Memo");
        }
    }
}
