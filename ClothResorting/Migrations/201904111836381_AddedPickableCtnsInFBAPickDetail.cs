namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPickableCtnsInFBAPickDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FBAPickDetails", "PickableCtns", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FBAPickDetails", "PickableCtns");
        }
    }
}
