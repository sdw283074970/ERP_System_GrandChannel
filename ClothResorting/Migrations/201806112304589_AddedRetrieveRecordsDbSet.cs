namespace ClothResorting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedRetrieveRecordsDbSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RetrievingRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Location = c.String(),
                        PurchaseOrderNumber = c.String(),
                        Style = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        RetrivedPcs = c.Int(),
                        TargetPcs = c.Int(),
                        NumberOfCartons = c.Int(),
                        TotalOfCartons = c.Int(),
                        IsOpened = c.Boolean(nullable: false),
                        RetrievedDate = c.DateTime(),
                        CartonDetail_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SilkIconCartonDetails", t => t.CartonDetail_Id)
                .Index(t => t.CartonDetail_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RetrievingRecords", "CartonDetail_Id", "dbo.SilkIconCartonDetails");
            DropIndex("dbo.RetrievingRecords", new[] { "CartonDetail_Id" });
            DropTable("dbo.RetrievingRecords");
        }
    }
}
