using FluentValidation;
using Order.Service;
using Order.WebAPI.DTOs;

namespace Order.WebAPI.Validators
{
    public class GetOrdersByStatusNameRequestValidator : AbstractValidator<GetOrdersByStatusNameRequest>
    {
        public GetOrdersByStatusNameRequestValidator(IOrderService orderService)
        {
            RuleFor(x => x.StatusName)
                .NotEmpty().WithMessage("StatusName is required.")
                .MaximumLength(50).WithMessage("StatusName cannot exceed 50 characters.")
                .MustAsync(async (statusName, cancellation) =>
                    await orderService.CheckStatusNameExistsAsync(statusName))
                .WithMessage(model => $"Order status '{model.StatusName}' does not exist.");
        }
    }
}