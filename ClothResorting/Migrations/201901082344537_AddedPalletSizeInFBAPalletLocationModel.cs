namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPalletSizeInFBAPalletLocationModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPalletLocations", "PltSize", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPalletLocations", "PltSize");
        }
    }
}
