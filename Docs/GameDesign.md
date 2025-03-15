### Game Design

##### 1. Game Concept and Setting  
- **Beschrijving**: "Wreck of the Fallen Star" is een roguelike sci-fi dungeon-crawler waar spelers een reparatiedrone besturen op een tropische planeet waar een ruimteschip is gecrasht. De drone moet verschillende sectoren van het wrak verkennen, essentiële onderdelen verzamelen en het schip repareren om te ontsnappen.  
- **Genre**: Roguelike, sci-fi, dungeon-crawler.  
- **Setting**: Een gecrasht ruimteschip verspreid over een jungle-achtige planeet, met een 8-bit pixelart-stijl en een sombere kleurenschema (grijs, groen, bruin).  
- **Rondes**: Korte, verslavende rondes (3-5 minuten), geïnspireerd door Balatro, met winnable savegames maar oneindige progression voor unlocks.  
- **Inspiratie**: Geïnspireerd door Balatro (kaartgebaseerde strategie, visuele stijl, geluidseffecten, collectie) en Minecraft Dungeons (dungeon-crawls, uitbreidbare gameplay).  

##### 2. Story and Narrative  
- **Overzicht**: De speler is een reparatiedrone die na de crash wordt geactiveerd. Het doel is het schip te repareren en het mysterie achter de crash te ontrafelen.  
- **Narrative Elements**:  
  - Memory Logs: Verspreid door het schip, bieden informatie over de bemanning en de crash, zoals "Dag 47: vreemde ruis op de radio".  
  - Interactieve clues: De drone kan beschadigde panelen en holografische berichten inspecteren, zoals "Log: Systeem overbelast" of een bloedspat met "Geen tijd meer", om het verhaal te onthullen.  
  - Mysterie: De crash was geen ongeluk, mogelijk sabotage, alien-aanval, of een mislukt experiment, wat de speler moet ontdekken.  
- **Intro**: Skipbare cutscene bij de start (pixelart-animatie van de crash), later terug te kijken via een menu in het hoofdscherm (het schip).  
- **Post-game**: Na het repareren van het schip, vlieg naar een veilige locatie waar een Memory Log het crashmysterie onthult, en keer terug naar de crashsite om meer items te verzamelen voor drone-collectie.  

##### 3. Gameplay Mechanics  
- **Beweging**: De drone beweegt in vier richtingen met toetsenbordinput (WASD of pijltjestoetsen), met acceleratie en friction voor smooth beweging, gevolgd door een camera voor een isometrische look.  
- **Combat**: De drone schiet kogels op vijanden, die gezondheid hebben en Data Shards droppen bij nederlaag. Vijanden zijn kapotte drones, jungle-beesten, of andere bedreigingen.  
- **Interactie**: De drone kan objecten activeren met een toets (bijv. 'E'), zoals schakelaars voor deuren, clues voor verhaal, en items om te verzamelen.  
- **Rondes**: Elke ronde is een ruimte (3-5 minuten) met een mix van gevechten, puzzels en exploratie. Bijvoorbeeld: vecht een vijand, activeer een schakelaar om een deur te openen, en vind een Memory Log achter een verschoven kist.  
- **Deuren**: Ruimtes zijn gescheiden door deuren, loop naar de deur en druk op "Confirm" om naar de volgende ruimte te gaan. Deuren achter je sluiten, maar na het voltooien gaan alle deuren open, zodat je terug kunt voor achtergelaten items of easter eggs.  
- **Glitch Mode**: Optioneel, activeer voor extra moeilijkheid (bijv. vijanden sneller, zwaartekracht-shifts) met betere beloningen (zeldzame items).  
- **Seeds**: Spelers kunnen een seed invoeren in het hoofdscherm voor consistente randomisatie (ruimtelayouts, items), en zien de seed na een run om favorieten te herhalen.  

##### 4. Progression System  
- **Savegames and Routes**: Meerdere routes (maximaal 3 savegames), elk met verschillende sectoren. Voltooi een route om nieuwe te ontgrendelen of upgrades te krijgen.  
- **Collection and Upgrades**:  
  - Data Shards: Verzamel van vijanden of omgeving, gebruik in de hub voor upgrades (HP, snelheid, wapens).  
  - Memory Logs: Voor verhaalprogressie, mogelijk extra voordelen.  
  - Drone-upgrades: Functionele verbeteringen (bijv. laser i.p.v. standaardwapen, hoverboost), met optionele cosmetische verschillen (bijv. antenne-vorm), focus op functionaliteit boven cosmetica.  
- **Route-mechaniek**: Begin met één open route, unlock nieuwe routes met items (bijv. "Key Module" na voltooiing). Na een route kies je: doorgaan voor meer unlocks (zelfde niveau) of route sluiten (visueel gemarkeerd als afgerond), waarna je een nieuwe route moet kiezen. Bij falen reset de route naar begin, maar blijft open.  
- **Einddoel**: Repareer het hele schip door alle routes te voltooien en essentiële onderdelen te verzamelen, waarna je naar een veilige locatie vliegt en het crashmysterie onthult.  
- **Post-game**: Keer terug naar de crashsite om je drone-collectie uit te breiden met zeldzame onderdelen, functioneel en cosmetisch.  

##### 5. Art and Visual Style  
- **Pixel Art Guidelines**: Alles in 8-bit-stijl, sprites 32x32px, tiles 32x32px, gemaakt met Aseprite.  
- **Color Palette**: Somber palet (grijs, bruin, groen), accenten in rood (belangrijke items) en blauw (technologie).  
- **Camera and Effects**: Vaste camera met isometrische hoek (schuin van boven, zoals in oude Fallout-games), schaduwen en hover-effecten voor diepte, parallax scrolling van de achtergrond voor 3D-gevoel.  
- **Assets**: Drone, vijanden, schakelaars, deuren, kapotte panelen, tiles voor map (asphalt, borders), explosie-frames, alfabet-textures voor tekst-rendering.  

##### 6. Technical Details  
- **Technology Stack**: Monogame (game engine), C# (programmeertaal).  
- **Development Environment**: Visual Studio Code met C#-extensie, GitHub Copilot voor hulp, Aseprite voor pixelart.  
- **Dependencies**: .NET SDK 6.0+, Monogame libraries.  
- **Performance**: Gecompileerd C# voor efficiëntie, lichtgewicht zonder zware engine.  

##### 7. Development Roadmap  
- **Current State**: Spelerbeweging, camera-follow, basisdrone en vijand, collision met explosie, start- en doodscherm, debug-opties (hitbox-toggle, zoom), tekst-rendering.  
- **Next Steps**:  
  1. Implementeer vijand-AI (patrouilleren, achtervolgen).  
  2. Voeg puzzels toe (schakelaars, deuren, interactieve clues).  
  3. Implementeer verzamelsysteem (Data Shards, upgrades in hub).  
  4. Ontwerp hub-gebied (ruimteschip, visueel evoluerend na progressie).  
  5. Stel meerdere routes in met sectoren, visueel verbonden.  
  6. Voeg verhaal toe (Memory Logs, interactieve clues).  
  7. Verfijn art-assets (finale pixelart, schaduwen, hover).  
  8. Voeg geluidseffecten toe (sci-fi geluiden zoals lasers, explosies).  
  9. Test en balanceer de game (grenzen, collisions, upgrades).  

##### 8. Resources and Assets  
- **Sound Effects**: Vrij te gebruiken op [Freesound.org](https://freesound.org), zoek naar sci-fi geluiden zoals lasers en explosies.  
- **Art Assets**: Maak met Aseprite, inspiratie van [OpenGameArt.org](https://opengameart.org), zoals retro-sprites en tilesets.  

##### 9. Testing and Quality Assurance  
- **Test Cases**: Test spelerbeweging (geen clipping door muren), combat (vijanden verslaan, Data Shards droppen), puzzels (schakelaars activeren, deuren openen), progressie (routes unlocken, upgrades toepassen), en verhaal (Memory Logs, clues).  
- **Bug Tracking**: Gebruik GitHub issues voor bugtracking en feature-verzoeken, link naar [GitHub repository](https://github.com/RaphaelA4U/WreckOfTheFallenStar).  
- **Performance Testing**: Controleer frame rate, input-lag, en schaalbaarheid bij meerdere vijanden.  

##### 10. Future Expansions  
- **DLC Ideas**: Nieuwe planeten (ijsplaneet, woestijnplaneet) met eigen schipwrakken, drones en thema’s, start met ander dronetype.  
- **New Features**: Crafting-systeem voor drone-upgrades, boss-gevechten aan het einde van routes, co-op-modus, drone-customization met meer functionele opties.
