using BepInEx;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[BepInDependency("Therzie.MonstrumDeepNorth", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin("exo.MonstrumDPMountFix", "MonstrumDPMountFix", "1.0.0")]
public class MonstrumDPMountFix : BaseUnityPlugin
{
    private static Assembly monstrumAsm;
    private static Type srPatchType;
    private static FieldInfo ridingField;
    private static FieldInfo humanoidField;

    void Awake()
    {
        var harmony = new Harmony("exo.MonstrumDPMountFix");

        // Locate the Monstrum assembly by partial name
        monstrumAsm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name.IndexOf("MonstrumDeepNorth", StringComparison.OrdinalIgnoreCase) >= 0);
        if (monstrumAsm == null)
        {
            Logger.LogWarning("MonstrumDeepNorth assembly not found, disabling MonstrumDPMountFix");
            return;
        }

        // Cache the internal patch type and its fields
        srPatchType = monstrumAsm.GetType("MonstrumDeepNorth.StartRidingMountPatch_MonstrumDeepNorth");
        if (srPatchType == null)
        {
            Logger.LogWarning("StartRidingMountPatch_MonstrumDeepNorth type not found");
            return;
        }
        ridingField = srPatchType.GetField("RidingMountMonstrumDeepNorth", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        humanoidField = srPatchType.GetField("RidingHumanoid", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (ridingField == null || humanoidField == null)
        {
            Logger.LogWarning("Required MonstrumDeepNorth fields not found");
            return;
        }

        // Patch Monstrum's Postfix method
        var patchType = monstrumAsm.GetType("MonstrumDeepNorth.TwPlayerUpdateDoodadControlsPatch");
        var postfix = AccessTools.Method(patchType, "Postfix", new[] { typeof(Player) });
        if (postfix != null)
        {
            var prefix = new HarmonyMethod(
                typeof(MonstrumDPMountFix).GetMethod(nameof(Prefix_MonstrumDeepNorthPostfix), BindingFlags.Static | BindingFlags.NonPublic)
            );
            harmony.Patch(postfix, prefix: prefix);
            Logger.LogInfo("[MonstrumDPMountFix] Patched MonstrumDeepNorth dismount Postfix");
        }
        else
        {
            Logger.LogWarning("MonstrumDeepNorth Postfix(Player) not found");
        }
    }

    // Prefix before MonstrumDeepNorth's Postfix
    [HarmonyPriority(Priority.First)]
    private static bool Prefix_MonstrumDeepNorthPostfix(Player __instance)
    {
        // Ensure we're on a Monstrum mount
        bool isMonstrum = (bool)ridingField.GetValue(null);
        if (isMonstrum)
        {
            // Detect jump input
            if (ZInput.GetButtonDown("Jump") || ZInput.GetButton("Jump") || ZInput.GetButtonDown("JoyJump"))
            {
                // Perform safe jump
                var humanoid = humanoidField.GetValue(null) as Humanoid;
                humanoid?.Jump(false);

                // Skip Monstrum's CustomAttachStop
                return false;
            }
        }
        return true;
    }
}