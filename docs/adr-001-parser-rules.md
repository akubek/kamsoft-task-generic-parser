# ADR-001: Zasady walidacji parsera

## Status

Accepted

## Kontekst

Endpoint przyjmuje dane generyczne i musi unikać niejawnej utraty danych przy parsowaniu CSV.

## Decyzja

1. Endpoint używa parsera CSV w trybie strict.
2. Powtarzające się nazwy kolumn CSV są odrzucane błędem w obu trybach parsera.
3. W strict brakujące pola i uszkodzone dane kończą się błędem parsera.
4. INTERNAL_JSON jest parsowany przez System.Text.Json z obsługą komentarzy i trailing commas.

## Konsekwencje

1. Zachowanie parsera jest bardziej przewidywalne dla klientów API.
2. Unikamy cichego nadpisywania danych przy duplikatach nagłówków.
3. Błędy wejścia są jawnie raportowane przez API jako 400 Bad Request.
