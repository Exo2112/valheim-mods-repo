# valheim-mods-repo

This repository provides two lightweight BepInEx plugins that fix the automatic dismount behavior when jumping on Therzie Monstrum mounts in Valheim.

---

## MonstrumMountFix
**Purpose:** Prevents being thrown off any mount added by the base Therzie Monstrum mod when you press Jump.

**Installation:**
1. Drop `MonstrumMountFix.dll` into `Valheim/BepInEx/plugins/`.
2. Make sure the core **Monstrum** mod is installed.
3. Launch Valheim; you will no longer dismount when jumping.

---

## MonstrumDPMountFix (DeepNorth)
**Purpose:** Applies the same "safe jump" fix for the **DeepNorth** expansion of Monstrum, covering mounts unique to that variant.

**Installation:**
1. Drop `MonstrumDPMountFix.dll` into `Valheim/BepInEx/plugins/`.
2. Ensure **MonstrumDeepNorth** is installed.
3. Launch Valheim; DeepNorth mounts will no longer dismount on jump.

---

*Both fixes are independent—install the one that matches your Monstrum version.*