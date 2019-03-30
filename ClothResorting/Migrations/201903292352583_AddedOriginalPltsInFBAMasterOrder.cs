namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOriginalPltsInFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "OriginalPlts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "OriginalPlts");
        }
    }
}
