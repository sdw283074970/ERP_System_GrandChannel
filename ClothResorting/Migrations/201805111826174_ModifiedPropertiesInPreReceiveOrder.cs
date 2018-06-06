namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedPropertiesInPreReceiveOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "CustomerName", c => c.String());
            DropColumn("dbo.SilkIconPreReceiveOrders", "Customer");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilkIconPreReceiveOrders", "Customer", c => c.String());
            DropColumn("dbo.SilkIconPreReceiveOrders", "CustomerName");
        }
    }
}
