# SCENE TRANSITION AUDIO FIX - Setup Instructions

## Problem Solved:
- Audio hilang saat pindah dari Main Menu ke Settings scene
- BacksoundPlayer mati/rusak saat scene transition
- Volume settings tidak ter-apply di scene baru

## Solution Applied:

### 1. AudioSettingsManager.cs - Enhanced Scene Transition Handling
- **ReapplySettingsForNewScene()**: Re-apply settings saat scene baru loaded
- **EnsureBacksoundPlayerAlive()**: Protection untuk BacksoundPlayer
- **OnEnable()**: Auto-apply settings setiap kali AudioSettingsManager aktif
- **DebugCurrentState()**: Public method untuk debugging

### 2. SettingsSceneAudioProtector.cs - Settings Scene Protection
- Script khusus untuk Settings scene
- Memastikan audio tetap hidup saat masuk Settings
- Auto-restart musik jika berhenti
- Emergency AudioSettingsManager creation

## Setup Instructions:

### Step 1: Update AudioSettingsManager
- AudioSettingsManager.cs sudah di-update dengan scene transition protection
- Tidak perlu setup tambahan, akan otomatis bekerja

### Step 2: Add Settings Scene Protector
1. Di Settings scene, buat **GameObject kosong** baru
2. Rename menjadi **"AudioProtector"** 
3. Drag & drop script **SettingsSceneAudioProtector.cs** ke GameObject tersebut
4. Di Inspector, enable **"Enable Detailed Logs"** untuk debugging

### Step 3: Testing
1. **Main Menu**: Pastikan musik bermain
2. **Pindah ke Settings**: Cek Console log, harusnya muncul:
   ```
   [SETTINGS-PROTECTOR] Settings scene started, checking audio status...
   [SETTINGS-PROTECTOR] BacksoundPlayer OK - isPlaying: True, volume: X.XX
   [SETTINGS-PROTECTOR] Audio protection sequence completed
   ```
3. **Test Volume Slider**: Geser slider, volume harus berubah real-time
4. **Test Save**: Klik Save, settings harus tersimpan

### Step 4: Debugging Commands
Jika masih ada masalah, gunakan debugging:

1. **In AudioProtector GameObject**, klik **Right Click → Force Audio Protection**
2. **In Console**, panggil: `AudioSettingsManager.Instance.DebugCurrentState()`
3. **In AudioProtector GameObject**, klik **Right Click → Test Audio System**

## Expected Log Flow (Normal):
```
[AudioSettingsManager] Created new instance
[SETTINGS-PROTECTOR] Settings scene started, checking audio status...
[SETTINGS-PROTECTOR] Ensuring AudioSettingsManager exists...
[SETTINGS-PROTECTOR] Checking BacksoundPlayer status...
[SETTINGS-PROTECTOR] BacksoundPlayer OK - isPlaying: True, volume: 1
[SETTINGS-PROTECTOR] Force applying audio settings...
=== APPLYING SETTINGS TO AUDIO ===
[PROTECTION] BacksoundPlayer OK - isPlaying: True, volume: 1
[SETTINGS-PROTECTOR] Audio protection sequence completed
```

## Troubleshooting:

### Problem: "BacksoundPlayer instance is NULL"
**Solution**: 
- Pastikan BacksoundPlayer di Main Menu menggunakan `DontDestroyOnLoad(gameObject)` di Awake()
- Check Console untuk log "[SETTINGS-PROTECTOR] Found X BacksoundPlayer(s) in scene"

### Problem: Audio tidak bermain di Settings
**Solution**:
- Gunakan "Force Audio Protection" button di AudioProtector GameObject
- Check apakah BacksoundPlayer.clip masih ada
- Pastikan AudioSource.enabled = true

### Problem: Volume tidak berubah real-time
**Solution**:
- Check apakah AudioSource sedang isPlaying = true
- Pastikan tidak ada script lain yang override volume
- Gunakan "Test Audio System" untuk debug detail

## Files Modified:
- `AudioSettingsManager.cs` - Enhanced dengan scene transition protection
- `SettingsSceneAudioProtector.cs` - NEW - Protection script untuk Settings scene

## Result:
✅ Audio tetap hidup saat Main Menu → Settings  
✅ Volume berubah real-time tanpa restart  
✅ Settings tersimpan ke JSON dengan benar  
✅ Robust error handling dan debugging tools
