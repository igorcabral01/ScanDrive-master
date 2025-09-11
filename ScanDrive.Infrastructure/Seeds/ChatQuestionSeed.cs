using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.Entities;
using ScanDrive.Infrastructure.Context;

namespace ScanDrive.Infrastructure.Seeds;

public static class ChatQuestionSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.ChatQuestions.AnyAsync())
            return;

        var step1Questions = new List<ChatQuestion>
        {
            new() { Question = "Quero veículos até 100 mil reais", Step = 1, IsEnabled = true },
            new() { Question = "Procurando carros automáticos", Step = 1, IsEnabled = true },
            new() { Question = "Veículos com baixa quilometragem", Step = 1, IsEnabled = true },
            new() { Question = "Carros de 2020 em diante", Step = 1, IsEnabled = true },
            new() { Question = "Veículos flex ou híbridos", Step = 1, IsEnabled = true },
            new() { Question = "Procurando SUVs", Step = 1, IsEnabled = true },
            new() { Question = "Carros de primeira mão", Step = 1, IsEnabled = true },
            new() { Question = "Veículos com câmbio manual", Step = 1, IsEnabled = true },
            new() { Question = "Carros de luxo", Step = 1, IsEnabled = true },
            new() { Question = "Veículos para família", Step = 1, IsEnabled = true }
        };

        var step2Questions = new List<ChatQuestion>
        {
            new() { Question = "Quais os prós e contras desse veículo?", Step = 2, IsEnabled = true },
            new() { Question = "Ele está em que loja?", Step = 2, IsEnabled = true },
            new() { Question = "Posso agendar um test drive?", Step = 2, IsEnabled = true },
            new() { Question = "Qual a quilometragem atual?", Step = 2, IsEnabled = true },
            new() { Question = "Tem histórico de acidentes?", Step = 2, IsEnabled = true },
            new() { Question = "Quantos donos já teve?", Step = 2, IsEnabled = true },
            new() { Question = "Está disponível para reserva?", Step = 2, IsEnabled = true },
            new() { Question = "Qual o valor do IPVA?", Step = 2, IsEnabled = true },
            new() { Question = "Tem documentação em dia?", Step = 2, IsEnabled = true },
            new() { Question = "Posso ver mais fotos?", Step = 2, IsEnabled = true }
        };

        context.ChatQuestions.AddRange(step1Questions);
        context.ChatQuestions.AddRange(step2Questions);
        
        await context.SaveChangesAsync();
    }
} 