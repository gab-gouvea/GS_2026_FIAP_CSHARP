namespace SwarmBuild.Dominio.Structs;

/// <summary>
/// Value Object imutavel que representa uma coordenada geografica do robo ou tarefa.
/// Implementado como struct readonly para garantir imutabilidade e eficiencia
/// (passagem por valor evita alocacao no heap).
/// </summary>
public readonly struct Coordenada : IEquatable<Coordenada>
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Coordenada(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude deve estar entre -90 e 90");
        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude deve estar entre -180 e 180");

        Latitude = latitude;
        Longitude = longitude;
    }

    public double DistanciaEuclidiana(Coordenada outra)
    {
        var dx = Latitude - outra.Latitude;
        var dy = Longitude - outra.Longitude;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public bool Equals(Coordenada outra) =>
        Latitude == outra.Latitude && Longitude == outra.Longitude;

    public override bool Equals(object? obj) => obj is Coordenada c && Equals(c);

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    public override string ToString() =>
        $"({Latitude:F4}, {Longitude:F4})";

    public static bool operator ==(Coordenada a, Coordenada b) => a.Equals(b);
    public static bool operator !=(Coordenada a, Coordenada b) => !a.Equals(b);
}
