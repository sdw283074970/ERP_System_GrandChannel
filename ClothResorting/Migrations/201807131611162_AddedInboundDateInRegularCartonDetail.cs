namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInboundDateInRegularCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegularCartonDetails", "InboundDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "InboundDate");
        }
    }
}
