namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPcsDetailInPackList : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPackingLists", "TotalPcs", c => c.Int());
            AddColumn("dbo.SilkIconPackingLists", "ActualReceivedPcs", c => c.Int());
            AddColumn("dbo.SilkIconPackingLists", "AvailablePcs", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconPackingLists", "AvailablePcs");
            DropColumn("dbo.SilkIconPackingLists", "ActualReceivedPcs");
            DropColumn("dbo.SilkIconPackingLists", "TotalPcs");
        }
    }
}
