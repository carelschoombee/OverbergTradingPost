using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FreeMarket.Models
{
    [MetadataType(typeof(CashOrderMetaData))]
    public partial class CashOrder
    {
        [DisplayName("Name")]
        public string CustomerName { get; set; }

        [DisplayName("Email")]
        public string CustomerEmail { get; set; }

        [DisplayName("Phone")]
        public string CustomerPhone { get; set; }

        [DisplayName("Address")]
        public string CustomerDeliveryAddress { get; set; }

        public static FreeMarketObject CreateNewCashOrder(CashOrderViewModel model)
        {
            FreeMarketObject result = new FreeMarketObject { Result = FreeMarketResult.NoResult, Argument = null, Message = null };

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                CashCustomer customer = db.CashCustomers.Find(model.Order.CashCustomerId);

                if (customer == null)
                {
                    customer = new CashCustomer
                    {
                        DeliveryAddress = model.Order.CustomerDeliveryAddress,
                        Email = model.Order.CustomerEmail,
                        Name = model.Order.CustomerName,
                        PhoneNumber = model.Order.CustomerPhone
                    };

                    db.CashCustomers.Add(customer);
                    db.SaveChanges();
                }
                else
                {
                    customer.DeliveryAddress = model.Order.CustomerDeliveryAddress;
                    customer.Email = model.Order.CustomerEmail;
                    customer.Name = model.Order.CustomerName;
                    customer.PhoneNumber = model.Order.CustomerPhone;

                    db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                CashOrder order = new CashOrder
                {
                    CashCustomerId = customer.Id,
                    DatePlaced = DateTime.Now,
                    Total = 0
                };

                db.CashOrders.Add(order);
                db.SaveChanges();

                foreach (Product p in model.Products.Products)
                {
                    if (p.CashQuantity > 0)
                    {
                        decimal price = decimal.Parse(p.SelectedPrice);
                        CashOrderDetail detail = new CashOrderDetail
                        {
                            CashOrderId = order.OrderId,
                            ProductNumber = p.ProductNumber,
                            SupplierNumber = p.SupplierNumber,
                            Quantity = p.CashQuantity,
                            Price = price,
                            OrderItemTotal = price * p.CashQuantity,
                            CustodianNumber = model.SelectedCustodian
                        };

                        db.CashOrderDetails.Add(detail);

                        order.Total += detail.OrderItemTotal;
                    }
                }

                db.SaveChanges();

                if (customer != null && order != null && db.CashOrderDetails.Any(c => c.CashOrderId == order.OrderId))
                    result.Result = FreeMarketResult.Success;
                else
                    result.Result = FreeMarketResult.Failure;
            }

            return result;
        }
    }

    public class CashOrderMetaData
    {

    }
}