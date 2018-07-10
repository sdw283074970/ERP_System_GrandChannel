namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdjustedRegularLocationDetailModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegularLocationDetails", "SizeCombine", c => c.String());
            AddColumn("dbo.RegularLocationDetails", "PcsCombine", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularLocationDetails", "PcsCombine");
            DropColumn("dbo.RegularLocationDetails", "SizeCombine");
        }
    }
}
