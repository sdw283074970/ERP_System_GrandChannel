namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddActualReceivedColumnInPackingList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPackingLists", "ActualReceived", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconPackingLists", "ActualReceived");
        }
    }
}
