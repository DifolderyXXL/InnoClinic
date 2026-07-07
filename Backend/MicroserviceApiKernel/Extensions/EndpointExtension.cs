using FluentValidation;
using FluentValidation.Results;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Sockets;
using System.Reflection;
using Asp.Versioning;

namespace MicroserviceApiKernel.Extensions;

public static class EndpointExtension
{
    public static IServiceCollection AddEndpoints(this IServiceCollection builder, Assembly assembly)
    {

        var descriptors = assembly.GetTypes().Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(x => ServiceDescriptor.Describe(typeof(IEndpoint), x, ServiceLifetime.Transient));

        builder.TryAddEnumerable(descriptors);

        return builder;
    }

    public static void MapEndpoints(this WebApplication app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();
        
        var v1Group = app.MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1, 0);
        var v2Group = app.MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(2, 0);
        
        var endpoints = app.Services.GetServices<IEndpoint>();
        foreach (var endpoint in endpoints)
        {
            if (endpoint.Version == 2.0)
            {
                endpoint.MapEndpoint(v2Group);
            }
            else if (endpoint.Version == 1.0)
            {
                endpoint.MapEndpoint(v1Group);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}


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

public static class ValidationHelper
{
    public static Error CreateError(IValidator validator, ValidationResult result)
    {
        return new Error($"[{validator.GetType().FullName}] Validatoin failed", "", ErrorType.Validation, result.ToDictionary());
    }

    public static async Task<Error?> Validate<TCommand>(IServiceProvider serviceProvider, TCommand command, CancellationToken ct)
    {
        var validators = serviceProvider.GetServices<IValidator<TCommand>>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(command, ct);
            if (!result.IsValid)
            {
                return CreateError(validator, result);
            }
        }

        return null;
    }
}