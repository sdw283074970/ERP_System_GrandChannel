namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddColorColumnsInSilkIconPackingList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPackingLists", "Color", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconPackingLists", "Color");
        }
    }
}
