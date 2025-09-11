# API de Importação de Veículos

Esta documentação explica como usar a nova rota de importação de veículos que aceita dados no formato do `base.json`.

## Endpoint

```
POST /api/vehicles/import
```

## Autenticação

A rota requer autenticação e o usuário deve ter uma das seguintes roles:
- `Admin`
- `ShopOwner`
- `ShopSeller`

## Cabeçalhos Obrigatórios

```
Authorization: Bearer {seu-token-jwt}
Content-Type: application/json
```

## Corpo da Requisição

```json
{
  "shopId": "guid-da-loja",
  "jsonData": "{\"cod_loja\":\"1750\",\"veiculos\":[...]}"
}
```

### Propriedades:

- **shopId** (Guid): ID da loja onde os veículos serão importados
- **jsonData** (string): JSON em formato string contendo os dados dos veículos

## Exemplo de Uso

### Requisição

```http
POST /api/vehicles/import
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "shopId": "12345678-1234-1234-1234-123456789012",
  "jsonData": "{\"cod_loja\":\"1750\",\"veiculos\":[{\"cod_veiculo\":\"1306868\",\"cod_importacao\":\"501882\",\"marca\":\"Peugeot\",\"modelo\":\"2008\",\"ano\":\"2023\",\"valor\":\"78990.00\",\"km\":\"53000\",\"cor\":\"Prata\",\"combustivel\":\"Bi-Combustível\",\"cambio\":\"auto\",\"estado\":\"usado\",\"categoria\":\"Carro/Camionetas\",\"tipo_categoria\":\"Hatch\",\"motor\":\"1.6\",\"versao\":\"PEUGEOT 2008 2023\",\"portas\":\"5\",\"placa\":\"JBO8J19\",\"cidade\":\"Joinville\",\"uf\":\"SC\",\"obs\":\"Descrição do veículo\",\"opcionais\":[{\"codigo\":\"1\",\"descricao\":\"Ar Condicionado\"},{\"codigo\":\"19\",\"descricao\":\"Air Bag\"}],\"fotos\":[\"foto1.webp\",\"foto2.webp\"]}]}"
}
```

### Resposta de Sucesso

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "message": "Importação concluída: 1 veículos importados, 0 ignorados, 0 erros",
  "processedCount": 1,
  "importedCount": 1,
  "skippedCount": 0,
  "errorCount": 0,
  "errors": [],
  "skippedVehicles": [],
  "importedAt": "2024-06-14T10:30:00Z"
}
```

### Resposta com Erros

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "message": "Importação concluída: 5 veículos importados, 2 ignorados, 1 erros",
  "processedCount": 8,
  "importedCount": 5,
  "skippedCount": 2,
  "errorCount": 1,
  "errors": [
    "Erro ao processar veículo: Marca é obrigatória"
  ],
  "skippedVehicles": [
    "Veículo com código 1306868 já existe",
    "Veículo com código 1297801 já existe"
  ],
  "importedAt": "2024-06-14T10:30:00Z"
}
```

## Mapeamento de Campos

| Campo JSON | Campo na Entidade | Tipo | Descrição |
|------------|-------------------|------|-----------|
| cod_veiculo | ExternalVehicleCode | string | Código externo do veículo |
| cod_importacao | ImportCode | string | Código de importação |
| marca | Brand | string | Marca do veículo |
| modelo | Model | string | Modelo do veículo |
| ano | Year | int | Ano do veículo |
| valor | Price | decimal | Preço do veículo |
| valor_oferta | OfferPrice | decimal | Preço em oferta |
| valor_fipe | FipePrice | decimal | Valor da tabela FIPE |
| km | Mileage | int | Quilometragem |
| cor | Color | string | Cor do veículo |
| combustivel | FuelType | string | Tipo de combustível |
| cambio | Transmission | string | Tipo de câmbio (auto/manual) |
| categoria | Category | string | Categoria do veículo |
| tipo_categoria | CategoryType | string | Tipo da categoria |
| motor | Engine | string | Motor do veículo |
| valvulas | Valves | string | Número de válvulas |
| versao | Version | string | Versão do veículo |
| veiculo | FullName | string | Nome completo do veículo |
| veiculo2 | AlternativeName | string | Nome alternativo |
| obs | Description | string | Observações/descrição |
| obs_site | SiteObservations | string | Observações para o site |
| placa | LicensePlate | string | Placa do veículo |
| renavan | Renavam | string | Código RENAVAM |
| portas | Doors | int | Número de portas |
| estado | Condition | string | Condição (novo/usado) |
| destaqueSite | IsHighlighted | bool | Se está em destaque |
| em_oferta | IsOnOffer | bool | Se está em oferta |
| nome_empresa | CompanyName | string | Nome da empresa |
| cidade | City | string | Cidade |
| uf | State | string | Estado (UF) |
| youtube | YouTubeUrl | string | URL do YouTube |
| opcionais | Optionals | array | Lista de opcionais |
| fotos | Photos | array | Lista de fotos |

## Códigos de Resposta

- **200 OK**: Importação processada (pode ter sucessos parciais)
- **400 Bad Request**: JSON inválido ou dados incorretos
- **401 Unauthorized**: Token de autenticação inválido ou ausente
- **403 Forbidden**: Usuário não tem permissão para a loja
- **404 Not Found**: Loja não encontrada

## Validações

1. **Loja deve existir**: A loja especificada no `shopId` deve existir e estar ativa
2. **Permissões**: O usuário deve ser admin, dono ou vendedor da loja
3. **JSON válido**: O `jsonData` deve ser um JSON válido
4. **Propriedade 'veiculos'**: O JSON deve conter a propriedade `veiculos` como array
5. **Duplicatas**: Veículos com o mesmo `cod_veiculo` são ignorados se já existirem

## Comportamento da Importação

1. **Verificação de duplicatas**: Veículos são verificados pelo `cod_veiculo` (ExternalVehicleCode)
2. **Transação**: Toda a importação é feita em uma transação única
3. **Rollback**: Se houver erro crítico no banco, toda a importação é revertida
4. **Opcionais**: Os opcionais do JSON são salvos como entidades relacionadas
5. **Fotos**: URLs das fotos são mantidas, mas não são baixadas automaticamente

## Exemplo Completo em JavaScript

```javascript
const importVehicles = async (shopId, vehicleData) => {
  const token = localStorage.getItem('authToken');
  
  const response = await fetch('/api/vehicles/import', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      shopId: shopId,
      jsonData: JSON.stringify(vehicleData)
    })
  });
  
  const result = await response.json();
  
  if (result.success) {
    console.log(`Importação concluída: ${result.importedCount} veículos importados`);
  } else {
    console.error('Erro na importação:', result.message);
  }
  
  return result;
};

// Uso
const vehicleData = {
  "cod_loja": "1750",
  "veiculos": [
    // ... seus dados de veículos aqui
  ]
};

importVehicles("12345678-1234-1234-1234-123456789012", vehicleData);
``` 