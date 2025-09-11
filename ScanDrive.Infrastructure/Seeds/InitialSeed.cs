using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScanDrive.Domain.Entities;
using ScanDrive.Infrastructure.Context;

namespace ScanDrive.Infrastructure.Seeds;

public static class InitialSeed
{
    public static async Task SeedData(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedAdminUser(userManager, configuration);

        // Verifica se o mock data está habilitado
        var enableMockData = configuration["SeedSettings:EnableMockData"];
        if (enableMockData?.ToLower() == "true")
        {
            await SeedMockData(context, userManager);
        }
    }

    private static async Task SeedAdminUser(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        var adminEmail = configuration["SeedSettings:AdminUser:Email"] ?? "admin@scandrive.com";
        var adminPassword = configuration["SeedSettings:AdminUser:Password"] ?? "Admin@123456";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task SeedMockData(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        // Seed das perguntas do chat
        await ChatQuestionSeed.SeedAsync(context);

        if (!context.Shops.Any())
        {
            // Criar alguns usuários para serem donos das lojas
            var shopOwners = new List<IdentityUser>();
            for (int i = 1; i <= 3; i++)
            {
                var owner = new IdentityUser
                {
                    UserName = $"owner{i}@scandrive.com",
                    Email = $"owner{i}@scandrive.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(owner, $"Owner@{i}23456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(owner, "ShopOwner");
                    shopOwners.Add(owner);
                }
            }

            // Criar lojas mockup
            var shops = new List<Shop>
            {
                new Shop
                {
                    Id = Guid.NewGuid(),
                    Name = "Kafka Multimarcas",
                    Description = "Especializada em carros de luxo e importados",
                    OwnerId = shopOwners[0].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Shop
                {
                    Id = Guid.NewGuid(),
                    Name = "Classic Motors",
                    Description = "Carros clássicos e colecionáveis",
                    OwnerId = shopOwners[1].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Shop
                {
                    Id = Guid.NewGuid(),
                    Name = "EcoVeículos",
                    Description = "Especializada em carros híbridos e elétricos",
                    OwnerId = shopOwners[2].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Shops.AddRangeAsync(shops);
            await context.SaveChangesAsync();

            // Criar veículos mockup para cada loja
            var vehicles = new List<Vehicle>();
            foreach (var shop in shops)
            {
                if (shop.Name == "Kafka Multimarcas")
                {
                    var kafkaVehicles = new List<Vehicle>
                    {
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = "BMW",
                            Model = "X5 M Competition",
                            Year = 2024,
                            Price = 1250000M,
                            Description = "BMW X5 M Competition, o SUV mais potente da marca. Equipado com motor V8 4.4L biturbo de 625cv.",
                            Mileage = 0,
                            Color = "Preto",
                            Transmission = "Automática",
                            FuelType = "Gasolina",
                            HasAuction = false,
                            HasAccident = false,
                            IsFirstOwner = true,
                            OwnersCount = 1,
                            Features = "Head-up Display, Teto Solar Panorâmico, Sistema de Som Harman Kardon, Bancos M Sport, Rodas 21\", Pacote Premium",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        },
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = "Mercedes-Benz",
                            Model = "AMG GT 63 S",
                            Year = 2024,
                            Price = 1450000M,
                            Description = "Mercedes-AMG GT 63 S 4MATIC+ 4 portas, o coupé mais potente da Mercedes. Motor V8 4.0L biturbo com 639cv.",
                            Mileage = 1500,
                            Color = "Cinza Selenita",
                            Transmission = "Automática",
                            FuelType = "Gasolina",
                            HasAuction = false,
                            HasAccident = false,
                            IsFirstOwner = true,
                            OwnersCount = 1,
                            Features = "Sistema de Som Burmester High-End 3D, Head-up Display, Pacote Night AMG, Rodas 21\" AMG Forge",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        },
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = "Porsche",
                            Model = "911 GT3 RS",
                            Year = 2023,
                            Price = 2100000M,
                            Description = "Porsche 911 GT3 RS, o mais puro dos 911. Motor 4.0L naturalmente aspirado com 525cv e pacote Weissach.",
                            Mileage = 800,
                            Color = "Verde Python",
                            Transmission = "PDK",
                            FuelType = "Gasolina",
                            HasAuction = false,
                            HasAccident = false,
                            IsFirstOwner = true,
                            OwnersCount = 1,
                            Features = "Pacote Weissach, Freios Cerâmicos PCCB, Sistema de Som Bose, Rodas de Magnésio",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        },
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = "Audi",
                            Model = "RS e-tron GT",
                            Year = 2024,
                            Price = 1350000M,
                            Description = "Audi RS e-tron GT, o Gran Turismo elétrico mais potente da Audi. Dois motores elétricos com 646cv combinados.",
                            Mileage = 2000,
                            Color = "Azul Táctico",
                            Transmission = "Automática",
                            FuelType = "Elétrico",
                            HasAuction = false,
                            HasAccident = false,
                            IsFirstOwner = true,
                            OwnersCount = 1,
                            Features = "Interior em Couro Nappa, Sistema de Som Bang & Olufsen, Rodas 21\" Performance, Pacote Dynamic Plus",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        },
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = "Lamborghini",
                            Model = "Huracán STO",
                            Year = 2023,
                            Price = 5200000M,
                            Description = "Lamborghini Huracán STO, versão homologada para rua do Super Trofeo. Motor V10 5.2L com 640cv.",
                            Mileage = 300,
                            Color = "Azul Laufey",
                            Transmission = "Automática",
                            FuelType = "Gasolina",
                            HasAuction = false,
                            HasAccident = false,
                            IsFirstOwner = true,
                            OwnersCount = 1,
                            Features = "Freios CCM-R, Telemetria, Sistema de Elevação, Interior em Alcantara, Pacote Carbon Fiber",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        },
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = "Ferrari",
                            Model = "F8 Tributo",
                            Year = 2022,
                            Price = 4200000M,
                            Description = "Ferrari F8 Tributo, o substituto da 488 GTB. Motor V8 3.9L biturbo com 720cv. Veículo recuperado de leilão internacional, totalmente restaurado.",
                            Mileage = 3500,
                            Color = "Rosso Corsa",
                            Transmission = "Automática",
                            FuelType = "Gasolina",
                            HasAuction = true,
                            HasAccident = true,
                            IsFirstOwner = false,
                            OwnersCount = 2,
                            AuctionHistory = "Veículo participou de leilão nos EUA em 2023. Documentação completa do processo de importação e regularização.",
                            AccidentHistory = "Acidente frontal leve em 2023, totalmente reparado com peças originais Ferrari. Laudo técnico disponível.",
                            Features = "Sistema de Som JBL Premium, Câmeras 360, Pacote Carbon Fiber, Interior em Alcantara",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        },
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = "Porsche",
                            Model = "Taycan Turbo S",
                            Year = 2023,
                            Price = 1150000M,
                            Description = "Porsche Taycan Turbo S, o sedan elétrico mais potente da marca. Dois motores elétricos com 761cv combinados. Veículo de leilão, excelente oportunidade.",
                            Mileage = 12000,
                            Color = "Branco Carrara",
                            Transmission = "Automática",
                            FuelType = "Elétrico",
                            HasAuction = true,
                            HasAccident = false,
                            IsFirstOwner = false,
                            OwnersCount = 2,
                            AuctionHistory = "Veículo arrematado em leilão premium da própria Porsche em 2023. Todas as revisões realizadas na concessionária.",
                            Features = "Interior em Couro Club, Pacote Sport Design, Rodas 21\" Mission E Design, Sistema de Som Burmester",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        },
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = "Mercedes-Benz",
                            Model = "G63 AMG",
                            Year = 2022,
                            Price = 1850000M,
                            Description = "Mercedes-Benz G63 AMG, o lendário G-Wagon em sua versão mais potente. Motor V8 4.0L biturbo com 585cv. Veículo com pequeno histórico de sinistro.",
                            Mileage = 15000,
                            Color = "Preto Obsidiana",
                            Transmission = "Automática",
                            FuelType = "Gasolina",
                            HasAuction = false,
                            HasAccident = true,
                            IsFirstOwner = false,
                            OwnersCount = 2,
                            AccidentHistory = "Colisão lateral leve em 2023, reparada na concessionária oficial Mercedes-Benz. Laudo cautelar disponível.",
                            Features = "Teto Solar Panorâmico, Sistema de Som Burmester, Pacote Night, Rodas 22\" AMG",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        }
                    };
                    vehicles.AddRange(kafkaVehicles);
                }
                else
                {
                    var shopVehicles = new List<Vehicle>
                    {
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = shop.Name == "Classic Motors" ? "Toyota" : "Honda",
                            Model = shop.Name == "Classic Motors" ? "Corolla" : "Civic",
                            Year = 2023,
                            Price = 150000M,
                            Description = "Veículo em excelente estado, completo com todos os opcionais disponíveis. Revisões em dia e documentação completa.",
                            Mileage = 15000,
                            Color = "Prata",
                            Transmission = "Automática",
                            FuelType = "Flex",
                            HasAuction = false,
                            HasAccident = false,
                            IsFirstOwner = true,
                            OwnersCount = 1,
                            Features = "Central Multimídia, Câmera de Ré, Sensor de Estacionamento, Bancos em Couro",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        },
                        new Vehicle
                        {
                            Id = Guid.NewGuid(),
                            Brand = shop.Name == "Classic Motors" ? "Honda" : "Toyota",
                            Model = shop.Name == "Classic Motors" ? "Civic" : "Corolla Cross",
                            Year = 2022,
                            Price = 130000M,
                            Description = "Único dono, todas as revisões em dia na concessionária. Veículo impecável com baixa quilometragem.",
                            Mileage = 25000,
                            Color = "Azul",
                            Transmission = "Automática",
                            FuelType = "Flex",
                            HasAuction = false,
                            HasAccident = false,
                            IsFirstOwner = true,
                            OwnersCount = 1,
                            Features = "Central Multimídia, Câmera de Ré, Sensor de Estacionamento, Ar Condicionado Digital",
                            ShopId = shop.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedById = shop.OwnerId,
                            IsActive = true
                        }
                    };
                    vehicles.AddRange(shopVehicles);
                }
            }

            await context.Vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();
        }
    }
} 