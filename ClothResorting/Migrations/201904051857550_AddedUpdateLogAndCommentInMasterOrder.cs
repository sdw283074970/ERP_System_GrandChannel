namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUpdateLogAndCommentInMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "UpdateLog", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "Comment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "Comment");
            DropColumn("dbo.FBAMasterOrders", "UpdateLog");
        }
    }
}
