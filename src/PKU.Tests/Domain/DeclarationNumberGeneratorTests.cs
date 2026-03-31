using PKU.Domain.Enums;
using PKU.Domain.Services;

namespace PKU.Tests.Domain;

public class DeclarationNumberGeneratorTests
{
    private readonly DeclarationNumberGenerator _sut = new();

    #region GenerateBasicNumber — format

    [Theory]
    [InlineData(FeeType.OP, "ENERGA", 2025, 3, 1, 1, "OSW/OP/ENERGA/2025/03/01/01")]
    [InlineData(FeeType.OZE, "TAURON", 2025, 12, 2, 5, "OSW/OZE/TAURON/2025/12/02/05")]
    [InlineData(FeeType.OKO, "PGE", 2024, 1, 1, 10, "OSW/OKO/PGE/2024/01/01/10")]
    [InlineData(FeeType.OM, "ENEA", 2025, 6, 3, 1, "OSW/OM/ENEA/2025/06/03/01")]
    public void GenerateBasicNumber_NonTransmission_ReturnsCorrectFormat(
        FeeType feeType, string contractor, int year, int month, int subperiod, int version, string expected)
    {
        var result = _sut.GenerateBasicNumber(feeType, contractor, year, month, subperiod, version);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(FeeType.OZ, "KLESZCZOW", 2025, 5, 1, 1, "OSW/OZ/KLESZCZOW/2025/05/01/01")]
    [InlineData(FeeType.OJ, "PSE", 2025, 7, 1, 2, "OSW/OJ/PSE/2025/07/01/02")]
    [InlineData(FeeType.OR, "ENERGA", 2025, 9, 1, 1, "OSW/OR/ENERGA/2025/09/01/01")]
    [InlineData(FeeType.ODO, "TAURON", 2025, 11, 2, 3, "OSW/ODO/TAURON/2025/11/02/03")]
    public void GenerateBasicNumber_Transmission_ReturnsCorrectFormat(
        FeeType feeType, string contractor, int year, int month, int subperiod, int version, string expected)
    {
        var result = _sut.GenerateBasicNumber(feeType, contractor, year, month, subperiod, version);
        Assert.Equal(expected, result);
    }

    #endregion

    #region GenerateBasicNumber — niedozwolone typy opłat

    [Theory]
    [InlineData(FeeType.OPPEB)]
    [InlineData(FeeType.OPMO)]
    public void GenerateBasicNumber_DisallowedFeeType_ThrowsArgumentException(FeeType feeType)
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.GenerateBasicNumber(feeType, "TEST", 2025, 1, 1, 1));
    }

    #endregion

    #region GenerateCorrectionNumber — format

    [Theory]
    [InlineData(FeeType.OP, "ENERGA", 2025, 3, 1, 1, 1, "OSW/OP/ENERGA/2025/03/01/01/01")]
    [InlineData(FeeType.OZE, "TAURON", 2025, 12, 2, 5, 3, "OSW/OZE/TAURON/2025/12/02/05/03")]
    [InlineData(FeeType.OKO, "PGE", 2024, 1, 1, 10, 12, "OSW/OKO/PGE/2024/01/01/10/12")]
    [InlineData(FeeType.OM, "ENEA", 2025, 6, 3, 1, 2, "OSW/OM/ENEA/2025/06/03/01/02")]
    public void GenerateCorrectionNumber_NonTransmission_ReturnsCorrectFormat(
        FeeType feeType, string contractor, int year, int month, int subperiod, int version,
        int correctionNumber, string expected)
    {
        var result = _sut.GenerateCorrectionNumber(feeType, contractor, year, month, subperiod, version, correctionNumber);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(FeeType.OJ, "PSE", 2025, 7, 1, 2, 1, "OSW/OJ/PSE/2025/07/01/02/01")]
    [InlineData(FeeType.OPPEB, "ENERGA", 2025, 5, 1, 1, 5, "OSW/OPPEB/ENERGA/2025/05/01/01/05")]
    [InlineData(FeeType.OPMO, "TAURON", 2025, 9, 2, 3, 2, "OSW/OPMO/TAURON/2025/09/02/03/02")]
    public void GenerateCorrectionNumber_Transmission_ReturnsCorrectFormat(
        FeeType feeType, string contractor, int year, int month, int subperiod, int version,
        int correctionNumber, string expected)
    {
        var result = _sut.GenerateCorrectionNumber(feeType, contractor, year, month, subperiod, version, correctionNumber);
        Assert.Equal(expected, result);
    }

    #endregion

    #region GenerateCorrectionNumber — niedozwolone typy opłat

    [Theory]
    [InlineData(FeeType.OZ)]
    [InlineData(FeeType.OR)]
    [InlineData(FeeType.ODO)]
    public void GenerateCorrectionNumber_DisallowedFeeType_ThrowsArgumentException(FeeType feeType)
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.GenerateCorrectionNumber(feeType, "TEST", 2025, 1, 1, 1, 1));
    }

    #endregion

    #region GenerateCorrectionNumber — numer korekty

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerateCorrectionNumber_InvalidCorrectionNumber_ThrowsArgumentOutOfRangeException(int correctionNumber)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.GenerateCorrectionNumber(FeeType.OP, "TEST", 2025, 1, 1, 1, correctionNumber));
    }

    #endregion

    #region Walidacja — skrót kontrahenta

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GenerateBasicNumber_EmptyContractor_ThrowsArgumentException(string? contractor)
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.GenerateBasicNumber(FeeType.OP, contractor!, 2025, 1, 1, 1));
    }

    [Fact]
    public void GenerateBasicNumber_ContractorTooLong_ThrowsArgumentException()
    {
        var longContractor = new string('A', 21);
        Assert.Throws<ArgumentException>(() =>
            _sut.GenerateBasicNumber(FeeType.OP, longContractor, 2025, 1, 1, 1));
    }

    [Fact]
    public void GenerateBasicNumber_ContractorExactly20Chars_Succeeds()
    {
        var contractor = new string('A', 20);
        var result = _sut.GenerateBasicNumber(FeeType.OP, contractor, 2025, 1, 1, 1);
        Assert.Contains(contractor, result);
    }

    [Theory]
    [InlineData("Łódź")]
    [InlineData("Kraków")]
    [InlineData("Gdańsk")]
    [InlineData("Wrocław")]
    [InlineData("ŻÓŁĆ")]
    public void GenerateBasicNumber_PolishDiacritics_ThrowsArgumentException(string contractor)
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.GenerateBasicNumber(FeeType.OP, contractor, 2025, 1, 1, 1));
    }

    #endregion

    #region Walidacja — rok rozliczenia

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void GenerateBasicNumber_InvalidYear_ThrowsArgumentOutOfRangeException(int year)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.GenerateBasicNumber(FeeType.OP, "TEST", year, 1, 1, 1));
    }

    [Theory]
    [InlineData(2000)]
    [InlineData(2100)]
    public void GenerateBasicNumber_BoundaryYear_Succeeds(int year)
    {
        var result = _sut.GenerateBasicNumber(FeeType.OP, "TEST", year, 1, 1, 1);
        Assert.Contains(year.ToString(), result);
    }

    #endregion

    #region Walidacja — miesiąc rozliczenia

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void GenerateBasicNumber_InvalidMonth_ThrowsArgumentOutOfRangeException(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.GenerateBasicNumber(FeeType.OP, "TEST", 2025, month, 1, 1));
    }

    #endregion

    #region Walidacja — podokres i wersja

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerateBasicNumber_InvalidSubperiod_ThrowsArgumentOutOfRangeException(int subperiod)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.GenerateBasicNumber(FeeType.OP, "TEST", 2025, 1, subperiod, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerateBasicNumber_InvalidVersion_ThrowsArgumentOutOfRangeException(int version)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.GenerateBasicNumber(FeeType.OP, "TEST", 2025, 1, 1, version));
    }

    #endregion

    #region GetFeeCategory

    [Theory]
    [InlineData(FeeType.OP, FeeCategory.Pozaprzesylowa)]
    [InlineData(FeeType.OZE, FeeCategory.Pozaprzesylowa)]
    [InlineData(FeeType.OKO, FeeCategory.Pozaprzesylowa)]
    [InlineData(FeeType.OM, FeeCategory.Pozaprzesylowa)]
    public void GetFeeCategory_NonTransmission_ReturnsPozaprzesylowa(FeeType feeType, FeeCategory expected)
    {
        Assert.Equal(expected, DeclarationNumberGenerator.GetFeeCategory(feeType));
    }

    [Theory]
    [InlineData(FeeType.OZ, FeeCategory.Przesylowa)]
    [InlineData(FeeType.OJ, FeeCategory.Przesylowa)]
    [InlineData(FeeType.OR, FeeCategory.Przesylowa)]
    [InlineData(FeeType.ODO, FeeCategory.Przesylowa)]
    [InlineData(FeeType.OPPEB, FeeCategory.Przesylowa)]
    [InlineData(FeeType.OPMO, FeeCategory.Przesylowa)]
    public void GetFeeCategory_Transmission_ReturnsPrzesylowa(FeeType feeType, FeeCategory expected)
    {
        Assert.Equal(expected, DeclarationNumberGenerator.GetFeeCategory(feeType));
    }

    #endregion

    #region Walidacja wspólna działa też dla GenerateCorrectionNumber

    [Fact]
    public void GenerateCorrectionNumber_EmptyContractor_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.GenerateCorrectionNumber(FeeType.OP, "", 2025, 1, 1, 1, 1));
    }

    [Fact]
    public void GenerateCorrectionNumber_PolishDiacritics_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _sut.GenerateCorrectionNumber(FeeType.OP, "Łódź", 2025, 1, 1, 1, 1));
    }

    [Fact]
    public void GenerateCorrectionNumber_InvalidMonth_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.GenerateCorrectionNumber(FeeType.OP, "TEST", 2025, 13, 1, 1, 1));
    }

    #endregion
}
