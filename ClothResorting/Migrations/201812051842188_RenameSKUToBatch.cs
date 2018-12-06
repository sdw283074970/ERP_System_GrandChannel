namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameSKUToBatch : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FCRegularLocationDetails", "Batch", c => c.String());
            AddColumn("dbo.RegularCartonDetails", "Batch", c => c.String());
            DropColumn("dbo.FCRegularLocationDetails", "SKU");
            DropColumn("dbo.RegularCartonDetails", "SKU");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RegularCartonDetails", "SKU", c => c.String());
            AddColumn("dbo.FCRegularLocationDetails", "SKU", c => c.String());
            DropColumn("dbo.RegularCartonDetails", "Batch");
            DropColumn("dbo.FCRegularLocationDetails", "Batch");
        }
    }
}
