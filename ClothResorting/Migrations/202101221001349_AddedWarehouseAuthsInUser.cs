namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedWarehouseAuthsInUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "WarehouseAuths", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "WarehouseAuths");
        }
    }
}
