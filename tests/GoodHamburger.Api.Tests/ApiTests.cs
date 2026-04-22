using FluentAssertions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GoodHamburger.Api.Tests;

public class ApiWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all EF Core options to avoid multi-provider conflict
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Testing");
    }
}

public class OrderEndpointTests : IClassFixture<ApiWebFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebFactory _factory;

    public OrderEndpointTests(ApiWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid xBurger, Guid xBacon, Guid fries, Guid soda)> GetMenuIds()
    {
        var response = await _client.GetAsync("/api/menu");
        var items = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>();
        var xBurger = items!.First(i => i.Code == "XBURGER").Id;
        var xBacon = items!.First(i => i.Code == "XBACON").Id;
        var fries = items!.First(i => i.Code == "FRIES").Id;
        var soda = items!.First(i => i.Code == "SODA").Id;
        return (xBurger, xBacon, fries, soda);
    }

    [Fact]
    public async Task GET_Menu_Returns200_With5Items()
    {
        var response = await _client.GetAsync("/api/menu");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>();
        items.Should().HaveCount(5);
    }

    [Fact]
    public async Task POST_Order_ComboCompleto_Returns201_WithCorrectTotal()
    {
        var (_, xBacon, fries, soda) = await GetMenuIds();

        var response = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest([xBacon, fries, soda]));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.DiscountPercent.Should().Be(20m);
        order.Total.Should().Be(9.20m); // 7+2+2.5 = 11.50 - 20% = 9.20
    }

    [Fact]
    public async Task POST_Order_TwoSandwiches_Returns409()
    {
        var (xBurger, xBacon, _, _) = await GetMenuIds();

        var response = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest([xBurger, xBacon]));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GET_Order_NotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PUT_Order_NotFound_Returns404()
    {
        var (xBurger, _, _, _) = await GetMenuIds();
        var response = await _client.PutAsJsonAsync(
            $"/api/orders/{Guid.NewGuid()}",
            new UpdateOrderRequest([xBurger]));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_Order_Returns204()
    {
        var (xBurger, _, _, _) = await GetMenuIds();
        var createResponse = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest([xBurger]));
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/orders/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
