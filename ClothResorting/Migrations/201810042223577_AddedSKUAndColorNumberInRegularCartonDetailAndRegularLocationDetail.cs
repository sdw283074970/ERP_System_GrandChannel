namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSKUAndColorNumberInRegularCartonDetailAndRegularLocationDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "SKU", c => c.String());
            AddColumn("dbo.FCRegularLocationDetails", "ColorCode", c => c.String());
            AddColumn("dbo.RegularCartonDetails", "SKU", c => c.String());
            AddColumn("dbo.RegularCartonDetails", "ColorCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "ColorCode");
            DropColumn("dbo.RegularCartonDetails", "SKU");
            DropColumn("dbo.FCRegularLocationDetails", "ColorCode");
            DropColumn("dbo.FCRegularLocationDetails", "SKU");
        }
    }
}
