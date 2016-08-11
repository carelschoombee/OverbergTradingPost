using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FreeMarket.Models
{
    public class CartBody
    {
        public List<OrderDetail> OrderDetails { get; set; }

        public static CartBody GetDetailsForShoppingCart(int orderNumber)
        {
            CartBody body = new CartBody();

            using (FreeMarketEntities db = new FreeMarketEntities())
            {
                body.OrderDetails = db.GetDetailsForShoppingCart(orderNumber)
                    .Select(c => new OrderDetail
                    {
                        CourierName = c.CourierName,
                        CourierFee = c.CourierFee,
                        CourierNumber = c.CourierNumber,
                        CustomerCourierOnTimeDeliveryRating = c.CustomerCourierOnTimeDeliveryRating,
                        CustomerProductQualityRating = c.CustomerProductQualityRating,
                        DeliveryDateActual = c.DeliveryDateActual,
                        DeliveryDateAgreed = c.DeliveryDateAgreed,
                        ItemNumber = c.ItemNumber,
                        MainImageNumber = 0,
                        OrderItemStatus = c.OrderItemStatus,
                        OrderItemValue = c.OrderItemValue,
                        OrderNumber = c.OrderNumber,
                        PaidCourier = c.PaidCourier,
                        PaidSupplier = c.PaidSupplier,
                        PayCourier = c.PayCourier,
                        PaySupplier = c.PaySupplier,
                        Price = c.PriceOrderDetail,
                        ProductDepartment = c.DepartmentName,
                        ProductDescription = c.Description,
                        ProductNumber = c.OrderDetailProductNumber,
                        ProductPrice = c.PricePerUnit,
                        ProductWeight = c.Weight,
                        Quantity = c.Quantity,
                        QuantityOnHand = c.QuantityOnHand,
                        Settled = false,
                        SupplierName = c.SupplierName,
                        SupplierNumber = c.SupplierNumber,
                        Selected = false
                    }
                    ).ToList();

                if (body.OrderDetails != null && body.OrderDetails.Count > 0)
                {
                    foreach (OrderDetail detail in body.OrderDetails)
                    {
                        int imageNumber = db.ProductPictures
                            .Where(c => c.ProductNumber == detail.ProductNumber && c.Dimensions == "80x79")
                            .Select(c => c.PictureNumber)
                            .FirstOrDefault();

                        detail.MainImageNumber = imageNumber;
                    }
                }
            }

            Debug.Write(body);

            return body;
        }

        public CartBody()
        {
            OrderDetails = new List<OrderDetail>();
        }

        public override string ToString()
        {
            string toString = "";

            toString += "Start Cart Body:";

            if (OrderDetails != null && OrderDetails.Count > 0)
            {
                foreach (OrderDetail detail in OrderDetails)
                {
                    toString += string.Format("\n{0}\n", detail.ToString());
                }
            }

            toString += "End Cart Body:";
            toString += string.Format("Total Items in Cart: {0}", OrderDetails.Count);

            return toString;
        }
    }
}