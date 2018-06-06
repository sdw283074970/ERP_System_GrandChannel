namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameTwoColumnsInSilkIconCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "PcsPerCartons", c => c.String());
            AddColumn("dbo.SilkIconCartonDetails", "TotalPcs", c => c.String());
            DropColumn("dbo.SilkIconCartonDetails", "TotalCartons");
            DropColumn("dbo.SilkIconCartonDetails", "Total");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconCartonDetails", "Total", c => c.String());
            AddColumn("dbo.SilkIconCartonDetails", "TotalCartons", c => c.String());
            DropColumn("dbo.SilkIconCartonDetails", "TotalPcs");
            DropColumn("dbo.SilkIconCartonDetails", "PcsPerCartons");
        }
    }
}
