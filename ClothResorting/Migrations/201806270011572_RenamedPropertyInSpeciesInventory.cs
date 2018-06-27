namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedPropertyInSpeciesInventory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpeciesInventories", "OrgPcs", c => c.Int(nullable: false));
            AddColumn("dbo.SpeciesInventories", "AdjPcs", c => c.Int(nullable: false));
            AddColumn("dbo.SpeciesInventories", "InvPcs", c => c.Int(nullable: false));
            DropColumn("dbo.SpeciesInventories", "Quantity");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SpeciesInventories", "Quantity", c => c.Int(nullable: false));
            DropColumn("dbo.SpeciesInventories", "InvPcs");
            DropColumn("dbo.SpeciesInventories", "AdjPcs");
            DropColumn("dbo.SpeciesInventories", "OrgPcs");
        }
    }
}
