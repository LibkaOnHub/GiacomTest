using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrderSummariesAsync();

        Task<OrderDetail> GetOrderDetailByIdAsync(Guid orderId);

        Task<IEnumerable<OrderSummary>> GetOrderSummariesByStatusNameAsync(string statusName);

        Task<Data.Entities.Order> GetOrderByIdAsync(Guid orderId);

        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus);

        Task<Guid> CreateOrderAsync(Data.Entities.Order order);

        Task<bool> CheckStatusExistsAsync(Guid statusId);

        Task<bool> CheckStatusNameExistsAsync(string statusName);

        Task<bool> CheckProductExistsAsync(Guid productId);

        Task<bool> CheckServiceExistsAsync(Guid serviceId);

        Task<bool> CheckOrderExistsAsync(Guid orderId);

        Task<IEnumerable<MonthlyProfit>> GetMonthlyProfitsForCompletedOrdersAsync();
    }
}