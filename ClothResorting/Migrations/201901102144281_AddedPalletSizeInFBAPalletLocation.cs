namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPalletSizeInFBAPalletLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPalletLocations", "PalletSize", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPalletLocations", "PalletSize");
        }
    }
}
