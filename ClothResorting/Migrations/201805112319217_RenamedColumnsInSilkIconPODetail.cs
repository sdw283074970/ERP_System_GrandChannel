namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedColumnsInSilkIconPODetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilKIconPODetails", "TotalCartons", c => c.String());
            DropColumn("dbo.SilKIconPODetails", "Total");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilKIconPODetails", "Total", c => c.String());
            DropColumn("dbo.SilKIconPODetails", "TotalCartons");
        }
    }
}
