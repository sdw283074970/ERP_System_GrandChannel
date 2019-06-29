namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedToPermanentCtnsAndPcsInRegularCartonDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegularCartonDetails", "ToPermanentCtns", c => c.Int(nullable: false));
            AddColumn("dbo.RegularCartonDetails", "ToPermanentPcs", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "ToPermanentPcs");
            DropColumn("dbo.RegularCartonDetails", "ToPermanentCtns");
        }
    }
}
