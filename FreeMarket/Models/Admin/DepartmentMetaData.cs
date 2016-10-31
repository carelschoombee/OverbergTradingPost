using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(DepartmentMetaData))]
    public partial class Department
    {
        public static List<Department> GetModel()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.Departments.ToList();
            }
        }

        public static Department GetDepartment(int Id)
        {
            Department model = new Department();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                model = db.Departments.Find(Id);
            }

            if (model == null)
                model = new Department();

            return model;
        }

        public static void SaveModel(Department model)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
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