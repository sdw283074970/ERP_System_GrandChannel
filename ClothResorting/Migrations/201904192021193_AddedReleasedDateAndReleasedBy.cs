namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedReleasedDateAndReleasedBy : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "ReleasedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "ReleasedBy");
        }
    }
}
