using System;
using MicroserviceApiKernel.Results;

namespace MicroserviceApiKernel.CQRS;

public interface IQuery<TResponse>;
public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    public Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct);
}


public interface ICommand<TResponse>;
public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    public Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct);
}

public interface ICommand;
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    public Task<Result> Handle(TCommand command, CancellationToken ct);
}
