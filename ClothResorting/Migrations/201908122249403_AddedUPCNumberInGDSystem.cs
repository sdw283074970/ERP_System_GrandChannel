namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUPCNumberInGDSystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "UPCNumber", c => c.String());
            AddColumn("dbo.PickDetails", "UPCNumber", c => c.String());
            AddColumn("dbo.RegularCartonDetails", "UPCNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "UPCNumber");
            DropColumn("dbo.PickDetails", "UPCNumber");
            DropColumn("dbo.FCRegularLocationDetails", "UPCNumber");
        }
    }
}
