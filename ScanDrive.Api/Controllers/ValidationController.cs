using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ScanDrive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidationController : ControllerBase
{
    private readonly ILogger<ValidationController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly HashSet<string> InvalidDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin.com",
        "test.com",
        "teste.com",
        "example.com",
        "domain.com",
        "mail.com",
        "email.com",
        "web.com",
        "site.com",
        "company.com",
        "business.com",
        "info.com",
        "net.com",
        "org.com",
        "com.com",
        "local.com",
        "localhost.com",
        "invalid.com",
        "fake.com",
        "dummy.com",
        "temp.com",
        "temporary.com",
        "demo.com",
        "sample.com",
        "example.org",
        "test.org",
        "example.net",
        "test.net"
    };

    public ValidationController(
        ILogger<ValidationController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("cpf/{cpf}")]
    [AllowAnonymous]
    public IActionResult ValidateCPF(string cpf)
    {
        try
        {
            // Remove caracteres não numéricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
                return Ok(new { isValid = false });

            // Verifica se todos os dígitos são iguais
            if (cpf.Distinct().Count() == 1)
                return Ok(new { isValid = false });

            // Validação do primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(cpf[i].ToString()) * (10 - i);

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            if (digito1 != int.Parse(cpf[9].ToString()))
                return Ok(new { isValid = false });

            // Validação do segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(cpf[i].ToString()) * (11 - i);

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return Ok(new { isValid = digito2 == int.Parse(cpf[10].ToString()) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar CPF");
            return Ok(new { isValid = false });
        }
    }

    [HttpGet("cnpj/{cnpj}")]
    [AllowAnonymous]
    public IActionResult ValidateCNPJ(string cnpj)
    {
        try
        {
            // Remove caracteres não numéricos
            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

            if (cnpj.Length != 14)
                return Ok(new { isValid = false });

            // Verifica se todos os dígitos são iguais
            if (cnpj.Distinct().Count() == 1)
                return Ok(new { isValid = false });

            // Validação do primeiro dígito verificador
            int[] multiplicadores1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(cnpj[i].ToString()) * multiplicadores1[i];

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            if (digito1 != int.Parse(cnpj[12].ToString()))
                return Ok(new { isValid = false });

            // Validação do segundo dígito verificador
            int[] multiplicadores2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(cnpj[i].ToString()) * multiplicadores2[i];

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return Ok(new { isValid = digito2 == int.Parse(cnpj[13].ToString()) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar CNPJ");
            return Ok(new { isValid = false });
        }
    }

    [HttpGet("telefone/{telefone}")]
    [AllowAnonymous]
    public IActionResult ValidateTelefone(string telefone)
    {
        try
        {
            // Remove caracteres não numéricos
            telefone = new string(telefone.Where(char.IsDigit).ToArray());

            // Validação básica de telefone brasileiro
            // Celular: 11 dígitos (com DDD)
            // Fixo: 10 dígitos (com DDD)
            if (telefone.Length != 10 && telefone.Length != 11)
                return Ok(new { isValid = false });

            // Verifica se o DDD é válido (11 a 99)
            int ddd = int.Parse(telefone.Substring(0, 2));
            if (ddd < 11 || ddd > 99)
                return Ok(new { isValid = false });

            // Verifica se o primeiro dígito após o DDD é válido (2-9)
            int primeiroDigito = int.Parse(telefone.Substring(2, 1));
            if (primeiroDigito < 2 || primeiroDigito > 9)
                return Ok(new { isValid = false });

            return Ok(new { isValid = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar telefone");
            return Ok(new { isValid = false });
        }
    }

    [HttpGet("email/{email}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateEmail(string email)
    {
        try
        {
            // Regex para validação de email
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            bool isValid = Regex.IsMatch(email, pattern);

            if (!isValid)
                return Ok(new { isValid = false });

            // Verifica se o domínio tem MX record
            string domain = email.Split('@')[1].ToLower();

            // Verifica se o domínio está na lista de inválidos
            if (InvalidDomains.Contains(domain))
                return Ok(new { isValid = false });

            try
            {
                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://dns.google/resolve?name={domain}&type=MX");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var dnsResponse = JsonSerializer.Deserialize<DnsResponse>(content);
                    
                    // Verifica se tem resposta e se o status é 0 (sucesso)
                    isValid = dnsResponse?.Status == 0 && 
                             dnsResponse.Answer?.Any() == true && 
                             dnsResponse.Answer.Any(a => a.Type == 15 && !string.IsNullOrEmpty(a.Data));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível verificar MX record para o domínio {Domain}", domain);
                // Se não conseguir verificar o MX, considera inválido
                isValid = false;
            }

            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar email");
            return Ok(new { isValid = false });
        }
    }

    [HttpGet("rg/{rg}")]
    [AllowAnonymous]
    public IActionResult ValidateRG(string rg)
    {
        try
        {
            // Remove caracteres não numéricos
            rg = new string(rg.Where(char.IsDigit).ToArray());

            // RG deve ter entre 7 e 9 dígitos
            if (rg.Length < 7 || rg.Length > 9)
                return Ok(new { isValid = false });

            // Verifica se todos os dígitos são iguais
            if (rg.Distinct().Count() == 1)
                return Ok(new { isValid = false });

            return Ok(new { isValid = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar RG");
            return Ok(new { isValid = false });
        }
    }

    [HttpGet("cep/{cep}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateCEP(string cep)
    {
        try
        {
            // Remove caracteres não numéricos
            cep = new string(cep.Where(char.IsDigit).ToArray());

            if (cep.Length != 8)
                return Ok(new { isValid = false });

            // Verifica se o CEP existe usando a API ViaCEP
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://viacep.com.br/ws/{cep}/json/");
            if (!response.IsSuccessStatusCode)
                return Ok(new { isValid = false });

            var content = await response.Content.ReadAsStringAsync();
            return Ok(new { isValid = !content.Contains("erro") });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar CEP");
            return Ok(new { isValid = false });
        }
    }

    [HttpGet("placa/{placa}")]
    [AllowAnonymous]
    public IActionResult ValidatePlaca(string placa)
    {
        try
        {
            // Remove espaços e converte para maiúsculas
            placa = placa.Trim().ToUpper();

            // Verifica se é placa antiga (ABC1234) ou nova (ABC1D23)
            bool isPlacaAntiga = Regex.IsMatch(placa, @"^[A-Z]{3}[0-9]{4}$");
            bool isPlacaNova = Regex.IsMatch(placa, @"^[A-Z]{3}[0-9][A-Z][0-9]{2}$");

            return Ok(new { isValid = isPlacaAntiga || isPlacaNova });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar placa");
            return Ok(new { isValid = false });
        }
    }

    [HttpGet("data/{data}")]
    [AllowAnonymous]
    public IActionResult ValidateData(string data)
    {
        try
        {
            // Tenta converter a data
            if (DateTime.TryParse(data, out DateTime dataConvertida))
            {
                // Verifica se a data está em um intervalo razoável (1900-2100)
                return Ok(new { isValid = dataConvertida.Year >= 1900 && dataConvertida.Year <= 2100 });
            }

            return Ok(new { isValid = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar data");
            return Ok(new { isValid = false });
        }
    }

    [HttpGet("hora/{hora}")]
    [AllowAnonymous]
    public IActionResult ValidateHora(string hora)
    {
        try
        {
            // Tenta converter a hora
            if (TimeSpan.TryParse(hora, out TimeSpan horaConvertida))
            {
                // Verifica se a hora está em um intervalo válido (00:00-23:59)
                return Ok(new { isValid = horaConvertida >= TimeSpan.Zero && horaConvertida < TimeSpan.FromDays(1) });
            }

            return Ok(new { isValid = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar hora");
            return Ok(new { isValid = false });
        }
    }

    private class DnsResponse
    {
        public int Status { get; set; }
        public bool TC { get; set; }
        public bool RD { get; set; }
        public bool RA { get; set; }
        public bool AD { get; set; }
        public bool CD { get; set; }
        public List<DnsQuestion> Question { get; set; } = new();
        public List<DnsAnswer> Answer { get; set; } = new();
    }

    private class DnsQuestion
    {
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
    }

    private class DnsAnswer
    {
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public int TTL { get; set; }
        public string Data { get; set; } = string.Empty;
    }
} 