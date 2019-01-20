namespace Fancyauth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MultiCert : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.CertificateCredentials");
            AddColumn("dbo.CertificateCredentials", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.CertificateCredentials", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.CertificateCredentials");
            DropColumn("dbo.CertificateCredentials", "Id");
            AddPrimaryKey("dbo.CertificateCredentials", "UserId");
        }
    }
}
