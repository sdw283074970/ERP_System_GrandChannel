namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTwoPropertiesInSpeciesInventory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpeciesInventories", "PickingPcs", c => c.Int(nullable: false));
            AddColumn("dbo.SpeciesInventories", "ShippedPcs", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SpeciesInventories", "ShippedPcs");
            DropColumn("dbo.SpeciesInventories", "PickingPcs");
        }
    }
}
