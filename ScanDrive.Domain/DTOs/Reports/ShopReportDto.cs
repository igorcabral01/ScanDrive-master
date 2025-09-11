namespace ScanDrive.Domain.DTOs.Reports;

public class ShopReportDto
{
    public string ShopId { get; set; } = null!;
    public string ShopName { get; set; } = null!;
    public int TotalVehicles { get; set; }
    public int SoldVehicles { get; set; }
    public int ReservedVehicles { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalProfit { get; set; }
    public List<SellerReportDto> SellerReports { get; set; } = new();
}

public class SellerReportDto
{
    public string SellerId { get; set; } = null!;
    public string SellerName { get; set; } = null!;
    public int VehiclesSold { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
} 