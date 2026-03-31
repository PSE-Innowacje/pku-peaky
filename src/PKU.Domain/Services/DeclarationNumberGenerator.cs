using PKU.Domain.Enums;

namespace PKU.Domain.Services;

/// <summary>
/// Domain Service generujący numery oświadczeń zgodnie z PRD (pkt 7–10).
/// </summary>
public class DeclarationNumberGenerator
{
    private const string Prefix = "OSW";

    private static readonly HashSet<FeeType> NonTransmissionFeeTypes = new()
    {
        FeeType.OP, FeeType.OZE, FeeType.OKO, FeeType.OM
    };

    private static readonly HashSet<FeeType> TransmissionFeeTypes = new()
    {
        FeeType.OZ, FeeType.OJ, FeeType.OR, FeeType.ODO,
        FeeType.OPPEB, FeeType.OPMO
    };

    /// <summary>
    /// Generuje numer oświadczenia podstawowego (pkt 7, 9).
    /// Format: OSW/Typ_opłaty/Skrót_kontrahenta/rok/miesiąc/podokres/wersja
    /// </summary>
    public string GenerateBasicNumber(
        FeeType feeType,
        string contractorAbbreviation,
        int billingYear,
        int billingMonth,
        int subperiod,
        int version)
    {
        ValidateCommonParameters(contractorAbbreviation, billingYear, billingMonth, subperiod, version);
        ValidateFeeTypeForBasic(feeType);

        return FormatNumber(feeType, contractorAbbreviation, billingYear, billingMonth, subperiod, version);
    }

    /// <summary>
    /// Generuje numer oświadczenia korygującego (pkt 8, 10).
    /// Format: OSW/Typ_opłaty/Skrót_kontrahenta/rok/miesiąc/podokres/wersja/n_kor
    /// </summary>
    public string GenerateCorrectionNumber(
        FeeType feeType,
        string contractorAbbreviation,
        int billingYear,
        int billingMonth,
        int subperiod,
        int version,
        int correctionNumber)
    {
        ValidateCommonParameters(contractorAbbreviation, billingYear, billingMonth, subperiod, version);
        ValidateFeeTypeForCorrection(feeType);

        if (correctionNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(correctionNumber), "Numer korekty musi być >= 1.");

        var baseNumber = FormatNumber(feeType, contractorAbbreviation, billingYear, billingMonth, subperiod, version);
        return $"{baseNumber}/{correctionNumber:D2}";
    }

    /// <summary>
    /// Określa kategorię opłaty na podstawie typu.
    /// </summary>
    public static FeeCategory GetFeeCategory(FeeType feeType)
    {
        if (NonTransmissionFeeTypes.Contains(feeType))
            return FeeCategory.Pozaprzesylowa;

        if (TransmissionFeeTypes.Contains(feeType))
            return FeeCategory.Przesylowa;

        throw new ArgumentOutOfRangeException(nameof(feeType), $"Nieznany typ opłaty: {feeType}");
    }

    private static string FormatNumber(
        FeeType feeType,
        string contractorAbbreviation,
        int billingYear,
        int billingMonth,
        int subperiod,
        int version)
    {
        return $"{Prefix}/{feeType}/{contractorAbbreviation}/{billingYear}/{billingMonth:D2}/{subperiod:D2}/{version:D2}";
    }

    private static void ValidateCommonParameters(
        string contractorAbbreviation,
        int billingYear,
        int billingMonth,
        int subperiod,
        int version)
    {
        if (string.IsNullOrWhiteSpace(contractorAbbreviation))
            throw new ArgumentException("Skrót kontrahenta nie może być pusty.", nameof(contractorAbbreviation));

        if (contractorAbbreviation.Length > 20)
            throw new ArgumentException("Skrót kontrahenta nie może przekraczać 20 znaków.", nameof(contractorAbbreviation));

        if (HasPolishDiacritics(contractorAbbreviation))
            throw new ArgumentException("Skrót kontrahenta nie może zawierać polskich znaków diakrytycznych.", nameof(contractorAbbreviation));

        if (billingYear < 2000 || billingYear > 2100)
            throw new ArgumentOutOfRangeException(nameof(billingYear), "Rok rozliczenia musi być w zakresie 2000–2100.");

        if (billingMonth < 1 || billingMonth > 12)
            throw new ArgumentOutOfRangeException(nameof(billingMonth), "Miesiąc rozliczenia musi być w zakresie 1–12.");

        if (subperiod < 1)
            throw new ArgumentOutOfRangeException(nameof(subperiod), "Numer podokresu musi być >= 1.");

        if (version < 1)
            throw new ArgumentOutOfRangeException(nameof(version), "Numer wersji musi być >= 1.");
    }

    private static void ValidateFeeTypeForBasic(FeeType feeType)
    {
        // Pkt 7: OP, OZE, OKO, OM
        // Pkt 9: OZ, OJ, OR, ODO
        var allowedBasic = new HashSet<FeeType>
        {
            FeeType.OP, FeeType.OZE, FeeType.OKO, FeeType.OM,
            FeeType.OZ, FeeType.OJ, FeeType.OR, FeeType.ODO
        };

        if (!allowedBasic.Contains(feeType))
            throw new ArgumentException(
                $"Typ opłaty {feeType} nie jest dozwolony dla oświadczeń podstawowych. " +
                "Dozwolone: OP, OZE, OKO, OM (pozaprzesyłowe) oraz OZ, OJ, OR, ODO (przesyłowe).",
                nameof(feeType));
    }

    private static void ValidateFeeTypeForCorrection(FeeType feeType)
    {
        // Pkt 8: OP, OZE, OKO, OM
        // Pkt 10: OJ, OPPEB, OPMO
        var allowedCorrection = new HashSet<FeeType>
        {
            FeeType.OP, FeeType.OZE, FeeType.OKO, FeeType.OM,
            FeeType.OJ, FeeType.OPPEB, FeeType.OPMO
        };

        if (!allowedCorrection.Contains(feeType))
            throw new ArgumentException(
                $"Typ opłaty {feeType} nie jest dozwolony dla oświadczeń korygujących. " +
                "Dozwolone: OP, OZE, OKO, OM (pozaprzesyłowe) oraz OJ, OPPEB, OPMO (przesyłowe).",
                nameof(feeType));
    }

    private static bool HasPolishDiacritics(string text)
    {
        const string polishDiacritics = "ąćęłńóśźżĄĆĘŁŃÓŚŹŻ";
        return text.Any(c => polishDiacritics.Contains(c));
    }
}
