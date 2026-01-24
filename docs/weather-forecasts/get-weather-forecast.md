# GET /weatherforecast

## Business popis

Endpoint vrací náhodně vygenerovanou předpověď počasí pro následujících 5 dní. Každý den obsahuje datum, teplotu v °C a °F, a slovní popis počasí.

## Technické detaily

- **URL**: `/weatherforecast`
- **Metoda**: `GET`
- **Parametry**: Žádné
- **Odpověď**: Kolekce objektů s následující strukturou:
  - `Date` - datum předpovědi
  - `TemperatureC` - teplota ve stupních Celsia (-20 až 55)
  - `TemperatureF` - teplota ve stupních Fahrenheita (vypočítáno z °C)
  - `Summary` - slovní popis počasí (Freezing, Bracing, Chilly, Cool, Mild, Warm, Balmy, Hot, Sweltering, Scorching)
