namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedParmenentLocationsDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PermanentLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseOrder = c.String(),
                        Style = c.String(),
                        Size = c.String(),
                        Quantity = c.Int(nullable: false),
                        Location = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PermanentLocations");
        }
    }
}
