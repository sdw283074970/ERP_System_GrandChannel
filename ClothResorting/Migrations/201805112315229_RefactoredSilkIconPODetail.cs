namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefactoredSilkIconPODetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilKIconPODetails", "PurchaseOrder_StyleNumber", c => c.String());
            DropColumn("dbo.SilKIconPODetails", "PurchaseOrderNumber");
            DropColumn("dbo.SilKIconPODetails", "Style");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SilKIconPODetails", "Style", c => c.String());
            AddColumn("dbo.SilKIconPODetails", "PurchaseOrderNumber", c => c.String());
            DropColumn("dbo.SilKIconPODetails", "PurchaseOrder_StyleNumber");
        }
    }
}
