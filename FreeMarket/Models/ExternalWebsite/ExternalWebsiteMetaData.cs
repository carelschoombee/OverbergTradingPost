using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    [MetadataType(typeof(ExternalWebsiteMetaData))]
    public partial class ExternalWebsite
    {
        public string SelectedDepartment { get; set; }
        public List<SelectListItem> Departments { get; set; }
        public int MainImageNumber { get; set; }
        public int AdditionalImageNumber { get; set; }

        public static ExternalWebsite GetNewWebsite()
        {
            ExternalWebsite website = new ExternalWebsite();

            website.DateAdded = DateTime.Now;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                website.Departments = db.Departments
                    .Select(c => new SelectListItem
                    {
                        Text = "(" + c.DepartmentNumber + ")" + c.DepartmentName,
                        Value = c.DepartmentNumber.ToString()
                    })
                    .ToList();
            }

            return website;
        }

        public static void CreateNewExternalWebsite(ExternalWebsite website)
        {
            try
            {
                website.DateAdded = DateTime.Now;
                website.Department = int.Parse(website.SelectedDepartment);
                if (website.Url.StartsWith("http://") || website.Url.StartsWith("https://"))
                {

                }
                else
                {
                    website.Url = website.Url.Insert(0, "http://");
                }
            }
            catch
            {
                return;
            }

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.ExternalWebsites.Add(website);
                db.SaveChanges();
            }
        }

        public void InitializeDropDowns()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Departments = db.Departments
                    .Select(c => new SelectListItem
                    {
                        Text = "(" + c.DepartmentNumber + ") " + c.DepartmentName,
                        Value = c.DepartmentNumber.ToString(),
                        Selected = (c.DepartmentNumber == Department) ? true : false
                    })
                    .ToList();
            }
        }

        public static FreeMarketResult SaveImage(int websiteNumber, PictureSize size, HttpPostedFileBase image)
        {
            if (image != null && websiteNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    ExternalWebsitePicture picture = new ExternalWebsitePicture();

                    ExternalWebsite site = db.ExternalWebsites.Find(websiteNumber);

                    if (site == null)
                        return FreeMarketResult.Failure;

                    picture = db.ExternalWebsitePictures
                    .Where(c => c.WebsiteNumber == websiteNumber && c.Dimensions == size.ToString())
                    .FirstOrDefault();

                    try
                    {
                        // No picture exists for this dimension/product
                        if (picture == null)
                        {
                            picture = new ExternalWebsitePicture();
                            picture.Picture = new byte[image.ContentLength];
                            picture.Annotation = site.Description;
                            picture.Dimensions = size.ToString();
                            image.InputStream.Read(picture.Picture, 0, image.ContentLength);
                            picture.PictureMimeType = image.ContentType;
                            picture.WebsiteNumber = websiteNumber;

                            db.ExternalWebsitePictures.Add(picture);
                            db.SaveChanges();

                            AuditUser.LogAudit(40, string.Format("Website ID: {0}", websiteNumber));
                            return FreeMarketResult.Success;
                        }
                        else
                        {
                            picture.Annotation = site.Description;
                            picture.Picture = new byte[image.ContentLength];
                            image.InputStream.Read(picture.Picture, 0, image.ContentLength);
                            picture.PictureMimeType = image.ContentType;

                            db.Entry(picture).State = EntityState.Modified;
                            db.SaveChanges();

                            AuditUser.LogAudit(40, string.Format("Website ID: {0}", websiteNumber));
                            return FreeMarketResult.Success;
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionLogging.LogException(e);
                        return FreeMarketResult.Failure;
                    }
                }
            }

            return FreeMarketResult.Failure;
        }

        public static ExternalWebsite GetWebsite(int websiteNumber)
        {
            ExternalWebsite website = new ExternalWebsite();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                var websiteInfo = db.ExternalWebsites.Find(websiteNumber);

                if (websiteInfo == null)
                    return website;
                else
                    website = websiteInfo;

                website.MainImageNumber = db.ExternalWebsitePictures
                    .Where(c => c.WebsiteNumber == website.LinkId && c.Dimensions == PictureSize.Medium.ToString())
                    .Select(c => c.PictureNumber)
                    .FirstOrDefault();

                website.AdditionalImageNumber = db.ExternalWebsitePictures
                    .Where(c => c.WebsiteNumber == website.LinkId && c.Dimensions == PictureSize.Large.ToString())
                    .Select(c => c.PictureNumber)
                    .FirstOrDefault();

                website.Departments = db.Departments
                    .Select(c => new SelectListItem
                    {
                        Text = "(" + c.DepartmentNumber + ") " + c.DepartmentName,
                        Value = c.DepartmentNumber.ToString(),
                        Selected = c.DepartmentNumber == website.Department ? true : false
                    })
                    .ToList();
            }

            return website;
        }

        public void InitializeDropDowns(string mode)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                Departments = db.Departments
                    .Select(c => new SelectListItem
                    {
                        Text = "(" + c.DepartmentNumber + ") " + c.DepartmentName,
                        Value = c.DepartmentNumber.ToString(),
                        Selected = (c.DepartmentNumber == Department) ? true : false
                    })
                    .ToList();
            }
        }

        public static List<ExternalWebsite> GetAllWebsites()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                List<ExternalWebsite> allWebsites = db.ExternalWebsites.ToList();

                if (allWebsites != null && allWebsites.Count > 0)
                {
                    foreach (ExternalWebsite website in allWebsites)
                    {
                        int imageNumber = db.ExternalWebsitePictures
                            .Where(c => c.WebsiteNumber == website.LinkId && c.Dimensions == PictureSize.Medium.ToString())
                            .Select(c => c.PictureNumber)
                            .FirstOrDefault();

                        int imageNumberSecondary = db.ExternalWebsitePictures
                            .Where(c => c.WebsiteNumber == website.LinkId && c.Dimensions == PictureSize.Medium.ToString())
                            .Select(c => c.PictureNumber)
                            .FirstOrDefault();

                        website.MainImageNumber = imageNumber;
                        website.AdditionalImageNumber = imageNumberSecondary;
                    }

                    return allWebsites;
                }
                else
                {
                    return new List<ExternalWebsite>();
                }
            }
        }

        public static void SaveExternalWebsite(ExternalWebsite website)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                try
                {
                    if (website.Url.StartsWith("http://") || website.Url.StartsWith("https://"))
                    {

                    }
                    else
                    {
                        website.Url = website.Url.Insert(0, "http://");
                    }

                    website.Department = int.Parse(website.SelectedDepartment);
                    db.Entry(website).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    ExceptionLogging.LogException(e);
                }
            }
        }
    }
    public class ExternalWebsiteMetaData
    {
        [DisplayName("Id")]
        public string LinkId { get; set; }

        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Website Address (URL)")]
        public string Url { get; set; }

        [Required]
        [DisplayName("Date Added")]
        public string DateAdded { get; set; }

        [Required]
        [DisplayName("Short Description")]
        public string Description { get; set; }

        [Required]
        [DisplayName("Department / Category")]
        public string Department { get; set; }

        [DisplayName("Activated")]
        public string Activated { get; set; }
    }
}