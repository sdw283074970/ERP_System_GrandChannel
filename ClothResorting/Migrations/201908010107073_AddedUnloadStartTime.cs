namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUnloadStartTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "UnloadStartTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "UnloadStartTime");
        }
    }
}
