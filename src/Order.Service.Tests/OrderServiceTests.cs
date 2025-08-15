using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using Order.Data;
using Order.Data.Entities;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service.Tests
{
    public class OrderServiceTests
    {
        private IOrderService _orderService;
        private IOrderRepository _orderRepository;
        private IMapper _mapper;
        private OrderContext _orderContext;
        private DbConnection _connection;

        private readonly byte[] _orderStatusCreatedId = Guid.NewGuid().ToByteArray();
        const string _orderStatusCreatedName = "Created";

        private readonly byte[] _orderStatusInProgressId = Guid.NewGuid().ToByteArray();
        const string _orderStatusInProgressName = "In Progress";

        private readonly byte[] _orderStatusCompletedId = Guid.NewGuid().ToByteArray();
        const string _orderStatusCompletedName = "Completed";

        private readonly byte[] _orderStatusFailedId = Guid.NewGuid().ToByteArray();
        const string _orderStatusFailedName = "Failed";

        private readonly byte[] _orderServiceEmailId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderProductEmailId = Guid.NewGuid().ToByteArray();


        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<OrderContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .Options;

            _connection = RelationalOptionsExtension.Extract(options).Connection;

            _orderContext = new OrderContext(options);
            _orderContext.Database.EnsureDeleted();
            _orderContext.Database.EnsureCreated();

            _orderRepository = new OrderRepository(_orderContext);
            _orderService = new OrderService(_orderRepository, _mapper);

            await AddReferenceDataAsync(_orderContext);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
            _orderContext.Dispose();
        }


        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            return connection;
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsCorrectNumberOfOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrderSummariesAsync();

            // Assert
            Assert.AreEqual(3, orders.Count());
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsOrdersWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrderSummariesAsync();

            // Assert
            var order1 = orders.SingleOrDefault(x => x.Id == orderId1);
            var order2 = orders.SingleOrDefault(x => x.Id == orderId2);
            var order3 = orders.SingleOrDefault(x => x.Id == orderId3);

            Assert.AreEqual(0.8m, order1.TotalCost);
            Assert.AreEqual(0.9m, order1.TotalPrice);

            Assert.AreEqual(1.6m, order2.TotalCost);
            Assert.AreEqual(1.8m, order2.TotalPrice);

            Assert.AreEqual(2.4m, order3.TotalCost);
            Assert.AreEqual(2.7m, order3.TotalPrice);
        }

        [Test]
        public async Task GetOrdersByStatusAsync_ReturnsOrdersWithSelectedStatus()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 75, _orderStatusCreatedId);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 5454, _orderStatusFailedId);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 233, _orderStatusCreatedId);

            var orderId4 = Guid.NewGuid();
            await AddOrder(orderId4, 862, _orderStatusFailedId);

            var orderId5 = Guid.NewGuid();
            await AddOrder(orderId5, 2233, _orderStatusInProgressId);

            // Act
            var orders = await _orderService.GetOrderSummariesByStatusNameAsync(_orderStatusCreatedName);

            // Assert
            Assert.AreEqual(2, orders.Count());
            Assert.IsTrue(orders.All(x => x.StatusName == _orderStatusCreatedName));

            Assert.IsTrue(orders.Any(x => x.Id == orderId1));
            Assert.IsTrue(orders.Any(x => x.Id == orderId3));
        }

        [Test]
        public async Task UpdateOrderStatusAsync_ChangesStatus()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 75, _orderStatusCreatedId);

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(orderId1, _orderStatusInProgressName);

            var updatedOrder = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(_orderStatusInProgressId, updatedOrder.StatusId);
        }

        [Test]
        public async Task CreateOrderAsync_CreatesRecord()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            var orderToCreate = new Data.Entities.Order
            {
                Id = orderId.ToByteArray(),
                ResellerId = Guid.NewGuid().ToByteArray(),
                CustomerId = Guid.NewGuid().ToByteArray(),
                CreatedDate = DateTime.Now,
                StatusId = _orderStatusCreatedId,
                Items =
                [
                    new OrderItem
                    {
                        Id = Guid.NewGuid().ToByteArray(),
                        ServiceId = _orderServiceEmailId,
                        ProductId = _orderProductEmailId,
                        Quantity = 456
                    }
                ]
            };

            // Act
            var result = await _orderService.CreateOrderAsync(orderToCreate);

            var orderFromDb = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.AreEqual(orderId, result);

            Assert.IsNotNull(orderFromDb);
            Assert.AreEqual(orderToCreate.Id, orderFromDb.Id);
            Assert.AreEqual(orderToCreate.ResellerId, orderFromDb.ResellerId);
            Assert.AreEqual(orderToCreate.CustomerId, orderFromDb.CustomerId);
            Assert.AreEqual(orderToCreate.StatusId, orderFromDb.StatusId);
            Assert.AreEqual(orderToCreate.CreatedDate, orderFromDb.CreatedDate);

            Assert.AreEqual(1, orderFromDb.Items.Count);
            var item = orderToCreate.Items.First();
            Assert.AreEqual(item.ServiceId, orderFromDb.Items.First().ServiceId);
            Assert.AreEqual(item.ProductId, orderFromDb.Items.First().ProductId);
            Assert.AreEqual(item.Quantity, orderFromDb.Items.First().Quantity);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrder()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderDetailByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(orderId1, order.Id);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrderItemCount()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderDetailByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1, order.Items.Count());
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsOrderWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);

            // Act
            var order = await _orderService.GetOrderDetailByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1.6m, order.TotalCost);
            Assert.AreEqual(1.8m, order.TotalPrice);
        }

        private async Task AddOrder(Guid orderId, int quantity)
        {
            await AddOrder(orderId, quantity, _orderStatusCreatedId);
        }

        private async Task AddOrder(Guid orderId, int quantity, byte[] status)
        {
            var orderIdBytes = orderId.ToByteArray();
            _orderContext.Order.Add(new Data.Entities.Order
            {
                Id = orderIdBytes,
                ResellerId = Guid.NewGuid().ToByteArray(),
                CustomerId = Guid.NewGuid().ToByteArray(),
                CreatedDate = DateTime.Now,
                StatusId = status,
            });

            _orderContext.OrderItem.Add(new OrderItem
            {
                Id = Guid.NewGuid().ToByteArray(),
                OrderId = orderIdBytes,
                ServiceId = _orderServiceEmailId,
                ProductId = _orderProductEmailId,
                Quantity = quantity
            });

            await _orderContext.SaveChangesAsync();
        }

        private async Task AddReferenceDataAsync(OrderContext orderContext)
        {
            // statuses

            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCreatedId,
                Name = _orderStatusCreatedName,
            });

            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusInProgressId,
                Name = _orderStatusInProgressName,
            });

            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCompletedId,
                Name = _orderStatusCompletedName,
            });

            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusFailedId,
                Name = _orderStatusFailedName,
            });

            // services

            orderContext.OrderService.Add(new Data.Entities.OrderService
            {
                Id = _orderServiceEmailId,
                Name = "Email"
            });

            // products

            orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = _orderProductEmailId,
                Name = "100GB Mailbox",
                UnitCost = 0.8m,
                UnitPrice = 0.9m,
                ServiceId = _orderServiceEmailId
            });

            await orderContext.SaveChangesAsync();
        }
    }
}
