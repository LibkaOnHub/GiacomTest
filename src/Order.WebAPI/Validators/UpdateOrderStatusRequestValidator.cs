using FluentValidation;
using Order.Service;
using Order.WebAPI.DTOs;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator(IOrderService orderService)
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.")
            .MustAsync(async (orderId, cancellation) =>
                await orderService.CheckOrderExistsAsync(orderId))
            .WithMessage(model => $"Order with ID '{model.OrderId}' does not exist.");

        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("NewStatus is required.")
            .MaximumLength(50).WithMessage("NewStatus cannot exceed 50 characters.")
            .MustAsync(async (statusName, cancellation) =>
                await orderService.CheckStatusNameExistsAsync(statusName))
            .WithMessage(model => $"Order status '{model.NewStatus}' does not exist.");
    }
}