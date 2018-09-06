namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedContainerDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Containers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Vendor = c.String(),
                        ContainerNumber = c.String(),
                        Reference = c.String(),
                        ReceiptNumber = c.String(),
                        ReceivedDate = c.String(),
                        Remark = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Containers");
        }
    }
}
