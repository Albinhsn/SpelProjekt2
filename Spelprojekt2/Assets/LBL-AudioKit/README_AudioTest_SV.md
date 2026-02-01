# AudioKit – AudioTest (genre‑neutral)

AudioTest är en **testscen‑generator** som skapar en enkel scen för att verifiera att **FMOD + AudioKit** fungerar i Unity.

## Skapa scenen
I Unity‑menyn:
- **AudioKit → Test → Skapa AudioTest‑scen**

Generatorn skapar:
- `Assets/AudioTest/AudioTest.unity`
- Test‑assets (tomma mallar):
  - `Assets/AudioTest/AC_TestOneShot.asset`
  - `Assets/AudioTest/AC_TestLoop.asset`
  - `Assets/AudioTest/AP_TestParam.asset`
- Resources‑assets (om de saknas):
  - `Assets/Resources/Audio/AudioConfig.asset`
  - `Assets/Resources/Audio/AudioParamLibrary.asset`
  - `Assets/Resources/Audio/EventRegistry.asset`

## Obligatoriskt att fylla i (för att testet ska ge ljud)
1) Öppna `AC_TestOneShot` och sätt **evt** till ett one‑shot FMOD‑event.
2) Öppna `AC_TestLoop` och sätt **evt** till ett loop‑FMOD‑event.
3) Öppna `AP_TestParam` och sätt **fmodName** till en parameter som finns i FMOD (t.ex. `Indoor`).
4) Valfritt: öppna `AudioConfig` och sätt `mainMusicEvent` om musikstart via `MusicDirector` ska testas.

## Kör testet
- Öppna `AudioTest.unity` och starta Play Mode.
- Gå in i kuben **OneShotTrigger**: ett one‑shot ska spelas.
- Loopen **LoopEmitter** kan startas/stoppas via UI‑knappar i scenen.
- Parameter‑UI kan användas för att se att parameter‑sättning fungerar.

## Felsökning (snabbt)
- FMOD banks måste vara byggda och hittas av Unity.
- Events måste synas i Unitys FMOD Browser.
- En **StudioListener** behöver finnas (vanligen på `MainCamera`).
