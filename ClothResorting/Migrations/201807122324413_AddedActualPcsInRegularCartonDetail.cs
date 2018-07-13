namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedActualPcsInRegularCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegularCartonDetails", "ActualPcs", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "ActualPcs");
        }
    }
}
