namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedSomeProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseOrderInventories", "Vender", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseOrderInventories", "Vender");
        }
    }
}
