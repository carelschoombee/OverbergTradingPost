using System;
using System.Collections.Generic;
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
                    Status = "Completed",
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
                        db.SaveChanges();

                        order.Total += detail.OrderItemTotal;

                        db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        ProductCustodian custodian = db.ProductCustodians
                            .Where(c => c.CustodianNumber == model.SelectedCustodian &&
                                                c.ProductNumber == p.ProductNumber &&
                                                c.SupplierNumber == p.SupplierNumber)
                                                .FirstOrDefault();

                        custodian.QuantityOnHand -= p.CashQuantity;

                        db.Entry(custodian).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
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

        public static FreeMarketObject ModifyOrder(CashOrderViewModel model)
        {
            FreeMarketObject result = new FreeMarketObject { Result = FreeMarketResult.NoResult, Argument = null, Message = null };

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                CashCustomer customer = db.CashCustomers.Find(model.Order.CashCustomerId);

                customer.DeliveryAddress = model.Order.CustomerDeliveryAddress;
                customer.Email = model.Order.CustomerEmail;
                customer.Name = model.Order.CustomerName;
                customer.PhoneNumber = model.Order.CustomerPhone;

                db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                CashOrder order = db.CashOrders.Find(model.Order.OrderId);

                List<GetCashOrderDetails_Result> orderDetails = db.GetCashOrderDetails(order.OrderId).ToList();

                foreach (Product p in model.Products.Products)
                {
                    if (p.CashQuantity > 0)
                    {
                        decimal price = decimal.Parse(p.SelectedPrice);

                        if (orderDetails.Any(c => c.ProductNumber == p.ProductNumber && c.SupplierNumber == p.SupplierNumber))
                        {
                            CashOrderDetail existingDetail = db.CashOrderDetails
                                .Where(c => c.CashOrderId == order.OrderId && c.ProductNumber == p.ProductNumber && c.SupplierNumber == p.SupplierNumber)
                                .FirstOrDefault();
                            existingDetail.Price = price;
                            existingDetail.OrderItemTotal = price * p.CashQuantity;

                            if (existingDetail.Quantity > p.CashQuantity)
                            {
                                int stock = existingDetail.Quantity - p.CashQuantity;
                                AddStockToCustodian(order.OrderId, p.ProductNumber, p.SupplierNumber, model.SelectedCustodian, stock);
                            }
                            else
                            {
                                int stock = p.CashQuantity - existingDetail.Quantity;
                                RemoveStockFromCustodian(order.OrderId, p.ProductNumber, p.SupplierNumber, model.SelectedCustodian, stock);
                            }

                            existingDetail.Quantity = p.CashQuantity;

                            db.Entry(existingDetail).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
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
                            db.SaveChanges();

                            RemoveStockFromCustodian(order.OrderId, p.ProductNumber, p.SupplierNumber, model.SelectedCustodian, p.CashQuantity);
                        }
                    }
                    else
                    {
                        CashOrderDetail toRemove = db.CashOrderDetails
                            .Where(c => c.CashOrderId == order.OrderId && c.ProductNumber == p.ProductNumber && c.SupplierNumber == p.SupplierNumber)
                            .FirstOrDefault();

                        if (toRemove != null)
                        {
                            db.CashOrderDetails.Remove(toRemove);

                            AddStockToCustodian(order.OrderId, p.ProductNumber, p.SupplierNumber, model.SelectedCustodian, toRemove.Quantity);
                        }
                    }
                }

                db.SaveChanges();

                List<GetCashOrderDetails_Result> details = db.GetCashOrderDetails(order.OrderId).ToList();
                order.Total = details.Sum(c => c.OrderItemTotal);
                order.DatePlaced = DateTime.Now;
                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                if (customer != null && order != null && db.CashOrderDetails.Any(c => c.CashOrderId == order.OrderId))
                    result.Result = FreeMarketResult.Success;
                else
                    result.Result = FreeMarketResult.Failure;
            }

            return result;
        }

        public static void RefundOrder(int id)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                CashOrder order = db.CashOrders.Find(id);

                if (order != null)
                {
                    order.Status = "Refunded";
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public static void AddStockToCustodian(int orderId, int productNumber, int supplierNumber, int custodianNumber, int quantity)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {

                ProductCustodian custodian = db.ProductCustodians
                                .Where(c => c.CustodianNumber == custodianNumber &&
                                                    c.ProductNumber == productNumber &&
                                                    c.SupplierNumber == supplierNumber)
                                                    .FirstOrDefault();

                custodian.QuantityOnHand += quantity;

                db.Entry(custodian).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }

        public static void RemoveStockFromCustodian(int orderId, int productNumber, int supplierNumber, int custodianNumber, int quantity)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {

                ProductCustodian custodian = db.ProductCustodians
                                .Where(c => c.CustodianNumber == custodianNumber &&
                                                    c.ProductNumber == productNumber &&
                                                    c.SupplierNumber == supplierNumber)
                                                    .FirstOrDefault();

                custodian.QuantityOnHand -= quantity;

                db.Entry(custodian).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
    }

    public class CashOrderMetaData
    {

    }
}