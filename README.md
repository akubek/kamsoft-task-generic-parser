# Generic Data Parser API

Prosty endpoint Minimal API do dekodowania payloadu Base64 i parsowania danych typu CSV lub INTERNAL_JSON.
Repozytorium zawiera rozwiązanie zadania rekrutacyjnego dla firmy Kamsoft.

## Założenia zadania

- Technologia: .NET 8, C#.
- Endpoint: POST /api/v1/parse-content.
- Wejście: application/json.
- Payload:

```json
{
    "type": "CSV" | "INTERNAL_JSON",
    "content": "<base64>"
}
```

## Co robi endpoint

1. Sprawdza poprawność requestu i pola content.
2. Dekoduje content z Base64.
3. Wybiera parser na podstawie type (factory + DI).
4. Parsuje dane i zwraca ujednoliconą odpowiedź.

Przykładowa odpowiedź sukcesu:

```json
{
    "status": "Success",
    "sourceType": "CSV",
    "processedItemsCount": 2,
    "data": [
        { "Id": "1", "Name": "Artur" },
        { "Id": "2", "Name": "Test" }
    ]
}
```

## Obsługa błędów

Endpoint zwraca 400 Bad Request dla:

- pustego lub białego content,
- niepoprawnego Base64,
- nieobsługiwanego type,
- błędu parsowania CSV,
- błędu parsowania JSON.

Przykłady 400:

- `content` nie jest Base64 (np. `"not-base64"`),
- `type` poza wspieranym zakresem (np. `999`),
- CSV z brakującymi polami w trybie strict lub zduplikowanymi nagłówkami.

Dodatkowo request z niewłaściwym Content-Type zwraca 415 Unsupported Media Type.

Przykład 415:

- wysłanie `POST /api/v1/parse-content` jako `Content-Type: text/plain`.

## Uwagi o parserach

- CSV działa w trybie strict po stronie endpointu.
- Powtarzające się nazwy kolumn CSV są traktowane jako błąd, aby uniknąć niejawnej utraty danych.

## Uruchomienie lokalne

W katalogu repozytorium:

```bash
dotnet restore
dotnet run --project src/GenericDataParser.Api
```

Domyślny adres lokalny jest wypisywany przez Kestrel po starcie aplikacji.

Przykład requestu `curl`:

```bash
curl -X POST "http://localhost:5000/api/v1/parse-content" \
    -H "Content-Type: application/json" \
    -d '{"type":"CSV","content":"SWQsTmFtZQoxLEFydHVyCjIsVGVzdA=="}'
```

## Testy

Uruchomienie wszystkich testów:

```bash
dotnet test
```

Uruchomienie tylko testów integracyjnych endpointu:

```bash
dotnet test --filter "FullyQualifiedName~ParseContentEndpointIntegrationTests"
```

## Autor

Artur Kubek

## Licencja

Projekt jest udostępniony na licencji MIT. Szczegóły: [LICENSE](LICENSE).
