namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModefiedInboundDateToNullableInRegularCartonDetail : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RegularCartonDetails", "InboundDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RegularCartonDetails", "InboundDate", c => c.DateTime(nullable: false));
        }
    }
}
