namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVenderPropertyInPermanentLocationAndAddedTartgetBanlancePropertyInPermanentLocIORecord : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PermanentLocations", "Vender", c => c.String());
            AddColumn("dbo.PermanentLocIORecords", "TargetBalance", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PermanentLocIORecords", "TargetBalance");
            DropColumn("dbo.PermanentLocations", "Vender");
        }
    }
}
