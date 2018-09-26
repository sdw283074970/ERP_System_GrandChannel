namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedEditorInReplenishmentLocationDetailDbSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReplenishmentLocationDetails", "Editor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReplenishmentLocationDetails", "Editor");
        }
    }
}
