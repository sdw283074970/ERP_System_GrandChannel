namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPltsInInventoryAndNewPlts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPickDetails", "PltsFromInventory", c => c.Int(nullable: false));
            AddColumn("dbo.FBAPickDetails", "NewPlts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPickDetails", "NewPlts");
            DropColumn("dbo.FBAPickDetails", "PltsFromInventory");
        }
    }
}
