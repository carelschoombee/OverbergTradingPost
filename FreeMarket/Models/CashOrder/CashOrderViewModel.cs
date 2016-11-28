using System.Collections.Generic;
using System.Linq;

namespace FreeMarket.Models
{
    public class CashOrderViewModel
    {
        public CashOrder Order { get; set; }
        public List<CashOrderDetail> OrderDetails { get; set; }

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
                        CustomerPhone = result.PhoneNumber
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
    }
}