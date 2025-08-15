using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrderSummariesAsync();

        Task<OrderDetail> GetOrderDetailByIdAsync(Guid orderId);

        Task<IEnumerable<OrderSummary>> GetOrderSummariesByStatusNameAsync(string status);

        Task<OrderStatus> GetOrderStatusByNameAsync(string statusName);

        Task<Entities.Order> GetOrderByIdAsync(Guid orderId);

        Task UpdateOrderAsync(Entities.Order order);

        Task AddOrderAsync(Entities.Order order);

        Task<bool> CheckStatusExistsAsync(Guid statusId);

        Task<bool> CheckStatusNameExistsAsync(string statusName);

        Task<bool> CheckProductExistsAsync(Guid productId);

        Task<bool> CheckServiceExistsAsync(Guid serviceId);

        Task<bool> CheckOrderExistsAsync(Guid orderId);

        Task<IEnumerable<MonthlyProfit>> GetMonthlyProfitsForCompletedOrdersAsync();
    }
}