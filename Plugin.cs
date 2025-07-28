using BepInEx;
using GorillaNetworking;
using iiMenu.Notifications;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Utilla;

namespace AutoPurchaseWitchHat
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        static float lastPurchaseTime = 0f;
        static bool redeemed = false;
        public static int ownsThing = 0;
        static Plugin instance;

        static IEnumerator RedeemShinyRocks()
        {
            Task<GetPlayerData_Data> newSessionDataTask = KIDManager.TryGetPlayerData(true);

            while (!newSessionDataTask.IsCompleted)
                yield return null;
            if (newSessionDataTask.IsFaulted)
                redeemed = false;

            GetPlayerData_Data newSessionData = newSessionDataTask.Result;
            if (newSessionData.responseType == GetSessionResponseType.NOT_FOUND)
            {
                Task optInTask = KIDManager.Server_OptIn();

                while (!optInTask.IsCompleted)
                    yield return null;
                if (optInTask.IsFaulted)
                    redeemed = false;

                CosmeticsController.instance.GetCurrencyBalance();
            }
        }

        void Awake()
        {
            instance = this;
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void Update()
        {
            if (GorillaLocomotion.GTPlayer.Instance == null)
                return;
        }

        public static void Check()
        {
            if (ownsThing != 1)
                return;

            if (!redeemed)
            {
                redeemed = true;
                instance.StartCoroutine(RedeemShinyRocks());
            }

            lastPurchaseTime = Time.time + 5f;

            if (CosmeticsController.instance.currencyBalance >= 1500)
            {
                lastPurchaseTime = Time.time + 60f;
                PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
                {
                    ItemId = "LHAAJ.",
                    Price = 1500,
                    VirtualCurrency = CosmeticsController.instance.currencyName,
                    CatalogVersion = CosmeticsController.instance.catalog
                }, delegate (PurchaseItemResult result)
                {
                    if (result.Items.Count <= 0)
                        return;

                    ownsThing = 2;
                    CosmeticsController.instance.ProcessExternalUnlock("LHAAJ.", false, false);
                    CosmeticsController.instance.GetCurrencyBalance();
                }, null, null, null);
            }
        }
    }
}
