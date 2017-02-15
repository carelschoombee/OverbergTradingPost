using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FreeMarket.Models
{
    [MetadataType(typeof(DepartmentMetaData))]
    public partial class Department
    {
        public int MainImageNumber { get; set; }
        public int SecondaryImageNumber { get; set; }

        public static List<Department> GetModel()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                List<Department> departments = db.Departments
                    .Where(c => c.Activated == true)
                    .ToList();

                Department.SetDepartmentData(departments);

                return departments;
            }
        }

        public static List<Department> GetModelIncludingAllDepartments()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                List<Department> departments = db.Departments.ToList();

                Department.SetDepartmentData(departments);

                return departments;
            }
        }

        public static Department GetDepartment(int Id)
        {
            Department model = new Department();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                model = db.Departments.Find(Id);

                model.MainImageNumber = db.DepartmentPictures
                    .Where(c => c.DepartmentNumber == model.DepartmentNumber && c.Dimensions == PictureSize.Medium.ToString())
                    .Select(c => c.PictureNumber)
                    .FirstOrDefault();

                model.SecondaryImageNumber = db.DepartmentPictures
                    .Where(c => c.DepartmentNumber == model.DepartmentNumber && c.Dimensions == PictureSize.Small.ToString())
                    .Select(c => c.PictureNumber)
                    .FirstOrDefault();
            }

            if (model == null)
                model = new Department();

            return model;
        }

        public static void SetDepartmentData(List<Department> departments)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                foreach (Department department in departments)
                {
                    int imageNumber = db.DepartmentPictures
                        .Where(c => c.DepartmentNumber == department.DepartmentNumber && c.Dimensions == PictureSize.Medium.ToString())
                        .Select(c => c.PictureNumber)
                        .FirstOrDefault();

                    int imageNumberSecondary = db.DepartmentPictures
                        .Where(c => c.DepartmentNumber == department.DepartmentNumber && c.Dimensions == PictureSize.Small.ToString())
                        .Select(c => c.PictureNumber)
                        .FirstOrDefault();

                    department.MainImageNumber = imageNumber;
                    department.SecondaryImageNumber = imageNumberSecondary;
                }
            }
        }

        public static void SaveModel(Department model)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static Department GetNewDepartment()
        {
            return new Department();
        }

        public static void CreateNewDepartment(Department department)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Departments.Add(department);
                db.SaveChanges();
            }
        }

        public static FreeMarketResult SaveDepartmentImage(int departmentNumber, PictureSize size, HttpPostedFileBase image)
        {
            if (image != null && departmentNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    DepartmentPicture picture = new DepartmentPicture();

                    Department department = db.Departments.Find(departmentNumber);

                    if (department == null)
                        return FreeMarketResult.Failure;

                    picture = db.DepartmentPictures
                        .Where(c => c.DepartmentNumber == departmentNumber && c.Dimensions == size.ToString())
                        .FirstOrDefault();

                    try
                    {
                        // No picture exists for this dimension/product
                        if (picture == null)
                        {
                            picture = new DepartmentPicture();
                            picture.Picture = new byte[image.ContentLength];
                            picture.Annotation = department.DepartmentName;
                            picture.Dimensions = size.ToString();
                            image.InputStream.Read(picture.Picture, 0, image.ContentLength);
                            picture.PictureMimeType = image.ContentType;
                            picture.DepartmentNumber = departmentNumber;

                            db.DepartmentPictures.Add(picture);
                            db.SaveChanges();

                            AuditUser.LogAudit(9, string.Format("Department: {0}", departmentNumber));
                            return FreeMarketResult.Success;
                        }
                        else
                        {
                            picture.Annotation = department.DepartmentName;
                            picture.Picture = new byte[image.ContentLength];
                            image.InputStream.Read(picture.Picture, 0, image.ContentLength);
                            picture.PictureMimeType = image.ContentType;

                            db.Entry(picture).State = EntityState.Modified;
                            db.SaveChanges();

                            AuditUser.LogAudit(9, string.Format("Department: {0}", departmentNumber));
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

    }

    public class DepartmentMetaData
    {
        [DisplayName("ID")]
        public int DepartmentNumber { get; set; }

        [Required]
        [DisplayName("Name")]
        public decimal DepartmentName { get; set; }
    }
}