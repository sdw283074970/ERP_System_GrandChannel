namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPermanentLocIORecordDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PermanentLocIORecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PermanentLocation = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        TargetPcs = c.Int(nullable: false),
                        InvBefore = c.Int(nullable: false),
                        InvChange = c.Int(nullable: false),
                        InvAfter = c.Int(nullable: false),
                        FromLocation = c.String(),
                        OperationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PermanentLocIORecords");
        }
    }
}
