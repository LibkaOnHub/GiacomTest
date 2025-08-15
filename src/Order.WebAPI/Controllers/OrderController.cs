using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Model;
using Order.Service;
using Order.WebAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrderController(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var orders = await _orderService.GetOrderSummariesAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderDetailByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }

        // Task 1: Return Orders with a specified Order Status e.g. 'Failed'
        [HttpGet("status/{statusName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOrdersByStatus([FromRoute] GetOrdersByStatusNameRequest request)
        {
            // FluentValidationFilter will run automatically here

            var orders = await _orderService.GetOrderSummariesByStatusNameAsync(request.StatusName);

            if (orders == null || !orders.Any())
                return NotFound($"No orders found with status '{request.StatusName}'.");

            return Ok(orders);
        }

        // Task 2: Allow an Order Status to be updated to a different status e.g. 'InProgress'
        [HttpPatch("{orderId}/status/{newStatus}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] UpdateOrderStatusRequest request)
        {
            var statusUpdated = await _orderService.UpdateOrderStatusAsync(request.OrderId, request.NewStatus);
            if (!statusUpdated)
                return BadRequest($"Failed to update status to '{request.NewStatus}'.");

            var updatedOrderDetail = await _orderService.GetOrderDetailByIdAsync(request.OrderId);
            return Ok(updatedOrderDetail);
        }

        // Task 3: Allow an Order to be created. This should include validation of any parameters
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest createOrderRequest)
        {
            var order = _mapper.Map<Order.Data.Entities.Order>(createOrderRequest);

            var newOrderId = await _orderService.CreateOrderAsync(order);

            var createdOrder = await _orderService.GetOrderDetailByIdAsync(newOrderId);

            return Ok(createdOrder);
        }

        // Task 4 - Calculate profit by month for all 'completed' Orders
        [HttpGet("monthly-profits")]
        public async Task<ActionResult<IEnumerable<MonthlyProfit>>> GetMonthlyProfits()
        {
            var monthlyProfits = await _orderService.GetMonthlyProfitsForCompletedOrdersAsync();
            return Ok(monthlyProfits);
        }
    }
}