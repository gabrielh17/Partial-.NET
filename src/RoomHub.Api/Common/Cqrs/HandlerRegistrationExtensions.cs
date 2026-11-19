using System.Reflection;

namespace RoomHub.Api.Common.Cqrs;

public static class HandlerRegistrationExtensions
{
    // Registers every ICommandHandler<,> in the assembly. Reflection runs once,
    // at startup; a new handler needs no extra line in Program.cs.
    public static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
    {
        Type[] handlerContracts = [typeof(ICommandHandler<,>)];

        var implementations = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false });

        foreach (var implementation in implementations)
        {
            var contracts = implementation.GetInterfaces()
                .Where(contract => contract.IsGenericType &&
                                   handlerContracts.Contains(contract.GetGenericTypeDefinition()));

            // Scoped, like the DbContext the handlers use.
            foreach (var contract in contracts)
                services.AddScoped(contract, implementation);
        }

        return services;
    }
}
