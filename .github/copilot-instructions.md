## Algemene instructies
- Voeg comments alleen in het Engels toe en alleen als ze nuttig zijn (bijv. om complexe logica te verduidelijken).
- Gebruik PascalCase voor klassen en methoden in C# (bijv. Player, UpdateGameState).
- Gebruik underscore voor private velden in C# (bijv. _playerPosition).
- Gebruik MonoGame-conventies, zoals SpriteBatch voor rendering en Vector2 voor posities.
- Check of nieuwe functionaliteiten overeenstemmen met het Docs/GameDesign.md document, zo niet beveel wat anders aan.

## Code-generation
- Gebruik dependency injection voor managers zoals InputManager en GraphicsManager.
- Maak klassen modulair en volg SOLID-principes waar mogelijk.

## Test-generation
- Gebruik xUnit voor het genereren van tests in C#.
- Voeg een korte Engelstalige comment toe boven elke testmethode om het doel te beschrijven (bijv. // Test if player movement updates position correctly).
- Test altijd de Update- en Draw-methoden van MonoGame game states.

## Code review
- Controleer of comments in het Engels zijn en alleen toegevoegd zijn waar ze nuttig zijn (bijv. bij complexe logica).
- Waarschuw als er hardcoded waarden in de code staan, zoals magische getallen of strings.
- Controleer of alle MonoGame-klassen correct IDisposable implementeren.
- Controleer of de documentatie (bijv. Docs/README.md of Docs/gameplay.md) up-to-date is bij grotere wijzigingen.
- Check of nieuwe functionaliteiten overeenstemmen met het Docs/GameDesign.md document, zo niet beveel wat anders aan.

## Commit messages
- Gebruik het formaat 'Type: Beschrijving' (bijv. 'Feature: Nieuwe Enemy-klasse toegevoegd').
- Voeg een regel toe zoals 'Updated documentation: [bestand]' als de wijziging groter is (bijv. nieuwe klasse, refactor, of feature).

## Pull requests
- Voeg een sectie 'Documentation Updates' toe aan de pull request description als de wijziging groter is (bijv. nieuwe klasse, refactor, of feature).
- Beschrijf de wijzigingen in het Engels en geef een voorbeeld van de impact (bijv. 'Dit voegt een nieuwe vijand toe die de speler volgt').
- Vermeld of de pull request invloed heeft op MonoGame-rendering of input.