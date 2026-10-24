using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace GestionCliente.Tests.Helpers;

public class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current!;

    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

public class TestDbAsyncEnumerable<T>(IEnumerable<T> inner) : IQueryable<T>, IAsyncEnumerable<T>
{
    private readonly IQueryable<T> _inner = inner.AsQueryable();

    public Type ElementType => _inner.ElementType;

    public Expression Expression => _inner.Expression;

    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner);

    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new TestAsyncEnumerator<T>(_inner.GetEnumerator());
}

public class TestAsyncQueryProvider<T>(IQueryable<T> inner) : IAsyncQueryProvider
{
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestDbAsyncEnumerable<TElement>(inner.Cast<TElement>());

    public IQueryable CreateQuery(Expression expression)
        => new TestDbAsyncEnumerable<T>(inner);

    public TResult Execute<TResult>(Expression expression)
        => throw new NotSupportedException("No soportado en pruebas.");

    public object? Execute(Expression expression)
        => throw new NotSupportedException("No soportado en pruebas.");

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        => throw new NotSupportedException("No soportado en pruebas.");
}
