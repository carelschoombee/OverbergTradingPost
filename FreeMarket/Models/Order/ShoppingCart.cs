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

                if (db.Specials.Any(c => c.SpecialPostalCode == Order.DeliveryAddressPostalCode))
                {
                    ApplyAllSpecialPrices();
                    UpdateTotal();
                }

            }
        }

        public FreeMarketObject AddItemFromProduct(int productNumber, int supplierNumber, int quantity, int custodianNumber)
        {
            // Validate
            if (productNumber == 0 || supplierNumber == 0)
                return new FreeMarketObject { Result = FreeMarketResult.Failure, Argument = null, Message = "No products could be found." };

            // Assign a courier. The cost will be calculated later.
            int courierNumber = 0;
            decimal? courierFeeCost = 0;
            bool undeliverableItem = false;
            CalculateDeliveryFee_Result result;
            FreeMarketObject res = new FreeMarketObject();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                result = db.CalculateDeliveryFee(productNumber, supplierNumber, quantity, Order.OrderNumber)
                    .FirstOrDefault();

                if (result != null)
                    courierNumber = (int)result.CourierNumber;
                else
                {
                    // If no courier could be found this item is undeliverable and must be marked as such.
                    // Assign a default courier to keep database constraints happy.
                    undeliverableItem = true;
                    courierNumber = db.Couriers.FirstOrDefault().CourierNumber;
                }

                // Check whether the item already exists
                OrderDetail existingItem = Body.OrderDetails
                    .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber)
                    .FirstOrDefault();

                if (existingItem != null)
                {
                    // Update the existing item
                    existingItem.Update(quantity);

                    // Setup return object
                    var productInfo = db.GetProduct(productNumber, supplierNumber).FirstOrDefault();

                    if (productInfo == null)
                        return new FreeMarketObject { Result = FreeMarketResult.Failure, Argument = null, Message = "Product, Supplier or Price does not exist." };

                    res.Result = FreeMarketResult.Success;
                    res.Message = string.Format("{0} ({1}) has been added to your cart.", productInfo.Description, quantity);
                }
                else
                {
                    // A new OrderDetail must be created
                    var productInfo = db.GetProduct(productNumber, supplierNumber)
                        .FirstOrDefault();

                    if (productInfo == null)
                        return new FreeMarketObject { Result = FreeMarketResult.Failure, Argument = null, Message = "No products could be found." };

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
                            CourierFee = courierFeeCost, // Will always be zero at this point.
                            CourierNumber = courierNumber, // May be a default value at this point.
                            CourierName = null,
                            CustodianNumber = custodianNumber,
                            OrderItemStatus = status,
                            OrderItemValue = productInfo.PricePerUnit * quantity,
                            OrderNumber = Order.OrderNumber,
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
                            MainImageNumber = imageNumber,
                            CannotDeliver = undeliverableItem // Must this item be marked as undeliverable?
                        });

                    ApplySpecialPrices(productNumber, supplierNumber);

                    res.Result = FreeMarketResult.Success;
                    res.Message = string.Format("{0} ({1}) has been added to your cart.", productInfo.Description, quantity);
                }

            }

            // Keep the OrderTotal in sync
            UpdateTotal();

            return res;
        }

        public void ApplySpecialPrices(int productNumber, int supplierNumber)
        {
            if (Order.OrderNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    if (db.Specials.Any(c => c.SpecialPostalCode == Order.DeliveryAddressPostalCode))
                    {
                        ProductSupplier productSupplier = db.ProductSuppliers
                            .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber)
                            .FirstOrDefault();

                        // If the user is ordering from a special region apply a special price.
                        if (productSupplier != null)
                        {
                            if (productSupplier.SpecialPricePerUnit != null)
                            {
                                int quantity = Body.OrderDetails
                                .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber)
                                .FirstOrDefault()
                                .Quantity;

                                Body.OrderDetails
                                .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber)
                                .FirstOrDefault()
                                .Price = (decimal)productSupplier.SpecialPricePerUnit;

                                Body.OrderDetails
                                .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber)
                                .FirstOrDefault()
                                .OrderItemValue = (decimal)productSupplier.SpecialPricePerUnit * quantity;
                            }
                        }
                    }
                }
            }
        }

        public void ApplyAllSpecialPrices()
        {
            foreach (OrderDetail detail in Body.OrderDetails)
            {
                ApplySpecialPrices(detail.ProductNumber, detail.SupplierNumber);
            }
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
                        FreeStock(item.ProductNumber, item.SupplierNumber, (int)item.CustodianNumber, item.Quantity);

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

        public void SetQuantityOnHand(List<OrderDetail> items)
        {
            foreach (OrderDetail item in items)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    item.QuantityOnHand = db.ProductCustodians
                        .Where(c => c.ProductNumber == item.ProductNumber && c.SupplierNumber == item.SupplierNumber && c.CustodianNumber == item.CustodianNumber)
                        .Select(c => c.QuantityOnHand)
                        .FirstOrDefault();
                }
            }
        }

        public FreeMarketObject UpdateQuantities(List<OrderDetail> changedItems)
        {
            FreeMarketObject res = new FreeMarketObject();
            OrderDetail temp = new OrderDetail();

            if (changedItems != null && changedItems.Count > 0)
            {
                SetQuantityOnHand(changedItems);

                foreach (OrderDetail detail in changedItems)
                {
                    temp = Body.OrderDetails
                        .Where(c => c.ProductNumber == detail.ProductNumber && c.SupplierNumber == detail.SupplierNumber)
                        .FirstOrDefault();

                    if (temp != null)
                    {
                        int oldQuantity = temp.Quantity;

                        if (oldQuantity > detail.Quantity)
                        {
                            int difference = oldQuantity - detail.Quantity;

                            FreeStock(detail.ProductNumber, detail.SupplierNumber, (int)detail.CustodianNumber, difference);
                        }
                        else
                        {
                            int difference = detail.Quantity - oldQuantity;

                            if (detail.QuantityOnHand > difference)
                                ReserveStock(detail.ProductNumber, detail.SupplierNumber, (int)detail.CustodianNumber, difference);
                            else
                            {
                                res.Result = FreeMarketResult.Failure;
                                res.Message += string.Format("\n {0} is out of stock. Please try a smaller quantity.", detail.ProductDescription);
                                continue;
                            }
                        }

                        Body.OrderDetails
                        .Where(c => c.ProductNumber == detail.ProductNumber && c.SupplierNumber == detail.SupplierNumber)
                        .FirstOrDefault()
                        .Quantity = detail.Quantity;
                    }
                }
            }

            // Keep the OrderTotal in sync
            UpdateTotal();

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

                // Keep the OrderTotal in sync
                UpdateTotal();
            }
        }

        public void Save(string userId = null)
        {
            // Compare the Session cart to the database cart and resolve differences
            Compare();

            // Re-initialize the Body
            Body = CartBody.GetDetailsForShoppingCart(Order.OrderNumber);

            ApplyAllSpecialPrices();

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

        public void Compare()
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                // Get a list of items which are on both the Session and database
                List<OrderDetail> existingItems = Body.OrderDetails.FindAll(c => c.ItemNumber != 0);

                if (existingItems != null && existingItems.Count > 0)
                {
                    foreach (OrderDetail temp in existingItems)
                    {
                        OrderDetail tempDb = db.OrderDetails.Find(temp.ItemNumber);

                        if (tempDb != null)
                        {
                            if (!temp.Equals(tempDb))
                            {
                                // If the item has changed update it
                                tempDb.Quantity = temp.Quantity;
                                tempDb.OrderItemValue = temp.OrderItemValue;
                                tempDb.CannotDeliver = temp.CannotDeliver;

                                db.Entry(tempDb).State = EntityState.Modified;
                                db.SaveChanges();

                                AuditUser.LogAudit(7, string.Format("Order number: {0}", Order.OrderNumber));
                            }
                        }
                    }

                }

                // Get a list of items that are on the Session variable but not in the database
                List<OrderDetail> newItems = Body.OrderDetails.FindAll(c => c.ItemNumber == 0);

                if (newItems != null && newItems.Count > 0)
                {
                    foreach (OrderDetail tempB in newItems)
                    {
                        tempB.OrderNumber = Order.OrderNumber;
                        CalculateDeliveryFee_Result result = CalculateDeliveryFee(tempB.ProductNumber, tempB.SupplierNumber, tempB.Quantity, tempB.OrderNumber);
                        if (result == null)
                        {
                            tempB.CustodianNumber = null;
                        }
                        else
                        {
                            tempB.CustodianNumber = result.CustodianNumber;
                            ReserveStock(tempB.ProductNumber, tempB.SupplierNumber, (int)tempB.CustodianNumber, tempB.Quantity);
                        }
                        db.OrderDetails.Add(tempB);
                    }

                    db.SaveChanges();

                    AuditUser.LogAudit(7, string.Format("Order number: {0}", Order.OrderNumber));
                }
            }
        }

        public void Merge(string userId, CartBody tempBody)
        {
            if (Order.OrderNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    // Compare the session body with the database
                    foreach (OrderDetail item in tempBody.OrderDetails)
                    {
                        OrderDetail existingItem = Body.OrderDetails
                            .Where(c => c.ProductNumber == item.ProductNumber && c.SupplierNumber == item.SupplierNumber)
                            .FirstOrDefault();

                        // If the item does not exist, add it
                        if (existingItem == null)
                            Body.OrderDetails.Add(item);
                        // Otherwise update the quantity only
                        else
                            Body.OrderDetails.Where(c => c.ProductNumber == item.ProductNumber && c.SupplierNumber == item.SupplierNumber)
                                .FirstOrDefault()
                                .Quantity += item.Quantity;
                    }
                }
            }

            UpdateAllDeliverableStatus();
            UpdateTotal();
            Save();
        }

        public bool UpdateCourier(OrderDetail tempB)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                bool noCharge = false;

                // Calculate the delivery fee
                CalculateDeliveryFee_Result result = db.CalculateDeliveryFee(tempB.ProductNumber, tempB.SupplierNumber, tempB.Quantity, Order.OrderNumber)
                    .FirstOrDefault();

                List<OrderDetail> otherItems = Body.OrderDetails.ToList();

                if (result != null)
                {
                    // Set the order detail
                    tempB.CourierNumber = result.CourierNumber;
                    tempB.CustodianNumber = result.CustodianNumber;

                    if (otherItems != null && otherItems.Count > 0)
                    {
                        // Determine noCharge
                        foreach (OrderDetail temp in otherItems)
                        {
                            if (temp.ProductNumber == tempB.ProductNumber && temp.SupplierNumber == tempB.SupplierNumber)
                                continue;

                            if (tempB.CustodianNumber == temp.CustodianNumber &&
                                tempB.CourierNumber == temp.CourierNumber &&
                                temp.CourierFee > 0)
                            {
                                noCharge = true;
                                break;
                            }
                        }
                    }

                    tempB.CannotDeliver = false;

                    if (noCharge)
                        tempB.CourierFee = 0;
                    else
                        tempB.CourierFee = result.CourierFee;
                }
                else
                {
                    // The courier could not deliver to that postal code.
                    tempB.CannotDeliver = true;

                    return true;
                }
            }

            return false;
        }

        public void UpdateAllCouriers()
        {
            List<OrderDetail> items = Body.OrderDetails
                .Where(c => c.CannotDeliver == false)
                .ToList();

            foreach (OrderDetail item in items)
            {
                UpdateCourier(item);
            }

            UpdateTotal();
        }

        public void UpdateDeliveryDetails(SaveCartViewModel model)
        {
            Order.UpdateDeliveryDetails(model);
            ApplyAllSpecialPrices();
            UpdateAllDeliverableStatus();
            UpdateAllCouriers();
            Save();
        }

        public static bool CalculateDeliverableStatus(int productNumber, int supplierNumber, int quantity, int orderNumber)
        {
            CalculateDeliveryFee_Result result;

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                result = db.CalculateDeliveryFee(productNumber, supplierNumber, quantity, orderNumber)
                    .FirstOrDefault();

                if (result == null)
                    // If no courier could be found this item is undeliverable and must be marked as such.
                    return true;
                else
                    return false;
            }
        }

        public static CalculateDeliveryFee_Result CalculateDeliveryFee(int productNumber, int supplierNumber, int quantity, int orderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.CalculateDeliveryFee(productNumber, supplierNumber, quantity, orderNumber)
                    .FirstOrDefault();
            }
        }

        public void UpdateAllDeliverableStatus()
        {
            foreach (OrderDetail detail in Body.OrderDetails)
            {
                detail.CannotDeliver = ShoppingCart.CalculateDeliverableStatus(detail.ProductNumber, detail.SupplierNumber, detail.Quantity, Order.OrderNumber);
            }
        }

        public static ProductCustodian GetStockAvailable(int productNumber, int supplierNumber, int quantity)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                return db.ProductCustodians
                    .Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber && c.QuantityOnHand >= quantity)
                    .FirstOrDefault();
            }
        }

        public decimal CalculateApproximateDeliveryFee()
        {
            // If the user is logged in
            if (Order.OrderNumber != 0)
            {
                using (FreeMarketEntities db = new FreeMarketEntities())
                {
                    List<OrderDetail> deliverableItems = Body.OrderDetails
                        .Where(c => c.CannotDeliver == false)
                        .ToList();

                    List<OrderDetail> clonedList = new List<OrderDetail>(deliverableItems.Count);

                    deliverableItems.ForEach((item) =>
                    {
                        clonedList.Add(new OrderDetail(item));
                    });

                    OrderHeader order = Order;

                    ShoppingCart virtualCart = new ShoppingCart
                    {
                        Body = new CartBody { OrderDetails = clonedList },
                        Order = order
                    };

                    foreach (OrderDetail detail in virtualCart.Body.OrderDetails)
                    {
                        bool noCharge = false;

                        // Calculate the delivery fee
                        CalculateDeliveryFee_Result result = db.CalculateDeliveryFee(detail.ProductNumber, detail.SupplierNumber, detail.Quantity, Order.OrderNumber)
                            .FirstOrDefault();

                        if (result != null)
                        {
                            // Set the order detail
                            detail.CourierNumber = result.CourierNumber;
                            detail.CustodianNumber = result.CustodianNumber;

                            // Determine noCharge
                            foreach (OrderDetail temp in virtualCart.Body.OrderDetails)
                            {
                                // Do not compare against itself
                                if (temp.ProductNumber == detail.ProductNumber && temp.SupplierNumber == detail.SupplierNumber)
                                    continue;

                                if (detail.CustodianNumber == temp.CustodianNumber &&
                                    detail.CourierNumber == temp.CourierNumber &&
                                    temp.CourierFee > 0)
                                {
                                    noCharge = true;
                                    break;
                                }
                            }

                            if (noCharge)
                                detail.CourierFee = 0;
                            else
                                detail.CourierFee = result.CourierFee;
                        }
                    }

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
                            return 0;
                        else
                            return virtualCart.Body.OrderDetails.Sum(c => (c.CourierFee ?? 0));
                    }
                    else
                        return virtualCart.Body.OrderDetails.Sum(c => (c.CourierFee ?? 0));
                }
            }
            else
                return Order.ShippingTotal ?? 0;
        }

        public void UpdateTotal()
        {
            Body.OrderDetails.ForEach(c => c.OrderItemValue = c.Price * c.Quantity);
            Order.SubTotal = Body.OrderDetails.Sum(c => c.OrderItemValue);
            Order.ShippingTotal = CalculateApproximateDeliveryFee();
            Order.TotalOrderValue = (Order.SubTotal ?? 0) + (Order.ShippingTotal ?? 0);
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
            }
            else
            {
                Order.ShippingTotal = Body.OrderDetails.Sum(c => (c.CourierFee ?? 0));
            }
        }

        public FreeMarketResult ReserveStock(int productNumber, int supplierNumber, int custodianNumber, int quantityRequested)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductCustodian custodian = db.ProductCustodians.Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber && c.CustodianNumber == custodianNumber)
                    .FirstOrDefault();

                if (custodian == null)
                    return FreeMarketResult.Failure;

                if (custodian.QuantityOnHand >= quantityRequested)
                {
                    if (custodian.StockReservedForOrders == null)
                        custodian.StockReservedForOrders = 0;

                    custodian.QuantityOnHand -= quantityRequested;
                    custodian.StockReservedForOrders += quantityRequested;

                    db.Entry(custodian).State = EntityState.Modified;
                    db.SaveChanges();

                    return FreeMarketResult.Success;
                }
                else
                {
                    return FreeMarketResult.Failure;
                }
            }
        }

        public FreeMarketResult FreeStock(int productNumber, int supplierNumber, int custodianNumber, int quantityRequested)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                ProductCustodian custodian = db.ProductCustodians.Where(c => c.ProductNumber == productNumber && c.SupplierNumber == supplierNumber && c.CustodianNumber == custodianNumber)
                    .FirstOrDefault();

                if (custodian == null)
                    return FreeMarketResult.Failure;

                if (custodian.StockReservedForOrders == null)
                    custodian.StockReservedForOrders = 0;

                custodian.QuantityOnHand += quantityRequested;
                custodian.StockReservedForOrders -= quantityRequested;

                db.Entry(custodian).State = EntityState.Modified;
                db.SaveChanges();

                return FreeMarketResult.Success;
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

        public static void SetOrderConfirmed(int orderNumber)
        {
            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                OrderHeader order = db.OrderHeaders.Find(orderNumber);
                order.OrderStatus = "Confirmed";
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
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