using System.Text.Json;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Domain.Extensions;

public static class VehicleExtensions
{
    public static Vehicle FromJsonObject(this Vehicle vehicle, JsonElement jsonVehicle, Guid shopId, string createdById)
    {
        // Campos básicos
        vehicle.Brand = GetStringValue(jsonVehicle, "marca") ?? "Não informado";
        vehicle.Model = GetStringValue(jsonVehicle, "modelo") ?? "Não informado";
        vehicle.Year = GetIntValue(jsonVehicle, "ano") ?? DateTime.Now.Year;
        vehicle.Price = GetDecimalValue(jsonVehicle, "valor") ?? 0;
        vehicle.Description = GetStringValue(jsonVehicle, "obs") ?? GetStringValue(jsonVehicle, "obs_site") ?? "";
        vehicle.Mileage = GetIntValue(jsonVehicle, "km") ?? 0;
        vehicle.Color = GetStringValue(jsonVehicle, "cor") ?? "Não informado";
        vehicle.Transmission = GetStringValue(jsonVehicle, "cambio")?.ToLower() switch
        {
            "auto" => "Automático",
            "manual" => "Manual",
            _ => "Manual"
        };
        vehicle.FuelType = GetStringValue(jsonVehicle, "combustivel") ?? "Gasolina";

        // Novos campos do JSON
        vehicle.ExternalVehicleCode = GetStringValue(jsonVehicle, "cod_veiculo");
        vehicle.ImportCode = GetStringValue(jsonVehicle, "cod_importacao");
        vehicle.Category = GetStringValue(jsonVehicle, "categoria");
        vehicle.CategoryType = GetStringValue(jsonVehicle, "tipo_categoria");
        vehicle.Engine = GetStringValue(jsonVehicle, "motor");
        vehicle.Valves = GetStringValue(jsonVehicle, "valvulas");
        vehicle.Version = GetStringValue(jsonVehicle, "versao");
        vehicle.FullName = GetStringValue(jsonVehicle, "veiculo");
        vehicle.AlternativeName = GetStringValue(jsonVehicle, "veiculo2");
        vehicle.OfferPrice = GetDecimalValue(jsonVehicle, "valor_oferta") ?? 0;
        vehicle.FipePrice = GetDecimalValue(jsonVehicle, "valor_fipe") ?? 0;
        vehicle.SiteObservations = GetStringValue(jsonVehicle, "obs_site");
        vehicle.LicensePlate = GetStringValue(jsonVehicle, "placa");
        vehicle.Renavam = GetStringValue(jsonVehicle, "renavan");
        vehicle.Doors = GetIntValue(jsonVehicle, "portas") ?? 4;
        vehicle.Condition = GetStringValue(jsonVehicle, "estado") == "novo" ? "Novo" : "Usado";
        vehicle.IsHighlighted = GetStringValue(jsonVehicle, "destaqueSite") == "1";
        vehicle.IsOnOffer = GetStringValue(jsonVehicle, "em_oferta") == "sim";
        vehicle.CompanyName = GetStringValue(jsonVehicle, "nome_empresa");
        vehicle.City = GetStringValue(jsonVehicle, "cidade");
        vehicle.State = GetStringValue(jsonVehicle, "uf");
        vehicle.YouTubeUrl = GetStringValue(jsonVehicle, "youtube");

        // Configurações padrão
        vehicle.ShopId = shopId;
        vehicle.CreatedById = createdById;
        vehicle.IsActive = GetStringValue(jsonVehicle, "situacao") == "1";
        vehicle.CreatedAt = DateTime.UtcNow;

        return vehicle;
    }

    public static List<VehicleOptional> GetOptionalsFromJson(this JsonElement jsonVehicle, Guid vehicleId)
    {
        var optionals = new List<VehicleOptional>();
        
        if (jsonVehicle.TryGetProperty("opcionais", out var optionalsArray) && optionalsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var optional in optionalsArray.EnumerateArray())
            {
                var code = GetStringValue(optional, "codigo");
                var description = GetStringValue(optional, "descricao");
                
                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(description))
                {
                    optionals.Add(new VehicleOptional
                    {
                        Id = Guid.NewGuid(),
                        Code = code,
                        Description = description,
                        VehicleId = vehicleId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }
        
        return optionals;
    }

    private static string? GetStringValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            var value = property.GetString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        return null;
    }

    private static int? GetIntValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.GetInt32();
            }
            if (property.ValueKind == JsonValueKind.String)
            {
                var stringValue = property.GetString();
                if (int.TryParse(stringValue, out var intValue))
                {
                    return intValue;
                }
            }
        }
        return null;
    }

    private static decimal? GetDecimalValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.GetDecimal();
            }
            if (property.ValueKind == JsonValueKind.String)
            {
                var stringValue = property.GetString();
                if (decimal.TryParse(stringValue, out var decimalValue))
                {
                    return decimalValue;
                }
            }
        }
        return null;
    }
} 