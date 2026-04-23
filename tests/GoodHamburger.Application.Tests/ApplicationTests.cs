using FluentAssertions;
using FluentValidation;
using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.DiscountRules.Commands;
using GoodHamburger.Application.DiscountRules.Queries;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Application.Menu.Queries;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Application.Orders.Queries;
using GoodHamburger.Application.Validators;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Exceptions;
using NSubstitute;
using Xunit;

namespace GoodHamburger.Application.Tests;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly IMenuRepository _menuRepo = Substitute.For<IMenuRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IAsyncDiscountPolicy _policy = Substitute.For<IAsyncDiscountPolicy>();

    private static MenuItem MakeSandwich() =>
        MenuItem.Create("XBURGER", "X Burger", 5.00m, ProductCategory.Sandwich);

    private static MenuItem MakeFries() =>
        MenuItem.Create("FRIES", "Batata frita", 2.00m, ProductCategory.Fries);

    private static MenuItem MakeDrink() =>
        MenuItem.Create("SODA", "Refrigerante", 2.50m, ProductCategory.Drink);

    [Fact]
    public async Task Handle_ValidCommand_CreatesOrder()
    {
        var sandwich = MakeSandwich();
        var fries = MakeFries();
        var menuItems = new List<MenuItem> { sandwich, fries };
        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(menuItems);
        _policy.CalculateAsync(Arg.Any<IReadOnlyCollection<OrderItem>>(), Arg.Any<decimal>(),
            Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new DiscountResult(10m, null, null)));

        var handler = new CreateOrderCommandHandler(_orderRepo, _menuRepo, _unitOfWork, _policy);
        var command = new CreateOrderCommand([sandwich.Id, fries.Id]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        await _orderRepo.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MenuItemsNotFound_ThrowsInvalidOrderException()
    {
        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<MenuItem>());

        var handler = new CreateOrderCommandHandler(_orderRepo, _menuRepo, _unitOfWork, _policy);
        var command = new CreateOrderCommand([Guid.NewGuid()]);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOrderException>();
    }

    [Fact]
    public async Task Handle_DuplicateCategory_ThrowsDuplicateItemException()
    {
        var sandwich1 = MakeSandwich();
        var sandwich2 = MenuItem.Create("XBACON", "X Bacon", 7.00m, ProductCategory.Sandwich);
        var menuItems = new List<MenuItem> { sandwich1, sandwich2 };
        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(menuItems);

        var handler = new CreateOrderCommandHandler(_orderRepo, _menuRepo, _unitOfWork, _policy);
        var command = new CreateOrderCommand([sandwich1.Id, sandwich2.Id]);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DuplicateItemException>();
    }
}

public class UpdateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly IMenuRepository _menuRepo = Substitute.For<IMenuRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IAsyncDiscountPolicy _policy = Substitute.For<IAsyncDiscountPolicy>();

    private static MenuItem MakeSandwich() =>
        MenuItem.Create("XBURGER", "X Burger", 5.00m, ProductCategory.Sandwich);

    [Fact]
    public async Task Handle_ExistingOrder_UpdatesAndReturns()
    {
        var order = Order.Create();
        var sandwich = MakeSandwich();
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(order);
        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<MenuItem> { sandwich });
        _policy.CalculateAsync(Arg.Any<IReadOnlyCollection<OrderItem>>(), Arg.Any<decimal>(),
            Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new DiscountResult(0m, null, null)));

        var handler = new UpdateOrderCommandHandler(_orderRepo, _menuRepo, _unitOfWork, _policy);
        var command = new UpdateOrderCommand(order.Id, [sandwich.Id]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        _orderRepo.Received(1).Update(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsInvalidOrderException()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        var handler = new UpdateOrderCommandHandler(_orderRepo, _menuRepo, _unitOfWork, _policy);
        var command = new UpdateOrderCommand(Guid.NewGuid(), [Guid.NewGuid()]);

        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOrderException>()
            .Where(e => e.IsNotFound);
    }
}

public class DeleteOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_ExistingOrder_DeletesIt()
    {
        var order = Order.Create();
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(order);

        var handler = new DeleteOrderCommandHandler(_orderRepo, _unitOfWork);
        await handler.Handle(new DeleteOrderCommand(order.Id), CancellationToken.None);

        _orderRepo.Received(1).Delete(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsInvalidOrderException()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        var handler = new DeleteOrderCommandHandler(_orderRepo, _unitOfWork);
        var act = async () => await handler.Handle(new DeleteOrderCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOrderException>()
            .Where(e => e.IsNotFound);
    }
}

public class GetOrderByIdQueryHandlerTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();

    [Fact]
    public async Task Handle_ExistingOrder_ReturnsResponse()
    {
        var order = Order.Create();
        _orderRepo.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var handler = new GetOrderByIdQueryHandler(_orderRepo);
        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsInvalidOrderException()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        var handler = new GetOrderByIdQueryHandler(_orderRepo);
        var act = async () => await handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOrderException>()
            .Where(e => e.IsNotFound);
    }
}

public class ListOrdersQueryHandlerTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();

    [Fact]
    public async Task Handle_ReturnsPaginatedResult()
    {
        var order = Order.Create();
        _orderRepo.ListAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Order> { order } as IReadOnlyList<Order>, 1));

        var handler = new ListOrdersQueryHandler(_orderRepo);
        var result = await handler.Handle(new ListOrdersQuery(1, 20), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }
}

public class GetMenuQueryHandlerTests
{
    private readonly IMenuRepository _menuRepo = Substitute.For<IMenuRepository>();

    [Fact]
    public async Task Handle_ReturnsMenuItems()
    {
        var items = new List<Domain.Entities.MenuItem>
        {
            MenuItem.Create("XBURGER", "X Burger", 5.00m, ProductCategory.Sandwich)
        };
        _menuRepo.GetAllAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(items);

        var handler = new GetMenuQueryHandler(_menuRepo);
        var result = await handler.Handle(new GetMenuQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Code.Should().Be("XBURGER");
    }
}

public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    [Fact]
    public async Task EmptyList_ReturnsValidationError()
    {
        var result = await _validator.ValidateAsync(new CreateOrderRequest([]));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "MenuItemIds");
    }

    [Fact]
    public async Task MoreThan3Items_ReturnsValidationError()
    {
        var ids = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToList();
        var result = await _validator.ValidateAsync(new CreateOrderRequest(ids));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task DuplicateIds_ReturnsValidationError()
    {
        var id = Guid.NewGuid();
        var result = await _validator.ValidateAsync(new CreateOrderRequest([id, id]));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidList_PassesValidation()
    {
        var result = await _validator.ValidateAsync(new CreateOrderRequest([Guid.NewGuid()]));
        result.IsValid.Should().BeTrue();
    }
}

public class CreateDiscountRuleCommandHandlerTests
{
    private readonly IDiscountRepository _discountRepo = Substitute.For<IDiscountRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateDiscountRuleCommand ValidCommand(int priority = 1) => new(
        "Combo Completo", 20m, DiscountMatchMode.CategoryAtLeast,
        RequiresSandwich: true, RequiresFries: true, RequiresDrink: true,
        RequiredMenuItemIds: null, Priority: priority, IsActive: true,
        MinimumSubtotal: null, ValidFrom: null, ValidUntil: null);

    [Fact]
    public async Task Handle_ValidCommand_CreatesRule()
    {
        _discountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DiscountRule>());

        var handler = new CreateDiscountRuleCommandHandler(_discountRepo, _unitOfWork);
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Combo Completo");
        result.Percent.Should().Be(20m);
        await _discountRepo.Received(1).AddAsync(Arg.Any<DiscountRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicatePriority_ThrowsConflict()
    {
        var existing = DiscountRule.Create("Existing", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, null, null, null);
        _discountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DiscountRule> { existing });

        var handler = new CreateDiscountRuleCommandHandler(_discountRepo, _unitOfWork);
        var act = async () => await handler.Handle(ValidCommand(priority: 1), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidDiscountRuleException>()
            .Where(e => e.IsConflict);
    }
}

public class SimulateDiscountQueryHandlerTests
{
    private readonly IDiscountRepository _discountRepo = Substitute.For<IDiscountRepository>();
    private readonly IMenuRepository _menuRepo = Substitute.For<IMenuRepository>();

    private static MenuItem MakeSandwich() =>
        MenuItem.Create("XBURGER", "X Burger", 5.00m, ProductCategory.Sandwich);
    private static MenuItem MakeFries() =>
        MenuItem.Create("FRIES", "Batata frita", 2.00m, ProductCategory.Fries);
    private static MenuItem MakeDrink() =>
        MenuItem.Create("SODA", "Refrigerante", 2.50m, ProductCategory.Drink);

    [Fact]
    public async Task Handle_ComboCompleto_Returns20Percent()
    {
        var sandwich = MakeSandwich();
        var fries = MakeFries();
        var drink = MakeDrink();

        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<MenuItem> { sandwich, fries, drink });

        var comboRule = DiscountRule.Create("Combo", 20m, DiscountMatchMode.CategoryAtLeast,
            true, true, true, null, 1, true, null, null, null);
        _discountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DiscountRule> { comboRule });

        var handler = new SimulateDiscountQueryHandler(_discountRepo, _menuRepo);
        var result = await handler.Handle(
            new SimulateDiscountQuery([sandwich.Id, fries.Id, drink.Id]),
            CancellationToken.None);

        result.AppliedPercent.Should().Be(20m);
        result.AppliedRuleName.Should().Be("Combo");
        result.Evaluations.Should().ContainSingle(e => e.Matched);
    }

    [Fact]
    public async Task Handle_NoMatchingRule_Returns0Percent()
    {
        var sandwich = MakeSandwich();
        _menuRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<MenuItem> { sandwich });

        _discountRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DiscountRule>());

        var handler = new SimulateDiscountQueryHandler(_discountRepo, _menuRepo);
        var result = await handler.Handle(
            new SimulateDiscountQuery([sandwich.Id]),
            CancellationToken.None);

        result.AppliedPercent.Should().Be(0m);
        result.AppliedRuleId.Should().BeNull();
    }
}
