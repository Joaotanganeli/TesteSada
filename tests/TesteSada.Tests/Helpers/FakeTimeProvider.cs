namespace TesteSada.Tests.Helpers;

/// <summary>
/// Relógio controlável para testes: torna determinísticas as regras que dependem da data atual.
/// </summary>
public class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public FakeTimeProvider(DateTimeOffset now) => _now = now;

    public override DateTimeOffset GetUtcNow() => _now;

    public void SetUtcNow(DateTimeOffset now) => _now = now;
}
