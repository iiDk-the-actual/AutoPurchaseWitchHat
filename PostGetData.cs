using GorillaNetworking;
using GorillaNetworking.Store;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPurchaseWitchHat
{
    [HarmonyPatch(typeof(BundleManager))]
    [HarmonyPatch("CheckIfBundlesOwned", MethodType.Normal)]
    public class PostGetData
    {
        private static void Postfix()
        {
            Plugin.ownsThing = CosmeticsController.instance.concatStringCosmeticsAllowed.Contains("LHAAJ.") ? 2 : 1;
            Plugin.Check();
        }
    }
}
