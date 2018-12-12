using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImageGallery.Models
{

    public class Photo
    {
        [Key] public int PhotoId { get; set; }

        [Display(Name = "Description")]
        [Required]
        public string Description { get; set; }

        public byte[] Data { get; set; }


        [Display(Name = "Created On")] public DateTime CreatedOn { get; set; }

    }
}