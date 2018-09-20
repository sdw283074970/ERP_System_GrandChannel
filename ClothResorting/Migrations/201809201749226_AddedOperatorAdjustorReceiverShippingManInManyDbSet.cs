namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOperatorAdjustorReceiverShippingManInManyDbSet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PreReceiveOrders", "Operator", c => c.String());
            AddColumn("dbo.FCRegularLocationDetails", "Allocator", c => c.String());
            AddColumn("dbo.RegularCartonDetails", "Receiver", c => c.String());
            AddColumn("dbo.RegularCartonDetails", "Adjustor", c => c.String());
            AddColumn("dbo.RegularCartonDetails", "Operator", c => c.String());
            AddColumn("dbo.POSummaries", "Operator", c => c.String());
            AddColumn("dbo.PullSheets", "Operator", c => c.String());
            AddColumn("dbo.PullSheets", "ShippingMan", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PullSheets", "ShippingMan");
            DropColumn("dbo.PullSheets", "Operator");
            DropColumn("dbo.POSummaries", "Operator");
            DropColumn("dbo.RegularCartonDetails", "Operator");
            DropColumn("dbo.RegularCartonDetails", "Adjustor");
            DropColumn("dbo.RegularCartonDetails", "Receiver");
            DropColumn("dbo.FCRegularLocationDetails", "Allocator");
            DropColumn("dbo.PreReceiveOrders", "Operator");
        }
    }
}
