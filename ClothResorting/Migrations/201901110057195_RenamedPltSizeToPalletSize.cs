namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedPltSizeToPalletSize : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPallets", "PalletSize", c => c.String());
            DropColumn("dbo.FBAPallets", "PltSize");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBAPallets", "PltSize", c => c.String());
            DropColumn("dbo.FBAPallets", "PalletSize");
        }
    }
}
