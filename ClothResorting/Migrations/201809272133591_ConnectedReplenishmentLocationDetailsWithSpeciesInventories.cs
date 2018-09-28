namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectedReplenishmentLocationDetailsWithSpeciesInventories : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReplenishmentLocationDetails", "SpeciesInventory_Id", c => c.Int());
            CreateIndex("dbo.ReplenishmentLocationDetails", "SpeciesInventory_Id");
            AddForeignKey("dbo.ReplenishmentLocationDetails", "SpeciesInventory_Id", "dbo.SpeciesInventories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReplenishmentLocationDetails", "SpeciesInventory_Id", "dbo.SpeciesInventories");
            DropIndex("dbo.ReplenishmentLocationDetails", new[] { "SpeciesInventory_Id" });
            DropColumn("dbo.ReplenishmentLocationDetails", "SpeciesInventory_Id");
        }
    }
}
