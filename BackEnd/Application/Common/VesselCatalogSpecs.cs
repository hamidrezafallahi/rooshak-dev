namespace Application.Common;

/// <summary>
/// Crystal/vessel catalog keys stored as ProductSpecification — not perfume SizeMl.
/// </summary>
public static class VesselCatalogSpecs
{
    public const string DiameterKey = "قطر";
    public const string HeightKey = "ارتفاع";
    public const string PieceCountKey = "تعداد پارچه";

    public static string FormatCm(decimal value) =>
        $"{Trim(value)} سانتی‌متر";

    public static string FormatPieces(int count) =>
        count.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string Trim(decimal value) =>
        value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
}
