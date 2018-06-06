namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPcsDetailInPreReceivedOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "TotalPcs", c => c.Int());
            AddColumn("dbo.SilkIconPreReceiveOrders", "ActualReceivedPcs", c => c.Int());
            AddColumn("dbo.SilkIconPreReceiveOrders", "AvailablePcs", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconPreReceiveOrders", "AvailablePcs");
            DropColumn("dbo.SilkIconPreReceiveOrders", "ActualReceivedPcs");
            DropColumn("dbo.SilkIconPreReceiveOrders", "TotalPcs");
        }
    }
}
