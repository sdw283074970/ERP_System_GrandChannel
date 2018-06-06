namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SimplifiedCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "Dimension", c => c.String());
            DropColumn("dbo.SilkIconCartonDetails", "Long");
            DropColumn("dbo.SilkIconCartonDetails", "Width");
            DropColumn("dbo.SilkIconCartonDetails", "Height");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconCartonDetails", "Height", c => c.Double());
            AddColumn("dbo.SilkIconCartonDetails", "Width", c => c.Double());
            AddColumn("dbo.SilkIconCartonDetails", "Long", c => c.Double());
            DropColumn("dbo.SilkIconCartonDetails", "Dimension");
        }
    }
}
