namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedFBAPalletLocationToFBAPallet : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.FBAPalletLocations", newName: "FBAPallets");
            RenameColumn(table: "dbo.FBACartonLocations", name: "FBAPalletLocation_Id", newName: "FBAPallet_Id");
            RenameIndex(table: "dbo.FBACartonLocations", name: "IX_FBAPalletLocation_Id", newName: "IX_FBAPallet_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.FBACartonLocations", name: "IX_FBAPallet_Id", newName: "IX_FBAPalletLocation_Id");
            RenameColumn(table: "dbo.FBACartonLocations", name: "FBAPallet_Id", newName: "FBAPalletLocation_Id");
            RenameTable(name: "dbo.FBAPallets", newName: "FBAPalletLocations");
        }
    }
}
