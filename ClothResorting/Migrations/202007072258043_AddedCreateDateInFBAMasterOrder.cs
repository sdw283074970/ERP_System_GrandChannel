namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCreateDateInFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "CreateDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "CreateDate");
        }
    }
}
