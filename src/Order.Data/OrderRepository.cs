using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrderSummariesAsync()
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDetail> GetOrderDetailByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            var order = await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes)
                .Select(x => new OrderDetail
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = new Guid(i.Id),
                        OrderId = new Guid(i.OrderId),
                        ServiceId = new Guid(i.ServiceId),
                        ServiceName = i.Service.Name,
                        ProductId = new Guid(i.ProductId),
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();

            return order;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrderSummariesByStatusNameAsync(string statusName)
        {
            var orders = await _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .Where(x => x.Status.Name == statusName)
                .Select(x => new OrderSummary
                {
                    Id = new Guid(x.Id),
                    ResellerId = new Guid(x.ResellerId),
                    CustomerId = new Guid(x.CustomerId),
                    StatusId = new Guid(x.StatusId),
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderStatus> GetOrderStatusByNameAsync(string statusName)
        {
            return await _orderContext.OrderStatus
                .FirstOrDefaultAsync(x => x.Name == statusName);
        }

        public async Task<Entities.Order> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            return await _orderContext.Order
                .Include(x => x.Status)
                .FirstOrDefaultAsync(o =>
                    _orderContext.Database.IsInMemory()
                        ? o.Id.SequenceEqual(orderIdBytes)
                        : o.Id == orderIdBytes);
        }

        public async Task UpdateOrderAsync(Entities.Order order)
        {
            _orderContext.Order.Update(order);
            await _orderContext.SaveChangesAsync();
        }

        public async Task AddOrderAsync(Entities.Order order)
        {
            await _orderContext.Order.AddAsync(order);
            await _orderContext.SaveChangesAsync();
        }

        public async Task<bool> CheckStatusExistsAsync(Guid statusId)
        {
            var idBytes = statusId.ToByteArray();
            return await _orderContext.OrderStatus.AnyAsync(s => s.Id == idBytes);
        }

        public async Task<bool> CheckStatusNameExistsAsync(string statusName)
        {
            return await _orderContext.OrderStatus.AnyAsync(s => s.Name == statusName);
        }

        public async Task<bool> CheckProductExistsAsync(Guid productId)
        {
            var idBytes = productId.ToByteArray();
            return await _orderContext.OrderProduct.AnyAsync(p => p.Id == idBytes);
        }

        public async Task<bool> CheckServiceExistsAsync(Guid serviceId)
        {
            var idBytes = serviceId.ToByteArray();
            return await _orderContext.OrderService.AnyAsync(s => s.Id == idBytes);
        }

        public async Task<bool> CheckOrderExistsAsync(Guid orderId)
        {
            var idBytes = orderId.ToByteArray();
            return await _orderContext.Order.AnyAsync(s => s.Id == idBytes);
        }

        public async Task<IEnumerable<MonthlyProfit>> GetMonthlyProfitsForCompletedOrdersAsync()
        {
            var orderItems = await _orderContext.Order
                .Where(o => o.Status.Name == "Completed")
                .SelectMany(o => o.Items, (o, i) => new
                {
                    o.CreatedDate,
                    Quantity = i.Quantity ?? 0,
                    i.Product.UnitPrice,
                    i.Product.UnitCost,
                    Profit = (i.Quantity ?? 0) * (i.Product.UnitPrice - i.Product.UnitCost)
                })
                .ToListAsync();

            var monthlyProfits = orderItems
                .GroupBy(x => new { x.CreatedDate.Year, x.CreatedDate.Month })
                .Select(g => new MonthlyProfit
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Profit = g.Sum(x => x.Profit)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month);

            return monthlyProfits;
        }
    }
}