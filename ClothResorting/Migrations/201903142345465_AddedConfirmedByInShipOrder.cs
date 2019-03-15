namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedConfirmedByInShipOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAShipOrders", "ConfirmedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAShipOrders", "ConfirmedBy");
        }
    }
}
