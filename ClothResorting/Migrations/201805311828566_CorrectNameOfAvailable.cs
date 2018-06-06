namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CorrectNameOfAvailable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "AvailablePcs", c => c.Int());
            DropColumn("dbo.SilkIconCartonDetails", "AvaliablePcs");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconCartonDetails", "AvaliablePcs", c => c.Int());
            DropColumn("dbo.SilkIconCartonDetails", "AvailablePcs");
        }
    }
}
