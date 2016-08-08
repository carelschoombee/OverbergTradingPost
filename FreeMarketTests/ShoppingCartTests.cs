using FreeMarket.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace FreeMarketTests
{
    [TestClass]
    public class ShoppingCartTests
    {
        public FreeMarketEntities db = new FreeMarketEntities();

        public const string userIdConst = "debba121-e845-4f50-b5fd-39a86c060e13";

        [TestMethod]
        public void ShoppingCartContainsCorrectNumberOfItems()
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
