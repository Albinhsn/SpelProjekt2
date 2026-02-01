# AudioKit (FMOD + Unity) – generiskt för 3D‑spel

AudioKit är ett återanvändbart ljudramverk för Unity (3D) med FMOD Studio. Paketet är designat för att vara **genre‑oberoende** och **projekt‑oberoende**: inga event‑ eller parameternamn behöver hårdkodas i kod.

## Innehåll (kort)
- **AudioSystem**: init/boot + gemensamma resurser.
- **AudioEventHub**: start/stop/one‑shots/loop‑instanser.
- **SfxDirector**: one‑shots via AudioCue (cooldown/preload).
- **MusicDirector**: valfri music‑instans + parametrar.
- **AudioParameterDriver**: global parameter‑mixning (prioritet + smoothing).
- **Zoner/Triggers**: parametrar och one‑shots via colliders.
- **Spatial**: **AudioOcclusionEmitter** (raycast‑occlusion per emitter).
- **Snapshots**: **SnapshotZone** (reverb/lowpass/duck snapshots via trigger).
- **Mix**: **AudioDucking** (token‑baserad ducking mot Master/Music/Sfx/UI eller custom VCA).
- **Debug**: **AudioDebugOverlay** + **FmodTimelineEvents** (markers/beats → UnityEvents).
- **Verktyg**: **AudioKitValidator** (meny‑validering).
- **EventRegistry / ParamLibrary / AudioConfig**: per‑projekt data i assets.
- **AudioTest Wizard**: genererar en neutral testscen.

---

## 1) Parametrar utan hårdkodning
Allt som tar en parameter använder **AudioParamRef**. I Inspectorn väljs ett av tre lägen:
- **Asset**: referens till en *AudioParameterSO*.
- **Key**: nyckel som mappar via **AudioParamLibrary**.
- **Namn**: exakt FMOD‑parameternamn direkt.

### AudioParamLibrary (”parameterlista” per projekt)
Skapa asset via menyn:
- `AudioKit/Ljud/Skapa AudioParamLibrary i Resources`

Förväntad sökväg:
- `Assets/Resources/Audio/AudioParamLibrary.asset`

Fyll `entries`:
- `key` = intern nyckel i Unity (t.ex. `Speed`, `Indoor`, `Intensity`)
- `fmodName` = exakt namn i FMOD (t.ex. `PlayerSpeed`)

Resultat: dropdown‑val i AudioParamRef när **Key** används.

---

## 2) AudioConfig
Skapa asset via menyn:
- `AudioKit/Ljud/Skapa AudioConfig i Resources`

Förväntad sökväg:
- `Assets/Resources/Audio/AudioConfig.asset`

`AudioConfig` innehåller projektets:
- VCA‑paths (Master/Music/SFX/UI)
- bus‑path
- snapshots (valfritt)
- main music event (valfritt)
- parametrar (kan anges som FMOD‑namn eller Key via ParamLibrary)

Obs: fälten är tomma från start för att undvika antaganden om namn.

---

## 3) Events utan hårdkodning (EventRegistry)
Skapa asset via menyn:
- `AudioKit/Ljud/Skapa EventRegistry i Resources`

Förväntad sökväg:
- `Assets/Resources/Audio/EventRegistry.asset`

I registry läggs `id -> EventReference`. Detta möjliggör start/stop av loopar via ID (t.ex. `TEST_LOOP`) i stället för att skriva event‑paths.

---

## 4) Generiska actions (AudioTrigger / AudioSceneSettings)
För enkla flöden utan specialskript:
- **AudioTrigger**: läggs på en trigger‑volym och kör en lista av `AudioAction` (play one‑shot, start/stop loop, set param, set snapshot, set VCA).
- **AudioSceneSettings**: kör `AudioAction` vid scene‑load.

---

## 5) Snabb AudioTest‑scen
### Rekommenderat (automatiskt)
Kör menyn:
- **AudioKit → Test → Skapa AudioTest‑scen**

Detta skapar en helt genre‑neutral scen samt tomma test‑assets (events/parametrar fylls i via Inspector).

### Manuellt (om en egen testscen ska byggas)
1. Skapa scen: `AudioTest`
2. Skapa GameObject: `_AudioSystem`
   - lägg på: `AudioSystem`, `AudioEventHub`, `SfxDirector`, `MusicDirector`, `AudioParameterDriver`
3. Säkerställ `AudioConfig.asset` i `Assets/Resources/Audio/`
4. Spelare: triggers/zones kan filtrera via tag/layer (default = `Player`)
5. Lägg ut testobjekt:
   - `AudioOneShotTrigger` (one‑shots)
   - `AudioParameterZone` (globala params)
   - `MusicParameterZone` (param i main music event)
   - `ApproachDistanceParamDriver` / `PathProgressParamDriver` (0..1 via avstånd/progress)



---

## 6) Spatial & mix (avancerat, men fortfarande generiskt)

### AudioOcclusionEmitter (per emitter)
1. Lägg `AudioEmitter` på ett GameObject som spelar en loop (eller en längre instans).
2. Lägg `AudioOcclusionEmitter` på samma GameObject.
3. I FMOD: skapa en parameter (t.ex. `Occlusion`) som styr LPF/volym.
4. I komponenten: sätt `occlusionParam` (Asset/Key/Namn).

Rekommendation:
- `sphereRadius`: 0.1–0.25
- `smoothSeconds`: 0.05–0.15
- `occluderMask`: exkludera Player/UI-lager

### SnapshotZone (reverb, lowpass, ducking m.m.)
1. Skapa snapshot i FMOD (t.ex. `event:/Snapshots/InteriorReverb`).
2. Lägg `SnapshotZone` på ett GameObject med `Collider` satt till *Is Trigger*.
3. Sätt snapshot via `snapshotEvent` eller registry‑id.
4. Välj `Stop Mode` (ALLOWFADEOUT rekommenderas).

### AudioDucking (token‑baserad ducking)
- Placera `AudioDucking` i scenen.
- Sätt `target = Music` (eller Master/Sfx/UI).
- Anropa i kod:
  - `AudioDucking.I.Begin("VO", 0.6f);`  // duck 60%
  - `AudioDucking.I.End("VO");`
  - `AudioDucking.I.Pulse("UI", 0.3f, 0.1f);`

Obs: för alias (Master/Music/Sfx/UI) multipliceras ducking ovanpå sparad volym i `AudioSystem`.

---

## 7) Debug & verktyg

### AudioDebugOverlay
- Lägg `AudioDebugOverlay` i testscenen.
- Toggle med `F1` (ändras i Inspector).
- Visar: basvolym, duck, effektiva volymer, aktiva loopar och globala params.

### FmodTimelineEvents
- Lägg `FmodTimelineEvents` på ett GameObject.
- Sätt `evt` till ett event som har markers/beats.
- Koppla `onMarker`/`onBeat` till UnityEvents.

### AudioKitValidator
- Meny: **AudioKit → Verktyg → Validera AudioKit**
- Kontrollerar Resources‑assets och vanliga misstag (duplicerade keys/ids, tomma paths).
