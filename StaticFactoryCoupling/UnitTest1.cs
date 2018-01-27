using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace StaticFactoryCoupling
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestShippingByStore_Seven_1_Order_Family_2_Orders()
        {
            //arrange
            var target = new ShipService();

            var orders = new List<Order>
            {
                new Order{ StoreType= StoreType.Seven, Id=1},
                new Order{ StoreType= StoreType.Family, Id=2},
                new Order{ StoreType= StoreType.Family, Id=3},
            };

            var seven = Substitute.For<IStoreService>();
            var family = Substitute.For<IStoreService>();

            SimpleFactory.SetStoreService(seven, family);

            //act
            target.ShippingByStore(orders);

            seven.Received().Ship(Arg.Any<Order>());
            family.Received(2).Ship(Arg.Any<Order>());

            //todo, assert
            //ShipService should invoke SevenService once and FamilyService twice
        }
    }

    public enum StoreType
    {
        /// <summary>
        /// 7-11
        /// </summary>
        Seven = 0,

        /// <summary>
        /// 全家
        /// </summary>
        Family = 1
    }

    public class ShipService
    {
        public void ShippingByStore(List<Order> orders)
        {
            foreach (var order in orders)
            {
                // simple factory pattern implementation
                IStoreService storeService = SimpleFactory.GetStoreService(order);
                storeService.Ship(order);
            }
        }
    }

    internal class SimpleFactory
    {
        private static IStoreService sevenService = new SevenService();
        private static IStoreService familyService = new FamilyService();

        internal static void SetStoreService(IStoreService seven, IStoreService family)
        {
            sevenService = seven;
            familyService = family;
        }

        internal static IStoreService GetStoreService(Order order)
        {
            if (order.StoreType == StoreType.Family)
            {
                return familyService;
            }
            else
            {
                return sevenService;
            }
        }
    }

    public class Order
    {
        public StoreType StoreType { get; set; }
        public int Id { get; set; }
        public int Amount { get; set; }
    }

    internal class SevenService : IStoreService
    {
        public void Ship(Order order)
        {
            // seven web service
            var client = new HttpClient();
            client.PostAsync("http://api.seven.com/Order", order, new JsonMediaTypeFormatter());
        }
    }

    internal class FamilyService : IStoreService
    {
        public void Ship(Order order)
        {
            // family web service
            var client = new HttpClient();
            client.PostAsync("http://api.family.com/Order", order, new JsonMediaTypeFormatter());
        }
    }

    public interface IStoreService
    {
        void Ship(Order order);
    }
}