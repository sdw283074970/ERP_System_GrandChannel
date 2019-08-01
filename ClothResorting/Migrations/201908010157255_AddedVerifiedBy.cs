namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVerifiedBy : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAMasterOrders", "VerifiedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAMasterOrders", "VerifiedBy");
        }
    }
}
