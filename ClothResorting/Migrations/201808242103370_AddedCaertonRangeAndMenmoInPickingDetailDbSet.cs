namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCaertonRangeAndMenmoInPickingDetailDbSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PickDetails", "Memo", c => c.String());
            AddColumn("dbo.PickDetails", "CartonRange", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PickDetails", "CartonRange");
            DropColumn("dbo.PickDetails", "Memo");
        }
    }
}
