namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedNameFromFCRegularLocationToFCRegularLocationDetail : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.FCRegularLocations", newName: "FCRegularLocationDetails");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.FCRegularLocationDetails", newName: "FCRegularLocations");
        }
    }
}
