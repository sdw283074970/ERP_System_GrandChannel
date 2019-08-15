namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUPCNumberForPermanentSKU : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PermanentSKUs", "UPCNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PermanentSKUs", "UPCNumber");
        }
    }
}
