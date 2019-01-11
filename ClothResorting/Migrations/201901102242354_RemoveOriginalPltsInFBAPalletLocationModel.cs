namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveOriginalPltsInFBAPalletLocationModel : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.FBAPalletLocations", "OriginalPlts");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FBAPalletLocations", "OriginalPlts", c => c.Int(nullable: false));
        }
    }
}
