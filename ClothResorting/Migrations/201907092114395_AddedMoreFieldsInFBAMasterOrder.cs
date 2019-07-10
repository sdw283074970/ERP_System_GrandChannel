namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMoreFieldsInFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "UnloadTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "Lumper", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "Instruction", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "DockNumber", c => c.String());
            AddColumn("dbo.FBAMasterOrders", "PushTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "AvailableTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.FBAMasterOrders", "OutTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "OutTime");
            DropColumn("dbo.FBAMasterOrders", "AvailableTime");
            DropColumn("dbo.FBAMasterOrders", "PushTime");
            DropColumn("dbo.FBAMasterOrders", "DockNumber");
            DropColumn("dbo.FBAMasterOrders", "Instruction");
            DropColumn("dbo.FBAMasterOrders", "Lumper");
            DropColumn("dbo.FBAMasterOrders", "UnloadTime");
        }
    }
}
