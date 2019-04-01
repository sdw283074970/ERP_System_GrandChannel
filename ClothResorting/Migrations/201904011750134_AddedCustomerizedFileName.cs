namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCustomerizedFileName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EFiles", "CustomizedFileName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EFiles", "CustomizedFileName");
        }
    }
}
