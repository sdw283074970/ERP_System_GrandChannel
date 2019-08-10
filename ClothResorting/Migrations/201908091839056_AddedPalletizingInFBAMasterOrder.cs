namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPalletizingInFBAMasterOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "Palletizing", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "Palletizing");
        }
    }
}
