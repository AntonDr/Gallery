using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using ImageGallery.Models;

namespace ImageGallery.DAL
{
    public class GalleryContext : DbContext
    {
        public GalleryContext()
            : base("DefaultConnection")
        {
           
        }

        public DbSet<Photo> Photos { get; set; }
    }
}