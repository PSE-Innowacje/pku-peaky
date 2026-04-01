using PKU.Domain.Enums;
using PKU.Domain.Services;

namespace PKU.Tests.Domain;

public class ContractorFeeMappingTests
{
    #region GetFeeTypesForContractor

    [Theory]
    [InlineData(ContractorType.OSDp, new[] { FeeType.OP, FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OJ, FeeType.OR })]
    [InlineData(ContractorType.OSDn, new[] { FeeType.OP, FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OR })]
    [InlineData(ContractorType.OdbiorcaKoncowy, new[] { FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OJ, FeeType.OR })]
    [InlineData(ContractorType.Wytworca, new[] { FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OJ, FeeType.OR })]
    [InlineData(ContractorType.Magazyn, new[] { FeeType.OZE, FeeType.OKO, FeeType.OM, FeeType.OJ, FeeType.OR })]
    public void GetFeeTypesForContractor_KnownContractorType_ReturnsExpectedFeeTypes(
        ContractorType contractorType, FeeType[] expectedFeeTypes)
    {
        var result = ContractorFeeMapping.GetFeeTypesForContractor(contractorType);

        Assert.Equal(expectedFeeTypes, result);
    }

    [Fact]
    public void GetFeeTypesForContractor_UnknownContractorType_ReturnsEmptyArray()
    {
        var result = ContractorFeeMapping.GetFeeTypesForContractor((ContractorType)999);

        Assert.Empty(result);
    }

    [Fact]
    public void GetFeeTypesForContractor_OSDp_ContainsOP()
    {
        var result = ContractorFeeMapping.GetFeeTypesForContractor(ContractorType.OSDp);

        Assert.Contains(FeeType.OP, result);
    }

    [Fact]
    public void GetFeeTypesForContractor_OSDn_DoesNotContainOJ()
    {
        var result = ContractorFeeMapping.GetFeeTypesForContractor(ContractorType.OSDn);

        Assert.DoesNotContain(FeeType.OJ, result);
    }

    [Theory]
    [InlineData(ContractorType.OdbiorcaKoncowy)]
    [InlineData(ContractorType.Wytworca)]
    [InlineData(ContractorType.Magazyn)]
    public void GetFeeTypesForContractor_NonOSD_DoesNotContainOP(ContractorType contractorType)
    {
        var result = ContractorFeeMapping.GetFeeTypesForContractor(contractorType);

        Assert.DoesNotContain(FeeType.OP, result);
    }

    [Theory]
    [InlineData(ContractorType.OSDp)]
    [InlineData(ContractorType.OSDn)]
    [InlineData(ContractorType.OdbiorcaKoncowy)]
    [InlineData(ContractorType.Wytworca)]
    [InlineData(ContractorType.Magazyn)]
    public void GetFeeTypesForContractor_AllContractorTypes_ContainOZE(ContractorType contractorType)
    {
        var result = ContractorFeeMapping.GetFeeTypesForContractor(contractorType);

        Assert.Contains(FeeType.OZE, result);
    }

    [Theory]
    [InlineData(ContractorType.OSDp)]
    [InlineData(ContractorType.OSDn)]
    [InlineData(ContractorType.OdbiorcaKoncowy)]
    [InlineData(ContractorType.Wytworca)]
    [InlineData(ContractorType.Magazyn)]
    public void GetFeeTypesForContractor_AllContractorTypes_ReturnNonEmptyArray(ContractorType contractorType)
    {
        var result = ContractorFeeMapping.GetFeeTypesForContractor(contractorType);

        Assert.NotEmpty(result);
    }

    #endregion

    #region GetFeeTypeName

    [Theory]
    [InlineData(FeeType.OP, "Oplata przejsciowa")]
    [InlineData(FeeType.OZE, "Oplata OZE")]
    [InlineData(FeeType.OKO, "Oplata kogeneracyjna")]
    [InlineData(FeeType.OM, "Oplata mocowa")]
    [InlineData(FeeType.OJ, "Oplata jakosciowa")]
    [InlineData(FeeType.OR, "Oplata rynkowa")]
    [InlineData(FeeType.OZ, "Oplata zmienna sieciowa")]
    [InlineData(FeeType.ODO, "Oplata dodatkowa")]
    [InlineData(FeeType.OPPEB, "Oplata za ponadumowny pobor energii biernej")]
    [InlineData(FeeType.OPMO, "Oplata za przekroczenie mocy umownej")]
    public void GetFeeTypeName_KnownFeeType_ReturnsExpectedName(FeeType feeType, string expectedName)
    {
        var result = ContractorFeeMapping.GetFeeTypeName(feeType);

        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void GetFeeTypeName_UnknownFeeType_ReturnsFeeTypeToString()
    {
        var unknownFeeType = (FeeType)999;

        var result = ContractorFeeMapping.GetFeeTypeName(unknownFeeType);

        Assert.Equal(unknownFeeType.ToString(), result);
    }

    #endregion

    #region GetFeeCategoryName

    [Theory]
    [InlineData(FeeCategory.Pozaprzesylowa, "Pozaprzesylowa")]
    [InlineData(FeeCategory.Przesylowa, "Przesylowa")]
    public void GetFeeCategoryName_KnownCategory_ReturnsExpectedName(FeeCategory category, string expectedName)
    {
        var result = ContractorFeeMapping.GetFeeCategoryName(category);

        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void GetFeeCategoryName_UnknownCategory_ReturnsCategoryToString()
    {
        var unknownCategory = (FeeCategory)999;

        var result = ContractorFeeMapping.GetFeeCategoryName(unknownCategory);

        Assert.Equal(unknownCategory.ToString(), result);
    }

    #endregion
}
