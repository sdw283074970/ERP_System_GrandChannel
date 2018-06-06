namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPropertiesInPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "Customer", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconPreReceiveOrders", "Customer");
        }
    }
}
