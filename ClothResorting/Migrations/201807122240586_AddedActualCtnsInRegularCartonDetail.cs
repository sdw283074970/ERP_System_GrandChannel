namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedActualCtnsInRegularCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegularCartonDetails", "ActualCtns", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "ActualCtns");
        }
    }
}
