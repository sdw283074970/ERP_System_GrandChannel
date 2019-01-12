namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedActualOrderTypeInFBAPickDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPickDetails", "OrderType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPickDetails", "OrderType");
        }
    }
}
