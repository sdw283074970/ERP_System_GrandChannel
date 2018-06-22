namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInvPcsPropertyInPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PreReceiveOrders", "InvPcs", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PreReceiveOrders", "InvPcs");
        }
    }
}
