namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSumPropertiesInSilkIconCartonDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SilkIconCartonDetails", "SumOfCarton", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SilkIconCartonDetails", "SumOfCarton");
        }
    }
}
