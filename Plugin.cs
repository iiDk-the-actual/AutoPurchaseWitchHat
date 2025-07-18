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
        float lastPurchaseTime = 0f;
        bool redeemed = false;

        IEnumerator RedeemShinyRocks()
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

        void Update()
        {
            if (GorillaLocomotion.GTPlayer.Instance == null)
                return;

            if (!GorillaComputer.instance.isConnectedToMaster)
                lastPurchaseTime = Time.time + 5f;

            if (Time.time > lastPurchaseTime && GorillaComputer.instance.isConnectedToMaster)
            {
                if (!redeemed)
                {
                    redeemed = true;
                    StartCoroutine(RedeemShinyRocks());
                }

                lastPurchaseTime = Time.time + 60f;

                if (!VRRig.LocalRig.concatStringOfCosmeticsAllowed.Contains("LHAAJ.") && CosmeticsController.instance.currencyBalance >= 2000)
                {
                    PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
                    {
                        ItemId = "LHAAJ.",
                        Price = 2000,
                        VirtualCurrency = CosmeticsController.instance.currencyName,
                        CatalogVersion = CosmeticsController.instance.catalog
                    }, delegate (PurchaseItemResult result)
                    {
                        // iiMenu.Notifications.NotifiLib.SendNotification("I bought the witch hat for you buddy");
                        CosmeticsController.instance.ProcessExternalUnlock("LHAAJ.", false, false);
                    }, null, null, null);
                }
            }
        }
    }
}
