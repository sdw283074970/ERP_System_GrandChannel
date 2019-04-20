namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedESTRangeAndInstructionInFBAShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "PlacedBy", c => c.String());
            AddColumn("dbo.FBAShipOrders", "Instruction", c => c.String());
            AddColumn("dbo.FBAShipOrders", "ETSTimeRange", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "ETSTimeRange");
            DropColumn("dbo.FBAShipOrders", "Instruction");
            DropColumn("dbo.FBAShipOrders", "PlacedBy");
        }
    }
}
