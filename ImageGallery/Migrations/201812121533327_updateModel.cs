namespace ImageGallery.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Photos", "Data", c => c.Binary());
            DropColumn("dbo.Photos", "ImagePath");
            DropColumn("dbo.Photos", "ThumbPath");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Photos", "ThumbPath", c => c.String());
            AddColumn("dbo.Photos", "ImagePath", c => c.String());
            DropColumn("dbo.Photos", "Data");
        }
    }
}
