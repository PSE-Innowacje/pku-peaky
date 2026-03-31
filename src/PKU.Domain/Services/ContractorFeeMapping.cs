using PKU.Domain.Enums;

namespace PKU.Domain.Services;

/// <summary>
/// Mapowanie typ kontrahenta -> typy oplat, dla ktorych kontrahent sklada oswiadczenia.
/// Na podstawie PRD pkt 15.6 (a-o).
/// </summary>
public static class ContractorFeeMapping
{
    private static readonly Dictionary<ContractorType, FeeType[]> Mapping = new()
    {
        // OSDp: OP, OZE, OKO, OM, OJ, OR
        [ContractorType.OSDp] = [FeeType.OP, FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OJ, FeeType.OR],

        // OSDn: OP, OZE, OKO, OM, OR
        [ContractorType.OSDn] = [FeeType.OP, FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OR],

        // OK (Odbiorca Koncowy): OZE, OKO, OM, OJ, OR
        [ContractorType.OdbiorcaKoncowy] = [FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OJ, FeeType.OR],

        // Wyt (Wytworca): OZE, OKO, OM, OJ, OR
        [ContractorType.Wytworca] = [FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OJ, FeeType.OR],

        // Mag (Magazyn): OZE, OKO, OM, OJ, OR
        [ContractorType.Magazyn] = [FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OJ, FeeType.OR],
    };

    /// <summary>
    /// Zwraca typy oplat dla danego typu kontrahenta.
    /// </summary>
    public static FeeType[] GetFeeTypesForContractor(ContractorType contractorType)
    {
        return Mapping.TryGetValue(contractorType, out var feeTypes)
            ? feeTypes
            : [];
    }

    /// <summary>
    /// Zwraca nazwe wyswietlana dla typu oplaty.
    /// </summary>
    public static string GetFeeTypeName(FeeType feeType) => feeType switch
    {
        FeeType.OP => "Oplata przejsciowa",
        FeeType.OZE => "Oplata OZE",
        FeeType.OKO => "Oplata kogeneracyjna",
        FeeType.OM => "Oplata mocowa",
        FeeType.OJ => "Oplata jakosciowa",
        FeeType.OR => "Oplata rynkowa",
        FeeType.OZ => "Oplata zmienna sieciowa",
        FeeType.ODO => "Oplata dodatkowa",
        FeeType.OPPEB => "Oplata za ponadumowny pobor energii biernej",
        FeeType.OPMO => "Oplata za przekroczenie mocy umownej",
        _ => feeType.ToString()
    };

    /// <summary>
    /// Zwraca nazwe wyswietlana dla kategorii oplaty.
    /// </summary>
    public static string GetFeeCategoryName(FeeCategory category) => category switch
    {
        FeeCategory.Pozaprzesylowa => "Pozaprzesylowa",
        FeeCategory.Przesylowa => "Przesylowa",
        _ => category.ToString()
    };
}
