namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStatusAndTwoPropertiesInRegularCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegularCartonDetails", "ToBeAllocatedCtns", c => c.Int(nullable: false));
            AddColumn("dbo.RegularCartonDetails", "ToBeAllocatedPcs", c => c.Int(nullable: false));
            AddColumn("dbo.RegularCartonDetails", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "Status");
            DropColumn("dbo.RegularCartonDetails", "ToBeAllocatedPcs");
            DropColumn("dbo.RegularCartonDetails", "ToBeAllocatedCtns");
        }
    }
}
