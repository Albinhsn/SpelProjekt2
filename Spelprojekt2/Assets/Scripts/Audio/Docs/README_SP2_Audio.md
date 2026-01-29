# SP2 Audio System (FMOD + Unity) — ALL

Det här paketet är byggt för **Spelprojekt 2** med en tydlig uppdelning:
- **Ludwig (musik):** 1 persistent musik‑instance + state/intensity/danger med valfri **quantize på takt 1**.
- **Arvid (ljud):** återanvändbara **AudioCue**‑assets, one‑shots, loopar/ambience, triggers, UI‑ljud, stingers/jingles.

Målet är att ni ska slippa:
- sätta upp audio i varje scen
- duplicera FMOD EventReferences i massa prefabs
- få log‑spam från saknade params/events

---

## 0 scene‑setup
Audio skapas automatiskt innan första scenen laddas via `AudioBootstrapper`.

Ni behöver bara skapa **en** config‑asset:
- `Assets/Resources/Audio/AudioConfig.asset`

Skapas via meny:
- **SP2/Audio/Tools → Create AudioConfig (Resources/Audio)**

---

## Install (5 min)
1. Kopiera mappen `SP2_AudioSystem` in i Unity‑projektet.
2. Öppna **SP2/Audio/Tools** och klicka **Create AudioConfig (Resources/Audio)**.
3. Fyll i i `AudioConfig`:
   - `mainMusicEvent` (loopande musik‑event)
   - VCA paths: `vca:/Music`, `vca:/SFX`, `vca:/UI`
   - `bus:/` (Master bus)
   - Param‑namn (om ni använder global params/zoner)
4. Skapa **AudioCue** assets:
   - Create → **SP2/Audio/Audio Cue**
   - Lägg dem t.ex. i `Assets/Audio/Cues/`

---

## Användning (snabba exempel)
### Musik
```csharp
AudioSystem.Instance.Music.SetState(MusicState.Combat);
AudioSystem.Instance.Music.SetIntensity(0.65f);
AudioSystem.Instance.Music.SetDanger(1.0f);
```

### SFX one‑shot
```csharp
AudioSystem.Instance.Sfx.Play(cue, transform.position);
AudioSystem.Instance.Sfx.Play2D(uiClickCue);
```

### Loop/ambience
- Lägg **AudioEmitter** på objektet
- Sätt en AudioCue (loop‑event)
- Kryssa i `Play On Enable` om den ska starta direkt

### UI‑ljud (utan extra kod)
- Lägg **AudioUIButtonSfx** på en UI‑Button (eller valfri UI‑GameObject med EventSystem)
- Sätt click/hover cues

### Footsteps (valfritt)
1. Skapa `FootstepLibrary.asset` via **SP2/Audio/Tools → Create FootstepLibrary (Resources/Audio)**
2. Lägg **FootstepController** på spelaren.
3. (Rekommenderat) Lägg **FootstepSurface** på mark-colliders (eller på en parent och låt `Affect Children` vara på).
4. Fyll i cues i FootstepLibrary per `SurfaceType`.

---

## Vad som ingår
### Core
- `AudioSystem` (root + access)
- `AudioBootstrapper` (autoload)
- `AudioConfigSO` + `AudioResources`
- `AudioMixer` (Master/Music/SFX/UI + pause snapshot)
- `MusicDirector` (persistent musik + cached param IDs + quantize)
- `SfxDirector` (one‑shots + cooldown)
- `AudioCueSO` (återanvändbara event‑assets)
- `AudioEmitter` (attachade loopar, preload)
- `AudioParameterDriver` + `AudioParameterZone` (prio + fade + overlap‑safe)
- `AudioOneShotTrigger`
- `AudioVolumeSlider`

### Gameplay hooks (nya, renare ersättare för gamla kursens scripts)
- `AudioActionSetSO` + `AudioActionRunner` (ersätter “AudioSceneSettings/AudioTrigger”)
- `CombatAudioState`
- `ApproachDistanceParamDriver`
- `BossAudioDriver`
- `ComboStingerPlayer`
- `SpawnJinglePlayer`
- `SnapshotPulse`
- `GameOverAudioDriver`
- `PauseAudioBridge`

### Footsteps
- `FootstepLibrarySO` (1 asset i Resources)
- `FootstepController` (auto-distance eller animation events)
- `FootstepSurface` + `FootstepResolver` (yta via collider/parent)
- `SurfaceType` (enum)

### Extras
- `AudioUIButtonSfx` (UI click/hover)
- `AudioCueOnStateEnterSMB` (Animator StateMachineBehaviour → spelar cue)
- `AudioDebugOverlay` (F9 för overlay)

### Editor
- Tools‑fönster: **SP2/Audio/Tools**
- Custom inspector för `AudioConfigSO` och `AudioCueSO`

---

## Vanliga misstag (snabbcheck)
- **Inga VCA:er hittas:** dubbelkolla att paths matchar FMOD (t.ex. `vca:/Music`).
- **Inget 3D‑ljud:** se till att en `StudioListener` finns (systemet försöker lägga den på `Camera.main`).
- **Quantize funkar inte:** musik‑eventet måste ha tempo/timeline så `TIMELINE_BEAT` triggar.

