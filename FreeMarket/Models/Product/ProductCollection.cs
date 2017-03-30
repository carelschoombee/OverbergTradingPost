using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class ProductCollection
    {
        public List<Product> Products { get; set; }
        public List<ExternalWebsite> Websites { get; set; }

        public ProductCollection()
        {
            Products = new List<Product>();
            Websites = new List<ExternalWebsite>();
        }

        public static ProductCollection GetAllProducts()
        {
            ProductCollection allProducts = new ProductCollection();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                allProducts.Products = db.GetAllProducts()
                    .Select(c => new Product
                    {
                        Activated = c.Activated,
                        DateAdded = c.DateAdded,
                        DateModified = c.DateModified,
                        DepartmentName = c.DepartmentName,
                        DepartmentNumber = c.DepartmentNumber,
                        Description = c.Description,
                        PricePerUnit = c.PricePerUnit,
                        ProductNumber = c.ProductNumberID,
                        Size = c.Size,
                        SupplierName = c.SupplierName,
                        SupplierNumber = c.SupplierNumberID,
                        SpecialPricePerUnit = c.SpecialPricePerUnit ?? c.PricePerUnit,
                        RetailPricePerUnit = c.RetailPricePerUnit ?? c.PricePerUnit,
                        IsVirtual = c.IsVirtual,
                        Weight = c.Weight
                    }
                    ).ToList();

                SetProductData(allProducts);

                Debug.Write(allProducts);

                return allProducts;
            }
        }

        public static ProductCollection GetProductsByDepartment(int departmentNumber)
        {
            ProductCollection departmentProducts = new ProductCollection();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                departmentProducts.Products = db.GetAllProductsByDepartment(departmentNumber)
                    .Select(c => new Product
                    {
                        Activated = c.Activated,
                        DateAdded = c.DateAdded,
                        DateModified = c.DateModified,
                        DepartmentName = c.DepartmentName,
                        DepartmentNumber = c.DepartmentNumber,
                        Description = c.Description,
                        PricePerUnit = c.PricePerUnit,
                        ProductNumber = c.ProductNumberID,
                        Size = c.Size,
                        SupplierName = c.SupplierName,
                        SupplierNumber = c.SupplierNumberID,
                        SpecialPricePerUnit = c.SpecialPricePerUnit ?? c.PricePerUnit,
                        RetailPricePerUnit = c.RetailPricePerUnit ?? c.PricePerUnit,
                        IsVirtual = c.IsVirtual,
                        Weight = c.Weight
                    }
                    ).ToList();

                SetProductData(departmentProducts);

                departmentProducts.Websites = db.ExternalWebsites
                    .Where(c => c.Department == departmentNumber)
                    .ToList();

                SetWebsiteData(departmentProducts);

                return departmentProducts;
            }
        }

        public static ProductCollection GetAllProductsIncludingDeactivated()
        {
            ProductCollection allProducts = new ProductCollection();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                allProducts.Products = db.GetAllProductsIncludingDeactivated()
                    .Select(c => new Product
                    {
                        Activated = c.Activated,
                        DateAdded = c.DateAdded,
                        DateModified = c.DateModified,
                        DepartmentName = c.DepartmentName,
                        DepartmentNumber = c.DepartmentNumber,
                        Description = c.Description,
                        PricePerUnit = c.PricePerUnit,
                        ProductNumber = c.ProductNumberID,
                        Size = c.Size,
                        SupplierName = c.SupplierName,
                        SupplierNumber = c.SupplierNumberID,
                        SpecialPricePerUnit = c.SpecialPricePerUnit ?? c.PricePerUnit,
                        IsVirtual = c.IsVirtual,
                        Weight = c.Weight
                    }
                    ).ToList();

                SetProductData(allProducts);

                Debug.Write(allProducts);

                return allProducts;
            }
        }

        public static ProductCollection GetProductsInOrder(int orderNumber)
        {
            ProductCollection allProducts = new ProductCollection();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                allProducts.Products = db.GetAllProductsInOrder(orderNumber)
                    .Select(c => new Product
                    {
                        Activated = c.Activated,
                        DateAdded = c.DateAdded,
                        DateModified = c.DateModified,
                        DepartmentName = c.DepartmentName,
                        DepartmentNumber = c.DepartmentNumber,
                        Description = c.Description,
                        LongDescription = c.LongDescription,
                        PricePerUnit = c.PricePerUnit,
                        ProductNumber = c.ProductNumberID,
                        Size = c.Size,
                        SupplierName = c.SupplierName,
                        SupplierNumber = c.SupplierNumberID,
                        SpecialPricePerUnit = c.SpecialPricePerUnit ?? c.PricePerUnit,
                        Weight = c.Weight,
                        ProductRating = c.ProductRating ?? 0,
                        ProductReviewText = c.ProductReviewText,
                        PriceRating = c.PriceRating ?? 0,
                        ReviewId = c.ReviewId,
                        PriceOrder = c.Price,
                        IsVirtual = c.IsVirtual
                    }
                    ).ToList();

                SetProductData(allProducts);

                Debug.Write(allProducts);

                return allProducts;
            }
        }

        public static void SetProductData(ProductCollection allProducts)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                if (allProducts.Products != null && allProducts.Products.Count > 0)
                {
                    foreach (Product product in allProducts.Products)
                    {
                        int imageNumber = db.ProductPictures
                            .Where(c => c.ProductNumber == product.ProductNumber && c.Dimensions == PictureSize.Medium.ToString())
                            .Select(c => c.PictureNumber)
                            .FirstOrDefault();

                        int imageNumberSecondary = db.ProductPictures
                            .Where(c => c.ProductNumber == product.ProductNumber && c.Dimensions == PictureSize.Small.ToString())
                            .Select(c => c.PictureNumber)
                            .FirstOrDefault();

                        product.MainImageNumber = imageNumber;
                        product.SecondaryImageNumber = imageNumberSecondary;

                        product.Prices = new List<SelectListItem>();

                        string normalPrice = string.Format("{0:C}", product.PricePerUnit);
                        string specialPrice = string.Format("{0:C}", product.SpecialPricePerUnit);
                        string retailPrice = string.Format("{0:C}", product.RetailPricePerUnit);

                        product.Prices.Add(new SelectListItem
                        {
                            Text = normalPrice,
                            Value = product.PricePerUnit.ToString()
                        });

                        product.Prices.Add(new SelectListItem
                        {
                            Text = specialPrice,
                            Value = product.SpecialPricePerUnit.ToString(),
                            Selected = true
                        });

                        product.Prices.Add(new SelectListItem
                        {
                            Text = retailPrice,
                            Value = product.RetailPricePerUnit.ToString(),
                            Selected = true
                        });

                        product.CashQuantity = 0;
                    }
                }
            }
        }

        public static void SetWebsiteData(ProductCollection allProducts)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                if (allProducts.Websites != null && allProducts.Websites.Count > 0)
                {
                    foreach (ExternalWebsite website in allProducts.Websites)
                    {
                        int imageNumber = db.ExternalWebsitePictures
                            .Where(c => c.WebsiteNumber == website.LinkId && c.Dimensions == PictureSize.Medium.ToString())
                            .Select(c => c.PictureNumber)
                            .FirstOrDefault();

                        int imageNumberSecondary = db.ExternalWebsitePictures
                            .Where(c => c.WebsiteNumber == website.LinkId && c.Dimensions == PictureSize.Large.ToString())
                            .Select(c => c.PictureNumber)
                            .FirstOrDefault();

                        website.MainImageNumber = imageNumber;
                        website.AdditionalImageNumber = imageNumberSecondary;
                    }
                }
            }
        }

        public override string ToString()
        {
            string toReturn = "";

            if (Products != null && Products.Count > 0)
            {
                foreach (Product product in Products)
                {
                    toReturn += string.Format("{0}", product.ToString());
                }

            }
            return toReturn;
        }
    }
}