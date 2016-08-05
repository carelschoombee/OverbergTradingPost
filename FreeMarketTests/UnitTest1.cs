using FreeMarket.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FreeMarketTests
{
    [TestClass]
    public class UnitTest1
    {
        FreeMarketEntities db = new FreeMarketEntities();

        [TestMethod]
        public void ShoppingCartContainsCorrectItems()
        {
            // Arrange
            int orderNumber = 1;

            List<OrderDetail> itemsForOrder = new List<OrderDetail>();
            itemsForOrder = db.OrderDetails
                .Where(c => c.OrderNumber == orderNumber)
                .ToList();

            // Act
            int numberOfItemsInOrder = itemsForOrder.Count;
            int numberOfItemsFromStoredProc = db.GetDetailsForShoppingCart(orderNumber).ToList().Count;

            // Assert
            Assert.AreEqual(numberOfItemsFromStoredProc, numberOfItemsInOrder);
        }
    }
}
