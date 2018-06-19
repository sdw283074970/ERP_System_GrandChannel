namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInventoryPcsAndeCtnInPackingList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PackingLists", "InventoryCtn", c => c.Int());
            AddColumn("dbo.PackingLists", "InventoryPcs", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PackingLists", "InventoryPcs");
            DropColumn("dbo.PackingLists", "InventoryCtn");
        }
    }
}
