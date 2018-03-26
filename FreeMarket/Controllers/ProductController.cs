﻿using FreeMarket.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Controllers
{
    public class ProductController : Controller
    {
        FreeMarketEntities db = new FreeMarketEntities();

        // GET: Product
        public ActionResult View(int id)
        {
            ProductCollection model = ProductCollection.GetProductsByDepartment(id);

            return View("Index", model);
        }

        public ActionResult IndexPlain()
        {
            return View();
        }

        public ActionResult Departments()
        {
            List<Department> departments = Department.GetModel();

            if (departments != null && departments.Count > 0)
                return View(departments);

            return View();
        }

        [ChildActionOnly]
        public ActionResult GetAllProducts()
        {
            ProductCollection products = ProductCollection.GetAllProducts();

            if (products.Products != null && products.Products.Count > 0)
                return PartialView("_ShowAllProducts", products);
            else
                return PartialView("_ShowAllProducts", new ProductCollection());
        }

        public ActionResult GetFullDescription(int productNumber, int supplierNumber)
        {
            return Content(Product.GetFullDescription(productNumber, supplierNumber));
        }


        public ActionResult GetDimensions(int productNumber)
        {
            string toReturn = "";
            Product product;

            if (productNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    product = db.Products
                       .Where(c => c.ProductNumber == productNumber)
                       .FirstOrDefault();

                    if (product != null)
                    {
                        if (product.Weight < 1)
                            toReturn = string.Format("{0} {1}", Math.Round(product.Weight * 1000, 0), "Grams");
                        else
                            toReturn = string.Format("{0} {1}", product.Weight, "KG");
                    }
                }
            }

            return Content(toReturn);
        }

        public ActionResult GetAverageRating(int productNumber, int supplierNumber)
        {
            return Content(ProductReviewsCollection.CalculateAverageRatingOnly(productNumber, supplierNumber).ToString());
        }

        public ActionResult GetReviews(int productNumber, int supplierNumber, int size = 4)
        {
            ProductReviewsCollection reviews = ProductReviewsCollection.GetReviewsOnly(productNumber, supplierNumber, size);

            return PartialView("_RatingPartial", reviews);
        }

        [HttpPost]
        public JsonResult LoadMoreReviews(int productNumber, int supplierNumber, int size)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductReviewsCollection model = new ProductReviewsCollection();

                model.ProductNumber = productNumber;
                model.SupplierNumber = supplierNumber;

                List<ProductReview> collection = db.ProductReviews
                    .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber && c.Approved == true)
                    .OrderByDescending(p => p.ReviewId)
                    .Skip(size)
                    .Take(size)
                    .ToList();

                model.Reviews = collection;

                int modelCount = db.ProductReviews
                    .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber && c.Approved == true)
                    .Count();

                if (model.Reviews.Any())
                {
                    string modelString = RenderRazorViewToString("_RatingPartialLoadMore", model);
                    return Json(new { ModelString = modelString, ModelCount = modelCount });
                }
                return Json(model);
            }
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext =
                     new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}