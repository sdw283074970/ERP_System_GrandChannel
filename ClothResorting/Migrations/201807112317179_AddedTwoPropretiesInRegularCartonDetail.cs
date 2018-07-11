namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTwoPropretiesInRegularCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegularCartonDetails", "Color", c => c.String());
            AddColumn("dbo.RegularCartonDetails", "Cartons", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "Cartons");
            DropColumn("dbo.RegularCartonDetails", "Color");
        }
    }
}
