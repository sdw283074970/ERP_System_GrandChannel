namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedNullableInSizeRatio : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SizeRatios", "Count", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SizeRatios", "Count", c => c.Int());
        }
    }
}
