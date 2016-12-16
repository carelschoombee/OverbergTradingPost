using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class ProductCollection
    {
        public List<Product> Products { get; set; }

        public ProductCollection()
        {
            Products = new List<Product>();
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
                        Weight = c.Weight
                    }
                    ).ToList();

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

                        product.CashQuantity = 0;
                    }
                }

                Debug.Write(allProducts);

                return allProducts;
            }
        }

        public static ProductCollection GetAllSnackingNuts()
        {
            ProductCollection allProducts = new ProductCollection();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                allProducts.Products = db.GetAllProducts()
                    .Where(c => c.DepartmentName == "Snacking Nuts")
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
                        Weight = c.Weight
                    }
                    ).ToList();

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

                        product.CashQuantity = 0;
                    }
                }

                Debug.Write(allProducts);

                return allProducts;
            }
        }

        public static ProductCollection GetAllBakingNuts()
        {
            ProductCollection allProducts = new ProductCollection();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                allProducts.Products = db.GetAllProducts()
                    .Where(c => c.DepartmentName == "Baking Nuts")
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
                        Weight = c.Weight
                    }
                    ).ToList();

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

                        product.CashQuantity = 0;
                    }
                }

                Debug.Write(allProducts);

                return allProducts;
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
                        Weight = c.Weight
                    }
                    ).ToList();

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
                    }
                }

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
                        PriceOrder = c.Price
                    }
                    ).ToList();

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
                    }
                }

                Debug.Write(allProducts);

                return allProducts;
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