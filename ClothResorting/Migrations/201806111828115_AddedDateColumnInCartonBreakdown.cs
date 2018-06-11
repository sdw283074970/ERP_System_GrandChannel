namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDateColumnInCartonBreakdown : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "ReceivedDate", c => c.DateTime());
            AddColumn("dbo.SilkIconPackingLists", "ReceivedDate", c => c.DateTime());
            DropColumn("dbo.SilkIconPackingLists", "Date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconPackingLists", "Date", c => c.DateTime());
            DropColumn("dbo.SilkIconPackingLists", "ReceivedDate");
            DropColumn("dbo.SilkIconCartonDetails", "ReceivedDate");
        }
    }
}
