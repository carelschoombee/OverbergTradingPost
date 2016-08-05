using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

namespace FreeMarket.Models
{
    public class ShoppingCart
    {
        public OrderHeader Order { get; set; }
        public CartBody Body { get; set; }

        public void Initialize(string userId)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Get the order and body from the database with all joining fields

                Order = OrderHeader.GetOrderForShoppingCart(userId);
                Body = CartBody.GetDetailsForShoppingCart(Order.OrderNumber);
            }
        }

        public FreeMarketResult AddItem(int productNumber, int supplierNumber, int courierNumber, short quantity, string userId = null)
        {
            // Check whether the item already exists

            OrderDetail existingItem = Body.OrderDetails
                .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber)
                .FirstOrDefault();

            if (existingItem != null)
            {
                // Update the existing item

                Debug.Write(string.Format("\nIncrementing quantity in session..."));

                existingItem.Quantity += quantity;
                existingItem.OrderItemValue = existingItem.Price * existingItem.Quantity;
            }
            else
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    Product tempProduct = db.Products.Find(productNumber);
                    Supplier tempSupplier = db.Suppliers.Find(supplierNumber);
                    Courier tempCourier = db.Couriers.Find(courierNumber);

                    if (tempProduct == null || tempSupplier == null || tempCourier == null)
                    {
                        Debug.Write("Product, Supplier or Courier does not exist.");
                        return FreeMarketResult.Failure;
                    }

                    Debug.Write(string.Format("\nAdding new item to session..."));

                    string status = "Unconfirmed";

                    // Add the new item to the Session variable

                    Body.OrderDetails.Add(
                        new OrderDetail()
                        {
                            CourierFee = null,
                            CourierNumber = tempCourier.CourierNumber,
                            CustomerCourierOnTimeDeliveryRating = null,
                            CustomerProductQualityRating = null,
                            DeliveryDateActual = null,
                            DeliveryDateAgreed = null,
                            OrderItemStatus = status,
                            OrderItemValue = tempProduct.Price * quantity,
                            PaidCourier = null,
                            PaidSupplier = null,
                            PayCourier = null,
                            PaySupplier = null,
                            Price = tempProduct.Price,
                            ProductNumber = tempProduct.ProductNumber,
                            Quantity = quantity,
                            Settled = null,
                            SupplierNumber = tempSupplier.SupplierNumber,
                            OrderNumber = Order.OrderNumber
                        });
                }
            }

            // Keep the OrderTotal in sync

            UpdateTotal();

            AuditUser.LogAudit(7, string.Format("Order number: {0}", Order.OrderNumber), userId);

            return FreeMarketResult.Success;
        }

        public void RemoveItem(int itemNumber, int productNumber, int supplierNumber, int courierNumber, string userId = null)
        {
            if (itemNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    OrderDetail item = db.OrderDetails.Find(itemNumber);

                    if (item != null)
                    {
                        Debug.Write(string.Format("Removing Item {0} from database...", itemNumber));

                        db.OrderDetails.Remove(item);
                        db.SaveChanges();
                    }
                }
            }

            Debug.Write(string.Format("Removing Product {0} from Session...", productNumber));

            Body.OrderDetails.RemoveAll(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber && c.CourierNumber == courierNumber);

            AuditUser.LogAudit(8, string.Format("Order number: {0}", Order.OrderNumber), userId);
        }

        public void UpdatePrices()
        {
            if (Order != null && Order.OrderNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    Debug.Write(string.Format("Updating prices for order {0} ...", Order.OrderNumber));

                    db.UpdatePrices(Order.OrderNumber);
                }
            }
        }

        public void Save(string userId = null)
        {
            // Compare the Session cart to the database cart

            Compare();

            // Re-initialize the Body

            Body = CartBody.GetDetailsForShoppingCart(Order.OrderNumber);

            // Keep the total order value in sync

            UpdateTotal();

            // Save the Order total
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                db.Entry(Order).State = EntityState.Modified;
                db.SaveChanges();
            }

            AuditUser.LogAudit(6, string.Format("Order number: {0}", Order.OrderNumber), userId);
        }

        public void UpdateTotal()
        {
            Debug.Write(string.Format("Updating Total..."));

            Order.TotalOrderValue = Body.OrderDetails.Sum(c => c.OrderItemValue);
        }

        public void Checkout() { }

        public void Compare()
        {
            // Get a list of items that are on the Session variable but not in the database

            List<OrderDetail> newItems = Body.OrderDetails.FindAll(c => c.ItemNumber == 0);

            if (newItems != null && newItems.Count > 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    foreach (OrderDetail tempB in newItems)
                    {
                        Debug.Write(string.Format("Adding product number {0} to database ...", tempB.ProductNumber));
                        db.OrderDetails.Add(tempB);
                    }

                    db.SaveChanges();
                }
            }

            // Get a list of items which are on both the Session and database

            List<OrderDetail> existingItems = Body.OrderDetails.FindAll(c => c.ItemNumber != 0);

            if (existingItems != null && existingItems.Count > 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    foreach (OrderDetail temp in existingItems)
                    {
                        OrderDetail tempDb = db.OrderDetails.Find(temp.ItemNumber);

                        if (tempDb != null)
                        {
                            if (!temp.Equals(tempDb))
                            {
                                // If the item has changed update it

                                Debug.Write(string.Format("Updating item number {0} ...", tempDb.ItemNumber));

                                tempDb.Quantity = temp.Quantity;
                                tempDb.SupplierNumber = temp.SupplierNumber;
                                tempDb.CourierNumber = temp.CourierNumber;
                                tempDb.ProductNumber = temp.ProductNumber;

                                db.Entry(tempDb).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        public void Merge(ShoppingCart tempCart, string userId)
        {
            // This is for when a user adds items to the Session variable without logging on first
            // When he logs on the database and Session are merged

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                if (tempCart != null && tempCart.Body.OrderDetails != null)
                {
                    foreach (OrderDetail tempOrderDetail in tempCart.Body.OrderDetails)
                    {
                        AddItem(tempOrderDetail.ProductNumber, tempOrderDetail.SupplierNumber, tempOrderDetail.CourierNumber, tempOrderDetail.Quantity, userId);
                    }

                    Save(userId);
                    Initialize(userId);
                }
            }
        }

        public ShoppingCart(string userId)
        {
            Initialize(userId);
        }

        public ShoppingCart()
        {
            Body = new CartBody();
            Order = new OrderHeader();
        }

        public override string ToString()
        {
            string toString = "";

            toString += "Shopping Cart Contents:";
            toString += string.Format("{0}{1}", Order.ToString(), Body.ToString());

            return toString;
        }
    }
}