namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedActualPltsInfbaPalletsLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPalletLocations", "ActualPlts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPalletLocations", "ActualPlts");
        }
    }
}
