namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPONumberInSilkIconCartonDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "PurchaseOrderNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconCartonDetails", "PurchaseOrderNumber");
        }
    }
}
