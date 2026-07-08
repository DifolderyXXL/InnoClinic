using System.Reflection;
using FluentValidation;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceApiKernel.Extensions;

public static class HandlerExtension
{
    private static IServiceCollection AddValidation(this IServiceCollection builder, Assembly assembly)
    {
        builder.AddValidatorsFromAssembly(assembly);
        return builder;
    }
    public static IServiceCollection AddHandlers(this IServiceCollection builder, Assembly assembly)
    {
        builder.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
        );

        builder.TryDecorate(typeof(ICommandHandler<>), typeof(ValidationCommandHandlerDecorator<>));
        builder.TryDecorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));
        builder.TryDecorate(typeof(IQueryHandler<,>), typeof(ValidationQueryHandlerDecorator<,>));

        builder.AddValidation(assembly);

        return builder;
    }
}

public class ValidationCommandHandlerDecorator<TCommand>(ICommandHandler<TCommand> handler, IServiceProvider serviceProvider)
    : ICommandHandler<TCommand> where TCommand : ICommand
{
    async Task<Result> ICommandHandler<TCommand>.Handle(TCommand command, CancellationToken ct)
    {
        var error = await ValidationHelper.Validate(serviceProvider, command, ct);

        if (error != null) return error;

        return await handler.Handle(command, ct);
    }
}

public class ValidationCommandHandlerDecorator<TCommand, TResponse>(ICommandHandler<TCommand, TResponse> handler, IServiceProvider serviceProvider)
    : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    async Task<Result<TResponse>> ICommandHandler<TCommand, TResponse>.Handle(TCommand command, CancellationToken ct)
    {
        var error = await ValidationHelper.Validate(serviceProvider, command, ct);

        if (error != null) return error;

        return await handler.Handle(command, ct);
    }
}

public class ValidationQueryHandlerDecorator<TQuery, TResponse>(IQueryHandler<TQuery, TResponse> handler, IServiceProvider serviceProvider)
    : IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct)
    {
        var error = await ValidationHelper.Validate(serviceProvider, query, ct);

        if (error != null) return error;

        return await handler.Handle(query, ct);
    }
}