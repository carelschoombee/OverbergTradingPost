using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

namespace FreeMarket.Models
{
    public class ShoppingCart
    {
        public OrderHeader Order { get; set; }
        public CartBody Body { get; set; }

        public OrderDetail GetOrderDetail(int productNumber, int supplierNumber)
        {
            return Body.OrderDetails
                .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber)
                .FirstOrDefault();
        }

        public void UpdateSelectedProperty(ShoppingCart cart, bool clear)
        {
            if (clear)
            {
                foreach (OrderDetail thisDetail in Body.OrderDetails)
                {
                    thisDetail.Selected = false;
                }
            }
            else
            {
                foreach (OrderDetail thisDetail in Body.OrderDetails)
                {
                    bool selected = cart.Body.OrderDetails
                        .Where(c => c.ProductNumber == thisDetail.ProductNumber && c.SupplierNumber == thisDetail.SupplierNumber)
                        .FirstOrDefault()
                        .Selected;

                    thisDetail.Selected = selected;
                }
            }
        }

        public void Initialize(string userId)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Get the order and body from the database with all joining fields

                Order = OrderHeader.GetOrderForShoppingCart(userId);
                Body = CartBody.GetDetailsForShoppingCart(Order.OrderNumber);
            }
        }

        public FreeMarketObject AddItemFromProduct(int productNumber, int supplierNumber, int courierNumber, int addressNumber, int quantity, int custodian, bool noCharge, string userId = null)
        {
            // Validate
            if (productNumber == 0 || supplierNumber == 0 || courierNumber == 0 || addressNumber == 0 || custodian == 0)
                return new FreeMarketObject { Result = FreeMarketResult.Failure, Argument = null, DebugMessage = "ERROR::Parameters invalid." };

            // Calculate Courier Fee
            decimal? courierFeeCost = 0;
            CalculateCourierFee_Result result;
            CustomerAddress address = new CustomerAddress();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                address = CustomerAddress.GetCustomerAddress(addressNumber);

                if (address != null)
                {
                    result = db.CalculateCourierFee(productNumber, supplierNumber, quantity, courierNumber, addressNumber)
                    .FirstOrDefault();

                    if (result != null)
                    {
                        if (noCharge)
                            courierFeeCost = 0;
                        else
                            courierFeeCost = result.CourierFee;

                        if (courierFeeCost == 0 && !noCharge)
                            return new FreeMarketObject
                            { Result = FreeMarketResult.Failure, Argument = null, DebugMessage = "ERROR::Courier fee could not be calculated." };
                    }
                    else
                    {
                        return new FreeMarketObject
                        { Result = FreeMarketResult.Failure, Argument = null, DebugMessage = "ERROR::Courier fee could not be calculated." };
                    }
                }

            }

            // Check whether the item already exists
            FreeMarketObject res = new FreeMarketObject();

            OrderDetail existingItem = Body.OrderDetails
                .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber)
                .FirstOrDefault();

            if (existingItem != null)
            {
                // Update the existing item
                existingItem.Update(quantity, courierNumber, courierFeeCost, address.ToString(), address.AddressPostalCode, custodian);

                // Setup return object
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    var productInfo = db.GetProduct(productNumber, supplierNumber).FirstOrDefault();

                    if (productInfo == null)
                        return new FreeMarketObject { Result = FreeMarketResult.Failure, Argument = null, DebugMessage = "Product, Supplier or Price does not exist." };

                    res.Result = FreeMarketResult.Success;
                    res.Argument = new Product
                    {
                        Activated = productInfo.Activated,
                        DateAdded = productInfo.DateAdded,
                        DateModified = productInfo.DateModified,
                        DepartmentName = productInfo.DepartmentName,
                        DepartmentNumber = productInfo.DepartmentNumber,
                        Description = productInfo.Description,
                        PricePerUnit = productInfo.PricePerUnit,
                        ProductNumber = productInfo.ProductNumber,
                        Size = productInfo.Size,
                        SupplierName = productInfo.SupplierName,
                        SupplierNumber = productInfo.SupplierNumberID,
                        Weight = productInfo.Weight
                    };
                }
            }
            else
            {
                // A new OrderDetail must be created
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    var productInfo = db.GetProduct(productNumber, supplierNumber).FirstOrDefault();

                    if (productInfo == null)
                        return new FreeMarketObject { Result = FreeMarketResult.Failure, Argument = null, DebugMessage = "Product, Supplier or Price does not exist." };

                    string status = "Unconfirmed";

                    // Add a small image for the CartBody
                    int imageNumber = db.ProductPictures
                            .Where(c => c.ProductNumber == productInfo.ProductNumber && c.Dimensions == PictureSize.Small.ToString())
                            .Select(c => c.PictureNumber)
                            .FirstOrDefault();

                    // Add the new item to the Session variable
                    Body.OrderDetails.Add(
                        new OrderDetail()
                        {
                            AddressNumber = address.AddressNumber,
                            CourierFee = courierFeeCost,
                            CourierNumber = courierNumber,
                            CourierName = null,
                            CustomerCourierOnTimeDeliveryRating = null,
                            CustomerProductQualityRating = null,
                            CustodianNumber = custodian,
                            DeliveryAddress = address.ToString(),
                            DeliveryPostalCode = address.AddressPostalCode,
                            DeliveryDateActual = null,
                            DeliveryDateAgreed = null,
                            OrderItemStatus = status,
                            OrderItemValue = productInfo.PricePerUnit * quantity,
                            PaidCourier = null,
                            PaidSupplier = null,
                            PayCourier = null,
                            PaySupplier = null,
                            Price = productInfo.PricePerUnit,
                            ProductNumber = productInfo.ProductNumber,
                            ProductDescription = productInfo.Description,
                            ProductDepartment = productInfo.DepartmentName,
                            Quantity = quantity,
                            Settled = null,
                            SupplierNumber = productInfo.SupplierNumberID,
                            SupplierName = productInfo.SupplierName,
                            OrderNumber = Order.OrderNumber,
                            MainImageNumber = imageNumber
                        });

                    res.Result = FreeMarketResult.Success;
                    res.Argument = new Product
                    {
                        Activated = productInfo.Activated,
                        DateAdded = productInfo.DateAdded,
                        DateModified = productInfo.DateModified,
                        DepartmentName = productInfo.DepartmentName,
                        DepartmentNumber = productInfo.DepartmentNumber,
                        Description = productInfo.Description,
                        PricePerUnit = productInfo.PricePerUnit,
                        ProductNumber = productInfo.ProductNumber,
                        Size = productInfo.Size,
                        SupplierName = productInfo.SupplierName,
                        SupplierNumber = productInfo.SupplierNumberID,
                        Weight = productInfo.Weight
                    };
                }
            }

            // Keep the OrderTotal in sync
            UpdateTotal();

            return res;
        }

        public FreeMarketObject RemoveItem(int itemNumber, int productNumber, int supplierNumber, string userId = null)
        {
            // If the item is in the database
            FreeMarketResult resultDatabase = RemoveItemFromDatabase(itemNumber, userId);

            // Remove the item from the Session as well
            FreeMarketResult resultSession = RemoveItemFromSession(productNumber, supplierNumber);

            if (resultDatabase == FreeMarketResult.Success && resultSession == FreeMarketResult.Success)
            {
                Product product = new Product();

                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    product = db.Products.Find(itemNumber);
                }

                // Keep the OrderTotal in sync
                UpdateTotal();

                return new FreeMarketObject { Result = FreeMarketResult.Success, Argument = product };
            }
            else
            {
                UpdateTotal();

                return new FreeMarketObject { Result = FreeMarketResult.Failure, Argument = null };
            }
        }

        private FreeMarketResult RemoveItemFromDatabase(int itemNumber, string userId = null)
        {
            FreeMarketResult result = FreeMarketResult.NoResult;

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

                        AuditUser.LogAudit(8, string.Format("Order number: {0}", Order.OrderNumber), userId);

                        result = FreeMarketResult.Success;
                    }
                    else
                    {
                        result = FreeMarketResult.Failure;
                    }
                }
            }
            else
            {
                result = FreeMarketResult.Success;
            }

            return result;
        }

        private FreeMarketResult RemoveItemFromSession(int productNumber, int supplierNumber)
        {
            Debug.Write(string.Format("Removing Product {0} from Session...", productNumber));

            Product product = new Product();
            FreeMarketResult result = FreeMarketResult.NoResult;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                product = db.Products.Find(productNumber);

                if (product != null)
                {
                    Body.OrderDetails.RemoveAll(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber);
                    result = FreeMarketResult.Success;
                }
                else
                {
                    result = FreeMarketResult.Failure;
                }
            }

            return result;
        }

        public FreeMarketObject UpdateQuantities(List<OrderDetail> changedItems)
        {
            FreeMarketObject res = new FreeMarketObject();
            OrderDetail temp = new OrderDetail();

            if (changedItems != null && changedItems.Count > 0)
            {
                foreach (OrderDetail detail in changedItems)
                {
                    temp = Body.OrderDetails
                        .Where(c => c.ProductNumber == detail.ProductNumber && c.SupplierNumber == detail.SupplierNumber)
                        .FirstOrDefault();

                    if (temp != null)
                    {
                        Body.OrderDetails
                        .Where(c => c.ProductNumber == detail.ProductNumber && c.SupplierNumber == detail.SupplierNumber)
                        .FirstOrDefault()
                        .Quantity = detail.Quantity;
                    }
                }
            }

            // Keep the OrderTotal in sync
            UpdateTotal();

            res.Result = FreeMarketResult.Success;
            return res;
        }

        public void UpdatePrices()
        {
            if (Order != null && Order.OrderNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    Debug.Write(string.Format("Updating prices for order {0} ...", Order.OrderNumber));

                    foreach (OrderDetail item in Body.OrderDetails)
                    {
                        item.ProductPrice = db.ProductSuppliers
                            .Find(item.ProductNumber, item.SupplierNumber)
                            .PricePerUnit;
                    }
                }
            }
        }

        public void Save(string userId = null)
        {
            // Compare the Session cart to the database cart and resolve differences
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

            Body.OrderDetails.ForEach(c => c.OrderItemValue = c.Price * c.Quantity);

            Order.SubTotal = Body.OrderDetails.Sum(c => c.OrderItemValue);

            CalculateShippingTotal();

            Order.TotalOrderValue = (Order.SubTotal ?? 0) + (Order.ShippingTotal ?? 0);

            // Don't persist as the user may be anonymous at this point
        }

        public void CalculateShippingTotal()
        {
            if ((ConfigurationManager.AppSettings["freeDeliveryAboveCertainOrderTotal"]) == "true")
            {
                decimal threshold = 0;

                try
                {
                    threshold = decimal.Parse(ConfigurationManager.AppSettings["freeDeliveryThreshold"]);
                }
                catch
                {

                }

                if (threshold != 0 && Order.SubTotal > threshold)
                    Order.ShippingTotal = 0;
                else
                    Order.ShippingTotal = Body.OrderDetails.Sum(c => (c.CourierFee ?? 0));
            }
        }

        public void Checkout() { }

        public void Compare()
        {
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
                                tempDb.OrderItemValue = temp.OrderItemValue;
                                tempDb.SupplierNumber = temp.SupplierNumber;
                                tempDb.CourierNumber = temp.CourierNumber;
                                tempDb.ProductNumber = temp.ProductNumber;
                                tempDb.DeliveryAddress = temp.DeliveryAddress;
                                tempDb.CourierFee = temp.CourierFee;
                                tempDb.CustodianNumber = temp.CustodianNumber;

                                db.Entry(tempDb).State = EntityState.Modified;
                                db.SaveChanges();

                                AuditUser.LogAudit(7, string.Format("Order number: {0}", Order.OrderNumber));
                            }
                        }
                    }
                }
            }

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

                    AuditUser.LogAudit(7, string.Format("Order number: {0}", Order.OrderNumber));
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

            if (Order != null && Body != null)
            {
                toString += "\nShopping Cart Contents:\n";
                toString += string.Format("\n{0}\n{1}\n", Order.ToString(), Body.ToString());
            }

            return toString;
        }
    }
}