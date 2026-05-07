# TankDestroyer

## Overzicht

Een nieuw spel in 3d of via de console. Bestuur een tank, schiet de andere tanks dood.
Maak je eigen bot en maak een PR naar de hoofdtak.

## Hoe de console game te draaien

1. Open een terminal in de repository map.
2. Navigeer naar de console projectmap:
   ```bash
   cd TankDestroyer.Game.Console
   ```
3. Bouw en voer de game uit met `dotnet`:
   ```bash
   dotnet run
   ```

> Als je alleen de build wilt uitvoeren zonder meteen te draaien, gebruik:
> ```bash
> dotnet build
> ```

## Hoe een nieuwe bot te maken

1. Maak een kopie van de voorbeeldbotmap:
   - `Bots/Example.Bot/`
2. Hernoem de map naar iets wat bij je bot past, bijvoorbeeld:
   - `Bots/MyNew.Bot/`
3. Open de nieuwe botmap en hernoem waar nodig de bestanden:
   - `Example.Bot.csproj` -> `MyNew.Bot.csproj`
   - `ExampleBot.cs` -> `MyNewBot.cs`
4. Pas de inhoud van het projectbestand en de C#-code aan zodat de namespace en klassenaam overeenkomen met je nieuwe naam.

## Nieuwe bot toevoegen aan de oplossing

1. Nadat je de nieuwe bot hebt gekopieerd en hernoemd, voer je het script uit:
   ```bash
   ./AddProjectsToSolution.sh
   ```
2. Als het script niet direct uitvoerbaar is, maak het eerst uitvoerbaar:
   ```bash
   chmod +x AddProjectsToSolution.sh
   ./AddProjectsToSolution.sh
   ```

## Korte regels

- Elke beurt kunnen bots draaien, schieten of bewegen, elke actie kan maar 1 keer.
- Kogels bewegen tot 6 cellen in de opgegeven richting per beurt.
- Een kogel ontploft als hij een tank, boom of gebouw raakt.
- Schade die je ontvangt is afhankelijk van de tegel waar je opstaat:
  - boom: 25%
  - gebouw: 50%
  - andere tegels: 75%
- Tanks worden vernietigd als hun health op of onder 0 komt.
- Bullets die buiten de wereld bewegen, worden verwijderd.
- Het spel eindigt als er nog één tank nog levend is
- Over water kan je niet bewegen
- Een kogel kan niet geschoten worden uit een boom tegel
- Een kogel kan wel worden geschoten uit een gebouw tegel
- Een kogel gaat niet door een boom of gebouw tegel heen
- Kogels zijn gelimiteert tot 10, als deze op zijn zal er niet meer geschoten worden
- Als het gemiddelde aantal kogels onder 5 zakt zal er ammo gespawnt worden beginnende bij de speler die het verste weg is van ammo
- Ammo word gespawnt op een tile die volledig vrij is in het open
