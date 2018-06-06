namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAvailableInThreeDomainModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "Available", c => c.Int());
            AddColumn("dbo.SilkIconPackingLists", "Available", c => c.Int());
            AddColumn("dbo.SilkIconPreReceiveOrders", "Available", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconPreReceiveOrders", "Available");
            DropColumn("dbo.SilkIconPackingLists", "Available");
            DropColumn("dbo.SilkIconCartonDetails", "Available");
        }
    }
}
