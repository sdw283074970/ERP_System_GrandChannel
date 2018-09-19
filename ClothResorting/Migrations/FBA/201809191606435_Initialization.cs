namespace ClothResorting.Migrations.FBA
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initialization : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChargeMethods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Period = c.String(),
                        WeekNumber = c.Int(nullable: false),
                        Fee = c.Double(nullable: false),
                        ChargeTemplate_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChargeTemplates", t => t.ChargeTemplate_Id)
                .Index(t => t.ChargeTemplate_Id);
            
            CreateTable(
                "dbo.ChargeTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TemplateName = c.String(),
                        Customer = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChargeMethods", "ChargeTemplate_Id", "dbo.ChargeTemplates");
            DropIndex("dbo.ChargeMethods", new[] { "ChargeTemplate_Id" });
            DropTable("dbo.ChargeTemplates");
            DropTable("dbo.ChargeMethods");
        }
    }
}
