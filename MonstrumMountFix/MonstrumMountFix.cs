using BepInEx;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[BepInDependency("Therzie.Monstrum", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin("exo.MonstrumMountFix", "MonstrumMountFix", "1.0.0")]
public class MonstrumMountFix : BaseUnityPlugin
{
    private static Assembly monstrumAsm;
    private static Type srPatchType;
    private static FieldInfo ridingField;
    private static FieldInfo humanoidField;

    void Awake()
    {
        var harmony = new Harmony("exo.MonstrumMountFix");

        // Locate the Monstrum assembly by partial name
        monstrumAsm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name.IndexOf("Monstrum", StringComparison.OrdinalIgnoreCase) >= 0);
        if (monstrumAsm == null)
        {
            Logger.LogWarning("Monstrum assembly not found, disabling MonstrumMountFix");
            return;
        }

        // Cache the internal patch type and its fields
        srPatchType = monstrumAsm.GetType("Monstrum.StartRidingMountPatch_Monstrum");
        if (srPatchType == null)
        {
            Logger.LogWarning("StartRidingMountPatch_Monstrum type not found");
            return;
        }
        ridingField = srPatchType.GetField("RidingMountMonstrum", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        humanoidField = srPatchType.GetField("RidingHumanoid", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (ridingField == null || humanoidField == null)
        {
            Logger.LogWarning("Required Monstrum fields not found");
            return;
        }

        // Patch Monstrum's Postfix method
        var patchType = monstrumAsm.GetType("Monstrum.TwPlayerUpdateDoodadControlsPatch");
        var postfix = AccessTools.Method(patchType, "Postfix", new[] { typeof(Player) });
        if (postfix != null)
        {
            var prefix = new HarmonyMethod(
                typeof(MonstrumMountFix).GetMethod(nameof(Prefix_MonstrumPostfix), BindingFlags.Static | BindingFlags.NonPublic)
            );
            harmony.Patch(postfix, prefix: prefix);
            Logger.LogInfo("[MonstrumMountFix] Patched Monstrum dismount Postfix");
        }
        else
        {
            Logger.LogWarning("Monstrum Postfix(Player) not found");
        }
    }

    // Prefix before Monstrum's Postfix
    [HarmonyPriority(Priority.First)]
    private static bool Prefix_MonstrumPostfix(Player __instance)
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