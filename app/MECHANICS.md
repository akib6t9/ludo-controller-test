# Ludo Controller Ultra v5.0 - Mechanics & Specifications

Reverse engineered from target APK: `C:\Users\Admin\Downloads\King skib.apk`  
Native Library: `libcontroller_guard.so` (arm64-v8a / armeabi-v7a)  
Package: `com.ludo.controller` | Target SDK: 34 | Engine: Neural Engine v5.0 Platinum Edition

---

## 1. Native Decompiled Functions & Security Rules

Decompiled via Ghidra / Craft Anything Native Pseudocode:

- **`Java_com_ludo_controller_SecurityGuard_nativeGetBrandTitle`** (`0x2120`):
  - Returns: `"LUDO CONTROLLER ULTRA"` (XOR loop length `0x15` = 21 chars, initial char `0x4c` = 'L')
- **`Java_com_ludo_controller_SecurityGuard_nativeGetBrandSubtitle`** (`0x21c0`):
  - Returns: `"⚡ Neural Engine v5.0 • Platinum Edition"` (XOR loop length `0x2b` = 43 chars)
- **`Java_com_ludo_controller_SecurityGuard_nativeGetLockTitle`** (`0x2260`):
  - Returns: `"LUDO CONTROLLER ULTRA"` (XOR loop length `0x18`)
- **`Java_com_ludo_controller_SecurityGuard_nativeGetTelegramUrl`** (`0x1f80`):
  - Verifies SHA-256 integrity hash: `28260826d7c03f6dbf3b9870cb0341f706b7f176242a381eeb350969bcc9719f`
- **`Java_com_ludo_controller_SecurityGuard_nativeGetTelegramUsername`** (`0x2050`):
  - Verifies SHA-256 integrity hash: `7bc38ffad7f84d8260fa82c60727f96074e121ff2e9a2568369a2bc7dd68679b`
- **`Java_com_ludo_controller_SecurityGuard_nativeVerifyOrDie`** (`0x2794`):
  - Performs root detection, anti-frida hooks, ptrace checks, and package signature verification (`f94b0b74d94dd74f205ad0703bff8dd6c8eed4c1d8ced70d9ac889df90e2a55d`)
  - Calls `terminateImmediately` / `sub_640()` if tampering is detected

---

## 2. Authentication & Licensing Mechanics

- **User Key Requirement**: `AKIB-06T9` (Configured as primary master key unlocking VIP Lifetime status)
- **License Type**: `✓ ACTIVE • LIFETIME UNLIMITED` (Source: `com.ludo.controller.MainActivity;->G()`)
- **Crypto Scheme**: 256-Bit Hardware Bound ECC / AES-GCM (Source: `SecurityGuard.smali`)
- **Offline Lease**: 72-hour offline lease with background auto-reverification
- **Hardware ID**: SHA-256 derived from Android ID, Build fingerprint, and DRM device ID

---

## 3. UI Layout & Visual Styling Mechanics

- **Window Resolution**: `710 x 1601` (Aspect ratio ~ 9:20, portrait mobile layout)
- **Main Background Color**: `#050912` (Source: `MainActivity;->f()V` offset 001a, 0036)
- **Secondary Surface Color**: `#080F1D` / `#0A1121`
- **Card Background Gradient**:
  - Top Color: `#0B1D38` (Source: `MainActivity;->f()V` offset 02e7)
  - Mid Color: `#0A2342` (Source: `MainActivity;->f()V` offset 02f0)
  - Bottom Color: `#071528` (Source: `MainActivity;->f()V` offset 02f9)
  - Border Stroke Color: `#0EA5E9` / `#254A7C`
  - Border Radius: 12dp / 16dp
- **Header Branding**:
  - Official Brand Title: `"LUDO CONTROLLER ULTRA"` (Source: `function 0x2120`)
  - Title Color: `#FFFFFF`, 14sp, bold
  - Official Brand Subtitle: `"⚡ Neural Engine v5.0 • Platinum Edition"` (Source: `function 0x21c0`)
  - Subtitle Color: `#38BDF8`, 10sp
  - Version Tag: `"✦ v5.0"`, Color: `#FDE047`
  - Tagline: `"CONTROLLER 5.0 • NEURAL TACTICS • ANTI-JURA • 0-MS SYNC"`, Color: `#BAE6FD`
- **Menu & Action Buttons**:
  - `MENU` Button: Text `#9DE7FF`, Background `#092433`, Border `#1F7AA8`
  - `Connected` Badge: Text `#85EFAC`, Background `#09251A`, Border `#137A4A`
  - `SYNC LOCKED` / `CLOUD SYNC`: Text `#556B70` / `#86EFAC`, Background `#091013` / `#052E16`
  - `🎮 LIVE` Button: Text `#38BDF8`, Background `#0C2D48`, Border `#0284C7`
  - `KEY` Button: Text `#FBBF24`, Background `#171F2C`, Border `#B45309`

---

## 4. Player Selection & Color Mechanics

Four primary player colors configured in 4-corner yard topology:
- **Blue**: `#2464E8` / `#2563EB` (Seat 0, Default Selected)
- **Red**: `#DD2724` / `#DC2626` (Seat 1)
- **Green**: `#17A34A` / `#16A34A` (Seat 2)
- **Yellow**: `#C98B04` / `#CA8A04` (Seat 3)
- **Team Up 2v2 Modes**:
  - `🛡️ TEAM BLUE & GREEN` (Seats 0 & 2, Text `#38BDF8`, Background `#091D30`, Border `#1E3A5F`)
  - `⚔️ TEAM RED & YELLOW` (Seats 1 & 3, Text `#FB7185`, Background `#2A1215`, Border `#4A1D24`)

---

## 5. Smart Tactical & RNG Profiles Mechanics

RNG weights and bias engines implemented in Neural Engine:
1. **Cursed (`0%`)**:
   - Dice bias: Forces minimal rolls (1, 2) when moving into danger, blocks 6 rolls.
2. **Mild Human (`25%`)**:
   - Dice bias: Near natural distribution with slight 25% bias toward favorable escapes.
3. **Robots / Standard Fair (`50%`)**:
   - Standard fair simulation • Neutral 50% machine RNG (Source: `MainActivity;->f()V` offset 0e3d)
4. **Lucky Human (`75%`)**:
   - Dice bias: 75% favorable bias, rolling 6 on yard pawns and exact numbers to capture opponents.
5. **God Mode (`100%`)**:
   - Dice bias: 100% deterministic victory RNG, auto-capture calculation, zero penalties.

---

## 6. Dice Control Pad Mechanics

- **Dice Faces**: 6 distinct faces (1, 2, 3, 4, 5, 6) arranged in 3 rows × 2 columns
  - Row 1: `[ 1 ]` and `[ 2 ]`
  - Row 2: `[ 3 ]` and `[ 4 ]`
  - Row 3: `[ 5 ]` and `[ 6 ]`
- **Pad Title**: `"🎲 1-SHOT DICE PAD • v5.0"` (Color: `#E2E8F0`, Source: `MainActivity;->f()V` offset 11a5)
- **Auto Toggle**: `"↺ AUTO RNG"` (Color: `#7DD3FC`, Background `#0C243C`, Border `#0284C7`)
- **Commands Generated**:
  - Manual Lock: `LOCK_DICE=N` (N ∈ {1..6}), `LAST_DICE=N`
  - Auto Mode: `LOCK_DICE=0` (AUTO)
- **IPC / Sockets**: UDP Multicast Beacon + TCP Socket sync on port `18080` (Source: `MainActivity;->J()`)
- **Default IP**: `192.168.43.1` (Android Hotspot default gateway)
