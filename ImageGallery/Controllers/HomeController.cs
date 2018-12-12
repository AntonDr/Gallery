using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImageGallery.Models;
using ImageGallery.DAL;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Drawing;
using System.IO;
using Microsoft.WindowsAzure.Storage.Auth;

namespace ImageGallery.Controllers
{

    public class HomeController : Controller
    {
        private StorageCredentials storageCredentials;
        private CloudStorageAccount cloudStorageAccount;
        private CloudBlobClient cloudBlobClient;
        private CloudBlobContainer cloudBlobContainer;
        private CloudBlockBlob cloudBlockBlob;

        public HomeController()
        {
            storageCredentials = new StorageCredentials("webgalstr", "IqnwjBAVTXPNc4WSX60L6BXT8XG29mxi2ZzLmIc+i55VHRuus4yp3JXlEam0lfzOAOyLZi1QPRkQP9zk4YZCGg==");
            cloudStorageAccount = new CloudStorageAccount(storageCredentials,true);
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            cloudBlobContainer = cloudBlobClient.GetContainerReference("imagedata");

            var permissions = cloudBlobContainer.GetPermissions();
            if (permissions.PublicAccess == BlobContainerPublicAccessType.Off || permissions.PublicAccess == BlobContainerPublicAccessType.Unknown)
            {
                // If blob isn't public, we can't directly link to the pictures
                cloudBlobContainer.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Blob });
            }

        }

        private GalleryContext db = new GalleryContext();

        public ActionResult Index(string filter = null, int page = 1, int pageSize = 20)
        {
            var records = new PagedList<Photo>();

            ViewBag.filter = filter;

            records.Content = db.Photos
                .Where(x => filter == null || (x.Description.Contains(filter)))
                .OrderByDescending(x => x.PhotoId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Count
            records.TotalRecords = db.Photos.Count(x => filter == null || (x.Description.Contains(filter)));

            records.CurrentPage = page;
            records.PageSize = pageSize;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ImagePartial", records);
            }

            return View(records);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var photo = new Photo();
            return View(photo);
        }

        [HttpPost]
        public ActionResult Create(Photo photo, IEnumerable<HttpPostedFileBase> files)
        {
            if (!ModelState.IsValid)
                return View(photo);

            if (files.Count() == 0 || files.FirstOrDefault() == null)
            {
                ViewBag.error = "Please choose a file";
                return View(photo);
            }

            var model = new Photo();

            foreach (var file in files)
            {
                if (file.ContentLength == 0) continue;

                model.Description = photo.Description;
                var fileName = Guid.NewGuid().ToString();
                var extension = System.IO.Path.GetExtension(file.FileName).ToLower();

                using (var img = System.Drawing.Image.FromStream(file.InputStream))
                {
                    model.ThumbPath = String.Format("/GalleryImages/thumbs/{0}{1}", fileName, extension);

                    model.ImagePath = String.Format("/GalleryImages/{0}{1}", fileName, extension);

                    //CloudBlob blob = cloudBlobContainer.GetBlobReference(model.ThumbPath);
                    //blob.DownloadToFile(model.ThumbPath,FileMode.Create);
                    //blob = cloudBlobContainer.GetBlobReference(model.ImagePath);
                    //blob.DownloadToFile(model.ImagePath,FileMode.Create);
                    //var imageBlob = cloudBlobContainer.GetBlockBlobReference(model.ThumbPath);
                    //imageBlob.UploadFromStreamAsync(file.InputStream);
                    //imageBlob = cloudBlobContainer.GetBlockBlobReference(model.ImagePath);
                    //imageBlob.UploadFromStreamAsync(file.InputStream);

                    //CloudBlockBlob blockBlobImage = cloudBlobContainer.GetBlockBlobReference(model.ImagePath);
                    //using (var fileStream = System.IO.File.OpenRead(model.ImagePath))
                    //{
                    //    blockBlobImage.UploadFromStream(fileStream);
                    //}

                    // Save thumbnail size image, 100 x 100
                    SaveToFolder(img, fileName, extension, new Size(100, 100), model.ThumbPath);

                    // Save large size image, 800 x 800
                    SaveToFolder(img, fileName, extension, new Size(800, 800), model.ImagePath);
                }

                // Save record to database
                model.CreatedOn = DateTime.Now;
                db.Photos.Add(model);
                db.SaveChanges();
            }

            return RedirectPermanent("/home");
        }


        public ActionResult PartialIndex(
            string filter = null, int page = 1, int pageSize = 20)
        {
            if (!Request.IsAjaxRequest())
            {
                return Redirect(this.GenerateRedirectUrl(filter, page));
            }

            var records = new PagedList<Photo>();

            ViewBag.filter = filter;

            records.Content = db.Photos
                .Where(x => filter == null || (x.Description.Contains(filter)))
                .OrderByDescending(x => x.PhotoId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Count
            records.TotalRecords = db.Photos.Count(x => filter == null || (x.Description.Contains(filter)));

            records.CurrentPage = page;
            records.PageSize = pageSize;

            return PartialView("_ImagePartial", records);
        }

        public Size NewImageSize(Size imageSize, Size newSize)
        {
            Size finalSize;
            double tempval;
            if (imageSize.Height > newSize.Height || imageSize.Width > newSize.Width)
            {
                if (imageSize.Height > imageSize.Width)
                    tempval = newSize.Height / (imageSize.Height * 1.0);
                else
                    tempval = newSize.Width / (imageSize.Width * 1.0);

                finalSize = new Size((int) (tempval * imageSize.Width), (int) (tempval * imageSize.Height));
            }
            else
                finalSize = imageSize; // image is already small size

            return finalSize;
        }

        private void SaveToFolder(Image img, string fileName, string extension, Size newSize, string pathToSave)
        {
            // Get new resolution
            Size imgSize = NewImageSize(img.Size, newSize);
            using (System.Drawing.Image newImg = new Bitmap(img, imgSize.Width, imgSize.Height))
            {
                var imageBlob = cloudBlobContainer.GetBlockBlobReference(pathToSave);
                newImg.Save(imageBlob.Uri.ToString(),img.RawFormat);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private string GenerateRedirectUrl(string filter, int page)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return $"../Home/Index?{nameof(page)}={page}";
            }

            return $"../Home/Index?{nameof(filter)}={filter}&&{nameof(page)}={page}";
        }
    }
}
