﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImageGallery.Models;
using ImageGallery.DAL;
using System.Drawing;

namespace ImageGallery.Controllers
{
    
    public class HomeController : Controller
    {
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
                return PartialView("_ImagePartial",records);
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
                model.Data = new byte[file.ContentLength];
                file.InputStream.Read(model.Data, 0, file.ContentLength);
                //var fileName = Guid.NewGuid().ToString();
                //var extension = System.IO.Path.GetExtension(file.FileName).ToLower();

                //using (var img = System.Drawing.Image.FromStream(file.InputStream))
                //{
                //    model.ThumbPath = String.Format("/GalleryImages/thumbs/{0}{1}", fileName, extension);
                //    model.ImagePath = String.Format("/GalleryImages/{0}{1}", fileName, extension);

                //    // Save thumbnail size image, 100 x 100
                //    SaveToFolder(img, fileName, extension, new Size(100, 100), model.ThumbPath);

                //    // Save large size image, 800 x 800
                //    SaveToFolder(img, fileName, extension, new Size(600, 600), model.ImagePath);
                //}

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

            return PartialView("_ImagePartial",records);
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

                finalSize = new Size((int)(tempval * imageSize.Width), (int)(tempval * imageSize.Height));
            }
            else
                finalSize = imageSize; // image is already small size

            return finalSize;
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

        //private int GetPagesCount(int picturesOnPage)
        //{
        //    var totalPagesAmount = db.Photos.Count();
        //    var filledPagesAmount = totalPagesAmount / picturesOnPage;

        //    return totalPagesAmount % 2 == 0 ? filledPagesAmount : filledPagesAmount + 1;
        //}

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