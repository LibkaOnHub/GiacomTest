using AutoMapper;
using Order.Data;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrderSummariesAsync()
        {
            var orders = await _orderRepository.GetOrderSummariesAsync();
            return orders;
        }

        public async Task<OrderDetail> GetOrderDetailByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderDetailByIdAsync(orderId);
            return order;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrderSummariesByStatusNameAsync(string statusName)
        {
            var ordersByStatusName = await _orderRepository.GetOrderSummariesByStatusNameAsync(statusName);
            return ordersByStatusName;
        }

        public async Task<Data.Entities.Order> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                return false;

            var status = await _orderRepository.GetOrderStatusByNameAsync(newStatus);
            if (status == null)
                return false;

            order.StatusId = status.Id;
            await _orderRepository.UpdateOrderAsync(order);

            return true;
        }

        public async Task<Guid> CreateOrderAsync(Data.Entities.Order order)
        {
            order.CreatedDate = DateTime.UtcNow;

            await _orderRepository.AddOrderAsync(order);

            return new Guid(order.Id);
        }

        public async Task<bool> CheckStatusExistsAsync(Guid statusId)
        {
            return await _orderRepository.CheckStatusExistsAsync(statusId);
        }

        public async Task<bool> CheckStatusNameExistsAsync(string statusName)
        {
            return await _orderRepository.CheckStatusNameExistsAsync(statusName);
        }

        public async Task<bool> CheckProductExistsAsync(Guid productId)
        {
            return await _orderRepository.CheckProductExistsAsync(productId);
        }

        public async Task<bool> CheckServiceExistsAsync(Guid serviceId)
        {
            return await _orderRepository.CheckServiceExistsAsync(serviceId);
        }

        public async Task<bool> CheckOrderExistsAsync(Guid orderId)
        {
            return await _orderRepository.CheckOrderExistsAsync(orderId);
        }

        public async Task<IEnumerable<MonthlyProfit>> GetMonthlyProfitsForCompletedOrdersAsync()
        {
            var monthlyProfits = await _orderRepository.GetMonthlyProfitsForCompletedOrdersAsync();
            return monthlyProfits;
        }
    }
}