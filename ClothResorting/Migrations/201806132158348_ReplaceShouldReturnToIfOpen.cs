namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReplaceShouldReturnToIfOpen : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RetrievingRecords", "IfOpen", c => c.Boolean(nullable: false));
            DropColumn("dbo.RetrievingRecords", "ShoulReturnPcs");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RetrievingRecords", "ShoulReturnPcs", c => c.Int());
            DropColumn("dbo.RetrievingRecords", "IfOpen");
        }
    }
}
