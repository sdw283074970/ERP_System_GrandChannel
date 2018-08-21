namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCommentAreaInFCRegularCartondETAIL : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RegularCartonDetails", "Comment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RegularCartonDetails", "Comment");
        }
    }
}
