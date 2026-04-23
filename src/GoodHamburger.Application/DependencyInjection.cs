using FluentValidation;
using GoodHamburger.Application.Behaviors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        return services;
    }
}
