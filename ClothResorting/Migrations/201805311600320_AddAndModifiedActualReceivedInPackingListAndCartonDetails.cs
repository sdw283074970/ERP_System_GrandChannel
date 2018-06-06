namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAndModifiedActualReceivedInPackingListAndCartonDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "ActualReceived", c => c.Int());
            AlterColumn("dbo.SilkIconPackingLists", "ActualReceived", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SilkIconPackingLists", "ActualReceived", c => c.String());
            DropColumn("dbo.SilkIconCartonDetails", "ActualReceived");
        }
    }
}
