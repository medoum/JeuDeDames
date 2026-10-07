namespace JeuDeDames.Core;

/// <summary>
/// Un coup complet : la case de départ, les cases d'arrivée successives (plusieurs en cas de rafle)
/// et les pièces prises.
/// </summary>
public sealed record Move(Square From, IReadOnlyList<Square> Path, IReadOnlyList<Square> Captured)
{
    public Square To => Path[^1];

    public bool IsCapture => Captured.Count > 0;

    // Égalité par valeur sur le contenu des listes (un record compare sinon les références).
    public bool Equals(Move? other) =>
        other is not null
        && From == other.From
        && Path.SequenceEqual(other.Path)
        && Captured.SequenceEqual(other.Captured);

    public override int GetHashCode() => HashCode.Combine(From, To, Captured.Count);

    /// <summary>Notation officielle : « 32-28 » pour un déplacement, « 28x19x10 » pour une rafle.</summary>
    public override string ToString() =>
        IsCapture
            ? string.Join("x", Path.Prepend(From).Select(s => s.Number))
            : $"{From.Number}-{To.Number}";
}
