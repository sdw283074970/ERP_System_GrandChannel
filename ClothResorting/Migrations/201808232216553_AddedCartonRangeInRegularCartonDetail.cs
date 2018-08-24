namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCartonRangeInRegularCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "CartonRange", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FCRegularLocationDetails", "CartonRange");
        }
    }
}
