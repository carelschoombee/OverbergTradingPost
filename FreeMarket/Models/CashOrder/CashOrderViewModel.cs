using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace FreeMarket.Models
{
    public class CashOrderViewModel
    {
        public CashOrder Order { get; set; }
        public List<CashOrderDetail> OrderDetails { get; set; }
        public int SelectedCustodian { get; set; }

        [DisplayName("Custodian")]
        public string CustodianName { get; set; }
        public List<SelectListItem> Custodians { get; set; }
        public ProductCollection Products { get; set; }

        public static List<CashOrderViewModel> GetOrders(string searchCriteria)
        {
            List<CashOrderViewModel> model = new List<CashOrderViewModel>();

            if (string.IsNullOrEmpty(searchCriteria))
                return model;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                List<FilterCashOrder_Result> orders = db.FilterCashOrder(searchCriteria).ToList();

                if (orders == null)
                    return model;

                foreach (FilterCashOrder_Result result in orders)
                {
                    CashOrderViewModel viewModel = new CashOrderViewModel();
                    viewModel.Order = new CashOrder
                    {
                        CashCustomerId = (int)result.CashCustomerId,
                        OrderId = (int)result.OrderId,
                        Total = (int)result.Total,
                        CustomerDeliveryAddress = result.DeliveryAddress,
                        CustomerEmail = result.Email,
                        CustomerName = result.Name,
                        CustomerPhone = result.PhoneNumber,
                        DatePlaced = result.DatePlaced

                    };

                    viewModel.OrderDetails = db.GetCashOrderDetails(viewModel.Order.OrderId)
                        .Select(c => new CashOrderDetail
                        {
                            CashOrderId = c.CashOrderId,
                            CashOrderItemId = c.CashOrderItemId,
                            CustodianNumber = c.CustodianNumber,
                            Price = c.Price,
                            ProductNumber = c.ProductNumber,
                            Quantity = c.Quantity,
                            SupplierNumber = c.SupplierNumber,
                            Description = c.Description,
                            SupplierName = c.SupplierName,
                            Weight = (int)c.Weight,
                            OrderItemTotal = c.OrderItemTotal
                        })
                        .ToList();

                    model.Add(viewModel);
                }
            }

            return model;
        }

        public static CashOrderViewModel GetOrder(int id)
        {
            CashOrderViewModel model = new CashOrderViewModel();

            if (id == 0)
                return model;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                model.Order = db.CashOrders.Find(id);
                CashCustomer customer = db.CashCustomers.Find(model.Order.CashCustomerId);

                model.Order.CustomerDeliveryAddress = customer.DeliveryAddress;
                model.Order.CustomerEmail = customer.Email;
                model.Order.CustomerName = customer.Name;
                model.Order.CustomerPhone = customer.PhoneNumber;

                model.OrderDetails = db.GetCashOrderDetails(model.Order.OrderId)
                    .Select(c => new CashOrderDetail
                    {
                        CashOrderId = c.CashOrderId,
                        CashOrderItemId = c.CashOrderItemId,
                        CustodianNumber = c.CustodianNumber,
                        Price = c.Price,
                        ProductNumber = c.ProductNumber,
                        Quantity = c.Quantity,
                        SupplierNumber = c.SupplierNumber,
                        Description = c.Description,
                        SupplierName = c.SupplierName,
                        Weight = (int)c.Weight,
                        OrderItemTotal = c.OrderItemTotal
                    })
                    .ToList();

                model.Products = ProductCollection.GetAllProducts();

                for (int i = 0; i < model.Products.Products.Count; i++)
                {
                    CashOrderDetail qty = model.OrderDetails
                        .Where(c => c.ProductNumber == model.Products.Products[i].ProductNumber && c.SupplierNumber == model.Products.Products[i].SupplierNumber)
                        .FirstOrDefault();

                    if (qty != null)
                        model.Products.Products[i].CashQuantity = qty.Quantity;
                    else
                        model.Products.Products[i].CashQuantity = 0;
                }

                model.Custodians = db.Custodians.Select
                    (c => new SelectListItem
                    {
                        Text = c.CustodianName,
                        Value = c.CustodianNumber.ToString()
                    }).ToList();
            }

            return model;
        }

        public static CashOrderViewModel CreateNewOrder(int id = 0)
        {
            CashOrderViewModel model = new CashOrderViewModel();

            model.Order = new CashOrder();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                if (id != 0)
                {
                    CashCustomer customer = db.CashCustomers.Find(id);
                    if (customer != null)
                    {
                        model.Order.CashCustomerId = customer.Id;
                        model.Order.CustomerDeliveryAddress = customer.DeliveryAddress;
                        model.Order.CustomerEmail = customer.Email;
                        model.Order.CustomerName = customer.Name;
                        model.Order.CustomerPhone = customer.PhoneNumber;
                    }
                }

                model.Products = ProductCollection.GetAllProducts();

                model.Custodians = db.Custodians.Select
                    (c => new SelectListItem
                    {
                        Text = c.CustodianName,
                        Value = c.CustodianNumber.ToString()
                    }).ToList();
            }

            model.OrderDetails = new List<CashOrderDetail>();

            return model;
        }

        public static void InitializeDropDowns(CashOrderViewModel model)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                model.Custodians = db.Custodians.Select
                    (c => new SelectListItem
                    {
                        Text = c.CustodianName,
                        Value = c.CustodianNumber.ToString()
                    }).ToList();

                foreach (Product product in model.Products.Products)
                {
                    string normalPrice = string.Format("{0:C}", product.PricePerUnit);
                    string specialPrice = string.Format("{0:C}", product.SpecialPricePerUnit);

                    product.Prices = new List<SelectListItem>();

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
                }
            }
        }
    }
}