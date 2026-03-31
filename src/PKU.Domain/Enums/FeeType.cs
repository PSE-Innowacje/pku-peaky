namespace PKU.Domain.Enums;

/// <summary>
/// Typ opłaty używany w numeracji oświadczeń.
/// </summary>
public enum FeeType
{
    // Opłaty pozaprzesyłowe (pkt 7, 8)
    OP,
    OZE,
    OKO,
    OM,

    // Opłaty przesyłowe (pkt 9, 10)
    OZ,
    OJ,
    OR,
    ODO,
    OPPEB,
    OPMO
}
