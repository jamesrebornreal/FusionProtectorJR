using BoneLib;
using FusionProtectorByJamesReborn;
using Harmony;
using HarmonyLib;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Bonelab.SaveData;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.AI;
using Il2CppSLZ.Marrow.Circuits;
using Il2CppSLZ.Marrow.Combat;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.PuppetMasta;
using Il2CppSLZ.Marrow.SaveData;
using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.VFX;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppSystem.IO;
using Il2CppWebSocketSharp;
using LabFusion.Data;
using LabFusion.Downloading;
using LabFusion.Downloading.ModIO;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Marrow.Extenders;
using LabFusion.Marrow.Messages;
using LabFusion.Marrow.Pool;
using LabFusion.Marrow.Proxies;
using LabFusion.Marrow.Serialization;
using LabFusion.Menu;
using LabFusion.MonoBehaviours;
using LabFusion.Network;
using LabFusion.Permissions;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.RPC;
using LabFusion.Safety;
using LabFusion.Scene;
using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Modules;
using LabFusion.SDK.Points;
using LabFusion.Senders;
using LabFusion.Utilities;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using static FusionProtectorByJamesReborn.Main;
using static FusionProtectorByJamesReborn.Main.CreateCheatToolsPreset;
using static LabFusion.RPC.NetworkAssetSpawner;
using static LabFusion.Safety.TrustedListManager;
using AccessTools = HarmonyLib.AccessTools;
using Action = System.Action;
using AmmoInventory = Il2CppSLZ.Marrow.AmmoInventory;
using Application = UnityEngine.Application;
using ArgumentException = System.ArgumentException;
using Color = UnityEngine.Color;
using Convert = System.Convert;
using Enum = System.Enum;
using Environment = System.Environment;
using EventType = UnityEngine.EventType;
using Exception = System.Exception;
using File = System.IO.File;
using FunctionElement = LabFusion.Marrow.Proxies.FunctionElement;
using GameObject = UnityEngine.GameObject;
using Hand = Il2CppSLZ.Marrow.Hand;
using HarmonyMethod = HarmonyLib.HarmonyMethod;
using HarmonyPatch = HarmonyLib.HarmonyPatch;
using HarmonyPrefix = HarmonyLib.HarmonyPrefix;
using HarmonyPriority = HarmonyLib.HarmonyPriority;
using IEnumerator = System.Collections.IEnumerator;
using JsonSerializer = System.Text.Json.JsonSerializer;
using LobbyInfo = LabFusion.Data.LobbyInfo;
using Menu = BoneLib.BoneMenu.Menu;
using MethodType = HarmonyLib.MethodType;
using NotificationType = LabFusion.UI.Popups.NotificationType;
using Page = BoneLib.BoneMenu.Page;
using Path = System.IO.Path;
using Random = System.Random;
using Screen = UnityEngine.Screen;
using StringComparer = System.StringComparer;
using StringComparison = System.StringComparison;
using StringSplitOptions = System.StringSplitOptions;
using Uri = System.Uri;
using UriKind = System.UriKind;

// READ ME
// - The code may appear messy in places, and that's fair. This project evolved over roughly six months of continuous development, experimentation, and iteration. Unlike previous projects I worked on with a teammate, this one was primarily built by me, so maintaining perfect structure often took a back seat to rapidly implementing protections and responding to new exploits.
// - Yes, AI was used throughout development. I understand the code I'm writing and don't rely on AI as a replacement for programming knowledge. Instead, I use it as a development tool to improve productivity, accelerate updates, and refine implementations, much like any other modern developer tool.
// - If this project is ever released, I hope others enjoy exploring it as much as I enjoyed building it. The goal of this mod has always been to identify and mitigate abusive behavior and client-related exploits within Fusion, making the experience more enjoyable and secure for legitimate players.
// - I have no issue with others learning from this project or creating their own forks or spinoffs. My hope is that Fusion eventually adopts many of the ideas implemented here, because the entire reason this project exists is to improve the game and protect its community.
// - During development I met many incredible people, several of whom I still talk to today. More than anything, I hope this project encourages other developers to focus their skills on creating protections, improving security, and contributing positively to the community rather than developing malicious tools.
// - Finally, there is intentionally nothing in this code that can be repurposed into an abusive client. Every exploit addressed by this project is paired with corresponding protections, meaning copying code from here provides little value for anyone attempting to create malicious software.
// - To the developers who intend to use this project for legitimate purposes: Feel free to learn from it, reuse parts of it, or build upon it. All I ask is that you give proper credit where it's due as a sign of respect for the time and effort that went into creating it.


[assembly: MelonInfo(typeof(FusionProtectorByJamesReborn.Main), FusionProtectorInfo.ClientName, FusionProtectorInfo.Version, FusionProtectorInfo.ClientCreator)]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
namespace FusionProtectorByJamesReborn
{
    internal static class FusionProtectorInfo
    {

        public const string ClientName = "Fusion Protector";
        public const string ClientCreator = "James Reborn";
        public const string Version = "1.48.95";

    }
    internal static class GameObjectExtensions
    {
        public static void DestroyNow(this GameObject target) => GameObject.DestroyImmediate(target, true);
        public static void ToggleActive(this GameObject target) => target?.SetActive(!target.activeSelf);
        public static bool IsTooClose(this GameObject target, Transform closetoobject, float meters)
        {
            if (target == null)
                return false;


            if (closetoobject == null)
                return false;

            float distance = Vector3.Distance(closetoobject.position, target.transform.position);

            return distance < meters;
        }
        public static AIBrain JR_GetNPCAIBrain(this GameObject entity) => entity?.GetComponentInChildren<AIBrain>();
        public static bool JR_IsNPC(this GameObject entity) => entity?.GetComponentInChildren<AgentLinkControl>() != null || entity?.GetComponentInChildren<AIBrain>() != null;
        public static bool JR_HasPoolee(this GameObject entity) => entity?.GetComponentInChildren<Poolee>() != null;
        public static Poolee JR_GetPoolee(this GameObject entity) => entity?.GetComponentInChildren<Poolee>();
        public static bool JR_IsWeapon(this GameObject entity) => entity?.GetComponentInChildren<Gun>() != null || entity?.GetComponentInChildren<Gun>() != null;
        public static bool JR_IsMelee(this GameObject entity) => entity?.GetComponentInChildren<StabSlash>() != null || entity?.GetComponentInChildren<StabSlash>() != null;

    }
    internal static class HandExtensions
    {
        public static SpawnGun JR_HandGrabbedSpawnGun(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponentInChildren<SpawnGun>();
        public static bool JR_IsGrabbedSpawnGun(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponentInChildren<SpawnGun>() != null;
        public static FlyingGun JR_HandGrabbedNimbusGun(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<FlyingGun>();
        public static bool JR_IsGrabbedNimbusGun(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<FlyingGun>() != null;
        public static AIBrain JR_IsHandGrabbedNPCAIBrain(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<AIBrain>();
        public static bool JR_IsHandGrabbingNPC(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<AIBrain>() != null;
        public static bool JR_IsHandGrabbingAnyNetPlayer(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<RigManager>() != null;
        public static bool JR_IsHandGrabbingNetPlayer(this Hand Handnow, NetworkPlayer PlayerNow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<RigManager>() == PlayerNow?.RigRefs?.RigManager;
        public static bool JR_IsHandGrabbingYou(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<RigManager>() == Player.RigManager;
        public static GameObject JR_GetAttachedObject(this Hand Handnow) => Handnow?.m_CurrentAttachedGO;
        public static MarrowEntity JR_GetMarrowEntity(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<MarrowEntity>();
        public static Gun JR_GetGun(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<Gun>();
        public static bool JR_HasGun(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<Gun>() != null;
        public static StabSlash JR_GetMelee(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<StabSlash>();
        public static bool JR_HasMelee(this Hand Handnow) => Handnow?.JR_GetAttachedObject()?.transform?.root?.GetComponent<StabSlash>() != null;

    }
    internal static class NetworkEntityExtensions
    {

        public static void JR_Despawn(this NetworkEntity entity) => DespawnNow(entity);
        public static bool JR_IsNetPlayer(this NetworkEntity entity) => entity?.JR_GetMarrowEntity() != null && (entity.JR_GetMarrowEntity()?.JR_IsNetPlayer() ?? false);
        public static bool JR_IsMagazine(this NetworkEntity entity)
        {
            if (entity == null)
                return false;

            var marrow = entity.JR_GetMarrowEntity();
            if (marrow == null)
                return false;

            var go = marrow.gameObject;
            if (go == null)
                return false;

            Magazine mag = null;
            try
            {
                mag = go.GetComponent<Magazine>();
            }
            catch
            {
                return false;
            }

            if (mag == null)
                return false;

            bool isGun = false;
            try
            {
                isGun = entity.JR_IsGun();
            }
            catch
            {
                isGun = false;
            }

            return !isGun;
        }
        public static bool JR_IsGun(this NetworkEntity entity) => entity?.JR_GetMarrowEntity()?.gameObject?.GetComponent<Gun>() != null;
        public static bool JR_IsMelee(this NetworkEntity entity) => entity?.JR_GetMarrowEntity()?.gameObject?.GetComponent<StabSlash>() != null;
        public static bool JR_IsNPC(this NetworkEntity entity) => entity?.JR_GetMarrowEntity()?.gameObject?.GetComponent<AIBrain>() != null;
        public static Gun JR_GetGun(this NetworkEntity entity) => entity?.JR_GetMarrowEntity()?.gameObject?.GetComponent<Gun>();
        public static StabSlash JR_GetMelee(this NetworkEntity entity) => entity?.JR_GetMarrowEntity()?.gameObject?.GetComponent<StabSlash>();
        public static AIBrain JR_GetNPCAIBrain(this NetworkEntity entity) => entity?.JR_GetMarrowEntity()?.gameObject?.GetComponent<AIBrain>();
        public static MarrowEntity JR_GetMarrowEntity(this NetworkEntity entity) => entity?.GetExtender<IMarrowEntityExtender>()?.MarrowEntity;
    }
    internal static class MarrowEntityExtensions
    {
        public static AIBrain JR_GetNPCAIBrain(this MarrowEntity entity) => entity?.gameObject?.GetComponent<AIBrain>();
        public static string JR_GetBarcodeID(this MarrowEntity entity) => entity?._poolee?._SpawnableCrate_k__BackingField?.Barcode?.ID ?? "NULL";
        public static bool JR_IsNetPlayer(this MarrowEntity entity) => entity?.gameObject?.transform?.root?.GetComponent<AntiHasher>() != null;
    }
    internal static class NetworkPlayerExtensions
    {
        public static bool IsMe(this NetworkPlayer player)
        {
            return player.JR_SteamID() == SteamIdYours();
        }
        public static PullCordDevice JR_PlayersBodyLog(this NetworkPlayer player)
        {
            Transform? right = player?.JR_PlayersPhysicsRig()?.m_elbowRt?.Find("BodyLogSlot/BodyLog");
            if (right != null)
            {
                var device = right.GetComponent<PullCordDevice>();
                if (device != null)
                    return device;
            }

            Transform? left = player?.JR_PlayersPhysicsRig()?.m_elbowLf?.Find("BodyLogSlot/BodyLog");
            if (left != null)
            {
                var device = left.GetComponent<PullCordDevice>();
                if (device != null)
                    return device;
            }

            return null;
        }
        public static bool JR_IsGrabbingNPC(this NetworkPlayer player, WhichHand hand)
        {
            return hand switch
            {
                WhichHand.Left => player?.JR_GetObjectInHand(WhichHand.Left)?.transform?.root?.GetComponent<AIBrain>() != null,
                WhichHand.Right => player?.JR_GetObjectInHand(WhichHand.Right)?.transform?.root?.GetComponent<AIBrain>() != null,
                WhichHand.Both =>
                    player?.JR_GetObjectInHand(WhichHand.Left)?.transform?.root?.GetComponent<AIBrain>() != null
                    || player?.JR_GetObjectInHand(WhichHand.Right)?.transform?.root?.GetComponent<AIBrain>() != null,
                _ => false
            };
        }
        public static PuppetMaster JR_GetGrabbedPuppetMaster(this NetworkPlayer player, WhichHand hand) => player?.JR_GetMarrowEntityInHand(hand)?.JR_GetNPCAIBrain()?.puppetMaster;
        public static bool JR_IsGrabbingSelf(this NetworkPlayer player, WhichHand Hand)
        {
            return Hand switch
            {
                WhichHand.Left => player?.JR_GetObjectInHand(WhichHand.Left)?.transform?.root?.GetComponent<RigManager>() == player?.RigRefs?.RigManager,
                WhichHand.Right => player?.JR_GetObjectInHand(WhichHand.Right)?.transform?.root?.GetComponent<RigManager>() == player?.RigRefs?.RigManager,
                WhichHand.Both =>
                    player?.JR_GetObjectInHand(WhichHand.Left)?.transform?.root?.GetComponent<RigManager>() == player?.RigRefs?.RigManager
                    || player?.JR_GetObjectInHand(WhichHand.Right)?.transform?.root?.GetComponent<RigManager>() == player?.RigRefs?.RigManager,
                _ => false
            };
        }
        public static bool JR_IsGrabbingAnyThing(this NetworkPlayer player, WhichHand Hand)
        {
            return Hand switch
            {
                WhichHand.Left => player?.JR_GetObjectInHand(WhichHand.Left) != null,
                WhichHand.Right => player?.JR_GetObjectInHand(WhichHand.Right) != null,
                WhichHand.Both =>
                    player?.JR_GetObjectInHand(WhichHand.Left) != null
                    || player?.JR_GetObjectInHand(WhichHand.Right) != null,
                _ => false
            };
        }
        public static MarrowEntity JR_GetMarrowEntityInHand(this NetworkPlayer player, WhichHand Hand)
        {
            return Hand switch
            {
                WhichHand.Left => player?.JR_GetObjectInHand(WhichHand.Left)?.gameObject?.transform?.root?.gameObject?.GetComponent<MarrowEntity>(),
                WhichHand.Right => player?.JR_GetObjectInHand(WhichHand.Right)?.gameObject?.transform?.root?.gameObject?.GetComponent<MarrowEntity>(),
                _ => null
            };
        }
        public static GameObject JR_GetObjectInHand(this NetworkPlayer player, WhichHand Hand)
        {
            return Hand switch
            {
                WhichHand.Left => player?.JR_GetHand(WhichHand.Left)?.m_CurrentAttachedGO,
                WhichHand.Right => player?.JR_GetHand(WhichHand.Right)?.m_CurrentAttachedGO,
                _ => null
            };
        }
        public static NetworkPlayer JR_GrabbedPlayer(this NetworkPlayer player, WhichHand hand)
        {
            var handObj = hand switch
            {
                WhichHand.Left => player?.JR_GetHand(WhichHand.Left),
                WhichHand.Right => player?.JR_GetHand(WhichHand.Right),
                _ => null
            };

            if (handObj?.m_CurrentAttachedGO == null)
                return null;

            var grabbedRig = handObj.m_CurrentAttachedGO.transform?.root?.GetComponent<RigManager>();
            if (grabbedRig == null)
                return null;

            return NetworkPlayerManager.TryGetPlayer(grabbedRig, out var networkPlayer) ? networkPlayer : null;
        }
        public static bool JR_IsGrabbingAnyNetPlayer(this NetworkPlayer player, WhichHand Hand)
        {
            return Hand switch
            {
                WhichHand.Left => player?.JR_GetObjectInHand(WhichHand.Left)?.transform.root.GetComponent<RigManager>() != null,
                WhichHand.Right => player?.JR_GetObjectInHand(WhichHand.Right)?.transform.root.GetComponent<RigManager>() != null,
                WhichHand.Both =>
                    player?.JR_GetObjectInHand(WhichHand.Left)?.transform.root.GetComponent<RigManager>() != null
                    || player?.JR_GetObjectInHand(WhichHand.Right)?.transform.root.GetComponent<RigManager>() != null,
                _ => false
            };
        }
        public static bool JR_IsGrabbingYou(this NetworkPlayer player, WhichHand hand)
        {
            if (player?.JR_PlayersPhysicsRig() == null || Player.RigManager == null)
                return false;

            return hand switch
            {
                WhichHand.Left => player?.JR_GetObjectInHand(WhichHand.Left)?.transform?.root?.GetComponent<RigManager>() == Player.RigManager,
                WhichHand.Right => player?.JR_GetObjectInHand(WhichHand.Right)?.transform?.root?.GetComponent<RigManager>() == Player.RigManager,
                WhichHand.Both =>
                    (player?.JR_GetObjectInHand(WhichHand.Left)?.transform?.root?.GetComponent<RigManager>() == Player.RigManager) ||
                    (player?.JR_GetObjectInHand(WhichHand.Right)?.transform?.root?.GetComponent<RigManager>() == Player.RigManager),
                _ => false
            };
        }
        public static Transform JR_PlayersHead(this NetworkPlayer player) => player?.JR_PlayersPhysicsRig()?.m_head;
        public static PhysicsRig JR_PlayersPhysicsRig(this NetworkPlayer player) => player?.RigRefs?.RigManager?.physicsRig;
        public static SerializedAvatarStats JR_SerializedAvatarStats(this NetworkPlayer player)
        {
            var avatar = player?.RigRefs?.RigManager?.avatar;
            if (avatar == null)
                return null;

            return new SerializedAvatarStats(avatar);
        }
        public static string JR_PlayersAvatarBarcodeID(this NetworkPlayer player) => player?.RigRefs?.RigManager?.AvatarCrate?.Barcode?.ID ?? "NULL";
        public static byte JR_SmallID(this NetworkPlayer player) => (byte)(player?.PlayerID?.SmallID ?? -1);
        public static ulong JR_SteamID(this NetworkPlayer player) => player?.PlayerID?.PlatformID ?? 0;
        public static int JR_AvatarMODIOID(this NetworkPlayer player) => player?.PlayerID?.Metadata?.AvatarModID?.GetValue() ?? 0;
        public static string JR_Username(this NetworkPlayer player) => player?.PlayerID?.Metadata?.Username?.GetValue() ?? "NULL";
        public static string JR_Nickname(this NetworkPlayer player) => player?.PlayerID?.Metadata?.Nickname?.GetValue() ?? "NULL";
        public static string JR_Description(this NetworkPlayer player) => player?.PlayerID?.Metadata?.Description?.GetValue() ?? "NULL";
        public static string JR_PermissionLevel(this NetworkPlayer player) => player?.PlayerID?.Metadata?.PermissionLevel?.GetValue() ?? "NULL";
        public static Hand JR_GetHand(this NetworkPlayer player, WhichHand hand)
        {
            if (player?.JR_PlayersPhysicsRig() == null)
                return null;

            return hand switch
            {
                WhichHand.Left => player.JR_PlayersPhysicsRig().leftHand,
                WhichHand.Right => player.JR_PlayersPhysicsRig().rightHand,
                _ => null
            };
        }
        public static bool JR_IsHoldingBarcode(this NetworkPlayer player, WhichHand hand, string barcode)
        {
            if (player?.JR_PlayersPhysicsRig() == null)
                return false;

            return hand switch
            {
                WhichHand.Left => player?.JR_GetObjectInHand(WhichHand.Left)?.transform?.root?.GetComponent<MarrowEntity>()?.JR_GetBarcodeID() == barcode,
                WhichHand.Right => player?.JR_GetObjectInHand(WhichHand.Right)?.transform?.root?.GetComponent<MarrowEntity>()?.JR_GetBarcodeID() == barcode,
                WhichHand.Both =>
                    (player?.JR_GetObjectInHand(WhichHand.Left)?.transform?.root?.GetComponent<MarrowEntity>()?.JR_GetBarcodeID() == barcode) ||
                    (player?.JR_GetObjectInHand(WhichHand.Right)?.transform?.root?.GetComponent<MarrowEntity>()?.JR_GetBarcodeID() == barcode),
                _ => false
            };
        }
    }
    internal static class BarCodeIDExtensions
    {
        public static string JR_BarcodeCrateName(this string idnow)
        {
            if (IsAvatarCrateExist(idnow))
                return StripColorTags(new AvatarCrateReference(idnow)?.Crate?.name);

            if (IsLevelCrateExist(idnow))
                return StripColorTags(new LevelCrateReference(idnow)?.Crate?.name);

            if (IsSpawnableCrateExist(idnow))
                return StripColorTags(new SpawnableCrateReference(idnow)?.Crate?.name);

            return "NULL";
        }

        public static string JR_BarcodePalletName(this string idnow)
        {
            if (IsAvatarCrateExist(idnow))
                return StripColorTags(new AvatarCrateReference(idnow)?.Crate?.Pallet?.name);

            if (IsLevelCrateExist(idnow))
                return StripColorTags(new LevelCrateReference(idnow)?.Crate?.Pallet?.name);

            if (IsSpawnableCrateExist(idnow))
                return StripColorTags(new SpawnableCrateReference(idnow)?.Crate?.Pallet?.name);

            return "NULL";
        }

        public static string JR_BarcodeAuthor(this string idnow)
        {
            if (IsAvatarCrateExist(idnow))
                return StripColorTags(new AvatarCrateReference(idnow)?.Crate?.Pallet?.Author);

            if (IsLevelCrateExist(idnow))
                return StripColorTags(new LevelCrateReference(idnow)?.Crate?.Pallet?.Author);

            if (IsSpawnableCrateExist(idnow))
                return StripColorTags(new SpawnableCrateReference(idnow)?.Crate?.Pallet?.Author);

            return "NULL";
        }


    }
    internal static class PageEx
    {

        public static System.Collections.Generic.List<(string entry, string page, string name, float value)> floatslogged = new();
        public static System.Collections.Generic.List<(string entry, string page, string name, int value)> intslogged = new();
        public static System.Collections.Generic.List<(string entry, string page, string name, Enum value)> enumvaluelogged = new();
        public static System.Collections.Generic.List<(string entry, string page, string name, string value)> stringslogged = new();
        public static System.Collections.Generic.List<(string entry, string page, string name, bool value)> boolslogged = new();


        public static BoneLib.BoneMenu.BoolElement Logsettings(
            this Page Page,
            string name,
            Color color,
            ref bool startingBool,
            System.Action<bool> ActionNow)
        {
            string entry = $"Page({Page.Name})Option={name}:{startingBool}";
            boolslogged.Add((entry, Page.Name, name, startingBool));

            boolslogged = boolslogged
                .GroupBy(x => (x.page, x.name))
                .Select(g => g.Last())
                .ToList();

            if (File.Exists(Main.ProtectorSettings))
            {
                var line = File.ReadAllLines(Main.ProtectorSettings)
                    .FirstOrDefault(l => l.StartsWith($"Page({Page.Name})Option={name}:"));

                if (!string.IsNullOrEmpty(line))
                {
                    var (_, val) = Main.ParseLine(line);
                    if (bool.TryParse(val, out var parsed))
                    {
                        startingBool = parsed;

                        var i = boolslogged.FindIndex(x => x.page == Page.Name && x.name == name);
                        if (i != -1)
                            boolslogged[i] = ($"Page({Page.Name})Option={name}:{parsed}", Page.Name, name, parsed);
                    }
                }
            }

            return Page.CreateBool(name, color, startingBool, newVal =>
            {
                boolslogged.RemoveAll(x => x.page == Page.Name && x.name == name);

                ActionNow?.Invoke(newVal);

                boolslogged.Add(($"Page({Page.Name})Option={name}:{newVal}", Page.Name, name, newVal));

                if (Main.togglesavesbool && File.Exists(Main.ProtectorSettings))
                    ManuallySave(false);
            });
        }
        public static BoneLib.BoneMenu.FloatElement Logsettingsfloat(
            this Page Page,
            string name,
            Color color,
            ref float startingValue,
            float increment,
            float minValue,
            float maxValue,
            System.Action<float> ActionNow)
        {
            string entry = $"Page({Page.Name})Option={name}:{startingValue}";
            floatslogged.Add((entry, Page.Name, name, startingValue));

            floatslogged = floatslogged
                .GroupBy(x => (x.page, x.name))
                .Select(g => g.Last())
                .ToList();

            if (File.Exists(Main.ProtectorSettings))
            {
                var line = File.ReadAllLines(Main.ProtectorSettings)
                    .FirstOrDefault(l => l.StartsWith($"Page({Page.Name})Option={name}:"));

                if (!string.IsNullOrEmpty(line))
                {
                    var (_, val) = Main.ParseLine(line);
                    if (float.TryParse(val, out var parsed))
                    {
                        startingValue = parsed;
                        var i = floatslogged.FindIndex(x => x.page == Page.Name && x.name == name);
                        if (i != -1)
                            floatslogged[i] = ($"Page({Page.Name})Option={name}:{parsed}", Page.Name, name, parsed);
                    }
                }
            }

            return Page.CreateFloat(name, color, startingValue, increment, minValue, maxValue, newVal =>
            {
                floatslogged.RemoveAll(x => x.page == Page.Name && x.name == name);

                ActionNow?.Invoke(newVal);

                floatslogged.Add(($"Page({Page.Name})Option={name}:{newVal}", Page.Name, name, newVal));

                if (Main.togglesavesbool && File.Exists(Main.ProtectorSettings))
                    ManuallySave(false);
            });
        }
        public static BoneLib.BoneMenu.IntElement Logsettingsint(
            this Page Page,
            string name,
            Color color,
            ref int startingValue,
            int increment,
            int minValue,
            int maxValue,
            System.Action<int> ActionNow)
        {
            string entry = $"Page({Page.Name})Option={name}:{startingValue}";
            intslogged.Add((entry, Page.Name, name, startingValue));

            intslogged = intslogged
                .GroupBy(x => (x.page, x.name))
                .Select(g => g.Last())
                .ToList();

            if (File.Exists(Main.ProtectorSettings))
            {
                var line = File.ReadAllLines(Main.ProtectorSettings)
                    .FirstOrDefault(l => l.StartsWith($"Page({Page.Name})Option={name}:"));

                if (!string.IsNullOrEmpty(line))
                {
                    var (_, val) = Main.ParseLine(line);
                    if (int.TryParse(val, out var parsed))
                    {
                        startingValue = parsed;
                        var i = intslogged.FindIndex(x => x.page == Page.Name && x.name == name);
                        if (i != -1)
                            intslogged[i] = ($"Page({Page.Name})Option={name}:{parsed}", Page.Name, name, parsed);
                    }
                }
            }

            return Page.CreateInt(name, color, startingValue, increment, minValue, maxValue, newVal =>
            {
                intslogged.RemoveAll(x => x.page == Page.Name && x.name == name);

                ActionNow?.Invoke(newVal);

                intslogged.Add(($"Page({Page.Name})Option={name}:{newVal}", Page.Name, name, newVal));

                if (Main.togglesavesbool && File.Exists(Main.ProtectorSettings))
                    ManuallySave(false);
            });
        }
        public static BoneLib.BoneMenu.EnumElement LogsettingsEnum<T>(
            this Page Page,
            string name,
            Color color,
            ref T startingValue,
            System.Action<T> ActionNow) where T : Enum
        {
            string entry = $"Page({Page.Name})Option={name}:{startingValue}";
            enumvaluelogged.Add((entry, Page.Name, name, startingValue));

            enumvaluelogged = enumvaluelogged
                .GroupBy(x => (x.page, x.name))
                .Select(g => g.Last())
                .ToList();

            if (File.Exists(Main.ProtectorSettings))
            {
                var line = File.ReadAllLines(Main.ProtectorSettings)
                    .FirstOrDefault(l => l.StartsWith($"Page({Page.Name})Option={name}:"));

                if (!string.IsNullOrEmpty(line))
                {
                    var (_, val) = Main.ParseLine(line);
                    try
                    {
                        var parsed = (T)Enum.Parse(typeof(T), val, true);
                        startingValue = parsed;
                        var i = enumvaluelogged.FindIndex(x => x.page == Page.Name && x.name == name);
                        if (i != -1)
                            enumvaluelogged[i] = ($"Page({Page.Name})Option={name}:{parsed}", Page.Name, name, parsed);
                    }
                    catch { }
                }
            }

            return Page.CreateEnum(name, color, startingValue, newVal =>
            {
                var typed = (T)newVal;

                enumvaluelogged.RemoveAll(x => x.page == Page.Name && x.name == name);

                ActionNow?.Invoke(typed);

                enumvaluelogged.Add(($"Page({Page.Name})Option={name}:{typed}", Page.Name, name, typed));

                if (Main.togglesavesbool && File.Exists(Main.ProtectorSettings))
                    ManuallySave(false);
            });
        }
        public static BoneLib.BoneMenu.StringElement LogsettingsString(
            this Page Page,
            string name,
            Color color,
            ref string startingValue,
            System.Action<string> ActionNow)
        {
            if (File.Exists(Main.ProtectorSettings))
            {
                var line = File.ReadAllLines(Main.ProtectorSettings)
                    .FirstOrDefault(l => l.StartsWith($"Page({Page.Name})Option={name}:"));

                if (!string.IsNullOrEmpty(line))
                    startingValue = line.Substring(line.IndexOf(':') + 1);
            }

            stringslogged.RemoveAll(x => x.page == Page.Name && x.name == name);
            stringslogged.Add(($"Page({Page.Name})Option={name}:{startingValue}", Page.Name, name, startingValue));

            return Page.CreateString(name, color, startingValue, newVal =>
            {
                stringslogged.RemoveAll(x => x.page == Page.Name && x.name == name);

                ActionNow?.Invoke(newVal);

                stringslogged.Add(($"Page({Page.Name})Option={name}:{newVal}", Page.Name, name, newVal));

                if (Main.togglesavesbool)
                    ManuallySave(false);
            });
        }
    }
    internal class TimedObject
    {
        public static System.Collections.Generic.Dictionary<object, TimedObject> ActiveTimers = new();

        public object Value;

        private readonly Stopwatch Timer;
        private readonly TimeSpan Duration;

        public TimedObject(object value, int minutes)
        {
            Value = value;
            Duration = TimeSpan.FromMinutes(minutes);

            Timer = Stopwatch.StartNew();

            ActiveTimers[value] = this;
        }

        public bool IsExpired()
        {
            bool expired = Timer.Elapsed >= Duration;

            if (expired)
            {
                ActiveTimers.Remove(Value);
            }

            return expired;
        }

        public TimeSpan TimeLeft()
        {
            return Duration - Timer.Elapsed;
        }

    }

    public class Main : MelonMod
    {

        internal class FusionProtectorMediaPlayerProtections
        {
            public string Link { get; set; }
            public string Reason { get; set; }
            public FusionProtectorMediaPlayerProtections(
                string link,
                string REASON)
            {
                Link = link;
                Reason = REASON;
            }

            public FusionProtectorMediaPlayerProtections() { }
        }
        internal class SpoofChecker
        {
            public string Username { get; set; }
            public ulong PlatformID { get; set; }
        }
    
        internal class AISpawner
        {
            public readonly string InstanceID;
            public System.Action<object> _spawnAction;
            public float _spawnIntervalMins = 1.0f;
            public object _spawnRoutine;
            public string BarcodeID;
            public string NameOfUsed;
            public Vector3 LocationOfSpawner;
            public System.Collections.Generic.HashSet<NetworkEntity> NetworkEntitiesMP = new();
            public NetworkEntity LastSpawnedEntityMP;
            public GameObject LastSpawnedEntitySP;
            public System.Collections.Generic.HashSet<GameObject> SinglePlayerEntites = new();
            public string SpawnerMapLockedTo;
            public int MaxSpawnsInSpawner = 10;
            public bool despawndeads = true;
            public bool onlyspawnifalldead = false;
            public bool eachspawnisrandom = false;
            public RandomizerType eachspawnrandomtype;

            public bool IsRunning => _spawnRoutine != null;
            public static AISpawner CurrentInstance;

            public AISpawner(System.Action<object> spawnAction, float spawnIntervalMins)
            {
                FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var yourcurrentpermlevel, out _);

                if (yourcurrentpermlevel != PermissionLevel.OWNER && !NetworkInfo.IsHost || !NetworkInfo.HasServer) // Not host
                    throw new System.InvalidOperationException("Cannot do AISpawner without owner permission.");

                _spawnAction = spawnAction ?? throw new System.ArgumentNullException(nameof(spawnAction));
                _spawnIntervalMins = spawnIntervalMins;
                InstanceID = System.Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                CurrentInstance = this;
            }

            public AISpawner Start(string Barcodeid, Vector3 locationOfSpawner, int maxEntitiesFromThisSpawner, RandomizerType randomizertypenow = RandomizerType.AllNPCs)
            {
                StopAndClear(false);

                if (eachspawnisrandom)
                {
                    eachspawnrandomtype = randomizertypenow;
                    BarcodeID = GetRandomByType(eachspawnrandomtype);
                    NameOfUsed = "Randomized Spawner";
                }
                else
                {
                    if (!IsBarcodeInGame(Barcodeid))
                    {
                        MelonLogger.Warning($"[Spawner] Barcode '{Barcodeid}' does not exist in game. Spawner not started.");
                        NotificationNow(FusionProtectorInfo.ClientName, $"[Spawner] Barcode '{Barcodeid}' does not exist in game. Spawner not started.", NotificationType.ERROR, 3f);
                        return null;
                    }

                    BarcodeID = Barcodeid ?? throw new System.ArgumentNullException(nameof(Barcodeid));
                    var crateRef = new SpawnableCrateReference(BarcodeID);
                    NameOfUsed = StripColorTags(crateRef?.Crate?.Title) ?? "Unknown";
                }

                LocationOfSpawner = locationOfSpawner;
                MaxSpawnsInSpawner = maxEntitiesFromThisSpawner;
                SpawnerMapLockedTo = SceneStreamer.Session?.Level?.Barcode?.ID;


                _spawnRoutine = MelonCoroutines.Start(SpawnEveryXMins());

                MelonLogger.Warning($"[Spawner {CurrentInstance.InstanceID}] Started spawner for '{NameOfUsed}' ({BarcodeID}) at {LocationOfSpawner}");
                NotificationNow(FusionProtectorInfo.ClientName, $"Spawner created for {NameOfUsed} at your current location!", NotificationType.SUCCESS, 3f);

                return this;
            }

            public void Pause(bool notification = true)
            {
                if (_spawnRoutine == null)
                    return;

                try
                {
                    MelonCoroutines.Stop(_spawnRoutine);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[Spawner] Error pausing coroutine: {ex.Message}");
                }

                _spawnRoutine = null;

                if (notification)
                {
                    MelonLogger.Warning($"[Spawner {CurrentInstance.InstanceID}] Paused spawner (can be resumed).");
                    NotificationNow(FusionProtectorInfo.ClientName, $"Paused Spawner {CurrentInstance.NameOfUsed} | ID: {CurrentInstance.InstanceID}", NotificationType.SUCCESS, 3f);
                }
            }

            public void StopAndClear(bool notification = true)
            {
                if (_spawnRoutine != null)
                    Pause(false);

                BarcodeID = null;
                LocationOfSpawner = Vector3.zero;
                NameOfUsed = string.Empty;
                NetworkEntitiesMP.Clear();
                SinglePlayerEntites.Clear();

                if (notification)
                    MelonLogger.Warning($"[Spawner {CurrentInstance.InstanceID}] Stopped and cleared instance.");
            }

            public void Resume(bool notification = true)
            {
                if (_spawnRoutine != null)
                {
                    MelonLogger.Error($"[Spawner {CurrentInstance.InstanceID}] Already running.");
                    return;
                }

                if (string.IsNullOrEmpty(BarcodeID))
                {
                    MelonLogger.Error($"[Spawner {CurrentInstance.InstanceID}] Cannot resume: NPC info missing.");
                    return;
                }

                _spawnRoutine = MelonCoroutines.Start(SpawnEveryXMins());

                if (notification)
                {
                    MelonLogger.Warning($"[Spawner {CurrentInstance.InstanceID}] Resumed spawner.");
                    NotificationNow(FusionProtectorInfo.ClientName, $"Resuming Spawner {CurrentInstance.NameOfUsed} | ID: {CurrentInstance.InstanceID}", NotificationType.SUCCESS, 3f);
                }
            }
            public void UpdateSpawnInterval(float newSpawnIntervalMins)
            {
                if (_spawnIntervalMins == newSpawnIntervalMins)
                    return;

                _spawnIntervalMins = newSpawnIntervalMins;

                if (_spawnRoutine != null)
                {
                    MelonCoroutines.Stop(_spawnRoutine);
                    _spawnRoutine = MelonCoroutines.Start(SpawnEveryXMins());
                }

                MelonLogger.Warning($"[Spawner {InstanceID}] Spawn interval updated to {_spawnIntervalMins} minutes.");
            }


            public void UpdateMaxSpawns(int newMaxEntities)
            {
                if (MaxSpawnsInSpawner == newMaxEntities)
                    return;

                MaxSpawnsInSpawner = newMaxEntities;

                MelonLogger.Warning($"[Spawner {InstanceID}] Max spawns updated to {MaxSpawnsInSpawner}.");
            }



            public static IEnumerator RunTaskAsCoroutine(Task task)
            {
                while (!task.IsCompleted)
                    yield return null;

                if (task.IsFaulted)
                    throw task.Exception!;
            }


            public IEnumerator SpawnEveryXMins()
            {
                FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var yourcurrentpermlevel, out _);

                if (yourcurrentpermlevel != PermissionLevel.OWNER && !NetworkInfo.IsHost || !NetworkInfo.HasServer)
                    yield break;

                WaitForSeconds wait = new(_spawnIntervalMins);

                var levelCrateTemp = new LevelCrateReference(SpawnerMapLockedTo);
                MelonLogger.Warning($"[Spawner {InstanceID}] Started for {NameOfUsed}. Interval: {_spawnIntervalMins} seconds). Map locked to: {levelCrateTemp?.Scannable?.Title}");

                yield return RunTaskAsCoroutine(SpawnOnce(MaxSpawnsInSpawner));

                while (_spawnRoutine != null)
                {
                    yield return wait;

                    if (_spawnRoutine == null)
                        yield break;

                    yield return RunTaskAsCoroutine(SpawnOnce(MaxSpawnsInSpawner));
                }
            }

            public void KillAllInSpawner()
            {
                if (NetworkInfo.HasServer)
                {
                    foreach (var entity in NetworkEntitiesMP)
                    {
                        if (entity.JR_IsNPC() && !entity.JR_GetNPCAIBrain().isDead)
                        {
                            entity?.JR_GetMarrowEntity()?.JR_GetNPCAIBrain()?.puppetMaster?.Kill();
                        }
                    }

                }
                else
                {
                    foreach (var entity in SinglePlayerEntites)
                    {
                        if (entity.JR_IsNPC() && !entity.JR_GetNPCAIBrain().isDead)
                        {
                            entity?.gameObject?.GetComponent<AIBrain>()?.puppetMaster?.Kill();
                        }
                    }
                }
            }

            public async Task SpawnOnce(int maxEntitiesFromThisSpawner)
            {
                FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var yourcurrentpermlevel, out _);

                if (yourcurrentpermlevel != PermissionLevel.OWNER && !NetworkInfo.IsHost || !NetworkInfo.HasServer)
                    return;

                if (eachspawnisrandom)
                {
                    BarcodeID = GetRandomByType(eachspawnrandomtype);
                    NameOfUsed = "Randomized Spawner";
                }

                if (string.IsNullOrEmpty(BarcodeID) || _spawnAction == null)
                    return;

                try
                {
                    if (SceneStreamer.Session?.Level?.Barcode?.ID != SpawnerMapLockedTo)
                        return;

                    if (NetworkInfo.HasServer)
                    {
                        foreach (var entity in NetworkEntitiesMP.ToList())
                        {
                            if (entity == null || entity.JR_GetMarrowEntity() == null)
                                NetworkEntitiesMP.Remove(entity);
                            else if (despawndeads && entity.JR_IsNPC() && entity.JR_GetNPCAIBrain().isDead)
                            {

                                var despawninfo = new DespawnRequestInfo
                                {
                                    DespawnEffect = true,
                                    EntityID = entity.ID

                                };
                                NetworkAssetSpawner.Despawn(despawninfo);

                                NetworkEntitiesMP.Remove(entity);

                            }
                        }

                        if (onlyspawnifalldead && NetworkEntitiesMP.Any(e => e.JR_IsNPC() && !e.JR_GetNPCAIBrain().isDead))
                            return;

                        if (NetworkEntitiesMP.Count >= maxEntitiesFromThisSpawner)
                            return;

                        var (netties, _, _, _) = await SpawnersSpawner(BarcodeID, LocationOfSpawner, Player.Head.rotation, false, false, false);
                        if (netties != null)
                        {
                            NetworkEntitiesMP.Add(netties);
                            LastSpawnedEntityMP = netties;
                            _spawnAction?.Invoke(netties);
                        }
                    }
                    else
                    {
                        foreach (var entity in SinglePlayerEntites.ToList())
                        {
                            if (entity == null || entity.gameObject == null)
                                SinglePlayerEntites.Remove(entity);
                            else if (despawndeads && entity.JR_IsNPC() && entity.JR_GetNPCAIBrain().isDead)
                            {
                                entity.JR_GetPoolee()?.Despawn();
                                SinglePlayerEntites.Remove(entity);
                            }
                        }

                        if (onlyspawnifalldead && SinglePlayerEntites.Any(e => e.JR_IsNPC() && !e.JR_GetNPCAIBrain().isDead))
                            return;

                        if (SinglePlayerEntites.Count >= maxEntitiesFromThisSpawner)
                            return;

                        var (_, _, gameobby, _) = await SpawnersSpawner(BarcodeID, LocationOfSpawner, Player.Head.rotation, false, false, false);
                        if (gameobby != null)
                        {
                            SinglePlayerEntites.Add(gameobby);
                            LastSpawnedEntitySP = gameobby;
                            _spawnAction?.Invoke(gameobby);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[Spawner {CurrentInstance.InstanceID}] Exception during spawn: {ex}");
                }
            }
        }
        internal class SimpleTimer
        {
            public System.Action? _codenow;
            public float _mins;
            public object? _coroutine;

            private bool _quicker;
            private float _quickerSeconds;

            private bool _running;

            public SimpleTimer(System.Action codenow, float mins)
            {
                _codenow = codenow ?? throw new System.ArgumentNullException(nameof(codenow));
                _mins = mins;
            }

            public SimpleTimer Start(bool quicker = false, float quickerseconds = 10f)
            {
                Stop();

                _quicker = quicker;
                _quickerSeconds = quickerseconds;

                _running = true;
                _coroutine = MelonCoroutines.Start(RunEveryXMins());

                return this;
            }

            public void Stop()
            {
                _running = false;

                if (_coroutine != null)
                {
                    try
                    {
                        MelonCoroutines.Stop(_coroutine);
                    }
                    catch { }

                    _coroutine = null;
                }
            }

            public void Refresh(System.Action? newAction = null, float? newMins = null)
            {
                Stop();

                if (newAction != null)
                    _codenow = newAction;

                if (newMins.HasValue)
                    _mins = newMins.Value;

                Start(_quicker, _quickerSeconds);

                MelonLogger.Warning("Timer refreshed, first execution will happen after interval.");
            }

            public System.Collections.IEnumerator RunEveryXMins()
            {
                while (_running)
                {
                    float waitTime = _quicker ? _quickerSeconds : _mins * 60f;

                    yield return new WaitForSecondsRealtime(waitTime);

                    if (!_running)
                        yield break;

                    try
                    {
                        var action = _codenow;

                        if (action == null)
                            continue;

                        action.Invoke();
                    }
                    catch (System.NullReferenceException)
                    {
                        MelonLogger.Warning("Timer skipped null Unity reference (object destroyed).");
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Error($"Timer error: {ex}");
                    }
                }
            }
        }
        internal static class SiteStuff
        {
            public static System.Collections.Generic.HashSet<string> mostusedexcl = new();
            public static System.Collections.Generic.HashSet<int> globalblocklistmodioid = new();

            public static System.Collections.Generic.HashSet<int> globalaviblocklistmodioid = new();
            public static System.Collections.Generic.HashSet<string> globalaviblocklistavatar = new();
            public static System.Collections.Generic.HashSet<string> globalaviblocklistpallet = new();
            public static System.Collections.Generic.HashSet<string> globalaviblocklistauthor = new();


            public static System.Collections.Generic.HashSet<string> globalblocklistspawnable = new();
            public static System.Collections.Generic.HashSet<string> globalblocklistpallet = new();
            public static System.Collections.Generic.HashSet<string> globalblocklistauthor = new();
            public static System.Collections.Generic.HashSet<string> globalblocklistavatar = new();
            public static System.Collections.Generic.HashSet<string> blockednsfw = new();
            public static System.Collections.Generic.HashSet<FusionProtectorMediaPlayerProtections> MediaPlayerProtectionnow = new();
            public static string custommediadoms;

            public static string VersionChecking;
            public static string GlobalBanCheckinglink;
            public static string AdditonWhitelistMediaPlayer;
            public static string MediaBlocker;
            public static string NSFWBlocker;
            public static string GlobalFPBlacklist;
            public static string mostusedthings;

            private static readonly JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            public static void CreateBackupAndStore(string fileName, string data)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    throw new ArgumentException("Filename cannot be null or empty.", nameof(fileName));

                data ??= string.Empty;

                string baseDir = Path.Combine(FusionProtectorFiles, "FusionProtectorPasteBackUps");

                System.IO.Directory.CreateDirectory(baseDir);

                string filePath = Path.Combine(baseDir, fileName);

                try
                {
                    File.WriteAllText(filePath, data);
                    MelonLogger.Msg($"Backup stored successfully: {fileName}");
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to store backup '{fileName}': {ex.Message}");
                }
            }


            public static void FetchPastesForFetchers()
            {


                MelonCoroutines.Start(ReadFromSite(
                    "http://tiny.cc/pastelinks",
                    (sitetext) =>
                    {
                        CreateBackupAndStore("FusionProtectorPasteLinks.txt", sitetext);


                        foreach (string line in sitetext.Split(
                            new[] { "\r\n", "\n" },
                            System.StringSplitOptions.RemoveEmptyEntries))
                        {
                            var trimmed = line.Trim();
                            if (!trimmed.Contains(":"))
                                continue;

                            var parts = trimmed.Split(new[] { ':' }, 2);
                            var left = parts[0].Trim().ToLowerInvariant();
                            var right = parts[1].Trim();

                            switch (left)
                            {
                                case "versionchecking":
                                    VersionChecking = right;
                                    break;

                                case "globalbancheckinglink":
                                    GlobalBanCheckinglink = right;
                                    break;

                                case "additonwhitelistmediaplayer":
                                    AdditonWhitelistMediaPlayer = right;
                                    break;

                                case "mediablocker":
                                    MediaBlocker = right;
                                    break;

                                case "nsfwblocker":
                                    NSFWBlocker = right;
                                    break;

                                case "globalfpblacklist":
                                    GlobalFPBlacklist = right;
                                    break;

                                case "mostuseditemsexclude":
                                    mostusedthings = right;
                                    break;



                                default:
                                    MelonLogger.Warning($"Unknown key: {left}");
                                    break;
                            }
                        }

                        MelonLogger.Warning("Fetched All Paste Links!!!");
                    }
                ));
            }

            public static void FetchMostItemsUsedExclude()
            {
                mostusedexcl.Clear();

                MelonCoroutines.Start(ReadFromSite(mostusedthings, (sitetext) =>
                {
                    CreateBackupAndStore("FusionProtectormostusedthingsexclude.txt", sitetext);

                    var excludesnow = new System.Collections.Generic.HashSet<string>(
                        sitetext.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    );

                    mostusedexcl = excludesnow;

                    MelonLogger.Warning($"Total Fusion Protector Spawn Delay Excluded Items : {mostusedexcl.Count}");

                }));




            }

            public static void FetchVersionChecker()
            {
                MelonCoroutines.Start(ReadFromSite(VersionChecking, (sitetext) =>
                {
                    CreateBackupAndStore("FusionProtectorVersion.txt", sitetext);

                    if (FusionProtectorInfo.Version.Trim() != sitetext.Trim())
                    {
                        NotificationNow(FusionProtectorInfo.ClientName, $"Update {sitetext} Is Available\nJoin the Discord To Download Update!", NotificationType.ERROR, 3.0f, true, true, () => { OpenPageNow("https://tinyurl.com/jamesreborn"); });
                    }
                    //MelonLogger.Warning("Checked Fusion Protector Version!!!");
                }));
            }
            public static void GlobalBanChecking()
            {
                MelonCoroutines.Start(ReadFromSite(GlobalBanCheckinglink, (siteText) =>
                {

                    GlobalBanList banList;
                    try
                    {
                        banList = JsonSerializer.Deserialize<GlobalBanList>(siteText, options);
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Failed to deserialize GlobalBanList: {ex.Message}");
                        return;
                    }

                    MelonLogger.Warning($"Total Fusion Global Bans: {banList.Bans.Count}");

                    var myBan = banList.Bans.FirstOrDefault(b =>
                        b?.Platforms != null &&
                        b.Platforms.Any(p => p.PlatformID == SteamIdYours())
                    );

                    if (myBan != null)
                    {
                        string message =
                            $"You're Fusion Global Banned!\n" +
                            $"User: {myBan.Username}\n" +
                            $"Reason: {myBan.Reason}";

                        MelonLogger.Error(message);

                        NotificationNow(
                            FusionProtectorInfo.ClientName,
                            message,
                            NotificationType.ERROR,
                            10.0f
                        );
                    }
                    else
                    {
                        MelonLogger.Warning($"✅ You're Not Banned! {SteamIdYours()}");
                    }
                }));
            }
            public static void FetchGlobalBlockList()
            {

                globalblocklistspawnable.Clear();
                globalblocklistmodioid.Clear();
                globalblocklistpallet.Clear();
                globalblocklistauthor.Clear();


                globalblocklistavatar.Clear();
                globalaviblocklistmodioid.Clear();
                globalaviblocklistavatar.Clear();
                globalaviblocklistpallet.Clear();
                globalaviblocklistauthor.Clear();


                MelonCoroutines.Start(ReadFromSite(GlobalFPBlacklist, sitetext =>
                {
                    CreateBackupAndStore("FusionProtectorGlobalBlacklist.txt", sitetext);


                    foreach (var line in sitetext.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                            continue;

                        if (!trimmed.Contains(':'))
                        {
                            continue;
                        }

                        var parts = trimmed.Split(':', 2);
                        var left = parts[0].Trim().ToLowerInvariant();
                        var right = parts[1].Trim();

                        if (left == "spawnable")
                        {
                            if (!globalblocklistspawnable.Contains(trimmed))
                                globalblocklistspawnable.Add(trimmed);
                        }
                        if (left == "spawnable_pallet")
                        {
                            if (!globalblocklistpallet.Contains(right))
                                globalblocklistpallet.Add(right);
                        }
                        else if (left == "spawnable_author")
                        {
                            if (!globalblocklistauthor.Contains(right))
                                globalblocklistauthor.Add(right);
                        }
                        else if (left == "spawnable_modid")
                        {
                            if (int.TryParse(right, out var id))
                            {
                                if (!globalblocklistmodioid.Contains(id))
                                    globalblocklistmodioid.Add(id);
                            }
                        }
                        else if (left == "avatar")
                        {
                            if (!globalaviblocklistavatar.Contains(right))
                                globalaviblocklistavatar.Add(right);
                        }
                        if (left == "avatar_pallet")
                        {
                            if (!globalaviblocklistpallet.Contains(right))
                                globalaviblocklistpallet.Add(right);
                        }
                        else if (left == "avatar_author")
                        {
                            if (!globalaviblocklistauthor.Contains(right))
                                globalaviblocklistauthor.Add(right);
                        }
                        else if (left == "avatar_modid")
                        {
                            if (int.TryParse(right, out var id))
                            {
                                if (!globalaviblocklistmodioid.Contains(id))
                                    globalaviblocklistmodioid.Add(id);
                            }
                        }


                    }

                    // MelonLogger.Warning("Updated/Completed Global Fusion Protector Blocklist Protection!!!");
                }));
            }
            public static void FetchNSFWBLOCKED()
            {
                blockednsfw.Clear();
                MelonCoroutines.Start(ReadFromSite(NSFWBlocker, (sitetext) =>
                {
                    CreateBackupAndStore("FusionProtectorNSFWBlocker.txt", sitetext);

                    foreach (string line in sitetext.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries))
                    {
                        string trimmed = line.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                            if (!blockednsfw.Contains(trimmed))
                            {
                                blockednsfw.Add(trimmed);
                            }
                    }
                    // MelonLogger.Warning("Updated/Completed NSFW Protections!!!");
                }));
            }
            public static void FetchMediaProtections()
            {
                MediaPlayerProtectionnow.Clear();
                MelonCoroutines.Start(ReadFromSite(MediaBlocker, (sitetext) =>
                {
                    CreateBackupAndStore("FusionProtectorMediaBlocker.txt", sitetext);
                    MediaPlayerProtectionnow = JsonSerializer.Deserialize<System.Collections.Generic.HashSet<FusionProtectorMediaPlayerProtections>>(sitetext, options);
                    //MelonLogger.Warning("Updated/Completed Media Player Protections!!!");
                }));
            }
            public static void FetchMediaDoms()
            {
                MelonCoroutines.Start(ReadFromSite(AdditonWhitelistMediaPlayer, (sitetext) =>
                {
                    CreateBackupAndStore("FusionProtectorAdditionalWhitelist.txt", sitetext);

                    custommediadoms = sitetext;

                    //MelonLogger.Warning("Updated/Completed Custom Media Domains!!!");
                }));
            }

            public static bool isUpdating = false;

            public static IEnumerator UpdateSites()
            {

                if (isUpdating)
                {
                    MelonLogger.Warning("Fusion Protector site update already running.");
                    yield break;
                }

                isUpdating = true;

                var steps = new (string Name, Action Action)[]
                {
        ("Fetching fusion protector pastes links", FetchPastesForFetchers),
        ("Fetching fusion protector NSFW block list", FetchNSFWBLOCKED),
        ("Fetching fusion protector media protections", FetchMediaProtections),
        ("Fetching fusion protector media domains", FetchMediaDoms),
        ("Fetching fusion protector global block list", FetchGlobalBlockList),
        ("Checking fusion protector version", FetchVersionChecker),
        ("Fetching fusion protector Spawn Delay Excluded Items", FetchMostItemsUsedExclude)
                };

                int totalSteps = steps.Length;

                for (int i = 0; i < totalSteps; i++)
                {
                    var step = steps[i];

                    MelonLogger.Warning($"[{i + 1}/{totalSteps}] {step.Name}...");

                    try
                    {
                        step.Action();
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Fusion Protector step failed: {step.Name}");
                        MelonLogger.Error(ex.ToString());
                    }

                    yield return new WaitForSeconds(3f);
                }

                MelonLogger.Warning("All Fusion Protector data successfully updated from sites.");

                isUpdating = false;
            }

            public static IEnumerator ReadFromSite(string url, Action<string> sitetextaction)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                        "AppleWebKit/537.36 (KHTML, like Gecko) " +
                        "Chrome/120.0.0.0 Safari/537.36"
                    );

                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("*/*")
                    );

                    Task<string> request = client.GetStringAsync(url);

                    while (!request.IsCompleted)
                        yield return null;

                    if (request.IsFaulted)
                    {
                        MelonLogger.Warning(
                            "Error fetching Site Text: " +
                            request.Exception +
                            $" url : {url}"
                        );

                        yield break;
                    }

                    try
                    {
                        sitetextaction?.Invoke(request.Result);
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning(
                            "Error processing Site Text: " +
                            ex.Message +
                            $" url : {url}"
                        );
                    }
                }
            }
            //

            //reading steamprofile data to prevent alts 
            public static void NotificationNowDifferent(string messagetype)
            {
                NotificationNow(FusionProtectorInfo.ClientName, messagetype, NotificationType.SUCCESS, 3.0f, true, true, () =>
                {
                    GUIUtility.systemCopyBuffer = messagetype;
                    NotificationNow(FusionProtectorInfo.ClientName, "Copied Details To Clipboard!", NotificationType.SUCCESS, 3.0f);
                });
            }

            public static IEnumerator RunNotificationThenKick(PlayerID playernow, string message, float messagetime, Action CodeNow)
            {
                if (HideFusionProtector)
                    yield break;

                var data = SendNotificationData.Create(
    PlayerIDManager.LocalSmallID,
    message,
    "Fusion Protetor Kick/Ban System",
    messagetime
);

                MessageRelay.RelayModule<SendNotificationMessage, SendNotificationData>(
                    data,
                    new MessageRoute(playernow.SmallID, NetworkChannel.Reliable)
                );

                yield return new WaitForSecondsRealtime(messagetime + 0.5f);

                CodeNow?.Invoke();
            }
            private static readonly System.Collections.Generic.Dictionary<ulong, bool> CheckedSteamIDs = new();

            public static async Task AltPrevention(ulong steamid)
            {
                if (!disablesteamreading || steamid == SteamIdYours())
                    return;

                if (CheckedSteamIDs.TryGetValue(steamid, out var isAlt))
                {
                    MelonLogger.Msg(steamid + ": Already Checked");

                    if (isAlt)
                    {
                        if (AltRemov && NetworkInfo.IsHost)
                        {
                            MelonLogger.Msg(steamid + ": Is A Alt");

                            var playerxz = NetworkPlayer.Players.FirstOrDefault(p => p.PlayerID.PlatformID == steamid);

                            try
                            {
                                if (!baninsteadalt)
                                {
                                    MelonCoroutines.Start(RunNotificationThenKick(playerxz.PlayerID,
                                        "Your Alt Account Was Detected On Fusion Protector, Kicking You From Lobby!",
                                        3.0f,
                                        () =>
                                        {
                                            NotificationNow(FusionProtectorInfo.ClientName,
                                                $"Alt Account Detected!\nKicking {steamid} {playerxz.PlayerID.Metadata.Nickname.GetValue()} [{playerxz.PlayerID.Metadata.Username.GetValue()}] ({playerxz.PlayerID.PlatformID})",
                                                NotificationType.ERROR,
                                                3.0f);
                                            NetworkHelper.KickUser(playerxz.PlayerID);
                                        }));
                                }
                                else
                                {
                                    if (!NetworkHelper.IsBanned(playerxz.PlayerID.PlatformID))
                                    {
                                        MelonCoroutines.Start(RunNotificationThenKick(playerxz.PlayerID,
                                            "Your Alt Account Was Detected On Fusion Protector, Banning You From Lobby!",
                                            3.0f,
                                            () =>
                                            {
                                                NotificationNow(FusionProtectorInfo.ClientName,
                                                    $"Alt Account Detected!\nBanning {steamid} {playerxz.PlayerID.Metadata.Nickname.GetValue()} [{playerxz.PlayerID.Metadata.Username.GetValue()}] ({playerxz.PlayerID.PlatformID})",
                                                    NotificationType.ERROR,
                                                    3.0f);
                                                NetworkHelper.BanUser(playerxz.PlayerID);
                                            }));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Msg(ex);
                            }
                        }
                    }

                    return;
                }

                CheckedSteamIDs[steamid] = false;

                try
                {
                    string steamname = string.Empty;
                    string accountlevel = string.Empty;
                    string region = string.Empty;
                    string accountage = string.Empty;
                    string status = string.Empty;
                    string worth = string.Empty;

                    string numberofgames = string.Empty;
                    string numberoffriends = string.Empty;
                    string recentplaytime = string.Empty;


                    string groups = string.Empty;
                    string vacBans = string.Empty;
                    string artwork = string.Empty;
                    string inventory = string.Empty;
                    string profileAwards = string.Empty;
                    string reviews = string.Empty;
                    string badges = string.Empty;

                    FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var yourcurrentpermlevel, out _);
                    var activeLobbyInfo = LobbyInfoManager.LobbyInfo;
                    var player = NetworkPlayer.Players.FirstOrDefault(p => p.PlayerID.PlatformID == steamid);
                    if (player == null)
                        return;

                    var site1 = await ReadFromSiteAsync($"https://steamcommunity.com/profiles/{steamid}");
                    var extracted1 = ExtractOther(site1);
                    steamname = StripColorTags(extracted1.steamname);
                    accountlevel = extracted1.level;
                    numberofgames = extracted1.games;
                    numberoffriends = extracted1.friends;
                    recentplaytime = extracted1.recentPlaytime;
                    groups = extracted1.groups;
                    vacBans = extracted1.vacBans;
                    artwork = extracted1.artwork;
                    inventory = extracted1.inventory;
                    profileAwards = extracted1.profileAwards;
                    reviews = extracted1.reviews;
                    badges = extracted1.badges;

                    var site2 = await ReadFromSiteAsync($"https://steamid.pro/lookup/{steamid}");
                    var extracted2 = ExtractPlayerInfo(site2);
                    region = extracted2.region;
                    accountage = extracted2.accountage;
                    status = extracted2.status;
                    worth = extracted2.accountworth;

                    var joiningUsername = StripColorTags(player.PlayerID.Metadata.Username.GetValue());

                    var joiningnickname = StripColorTags(player.PlayerID.Metadata.Nickname.GetValue());

                    MelonLogger.Msg("Fusion Info:\n" +
                    (player.PlayerID.Metadata.Nickname.GetValue() != "?" ? $"Nickname : {player.PlayerID.Metadata.Nickname.GetValue()}\n" : "") +
                    (joiningUsername != "?" ? $"Username : {joiningUsername}\n\n" : "\n") +
                    "Steam Account Info:\n" +
                    (steamname != "?" ? $"Name : {steamname}\n" : "") +
                    (accountlevel != "?" ? $"Level : {accountlevel}\n" : "") +
                    (region != "?" ? $"Region : {region}\n" : "") +
                    (accountage != "?" ? $"Account Age : {accountage}\n" : "") +
                    (numberofgames != "?" ? $"Number of Games : {numberofgames}\n" : "") +
                    (numberoffriends != "?" ? $"Number of Friends : {numberoffriends}\n" : "") +
                    (recentplaytime != "?" ? $"Recent Playtime : {recentplaytime}\n" : "") +
                    (vacBans != "?" ? $"VAC Bans : {vacBans}\n" : "") +
                    (profileAwards != "?" ? $"Profile Awards : {profileAwards}\n" : "") +
                    (badges != "?" ? $"Badges : {badges}\n" : "") +
                    (artwork != "?" ? $"Artwork : {artwork}\n" : "") +
                    (inventory != "?" ? $"Inventory : {inventory}\n" : "") +
                    (reviews != "?" ? $"Reviews : {reviews}\n" : "") +
                    (status != "?" ? $"Status : {status}\n" : "") +
                    (worth != "$1" ? $"Account Worth : {worth}" : "")
    );

                    if (clonedetector && NetworkInfo.HasLayer)
                    {
                        var matchmaker = NetworkLayerManager.Layer.Matchmaker;
                        if (matchmaker != null)
                        {
                            try
                            {
                                var tcs = new TaskCompletionSource<LabFusion.Network.IMatchmaker.MatchmakerCallbackInfo>();
                                matchmaker.RequestLobbies(i => tcs.TrySetResult(i));
                                var info = await tcs.Task;

                                if (info.Lobbies == null)
                                    return;

                                foreach (var lobby in info.Lobbies)
                                {
                                    if (lobby.Metadata.LobbyInfo == null || lobby.Metadata.LobbyInfo == LobbyInfoManager.LobbyInfo)
                                        continue;

                                    foreach (var playerInLobby in lobby.Metadata.LobbyInfo.PlayerList.Players)
                                    {
                                        if (playerInLobby == null)
                                            continue;

                                        if (playerInLobby.PlatformID == player.PlayerID.PlatformID)
                                            continue;
                                        if (!string.IsNullOrEmpty(playerInLobby.Username))
                                        {
                                            if (playerInLobby.Username == joiningUsername)
                                            {
                                                NotificationNowDifferent($"Player is cloning username of another player => Cheater: [{joiningUsername}] ({player.PlayerID.PlatformID})");
                                                break;
                                            }
                                        }

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Msg($"Error while checking lobbies: {ex.Message}");
                            }
                        }
                    }

                    if (spooferprofiledetection && Encoding.UTF8.GetString(Encoding.Default.GetBytes(StripColorTags(player.PlayerID.Metadata.Username.GetValue()))).ToLower() != steamname.ToLower())
                    {
                        var sb = new StringBuilder()
         .AppendLine("Player Is Spoofing Profile:")
         .AppendLine("Spoofer's Current Account:")
         .AppendLine($"Steam : {steamname} {steamid}");

                        if (!string.IsNullOrEmpty(joiningnickname))
                            sb.AppendLine($"Nickname : {joiningnickname}");

                        sb.AppendLine($"Username : {joiningUsername}");

                        NotificationNow(
                            FusionProtectorInfo.ClientName,
                            sb.ToString(),
                            NotificationType.WARNING,
                            3.0f,
                            true,
                            true
                        );
                    }
                   
                    if (privatekicksteam && NetworkInfo.IsHost)
                    {
                        if (accountlevel == "?")
                        {
                            MelonCoroutines.Start(RunNotificationThenKick(player.PlayerID, "Steam Profile Is Private Fusion Protector Is Kicking You!", 3.0f, () => {
                                NotificationNow(FusionProtectorInfo.ClientName, $"Steam Profile Is Private Removing! : {joiningUsername}", NotificationType.ERROR, 3.0f);
                                NetworkHelper.KickUser(player.PlayerID);
                            }));

                        }
                        else
                        {
                            NotificationNow(FusionProtectorInfo.ClientName, $"Profile Is Private Can't Detect If It's A Alt Account! : {joiningUsername}", NotificationType.ERROR, 3.0f);
                        }
                    }

                    if (AltNotifications && accountage == "new")
                    {
                        NotificationNowDifferent($"Alt Account Detected!\nPossibly Cheater : {player.PlayerID.Metadata.Nickname.GetValue()} [{player.PlayerID.Metadata.Username.GetValue()}] ({player.PlayerID.PlatformID})");
                    }

                    if (teleportaltacctoyou && NetworkInfo.IsHost)
                    {
                        PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_ME, player.PlayerID.SmallID);
                    }
 
                    if (AltRemov && NetworkInfo.IsHost)
                    {
                        if (accountlevel == "0" || accountage == "new")
                        {
                            CheckedSteamIDs[steamid] = true;

                            try
                            {
                                if (!baninsteadalt)
                                {

                                    MelonCoroutines.Start(RunNotificationThenKick(player.PlayerID, "Your Alt Account Was Detected On Fusion Protector, Kicking You From Lobby!", 3.0f, () => {
                                        NotificationNow(FusionProtectorInfo.ClientName, $"Alt Account Detected!\nKicking {steamname} {player.PlayerID.Metadata.Nickname.GetValue()} [{player.PlayerID.Metadata.Username.GetValue()}] ({player.PlayerID.PlatformID})", NotificationType.ERROR, 3.0f);
                                        NetworkHelper.KickUser(player.PlayerID);
                                    }));

                                }
                                else
                                {
                                    if (!NetworkHelper.IsBanned(player.PlayerID.PlatformID))
                                    {

                                        MelonCoroutines.Start(RunNotificationThenKick(player.PlayerID, "Your Alt Account Was Detected On Fusion Protector, Banning You From Lobby!", 3.0f, () => {
                                            NotificationNow(FusionProtectorInfo.ClientName, $"Alt Account Detected!\nBanning {steamname} {player.PlayerID.Metadata.Nickname.GetValue()} [{player.PlayerID.Metadata.Username.GetValue()}] ({player.PlayerID.PlatformID})", NotificationType.ERROR, 3.0f);
                                            NetworkHelper.BanUser(player.PlayerID);
                                        }));

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Msg(ex);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    if (alterrornotis)
                    {
                        NotificationNow(FusionProtectorInfo.ClientName, $"AltPrevention ERROR: {ex.Message}", NotificationType.ERROR, 2.0f);
                    }
                    else
                    {
                        MelonLogger.Msg($"AltPrevention ERROR: {ex.Message}");
                    }
                }
            }
            public static async Task<string> ReadFromSiteAsync(string url)
            {
                using var client = new HttpClient();

                await Task.Delay(2000);

                var bytes = await client.GetByteArrayAsync(url);
                return Encoding.UTF8.GetString(bytes);
            }
            public static (string region, string accountage, string status, string accountworth) ExtractPlayerInfo(string html)
            {

                var blockMatch = Regex.Match(html, @"<ul class=""player-info"">(.*?)</ul>", RegexOptions.Singleline);
                if (!blockMatch.Success)
                    return ("?", "?", "?", "?");

                string block = blockMatch.Groups[1].Value;
                string region = Regex.Match(block, @"flag-icon-([a-z]{2})", RegexOptions.IgnoreCase).Groups[1].Value.ToUpper();
                string age = Regex.Match(block, @"<span class=""number""[^>]*>([^<]+)</span>").Groups[1].Value.Trim();
                string status = Regex.Match(block, @"<li>\s*(online|offline)\s*</li>", RegexOptions.IgnoreCase).Groups[1].Value;

                var worthMatch = Regex.Match(html, @"<div class=""prices tooltipped tooltipped-s"">.*?<span class=""number-price"">(.*?)</span>", RegexOptions.Singleline);
                string worth = worthMatch.Success ? worthMatch.Groups[1].Value.Trim() : "?";

                if (string.IsNullOrEmpty(region)) region = "?";
                if (string.IsNullOrEmpty(age)) age = "?";
                if (string.IsNullOrEmpty(status)) status = "?";
                if (string.IsNullOrEmpty(worth)) worth = "?";

                age = age.Replace(",", "").Replace(".", "").Trim();

                return (region, age, status, worth);
            }
            public static (string steamname, string level, string games, string friends, string recentPlaytime, string vacBans, string profileAwards, string badges, string reviews, string artwork, string inventory, string groups) ExtractOther(string html)
            {
                string steamname = Regex.Match(
                    html,
                    @"<span class=""actual_persona_name"">([\s\S]*?)</span>",
                    RegexOptions.Singleline
                ).Groups[1].Value;

                steamname = WebUtility.HtmlDecode(steamname).Trim();

                if (string.IsNullOrEmpty(steamname))
                    steamname = "?";

                string level = Regex.Match(html, @"<span class=""friendPlayerLevelNum"">(\d+)</span>", RegexOptions.Singleline).Groups[1].Value;
                if (string.IsNullOrEmpty(level)) level = "?";

                string recentPlaytime = Regex.Match(
                    html,
                    @"<div class=""recentgame_quicklinks recentgame_recentplaytime"">(?:\r?\n\s*)<div>(.*?)</div>",
                    RegexOptions.Singleline
                ).Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(recentPlaytime)) recentPlaytime = "?";

                string GetCount(string label)
                {
                    var match = Regex.Match(
                        html,
                        $@"<span class=""count_link_label"">\s*{Regex.Escape(label)}\s*</span>.*?<span class=""profile_count_link_total"">\s*(\d+)\s*</span>",
                        RegexOptions.Singleline | RegexOptions.IgnoreCase
                    );
                    return string.IsNullOrEmpty(match.Groups[1].Value) ? "?" : match.Groups[1].Value;
                }

                string games = GetCount("Games");
                string friends = GetCount("Friends");
                string profileAwards = GetCount("Profile Awards");
                string badges = GetCount("Badges");
                string reviews = GetCount("Reviews");
                string artwork = GetCount("Artwork");
                string inventory = GetCount("Inventory");
                string groups = GetCount("Groups");

                string vacBans = Regex.Match(
                    html,
                    @"<div class=""profile_ban"">.*?<span class=""profile_count_link_total"">\s*(\d+)\s*</span>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase
                ).Groups[1].Value;
                if (string.IsNullOrEmpty(vacBans)) vacBans = "?";

                return (steamname, level, games, friends, recentPlaytime, vacBans, profileAwards, badges, reviews, artwork, inventory, groups);
            }
            //
        }
        internal class TeleporterManager
        {
            public static System.Collections.Generic.List<TeleporterManager> Teleportersnowx = new();

            public static readonly string teleportmanager =
                Path.Combine(FusionProtectorFiles, "FusionProtectorTeleportManager.txt");

            [JsonProperty] public string Map { get; set; }
            [JsonProperty] public Vector3 Position { get; set; }
            [JsonProperty] public Quaternion Rotation { get; set; }
            [JsonProperty] public string TitleOfTeleporter { get; set; }
            [JsonProperty] public string LevelBarcode { get; set; }



            [JsonConstructor]
            public TeleporterManager(
string map, Vector3 position,
                Quaternion rotation, string titleOfTeleporter, string levelBarcode)
            {
                this.TitleOfTeleporter = titleOfTeleporter;
                this.Position = new Vector3(position.x, position.y, position.z);
                this.Rotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
                this.LevelBarcode = levelBarcode;
                this.Map = map;
            }


            public void TeleportToIt()
            {
                if (SceneStreamer.Session?.Level?.Barcode?.ID == LevelBarcode)
                {
                    var rig = Player.RigManager.physicsRig;

                    foreach (var rb in rig.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    rig.marrowEntity.Teleport(
                        Position,
                        Rotation,
                        true
                    );
                }
                else
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        $"Can't Use — You Are Not On {Map}",
                        NotificationType.WARNING,
                        2.5f
                    );
                }
            }
            public void EditPresetName(string newValue)
            {
                if (Teleportersnowx.Any(X => X.TitleOfTeleporter == newValue))
                    return;


                this.TitleOfTeleporter = newValue;
                SaveTeleporters();
                NotificationNow(FusionProtectorInfo.ClientName, $"Changed Preset Name To [{TitleOfTeleporter}]!", NotificationType.SUCCESS, 3.5f);

            }
            public static void LoadTeleporters()
            {
                try
                {

                    if (File.Exists(teleportmanager))
                    {
                        string json = File.ReadAllText(teleportmanager);
                        var loaded = JsonConvert.DeserializeObject<System.Collections.Generic.List<TeleporterManager>>(json,
                            new JsonSerializerSettings
                            {
                                Formatting = Formatting.Indented,
                                FloatFormatHandling = FloatFormatHandling.Symbol,
                                Culture = CultureInfo.InvariantCulture,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });

                        if (loaded != null)
                            Teleportersnowx = loaded;
                    }

                }
                catch
                {
                }
            }

            public static void SaveTeleporters()
            {
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        FloatFormatHandling = FloatFormatHandling.Symbol,
                        Culture = CultureInfo.InvariantCulture,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    string jsonString = JsonConvert.SerializeObject(Teleportersnowx, settings);
                    File.WriteAllText(teleportmanager, jsonString);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to save teleporters: {ex.Message}");
                }
            }
        }
        internal static class ColorClasseyPoo
        {
            static readonly Color[] presetColors =
            {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        Color.white
            };

            public static Color RandomUnityColor()
            {
                return presetColors[Random.Shared.Next(0, presetColors.Length)];
            }

            public static Color ReturnColor(float r, float g, float b)
            {
                return new Color(r, g, b, 30);
            }

            public static Color RandomColor()
            {
                var rrandom = new Random();
                var grandom = new Random();
                var brandom = new Random();
                return ReturnColor(rrandom.Next(0, 255), grandom.Next(0, 255), brandom.Next(0, 255));
            }
            public static Color ConvertRGBAToUnityColor(int r, int g, int b, int a = 255)
            {
                return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }
        }
        internal class CreateCheatToolsPreset
        {
            public class Item
            {
                [JsonProperty]
                public string BarcodeId { get; set; } = "Empty";

                [JsonProperty]
                public string SpawnableName { get; set; } = "Empty";

                [JsonProperty]
                public bool LocalSpawn { get; set; } = false;

                private int? _modIoID;

                [JsonProperty]
                public int ModIoID
                {
                    get
                    {

                        if (_modIoID.HasValue && _modIoID.Value != -1)
                            return _modIoID.Value;

                        try
                        {
                            if (string.IsNullOrEmpty(BarcodeId) ||
                                BarcodeId == "Empty")
                                return -1;

                            var crateRef =
                                new SpawnableCrateReference(BarcodeId);

                            var crate =
                                crateRef?.Crate;

                            if (crate == null)
                                return -1;

                            var pallet = crate.Pallet;

                            var detected =
                                CrateFilterer.GetModID(pallet);

                            if (detected == -1)
                                return -1;

                            _modIoID = detected;

                            return detected;
                        }
                        catch
                        {
                            return -1;
                        }
                    }
                    set
                    {
                        _modIoID = value;
                    }
                }
            }


            public static CreateCheatToolsPreset CurrentPresetNow;

            public static System.Collections.Generic.List<CreateCheatToolsPreset> CheatPresets = new();

            public static readonly string devitemscurrent =
                Path.Combine(FusionProtectorFiles,
                    "CustomDevToolsCurrent.txt");

            public static readonly string devitems =
                Path.Combine(FusionProtectorFiles,
                    "CustomDevItems.txt");


            [JsonProperty] public string TitleOfPreset { get; set; }

            [JsonProperty] public Item Item1 { get; set; }

            [JsonProperty] public Item Item2 { get; set; }

            [JsonProperty] public Item Item3 { get; set; }

            [JsonProperty] public Item Item4 { get; set; }

            [JsonProperty] public Item Item5 { get; set; }


            [JsonIgnore]
            public System.Collections.Generic.IEnumerable<Item> Items
            {
                get
                {
                    yield return Item1;
                    yield return Item2;
                    yield return Item3;
                    yield return Item4;
                    yield return Item5;
                }
            }
            public CreateCheatToolsPreset()
            {
                TitleOfPreset = "Empty";

                Item1 = new Item();
                Item2 = new Item();
                Item3 = new Item();
                Item4 = new Item();
                Item5 = new Item();
            }

            [JsonConstructor]
            public CreateCheatToolsPreset(
                string TitleOfPreset,
                Item Item1,
                Item Item2,
                Item Item3,
                Item Item4,
                Item Item5)
            {
                this.TitleOfPreset = TitleOfPreset;

                this.Item1 = Item1;
                this.Item2 = Item2;
                this.Item3 = Item3;
                this.Item4 = Item4;
                this.Item5 = Item5;

                Normalize();
            }


            void Normalize()
            {
                TitleOfPreset ??= "Empty";

                Item1 ??= new Item();
                Item2 ??= new Item();
                Item3 ??= new Item();
                Item4 ??= new Item();
                Item5 ??= new Item();


                foreach (var item in Items)
                {
                    _ = item.ModIoID;
                }
            }


            static void NormalizeCurrentPresetNow()
            {
                CurrentPresetNow ??= new CreateCheatToolsPreset(
                    "Empty",
                    null,
                    null,
                    null,
                    null,
                    null);

                CurrentPresetNow.Normalize();
            }


            Item GetItem(int slotNumber)
            {
                return slotNumber switch
                {
                    1 => Item1,
                    2 => Item2,
                    3 => Item3,
                    4 => Item4,
                    5 => Item5,
                    _ => null
                };
            }


            public static void LoadPresets()
            {
                try
                {
                    CheatPresets ??= new();

                    if (File.Exists(devitems))
                    {
                        var json = File.ReadAllText(devitems);

                        try
                        {
                            var loaded =
                                JsonConvert.DeserializeObject<System.Collections.Generic.List<CreateCheatToolsPreset>>(json);

                            if (loaded != null)
                            {
                                CheatPresets = loaded;

                                foreach (var preset in CheatPresets)
                                    preset.Normalize();
                            }
                        }
                        catch
                        {
                            MelonLogger.Warning("Old or invalid preset format detected. Resetting preset list.");
                            CheatPresets = new();
                        }
                    }

                    if (File.Exists(devitemscurrent))
                    {
                        var json = File.ReadAllText(devitemscurrent);

                        CreateCheatToolsPreset current = null;

                        try
                        {
                            current =
                                JsonConvert.DeserializeObject<CreateCheatToolsPreset>(json);
                        }
                        catch
                        {
                            try
                            {
                                var list =
                                    JsonConvert.DeserializeObject<System.Collections.Generic.List<CreateCheatToolsPreset>>(json);

                                if (list != null && list.Count > 0)
                                    current = list[0];
                            }
                            catch
                            {
                                current = null;
                            }
                        }

                        if (current != null)
                        {
                            current.Normalize();
                            CurrentPresetNow = current;
                        }
                    }

                    NormalizeCurrentPresetNow();

                    if (InstanceOfIt != null)
                    {
                        InstanceOfIt.crates =
                            CurrentPresetNow.Items
                                .Select(x => new SpawnableCrateReference(x.BarcodeId))
                                .ToArray();
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Load Presets Failed : {ex}");
                }
            }


            public static void SavePresets()
            {
                try
                {
                    CheatPresets ??= new();

                    foreach (var preset in CheatPresets)
                        preset.Normalize();

                    NormalizeCurrentPresetNow();

                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        Culture = CultureInfo.InvariantCulture,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    File.WriteAllText(
                        devitems,
                        JsonConvert.SerializeObject(CheatPresets, settings));

                    File.WriteAllText(
                        devitemscurrent,
                        JsonConvert.SerializeObject(CurrentPresetNow, settings));
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to save Presets: {ex}");
                }
            }

            public void EditDevSlot(int slotNumber,
                string newValue)
            {
                var item = GetItem(slotNumber);

                if (item == null)
                    return;

                item.BarcodeId = newValue;

                var crateRef =
                    new SpawnableCrateReference(newValue);

                item.SpawnableName =
                    StripColorTags(
                        crateRef?.Crate?.name) ?? "Unknown";

                item.ModIoID =
                    CrateFilterer.GetModID(crateRef.Crate.Pallet);

                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    $"Edited Dev Slot {slotNumber} With {item.SpawnableName}",
                    NotificationType.SUCCESS,
                    1.5f);

                SavePresets();
                RefreshIt();
            }


            public void EditPresetName(string newValue)
            {
                if (CheatPresets.Any(X => X.TitleOfPreset == newValue))
                    return;

                this.TitleOfPreset = newValue;
                SavePresets();
                NotificationNow(FusionProtectorInfo.ClientName, $"Changed Preset Name To [{TitleOfPreset}]!", NotificationType.SUCCESS, 3.5f);

            }




            public void EditSlotLocalSpawn(
                int slotNumber,
                bool localspawningvalue)
            {
                var item = GetItem(slotNumber);

                if (item == null)
                    return;

                item.LocalSpawn = localspawningvalue;

                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    $"Edited Dev Slot [{slotNumber}] With Local Spawning Value [{localspawningvalue}]",
                    NotificationType.SUCCESS,
                    1.5f);

                SavePresets();
                RefreshIt();
            }


            public void ClearDevSlot(int slotNumber)
            {
                var item = GetItem(slotNumber);

                if (item == null)
                    return;

                item.BarcodeId = "Empty";
                item.SpawnableName = "Empty";
                item.LocalSpawn = false;

                item.ModIoID = -1;

                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    $"Cleared Dev Slot {slotNumber}!",
                    NotificationType.SUCCESS,
                    1.5f);

                SavePresets();
                RefreshIt();
            }


            public void RefreshIt()
            {
                CurrentPresetNow = this;

                InstanceOfIt.crates =
                    Items.Select(i =>
                        new SpawnableCrateReference(
                            i.BarcodeId))
                    .ToArray();

                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    $"Refreshed Preset & Applied!",
                    NotificationType.SUCCESS,
                    1.5f);
            }


            public string GetDevBarcode(
                int slotNumber)
                => GetItem(slotNumber)?.BarcodeId;


            public void RemovePreset()
            {
                var item =
                    CheatPresets.FirstOrDefault(p =>
                        p.TitleOfPreset == TitleOfPreset);

                if (item != null)
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        $"Remove Preset {item.TitleOfPreset}",
                        NotificationType.SUCCESS,
                        1.5f);

                    CheatPresets.Remove(item);

                    SavePresets();
                }
            }
        }
        internal class BodyLogPage
        {
            public static System.Collections.Generic.List<BodyLogPage> BodyLogPages = new();

            public static readonly string Bodylogpages =
                Path.Combine(FusionProtectorFiles, "FusionProtectorBodyLogPages.txt");
            [JsonProperty] public string TitleOfPreset { get; set; }
            [JsonProperty] public string Slot1 { get; set; }
            [JsonProperty] public string Slot2 { get; set; }
            [JsonProperty] public string Slot3 { get; set; }
            [JsonProperty] public string Slot4 { get; set; }
            [JsonProperty] public string Slot5 { get; set; }
            [JsonProperty] public string Slot6 { get; set; }

            private int? _modIoID1;
            private int? _modIoID2;
            private int? _modIoID3;
            private int? _modIoID4;
            private int? _modIoID5;
            private int? _modIoID6;

            [JsonProperty]
            public int ModIoID1 { get => RepairModId(ref _modIoID1, Slot1); set => _modIoID1 = value; }
            [JsonProperty]
            public int ModIoID2 { get => RepairModId(ref _modIoID2, Slot2); set => _modIoID2 = value; }
            [JsonProperty]
            public int ModIoID3 { get => RepairModId(ref _modIoID3, Slot3); set => _modIoID3 = value; }
            [JsonProperty]
            public int ModIoID4 { get => RepairModId(ref _modIoID4, Slot4); set => _modIoID4 = value; }
            [JsonProperty]
            public int ModIoID5 { get => RepairModId(ref _modIoID5, Slot5); set => _modIoID5 = value; }
            [JsonProperty]
            public int ModIoID6 { get => RepairModId(ref _modIoID6, Slot6); set => _modIoID6 = value; }

            [JsonConstructor]
            public BodyLogPage(
                string TitleOfPreset,
                string Slot1,
                string Slot2,
                string Slot3,
                string Slot4,
                string Slot5,
                string Slot6,
                int? ModIoID1 = null,
                int? ModIoID2 = null,
                int? ModIoID3 = null,
                int? ModIoID4 = null,
                int? ModIoID5 = null,
                int? ModIoID6 = null)
            {
                this.TitleOfPreset = TitleOfPreset ?? "Empty";

                this.Slot1 = Slot1; this.Slot2 = Slot2; this.Slot3 = Slot3;
                this.Slot4 = Slot4; this.Slot5 = Slot5; this.Slot6 = Slot6;

                _modIoID1 = ModIoID1; _modIoID2 = ModIoID2; _modIoID3 = ModIoID3;
                _modIoID4 = ModIoID4; _modIoID5 = ModIoID5; _modIoID6 = ModIoID6;
            }

            private int RepairModId(ref int? storedId, string barcode)
            {
                const string EMPTY_BARCODE = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

                if (string.IsNullOrEmpty(barcode) || barcode == EMPTY_BARCODE)
                    return -1;

                if (storedId.HasValue && storedId.Value > 0)
                    return storedId.Value;

                try
                {
                    var crateRef = new SpawnableCrateReference(barcode);
                    var pallet = crateRef?.Crate?.Pallet;
                    int id = CrateFilterer.GetModID(pallet);
                    storedId = id;
                    return id;
                }
                catch
                {
                    return -1;
                }
            }

            string GetBarcode(int slot) => slot switch
            {
                1 => Slot1,
                2 => Slot2,
                3 => Slot3,
                4 => Slot4,
                5 => Slot5,
                6 => Slot6,
                _ => null
            };

            int GetModId(int slot) => slot switch
            {
                1 => ModIoID1,
                2 => ModIoID2,
                3 => ModIoID3,
                4 => ModIoID4,
                5 => ModIoID5,
                6 => ModIoID6,
                _ => -1
            };

            void SetModId(int slot, int id)
            {
                switch (slot)
                {
                    case 1: ModIoID1 = id; break;
                    case 2: ModIoID2 = id; break;
                    case 3: ModIoID3 = id; break;
                    case 4: ModIoID4 = id; break;
                    case 5: ModIoID5 = id; break;
                    case 6: ModIoID6 = id; break;
                }
            }

            public void EditPresetName(string newValue)
            {
                if (BodyLogPages.Any(X => X.TitleOfPreset == newValue))
                    return;
                this.TitleOfPreset = newValue;
                SavePresets();
                NotificationNow(FusionProtectorInfo.ClientName, $"Changed Preset Name To [{TitleOfPreset}]!", NotificationType.SUCCESS, 3.5f);

            }

            public void ApplyPreset()
            {
                const string EMPTY_BARCODE = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

                for (int i = 1; i <= 6; i++)
                {
                    string barcode = GetBarcode(i);
                    int modId = GetModId(i);

                    if (string.IsNullOrEmpty(barcode) || barcode == EMPTY_BARCODE)
                        continue;

                    if (modId <= 0)
                        continue;

                    try
                    {

                        if (!IsAvatarCrateExist(barcode))
                        {
                            DownloadModIOMod(modId, false);
                            MelonLogger.Msg($"Downloading missing ModIO mod {modId} for barcode {barcode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Failed to process slot {i} with ModIO {modId}: {ex}");
                    }
                }
            }
            public void EditSlot(int slotNumber, string newValue)
            {
                switch (slotNumber)
                {
                    case 1: Slot1 = newValue; break;
                    case 2: Slot2 = newValue; break;
                    case 3: Slot3 = newValue; break;
                    case 4: Slot4 = newValue; break;
                    case 5: Slot5 = newValue; break;
                    case 6: Slot6 = newValue; break;
                    default: return;
                }

                SetModId(slotNumber, -1);
                SavePresets();
                NotificationNow(FusionProtectorInfo.ClientName,
                    $"Edited Slot With {newValue}", NotificationType.SUCCESS, 3.5f);
            }

            public void ClearSlot(int slotNumber)
            {
                const string EMPTY_BARCODE = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

                switch (slotNumber)
                {
                    case 1: Slot1 = EMPTY_BARCODE; break;
                    case 2: Slot2 = EMPTY_BARCODE; break;
                    case 3: Slot3 = EMPTY_BARCODE; break;
                    case 4: Slot4 = EMPTY_BARCODE; break;
                    case 5: Slot5 = EMPTY_BARCODE; break;
                    case 6: Slot6 = EMPTY_BARCODE; break;
                    default: return;
                }

                SetModId(slotNumber, -1);
                SavePresets();

                NotificationNow(FusionProtectorInfo.ClientName, "Cleared Slot!", NotificationType.SUCCESS, 3.5f);
            }

            public void RemovePreset()
            {
                var item = BodyLogPages.FirstOrDefault(p => p.TitleOfPreset == TitleOfPreset);
                if (item != null)
                {
                    NotificationNow(FusionProtectorInfo.ClientName,
                        $"Remove Preset {item.TitleOfPreset}", NotificationType.SUCCESS, 3.5f);

                    BodyLogPages.Remove(item);
                    SavePresets();
                }
            }

            public static void LoadPresets()
            {
                try
                {
                    if (!File.Exists(Bodylogpages)) return;

                    string json = File.ReadAllText(Bodylogpages);
                    var loaded = JsonConvert.DeserializeObject<System.Collections.Generic.List<BodyLogPage>>(json);
                    if (loaded != null)
                        BodyLogPages = loaded;


                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Load BodyLogs Failed : {ex}");
                }
            }

            public static void SavePresets()
            {
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        FloatFormatHandling = FloatFormatHandling.Symbol,
                        Culture = CultureInfo.InvariantCulture,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    string json = JsonConvert.SerializeObject(BodyLogPages, settings);
                    File.WriteAllText(Bodylogpages, json);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to save Pages: {ex}");
                }
            }
        }
        internal class InventoryPage
        {
            public static System.Collections.Generic.List<InventoryPage> InventoryPresets = new();
            public static System.Collections.Generic.Dictionary<string, string> CurrentPreset;
            [JsonProperty] public string TitleOfPreset { get; set; }
            [JsonProperty] public System.Collections.Generic.Dictionary<string, string> Slots { get; set; }

            [JsonConstructor]
            public InventoryPage(string TitleOfPreset, System.Collections.Generic.Dictionary<string, string> Slots)
            {
                this.TitleOfPreset = TitleOfPreset;
                this.Slots = Slots;
            }

            public static readonly string PresetsFile = Path.Combine(FusionProtectorFiles, "InventoryPresets.txt");
            public static readonly string PresetsFileCurrent = Path.Combine(FusionProtectorFiles, "FusionInventoryPresetsCurrent.txt");


            void NormalizeSlots()
            {
                if (Slots == null)
                {
                    Slots = new System.Collections.Generic.Dictionary<string, string>();
                    return;
                }

                var keys = Slots.Keys.ToList();
                foreach (var key in keys)
                {
                    if (Slots[key] == null)
                        Slots[key] = "Empty";
                }
            }

            public void SaveToFile()
            {
                NormalizeSlots();

                var existing = InventoryPresets.FirstOrDefault(p => p.TitleOfPreset == this.TitleOfPreset);
                if (existing == null)
                    InventoryPresets.Add(this);

                try
                {
                    string json = JsonConvert.SerializeObject(InventoryPresets, Formatting.Indented);
                    File.WriteAllText(PresetsFile, json);


                    string jsonStringcurrent = JsonConvert.SerializeObject(PresetsFileCurrent, Formatting.Indented);
                    File.WriteAllText(PresetsFileCurrent, jsonStringcurrent);


                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Failed to save presets: {e}");
                }
            }
            public void EditPresetName(string newValue)
            {
                if (InventoryPresets.Any(X => X.TitleOfPreset == newValue))
                    return;
                this.TitleOfPreset = newValue;
                SaveAllPresetsToFile();
                NotificationNow(FusionProtectorInfo.ClientName, $"Changed Preset Name To [{TitleOfPreset}]!", NotificationType.SUCCESS, 3.5f);

            }
            public static void LoadAllPresets()
            {
                try
                {
                    if (!File.Exists(PresetsFile)) return;
                    string json = File.ReadAllText(PresetsFile);
                    InventoryPresets = JsonConvert.DeserializeObject<System.Collections.Generic.List<InventoryPage>>(json) ?? new System.Collections.Generic.List<InventoryPage>();

                    if (File.Exists(PresetsFileCurrent))
                    {
                        string jsonDefault = File.ReadAllText(PresetsFileCurrent);
                        var loadedCurrent = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(jsonDefault);
                        if (loadedCurrent != null)
                            CurrentPreset = loadedCurrent;
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Failed to load presets: {e}");
                }
            }

            public void LoadIntoPlayer(bool notificationnow = true)
            {
                if (Player.RigManager == null) return;

                var headSlot = Player.RigManager.physicsRig.m_head.transform
                    ?.Find("HeadSlotContainer/WeaponReciever_01")
                    ?.GetComponent<InventorySlotReceiver>();

                var allSlots = Player.RigManager.inventory.bodySlots
                    .Concat(Player.RigManager.inventory.specialItems)
                    .ToArray();


                foreach (var kvp in Slots)
                {
                    string slotName = kvp.Key;
                    string barcode = kvp.Value;
                    if (string.IsNullOrEmpty(barcode)) continue;

                    if (slotName == "Head" && headSlot != null)
                    {
                        headSlot.DropWeapon();
                        headSlot.SpawnInSlotAsync(new Barcode(barcode));
                        continue;
                    }

                    var receiver = allSlots.FirstOrDefault(s => s.name == slotName)?.inventorySlotReceiver;
                    if (receiver != null)
                    {
                        receiver.DropWeapon();
                        receiver.SpawnInSlotAsync(new Barcode(barcode));
                    }
                }

                if (notificationnow)
                {
                    NotificationNow(FusionProtectorInfo.ClientName, $"Loadout [{TitleOfPreset.Trim()}] Loaded Into Player!", NotificationType.SUCCESS, 3.5f);
                }
            }

            public static InventoryPage CaptureFromCurrentInventory(string title)
            {
                var inventory = Player.RigManager?.inventory;
                if (inventory == null) return null;

                if (InventoryPresets.Any(p =>
          !string.IsNullOrEmpty(p.TitleOfPreset) &&
          p.TitleOfPreset.Trim().Equals(title.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    NotificationNow(FusionProtectorInfo.ClientName, $"Preset [{title.Trim()}] Already Exist!", NotificationType.SUCCESS, 3.5f);
                    return null;
                }

                var slots = new System.Collections.Generic.Dictionary<string, string>();

                var headSlot = Player.RigManager.physicsRig.m_head.transform
                    ?.Find("HeadSlotContainer/WeaponReciever_01")
                    ?.GetComponent<InventorySlotReceiver>();
                var headBarcode = headSlot?._slottedWeapon?.interactableHost?.marrowEntity?._poolee?._SpawnableCrate_k__BackingField?.Barcode?.ID;
                slots["Head"] = headBarcode;

                var allSlots = inventory.bodySlots.Concat(inventory.specialItems);
                foreach (var slot in allSlots)
                {
                    var receiver = slot?.inventorySlotReceiver;
                    var barcode = receiver?._slottedWeapon?.interactableHost?.marrowEntity?._poolee?._SpawnableCrate_k__BackingField?.Barcode?.ID;
                    slots[slot.name] = barcode;
                }

                var newPreset = new InventoryPage(title, slots);
                InventoryPresets.Add(newPreset);
                newPreset.SaveToFile();



                CurrentPreset = slots;

                try
                {
                    string json = JsonConvert.SerializeObject(CurrentPreset, Formatting.Indented);
                    File.WriteAllText(PresetsFileCurrent, json);
                    NotificationNow(FusionProtectorInfo.ClientName, $"Saved Loadout [{title.Trim()}]!", NotificationType.SUCCESS, 3.5f);

                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Failed to save current preset: {e}");
                }





                return newPreset;
            }

            public void RemovePreset()
            {
                var item = InventoryPresets.FirstOrDefault(p => p.TitleOfPreset == TitleOfPreset);
                if (item != null)
                {
                    NotificationNow(FusionProtectorInfo.ClientName, $"Remove Preset {item.TitleOfPreset}", NotificationType.SUCCESS, 3.5f);

                    InventoryPresets.Remove(item);
                    SaveAllPresetsToFile();
                }
            }

            public static void SaveAllPresetsToFile()
            {
                try
                {
                    string json = JsonConvert.SerializeObject(InventoryPresets, Formatting.Indented);
                    File.WriteAllText(PresetsFile, json);
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"Failed to save presets: {e}");
                }
            }
            public string GetSlotBarcode(string slotName)
            {
                if (string.IsNullOrEmpty(slotName)) return null;
                if (Slots == null) return null;

                Slots.TryGetValue(slotName, out string barcode);
                return barcode;
            }

            public void EditSlotBarcode(string slotName, string newBarcode)
            {
                if (string.IsNullOrEmpty(slotName)) return;



                var preset = InventoryPresets.FirstOrDefault(p => p.TitleOfPreset == TitleOfPreset);
                if (preset == null) return;

                if (!preset.Slots.ContainsKey(slotName)) return;

                preset.Slots[slotName] = newBarcode;


                var crateName = new SpawnableCrateReference(newBarcode)?.Crate?.name ?? "Empty";
                NotificationNow(FusionProtectorInfo.ClientName, $"Added {crateName} To {slotName}", NotificationType.SUCCESS, 3.5f);

                SaveAllPresetsToFile();
            }
        }
        internal class BodyLogRadialMenuColorPreset
        {
            public static string[] CurrentPreset;
            public static System.Collections.Generic.List<BodyLogRadialMenuColorPreset> ColorPresets = new();

            public static readonly string ColorsCurrent = Path.Combine(FusionProtectorFiles, "ColorsCurrent.txt");
            public static readonly string ColorsPresets = Path.Combine(FusionProtectorFiles, "ColorPresetsNow.txt");

            [JsonProperty] public string TitleOfPreset { get; set; }

            [JsonProperty] public float BodyLogColor_R { get; set; }
            [JsonProperty] public float BodyLogColor_G { get; set; }
            [JsonProperty] public float BodyLogColor_B { get; set; }
            [JsonProperty] public float BodyLogColor_A { get; set; }

            [JsonProperty] public float BodyLogBallColor_R { get; set; }
            [JsonProperty] public float BodyLogBallColor_G { get; set; }
            [JsonProperty] public float BodyLogBallColor_B { get; set; }
            [JsonProperty] public float BodyLogBallColor_A { get; set; }

            [JsonProperty] public float BodyLogLineColor_R { get; set; }
            [JsonProperty] public float BodyLogLineColor_G { get; set; }
            [JsonProperty] public float BodyLogLineColor_B { get; set; }
            [JsonProperty] public float BodyLogLineColor_A { get; set; }

            [JsonProperty] public float RadialMenuColor_R { get; set; }
            [JsonProperty] public float RadialMenuColor_G { get; set; }
            [JsonProperty] public float RadialMenuColor_B { get; set; }
            [JsonProperty] public float RadialMenuColor_A { get; set; }

            [JsonConstructor]
            public BodyLogRadialMenuColorPreset(
                string TitleOfPreset,
                float BodyLogColor_R, float BodyLogColor_G, float BodyLogColor_B, float BodyLogColor_A,
                float BodyLogBallColor_R, float BodyLogBallColor_G, float BodyLogBallColor_B, float BodyLogBallColor_A,
                float BodyLogLineColor_R, float BodyLogLineColor_G, float BodyLogLineColor_B, float BodyLogLineColor_A,
                float RadialMenuColor_R, float RadialMenuColor_G, float RadialMenuColor_B, float RadialMenuColor_A)
            {
                this.TitleOfPreset = TitleOfPreset;

                this.BodyLogColor_R = BodyLogColor_R;
                this.BodyLogColor_G = BodyLogColor_G;
                this.BodyLogColor_B = BodyLogColor_B;
                this.BodyLogColor_A = BodyLogColor_A;

                this.BodyLogBallColor_R = BodyLogBallColor_R;
                this.BodyLogBallColor_G = BodyLogBallColor_G;
                this.BodyLogBallColor_B = BodyLogBallColor_B;
                this.BodyLogBallColor_A = BodyLogBallColor_A;

                this.BodyLogLineColor_R = BodyLogLineColor_R;
                this.BodyLogLineColor_G = BodyLogLineColor_G;
                this.BodyLogLineColor_B = BodyLogLineColor_B;
                this.BodyLogLineColor_A = BodyLogLineColor_A;

                this.RadialMenuColor_R = RadialMenuColor_R;
                this.RadialMenuColor_G = RadialMenuColor_G;
                this.RadialMenuColor_B = RadialMenuColor_B;
                this.RadialMenuColor_A = RadialMenuColor_A;
            }

            public static void LoadPresets()
            {
                try
                {
                    if (File.Exists(ColorsPresets))
                    {
                        string json = File.ReadAllText(ColorsPresets);
                        var loaded = JsonConvert.DeserializeObject<System.Collections.Generic.List<BodyLogRadialMenuColorPreset>>(json);
                        if (loaded != null) ColorPresets = loaded;
                    }

                    if (File.Exists(ColorsCurrent))
                    {
                        string jsonDefault = File.ReadAllText(ColorsCurrent);
                        var loadedCurrent = JsonConvert.DeserializeObject<string[]>(jsonDefault);
                        if (loadedCurrent != null) CurrentPreset = loadedCurrent;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to load presets: {ex.Message}");
                }
            }

            public static void SavePresets()
            {
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        Culture = CultureInfo.InvariantCulture
                    };

                    string jsonPresets = JsonConvert.SerializeObject(ColorPresets, settings);
                    File.WriteAllText(ColorsPresets, jsonPresets);

                    string jsonCurrent = JsonConvert.SerializeObject(CurrentPreset, settings);
                    File.WriteAllText(ColorsCurrent, jsonCurrent);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to save presets: {ex.Message}");
                }
            }
            public void EditPresetName(string newValue)
            {
                if (ColorPresets.Any(X => X.TitleOfPreset == newValue))
                    return;
                this.TitleOfPreset = newValue;
                SavePresets();
                NotificationNow(FusionProtectorInfo.ClientName, $"Changed Preset Name To [{TitleOfPreset}]!", NotificationType.SUCCESS, 3.5f);

            }
            public void RemovePreset()
            {
                var item = ColorPresets.FirstOrDefault(p => p.TitleOfPreset == TitleOfPreset);
                if (item != null)
                {
                    NotificationNow(FusionProtectorInfo.ClientName, $"Removed preset {item.TitleOfPreset}", NotificationType.SUCCESS, 3.5f);
                    ColorPresets.Remove(item);
                    SavePresets();
                }
            }


            public float GetValue(string propertyName)
            {
                var prop = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null && prop.PropertyType == typeof(float))
                    return (float)prop.GetValue(this);
                throw new ArgumentException($"Property {propertyName} not found or not float.");
            }

            public void SetValue(string propertyName, float value)
            {
                var prop = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null && prop.PropertyType == typeof(float))
                {
                    prop.SetValue(this, value);
                    return;
                }
                throw new ArgumentException($"Property {propertyName} not found or not float.");
            }

        }
        internal class StatsKickerPresets
        {
            public static string[] CurrentPreset;
            public static System.Collections.Generic.List<StatsKickerPresets> StatsKickerPresetz = new();

            public static readonly string StatsKickerPresetsNow = Path.Combine(FusionProtectorFiles, "FusionStatKickerPresets.txt");
            public static readonly string StatsKickerCurrent = Path.Combine(FusionProtectorFiles, "FusionStatKickerCurrents.txt");

            [JsonProperty] public string TitleOfPreset { get; set; }

            [JsonProperty] public string Height { get; set; }
            [JsonProperty] public string MassArm { get; set; }
            [JsonProperty] public string MassChest { get; set; }
            [JsonProperty] public string MassHead { get; set; }
            [JsonProperty] public string MassLeg { get; set; }
            [JsonProperty] public string MassPelvis { get; set; }
            [JsonProperty] public string MassTotal { get; set; }
            [JsonProperty] public string Speed { get; set; }
            [JsonProperty] public string StrengthLower { get; set; }
            [JsonProperty] public string StrengthUpper { get; set; }
            [JsonProperty] public string Vitality { get; set; }

            [JsonConstructor]
            public StatsKickerPresets(
                string TitleOfPreset,
                string height,
                string massArm,
                string massChest,
                string massHead,
                string massLeg,
                string massPelvis,
                string massTotal,
                string speed,
                string strengthLower,
                string strengthUpper,
                string vitality)
            {
                this.TitleOfPreset = TitleOfPreset;
                this.Height = height;
                this.MassArm = massArm;
                this.MassChest = massChest;
                this.MassHead = massHead;
                this.MassLeg = massLeg;
                this.MassPelvis = massPelvis;
                this.MassTotal = massTotal;
                this.Speed = speed;
                this.StrengthLower = strengthLower;
                this.StrengthUpper = strengthUpper;
                this.Vitality = vitality;
            }
            public void EditPresetName(string newValue)
            {
                if (StatsKickerPresetz.Any(X => X.TitleOfPreset == newValue))
                    return;
                this.TitleOfPreset = newValue;
                SavePresets();
                NotificationNow(FusionProtectorInfo.ClientName, $"Changed Preset Name To [{TitleOfPreset}]!", NotificationType.SUCCESS, 3.5f);

            }
            public static void LoadPresets()
            {
                try
                {
                    if (File.Exists(StatsKickerPresetsNow))
                    {
                        string json = File.ReadAllText(StatsKickerPresetsNow);
                        var loaded = JsonConvert.DeserializeObject<System.Collections.Generic.List<StatsKickerPresets>>(json,
                            new JsonSerializerSettings
                            {
                                Formatting = Formatting.Indented,
                                FloatFormatHandling = FloatFormatHandling.Symbol,
                                Culture = CultureInfo.InvariantCulture,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });

                        if (loaded != null)
                            StatsKickerPresetz = loaded;
                    }

                    if (File.Exists(StatsKickerCurrent))
                    {
                        string jsonCurrent = File.ReadAllText(StatsKickerCurrent);
                        CurrentPreset = JsonConvert.DeserializeObject<string[]>(jsonCurrent);
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to load presets: {ex.Message}");
                }
            }

            public static void SavePresets()
            {
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        FloatFormatHandling = FloatFormatHandling.Symbol,
                        Culture = CultureInfo.InvariantCulture,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    string jsonString = JsonConvert.SerializeObject(StatsKickerPresetz, settings);
                    File.WriteAllText(StatsKickerPresetsNow, jsonString);

                    if (CurrentPreset != null)
                    {
                        string jsonCurrent = JsonConvert.SerializeObject(CurrentPreset, settings);
                        File.WriteAllText(StatsKickerCurrent, jsonCurrent);
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to save presets: {ex.Message}");
                }
            }

            public void RemovePreset()
            {
                var item = StatsKickerPresetz.FirstOrDefault(p => p.TitleOfPreset == TitleOfPreset);
                if (item != null)
                {
                    NotificationNow(FusionProtectorInfo.ClientName, $"Removed Preset {item.TitleOfPreset}", NotificationType.SUCCESS, 3.5f);
                    StatsKickerPresetz.Remove(item);
                    SavePresets();
                }
            }

        }
        internal class FusionProfilePresets
        {
            public static string[] CurrentPreset;
            public static System.Collections.Generic.List<FusionProfilePresets> ProfilePresets = new();
            public static readonly string PresetsPath = Path.Combine(FusionProtectorFiles, "FusionPresetsNow.txt");
            [JsonProperty] public string TitleOfPreset { get; set; }
            [JsonProperty] public string Nickname { get; set; }
            [JsonProperty] public string Description { get; set; }
            [JsonProperty] public string AvatarAtTheTime { get; set; }
            [JsonProperty] public System.Collections.Generic.List<string> BitMartItems { get; set; }

            public static bool IsApplying;


            [JsonConstructor]
            public FusionProfilePresets(
            string TitleOfPreset,
            string Nickname,
            string Description,
            string AvatarAtTheTime,
            System.Collections.Generic.List<string> BitMartItems)
            {
                this.TitleOfPreset = TitleOfPreset;
                this.Nickname = Nickname;
                this.Description = Description;
                this.AvatarAtTheTime = AvatarAtTheTime;
                this.BitMartItems = BitMartItems;
            }


            public static void LoadPresets()
            {
                try
                {
                    if (File.Exists(PresetsPath))
                    {
                        string json = File.ReadAllText(PresetsPath);
                        var loaded = JsonConvert.DeserializeObject<System.Collections.Generic.List<FusionProfilePresets>>(json);
                        if (loaded != null) ProfilePresets = loaded;
                    }

                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to load presets: {ex.Message}");
                }
            }


            public static void SavePresets()
            {
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        Culture = CultureInfo.InvariantCulture
                    };


                    File.WriteAllText(PresetsPath, JsonConvert.SerializeObject(ProfilePresets, settings));
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to save presets: {ex.Message}");
                }
            }



            public IEnumerator ApplyPreset()
            {
                if (IsApplying)
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        "Preset is already applying, please wait...",
                        NotificationType.WARNING,
                        2.5f
                    );
                    yield break;
                }

                IsApplying = true;

                var player = JR_YourNetworkPlayer();

                if (player == null)
                {
                    MelonLogger.Warning("ApplyPreset aborted: player was null.");
                    IsApplying = false;
                    yield break;
                }


                try
                {
                    PointItemManager.UnequipAll();
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"ApplyPreset step failed: {ex}");
                }
                yield return new WaitForSecondsRealtime(1.5f);

                try
                {
                    player.PlayerID.Metadata.Nickname.SetValue(Nickname);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"ApplyPreset step failed: {ex}");
                }
                yield return new WaitForSecondsRealtime(1.5f);

                try
                {
                    player.PlayerID.Metadata.Description.SetValue(Description);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"ApplyPreset step failed: {ex}");
                }
                yield return new WaitForSecondsRealtime(1.5f);


                try
                {
                    if (AvatarAtTheTime != null)
                        ChangeIntoAvi(AvatarAtTheTime);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"ApplyPreset step failed: {ex}");
                }
                yield return new WaitForSecondsRealtime(1.5f);


                if (BitMartItems != null)
                {
                    foreach (var barcode in BitMartItems)
                    {
                        if (string.IsNullOrWhiteSpace(barcode))
                            continue;

                        if (PointItemManager.TryGetPointItem(barcode, out var pointy) && pointy != null)
                        {
                            MelonLogger.Msg($"Equipping item: {pointy.Barcode}");
                            PointItemManager.SetEquipped(pointy, true);
                            yield return new WaitForSecondsRealtime(0.5f);
                        }
                    }
                }


                try
                {
                    EditFusionPreferences("Nickname", Nickname);
                    EditFusionPreferences("Description", Description);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"ApplyPreset step failed: {ex}");
                }
                yield return new WaitForSecondsRealtime(1.0f);

                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    $"Applied Preset {TitleOfPreset}",
                    NotificationType.SUCCESS,
                    3.5f
                );
                IsApplying = false;

            }

            public void EditPresetName(string newValue)
            {
                if (ProfilePresets.Any(X => X.TitleOfPreset == newValue))
                    return;
                this.TitleOfPreset = newValue;
                SavePresets();
                NotificationNow(FusionProtectorInfo.ClientName, $"Changed Preset Name To [{TitleOfPreset}]!", NotificationType.SUCCESS, 3.5f);

            }

            public string GetValue(string propertyName)
            {
                var prop = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null && prop.PropertyType == typeof(string))
                    return (string)prop.GetValue(this);
                throw new ArgumentException($"Property {propertyName} not found or not string.");
            }

            public void SetValue(string propertyName, string value)
            {
                var prop = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null && prop.PropertyType == typeof(string))
                {
                    prop.SetValue(this, value);
                    return;
                }
                throw new ArgumentException($"Property {propertyName} not found or not string.");
            }


            public void RemovePreset()
            {
                var item = ProfilePresets.FirstOrDefault(p => p.TitleOfPreset == TitleOfPreset);
                if (item != null)
                {
                    NotificationNow(FusionProtectorInfo.ClientName, $"Removed preset {item.TitleOfPreset}", NotificationType.SUCCESS, 3.5f);
                    ProfilePresets.Remove(item);
                    SavePresets();
                }
            }
        }

        internal static System.Version FPVersionCurrent = new(FusionProtectorInfo.Version);

        internal static readonly string FusionProtectorFiles =Path.Combine(MelonEnvironment.UserDataDirectory, "FusionProtectorFiles");
        internal static readonly string FPSavetxt = Path.Combine(FusionProtectorFiles, "FPSavedFiles");
        internal static readonly string RECNETLYMETLOGGED = Path.Combine(FPSavetxt, "RecentlyMetPlayersLog.txt");
        internal static readonly string MEDIAPLAYERLOGS = Path.Combine(FPSavetxt, "MediaPlayerLogs.txt");
        internal static readonly string LOBBIESLOGGEDSINCE = Path.Combine(FPSavetxt, "LobbiesSinceLoginLog.txt");
        internal static readonly string PLAYERSLOGGEDSINCE = Path.Combine(FPSavetxt, "PlayersSinceLoginLog.txt");
        internal static readonly string spawnlimitshostonly = Path.Combine(FusionProtectorFiles, "HostOnlySpawnLimits.txt");
        internal static readonly string avatarsblocked = Path.Combine(FusionProtectorFiles, "BlockAvatarsNow.txt");
        internal static readonly string homeworldnow = Path.Combine(FusionProtectorFiles, "FusionHomeWorld.txt");
        internal static readonly string PalletDumpLocation = Path.Combine(FusionProtectorFiles, "Bonelab_PalletDump.txt");
        internal static readonly string BlockedSpawnablesPath = Path.Combine(FusionProtectorFiles, "BlockedSpawnables.txt");
        internal static readonly string WarnedSpawnablesPath = Path.Combine(FusionProtectorFiles, "WarnedSpawnables.txt");
        internal static readonly string ProtectorSettings = Path.Combine(FusionProtectorFiles, "FusionProtectorSettings.txt");
        internal static readonly string SpawnablesKickPath = Path.Combine(FusionProtectorFiles, "SpawnablesKick.txt");
        internal static readonly string AvatarsKickPath = Path.Combine(FusionProtectorFiles, "AvatarsKick.txt");
        internal static readonly string SpawnableCustomFav = Path.Combine(FusionProtectorFiles, "CustomSpawnableFavorites.txt");
        internal static readonly string blockpalletnowlist = Path.Combine(FusionProtectorFiles, "BlockedPalletsFP.txt");
        internal static readonly string blockauthornowlist = Path.Combine(FusionProtectorFiles, "BlockedAuthorsFP.txt");
        internal static readonly string MEDIAPLAYERBLOCKERNOW = Path.Combine(FusionProtectorFiles, "MediaPlayerBlocker.txt");
        internal static readonly string AvatarCustomFav = Path.Combine(FusionProtectorFiles, "CustomAvatarFavorites.txt");
        internal static readonly string DamageBlockPath = Path.Combine(FusionProtectorFiles, "HostOnlyBlockDamageOfPlayers.txt");
        internal static readonly string ServerBlockSpawnPath = Path.Combine(FusionProtectorFiles, "HostOnlyBlockSpawns.txt");
        internal static readonly string BlockMovementsPath = Path.Combine(FusionProtectorFiles, "HostOnlyBlockMovements.txt");
        internal static readonly string blockmessagingnowpath = Path.Combine(FusionProtectorFiles, "HostOnlyBlockMessages.txt");
        internal static readonly string ModIDBLOCKSPATH = Path.Combine(FusionProtectorFiles, "ModIDBlocks.txt");
        internal static readonly string voicepathblocked = Path.Combine(FusionProtectorFiles, "VoiceBlockerIds.txt");
        internal static readonly string WarnAvisNow = Path.Combine(FusionProtectorFiles, "WarnAvatars.txt");
        internal static readonly string BlockAviAuthorNowp = Path.Combine(FusionProtectorFiles, "BlockAuthorAvatars.txt");
        internal static readonly string BlockPalletAviNowp = Path.Combine(FusionProtectorFiles, "BlockPalletAvatars.txt");
        internal static readonly string permissionshere = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Stress Level Zero", "BONELAB", "Fusion", "permissionList.xml");
        internal static readonly string modiotokenfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Stress Level Zero", "BONELAB", "mod_settings.json");

        internal static string searchavi = "base";
        internal static string spwnblesearch = "flash";
        internal static string SteamIDSearch = "";
        internal static string customspwner = "Drax.DraxPack.Spawnable.AK50";
        internal static string teleportername = "Spawn Here";
        internal static string CHEATPRS = "Spawn Here";
        internal static string bodylogpagename = "Spawn Here";
        internal static string statskickernows = "Kick Preset Here";
        internal static string maxdamagethressy = "1000";
        internal static string colornamenowx = "Color Preset Here";
        internal static string profilepresz = "Profile Preset Here";
        internal static string loadoutname = "Loadout Here";
        internal static string levelsrch = "SLZ";

        internal static Page PlayersOnlinePage;
        internal static Page FusionProtectorPage;
        internal static Page ProtectionLogs;
        internal static Page AltpreventPG;
        internal static Page Protectsettings;
        internal static Page playermessages;
        internal static Page avatarsearcher;
        internal static Page aviresults;
        internal static Page avisearchhistory;
        internal static Page spawnablesearch;
        internal static Page spawnableresults;
        internal static Page spawnablehistory;
        internal static Page AISpawnersPage;
        internal static Page Timersz;
        internal static Page OwnerOnlyPg;
        internal static Page OPERATORPG;
        internal static Page cheatspresetsnow;
        internal static Page bodylognowpage;
        internal static Page bodylognowpagexx;
        internal static Page loadoutpages;
        internal static Page loadoutpagesnow;
        internal static Page colorpresets;
        internal static Page colorpresetsnow;
        internal static Page searchersnow;
        internal static Page FusionProfiles;
        internal static Page FusionProfilesnow;
        internal static Page statskick;
        internal static Page statskicknow;
        internal static Page teleportersnow;
        internal static Page teleporters;
        internal static Page cheatspreset;
        internal static Page pubs;
        internal static Page fppubs;
        internal static Page fpsdespawn;
        internal static Page spawnlimitersz;
        internal static Page levelsearcher;
        internal static Page levelresults;
        internal static Page HOSTONLYPGE;
        internal static Page levelhistory;
        internal static Page protectionstuff;
        internal static Page spawngunesst;
        internal static Page holdinginhands;
        internal static Page playeroptions;
        internal static Page selfrestrictions;
        internal static Page Notifications;
        internal static Page playerjoinlogsnow;
        internal static Page unblockingnow;
        internal static Page WarnedSpawnablesnow;
        internal static Page modidblockednow;
        internal static Page BlockedSpawnablesnow;
        internal static Page SpawnablesKicknow;
        internal static Page blockentirepalletnow;
        internal static Page blockentireauthornow;
        internal static Page permissioneditornow;
        internal static Page OnlineFriends;
        internal static Page Mostusedprotections;
        internal static Page featuresprotection;
        internal static Page SaveTotxtpg;

        internal static System.Collections.Generic.SortedSet<(string Nickname, string Username, string PlatformId, string ExploitType)> ClientExploitLogs = new();
        internal static System.Collections.Generic.SortedSet<(string PlayerName, string Username, ulong PlatformID, string PalletName, string PalletAuthor, int ModioID, string BarcodeID, bool isspawnableavatar)> SpawnLogs = new();
        internal static System.Collections.Generic.HashSet<string> BlockedSpawnables = new();
        internal static System.Collections.Generic.HashSet<string> MediaPlayerLogs = new();
        internal static System.Collections.Generic.HashSet<string> WarnedSpawnables = new();
        internal static System.Collections.Generic.HashSet<PlayerInfo> PlayersOnlines = new();
        internal static System.Collections.Generic.HashSet<string> SpawnablesKick = new();
        internal static System.Collections.Generic.HashSet<string> voicblocked = new();
        internal static System.Collections.Generic.Dictionary<string, int> spawnlimitline = new();
        internal static System.Collections.Generic.HashSet<string> spawnlimitlinelist = new();
        internal static System.Collections.Generic.HashSet<ulong> kicktillrestart = new();
        internal static System.Collections.Generic.HashSet<string> AvatarsKick = new();
        internal static System.Collections.Generic.HashSet<string> blockedplatformids = new();
        internal static System.Collections.Generic.HashSet<string> blockedspawnies = new();
        internal static System.Collections.Generic.Dictionary<PlayerID, System.Collections.Generic.List<string>> despawnresponselogger = new();
        internal static System.Collections.Generic.HashSet<PlayerInfo> JoinLogger = new();
        internal static System.Collections.Generic.HashSet<string> blockmovements = new();
        internal static System.Collections.Generic.HashSet<string> blockmessages = new();
        internal static System.Collections.Generic.HashSet<string> blockedavifallbacks = new();
        internal static System.Collections.Generic.HashSet<string> CustomAvFav = new();
        internal static System.Collections.Generic.HashSet<AvatarCrateReference> CustomAvFavref = new();
        internal static System.Collections.Generic.HashSet<string> CustomSpawnFav = new();
        internal static System.Collections.Generic.HashSet<SpawnableCrateReference> CustomSpawnFavref = new();
        internal static System.Collections.Generic.HashSet<AISpawner> NPCSpawnersNow = new();
        internal static System.Collections.Generic.HashSet<string> modidblocked = new();
        internal static System.Collections.Generic.HashSet<string> blockentirepallet = new();
        internal static System.Collections.Generic.HashSet<string> blockentireauthor = new();
        internal static System.Collections.Generic.HashSet<string> blockavipalletlist = new();
        internal static System.Collections.Generic.HashSet<string> blockaviauthorlist = new();
        internal static System.Collections.Generic.HashSet<string> warnavilist = new();
        internal static System.Collections.Generic.HashSet<string> MEDIAPLAYERBLOCKERNOWList = new();
        internal static System.Collections.Generic.HashSet<AvatarCrateReference> AvatarsStored = new();
        internal static System.Collections.Generic.SortedSet<string> SpawnablesStored = new();
        internal static System.Collections.Generic.SortedSet<string> LevelStored = new();
        internal static System.Collections.Generic.SortedSet<string> AllWeaponsStored = new();
        internal static System.Collections.Generic.SortedSet<string> AllNPCStored = new();
        internal static System.Collections.Generic.SortedSet<string> GunRiflesStored = new();
        internal static System.Collections.Generic.SortedSet<string> GunSMGStored = new();
        internal static System.Collections.Generic.SortedSet<string> GunRangedStored = new();
        internal static System.Collections.Generic.SortedSet<string> GunPistolStored = new();
        internal static System.Collections.Generic.SortedSet<string> GunShotgunStored = new();
        internal static System.Collections.Generic.SortedSet<string> GunSniperStored = new();
        internal static System.Collections.Generic.SortedSet<string> MeleeStored = new();
        internal static System.Collections.Generic.SortedSet<string> MeleeStoredBlunt = new();
        internal static System.Collections.Generic.SortedSet<string> MeleeStoredBlade = new();
        internal static System.Collections.Generic.SortedSet<string> MeleeStoredKnife = new();
        internal static System.Collections.Generic.SortedSet<string> NoTagSpawnables = new();
        internal static System.Collections.Generic.HashSet<IMatchmaker.LobbyInfo> CachedLobbies = new();
        internal static System.Collections.Generic.HashSet<PlayerInfo> PlayersOnline = new();

        internal static bool Invalidstatsnow = true;
        internal static bool spamspawnprevention = true;
        internal static bool notificationspamspawn = true;
        internal static bool AntiModTP = false;
        internal static bool blindplayersprevention = true;
        internal static bool ballplayersprevention = false;
        internal static bool safedistancespawning = false;
        internal static bool base64files = true;
        internal static PlayerInfo StoredNowplayerrrr;
        internal static SimpleTimer DespawnAllTimera;
        internal static bool notificationsofexploits = false;
        internal static bool blockexploitscompletely = true;
        internal static bool antibodylogeffect = true;
        internal static bool strengththresprotection = true;
        internal static bool kickifvitaly = false;
        internal static bool strengthnotif = false;
        internal static float strengththreshnow = 350f;
        internal static float speedthreshold = 120.0f;
        internal static bool TeleportThresHold = true;
        internal static bool spawnlogsmelonlog = false;
        internal static bool homeworldsnow = true;
        internal static bool disablesteamreading = true;
        internal static bool AltRemov = true;
        internal static bool baninsteadalt = false;
        internal static bool AltNotifications = false;
        internal static bool teleportaltacctoyou = false;
        internal static bool clonedetector = true;
        internal static bool togglesavesbool = false;
        internal static bool clientexploitclearonnewserver = false;
        internal static bool spawnlogexploitclearonnewserver = true;
        internal static bool switchlogexploitclearonnewserver = false;
        internal static bool godmode = false;
        internal static bool show = false;
        internal static bool antiknockout = false;
        internal static bool _isDumpRunning = false;
        internal static int timertodo = 0;
        internal static int timeravisafe = 0;
        internal static int timerreloadlevel = 0;
        internal static bool spooferprofiledetection = true;
        internal static bool unlammo = false;
        internal static bool selfconstraint = false;
        internal static bool isSearching = false;
        internal static int maxentrefresh = 10;
        internal static float Timerrefresh = 5;
        internal static int vmaxentrefresh = 10;
        internal static float vTimerrefresh = 5;
        internal static int cmaxentrefresh = 10;
        internal static int kicktime = 30;
        internal static float cTimerrefresh = 5;
        internal static bool DespawnAllTimer = false;
        internal static int DespawnAllTimerMins = 30;
        internal static bool randomizerslzonly = false;
        internal static bool dashingnow = false;
        internal static bool autorunnow = false;
        internal static bool doublejumpnow = false;
        internal static bool Aircontrolnow = false;
        internal static bool alterrornotis = false;
        internal static bool antidespawneffect = false;
        internal static bool blockallspawnslocally = false;
        internal static bool AntiGrab = false;
        internal static bool Bodylogradialcolors = true;
        internal static bool spawnbypassprotection = true;
        internal static float fps = 0;
        internal static float fpslimit = 13f;
        internal static bool fpsdesapwner = false;
        internal static bool servermaxdamagethres = false;
        internal static bool grippy = false;
        internal static string outofboundslobbycode;
        internal static bool outofboundsnow = false;
        internal static bool bodylog = false;
        internal static bool bodylogplayers = false;
        internal static bool AntiDevManip = false;
        internal static bool BLOCKAVATARSASSPAWNABLES = true;
        internal static bool AntiLasereyes = false;
        internal static bool AntiBodyLogGrief = true;
        internal static bool antidecal = false;
        internal static string rejoinlastserver = "";
        internal static bool globalblocklistnotification = true;
        internal static bool globalblocklistnow = true;
        internal static float timerfoeesa = 10;
        internal static bool globalbannotification = true;
        internal static bool DESPAWNPROTECTION = true;
        internal static bool privatekicksteam = false;
        internal static bool AntiGravityChange = false;
        internal static bool hideholsters = false;
        internal static bool hideholstersplayers = false;
        internal static bool infiniteinventory = false;
        internal static bool infiniteinvall = true;
        internal static int bodylogindex = 1;
        internal static int currentbodylogindex = 1;
        internal static bool spawnableskickon = true;
        internal static bool avatarskickon = true;
        internal static bool tpback10seconds = false;
        internal static Vector3 Seconds10back = new(0, 0, 0);
        internal static bool ownerscanchangemap = true;
        internal static bool localonlydevtools = false;
        internal static bool bodylogsending = true;
        internal static string COLORR = "0";
        internal static string COLORG = "255";
        internal static string COLORB = "0";
        internal static string COLORA = "255";
        internal static bool dropallbefore = false;
        internal static bool statkicker = false;
        internal static bool KEEPLOADOUTINVENTORY = false;
        internal static string bansearcher = "";
        internal static bool SpawnGunProtection = false;
        internal static bool showammoalways = false;
        internal static bool personalspace = false;
        internal static float personalspacevalue = 1.8f;
        internal static bool blockspwnnotis = true;
        internal static bool autosavenow = false;
        internal static bool warnavinow = true;
        internal static bool blockaviauthornow = true;
        internal static bool blockavipalletnow = true;
        internal static bool OWNERSCANCHANGESERVER = true;
        internal static bool ownerscanchangegamemode = true;
        internal static bool aviswitchprotection = false;
        internal static bool HideFusionProtector = false;
        internal static bool removesounds = false;
        internal static bool sharebodylogpagenow = true;
        internal static bool sharedevtoolpresets = true;
        internal static SearchMethod searchmethodavatarreal = SearchMethod.CrateNames;
        internal static SearchMethod searchmethodlevelreal = SearchMethod.CrateNames;
        internal static SearchMethod searchspawnabletypereal = SearchMethod.CrateNames;
        internal static handnow handnowreal = handnow.Right;
        internal static PermissionLevel permlevel = PermissionLevel.DEFAULT;
        internal static DespawnerAll DespawnerAllReal = DespawnerAll.AllNotButtonsLeverCircuits;
        internal static DespawnerAll DespawnerAllReal2 = DespawnerAll.AllNotButtonsLeverCircuits;
        internal static DespawnerAll DespawnerTimerAllReal = DespawnerAll.AllNotButtonsLeverCircuits;
        internal static Slots BodySlotReal = Slots.BackRight;
        internal static DespawnerAll DespawnerTimerz = DespawnerAll.AllNotButtonsLeverCircuits;
        internal static SpawnableSearchType spawnablesrchtype = SpawnableSearchType.Spawn;
        internal static AvatarSearchType AvatarSearchTypeReal = AvatarSearchType.ChangeInto;
        internal static Slots SlotsNowReal = Slots.HolsterLeft;
        internal static antimodguntype antimodguntypereal = antimodguntype.AnySpawnGun;

        internal static float scroll = 0f;
        internal static float scrollX = 0f;
        internal static float pscroll = 0f;
        internal static MenuSections PageNow = MenuSections.SelfCat;
        internal static MenuSections PreviousPage = MenuSections.SelfCat;
        internal static string aviSpawnLogsSearcher = "";
        internal static string SpawnLogsSearcher = "";
        internal static string modrecnon = "";
        internal static string modrecmature = "";
        internal static string aviseachnow = "";
        internal static string damagelogsearcher = "";
        internal static string SpawnableSearchies = "";
        internal static string AvatarSearchies = "";
        internal static string avifavsearcher = "";
        internal static string spawnablefavsearcher = "";
        internal static string playerinfospawnlogs = "";
        internal static string lobbyinfospawnlogs = "";
        internal static string playerinfoavatarswitch = "";
        internal static string netentities = "";
        internal static string findem = "";
        internal static string avichnge = "SLZ.BONELAB.Core.Avatar.PeasantFemaleA";
        internal static (string PlayerName, string Username, string PlatformID, string PalletName, string PalletAuthor, string ModioID, string BarcodeID, string IsSpawnableAvatar) SpawnLogsRef;
        internal static (string modname, string thumbnail, string id, string ismature) ModInforecv;
        internal static (string PlayerName, string Username, string PlatformID, string PalletName, string PalletAuthor, string ModioID, string BarcodeID) AvatarSwitchyNow;
        internal static System.Collections.Generic.List<string> SpawnablerResultsNow = new();
        internal static System.Collections.Generic.List<string> AvisResultsNow = new();
        internal static System.Collections.Generic.List<NetworkEntity> ListNetworkEntities = new();
        internal static SpawnableCrateReference serresult;
        internal static AvatarCrateReference avirez;
        internal static SpawnableCrateReference spawnyfavorite;
        internal static AvatarCrateReference avifavorite;
        internal static string MODIOINT = "";
        internal static NetworkPlayer storeynow;
        internal static string[] testholder = new string[9];
        internal static System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>> PlayerSpawningStuff = new();
        internal static System.Collections.Generic.HashSet<SpawnableCrateReference> playersspawnrefs = new();
        internal static SpawnableCrateReference playersspawnrefsstored;
        internal static NetworkEntity nettyspawnedc;
        internal static System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>> PlayeravatarStuff = new();
        internal static System.Collections.Generic.HashSet<AvatarCrateReference> playersavatarrefs = new();
        internal static AvatarCrateReference playersavatarrefsstored;
        internal static int itemsPerPage = 20;
        internal static int currentPage = 0;
        internal static int currentPage2 = 0;
        internal static int currentPage3 = 0;
        internal static CheatTool InstanceOfIt;
        internal static LobbyInfo storedlobby;
        internal static NetworkPlayer storedplz;
        internal static Pallet modiostorednow;
        internal static LobbyInfo lobbyinfofrominstall;
        internal static string storedmedia;
        internal static BanInfo storedban;
        internal static System.Collections.Generic.HashSet<LobbyInfo> ServerHistorys = new();
        internal static float spawnlimitertimer = 0.75f;
        internal static bool spawnlimiternow = false;
        internal static bool hostonlyspawnlimiter = false;
        internal static bool OwnerCheckEnabled = true;
        internal static bool despawndeadnpcs = false;
        internal static bool limitplayercount = false;
        internal static bool BlockPalletCompletely = true;
        internal static bool BlockAuthorOfSpawnable = true;
        internal static bool blockedspawnables = true;
        internal static bool warnedspawnables = true;
        internal static bool ModIDBlocker = true;
        internal static bool donotdisturb = false;
        internal static bool spoofedusernameusername = true;
        internal static bool globalspawnlimitperitem = false;
        internal static int limitnowofglobal = 10;
        internal static int limithostonly = 10;
        internal static string messagenowplayer = "Hi!...";
        internal static float messgfloattime = 2.5f;
        internal static bool fusionprotectedlobby = true;
        internal static bool disablemediaplayers = false;
        internal static bool mediaplayerprotection = true;
        internal static bool spawnprotectionsnot_host = true;
        internal static readonly HarmonyLib.Harmony harmony = new("fp_bonelabsupportpatches");
        internal static bool REMOVEDGLOBALBANLIST = false;
        internal static bool forcegrabdisablernow = false;
        internal static bool disablewindsfx = false;
        internal static bool cleandisconnect = false;
        internal static bool modomatonload = false;
        internal static bool autokickoldfusionprotectorusers = false;
        internal static bool AutoKickSpoofers = false;
        internal static bool fullspawnprotection = false;
        internal static int spawnprotectiontimer = 20;
        internal static PermissionLevel TempLevelNow = PermissionLevel.DEFAULT;
        internal static SimpleTimer EmergencyEscapetimer;
        internal static bool removeproxchat = false;
        internal static bool forcenametagson = false;
        internal static bool preventnotificationlag = true;
        internal static bool spawngunatleastonce = false;
        internal static bool spawngunuialways = true;
        internal static bool keephiddenmods = false;
        internal static bool DeleteLastLobbyMods = false;
        internal static bool nodamageunlessweapons = false;
        internal static bool modidsending = true;
        internal static bool kickunincodenames = false;
        internal static bool AntiAnimatedName = false;
        internal static bool playermessaging = true;
        internal static bool bitsending = true;
        internal static Il2CppSystem.Collections.Generic.List<string> originalbodylog;
        internal static PlayerInfo originalprofiledetails;
        internal static string searchedloggedlobbies = "";
        internal static string searchedloggedPLAYER = "";
        internal static string modiosearchernow = "";
        internal static bool logrecentlymet = false;
        internal static bool logmediaplayer = false;
        internal static bool loglobbiessince = false;
        internal static bool logplayersince = false;
        internal static ModCallbackInfo moddyinfostored;
        internal static bool HIDEPLAYERLIST = false;
        internal static string modiotoken = "";
        internal static bool antioneshot = false;
        internal static int countbeforespam = 10;
        internal static int antispamspawntimer = 1000;
        internal static readonly System.Collections.Generic.Dictionary<InventorySlotReceiver, (string Barcode, string SlotName)> weaponsInInventory = new();
        internal static readonly System.Collections.Generic.List<FileSystemWatcher> watchers = new(); 
        internal static System.Collections.Generic.List<SpoofChecker> StoredJoinPlayers = new();
        internal static System.Collections.Generic.Dictionary<ulong, string> LastKnownUsernames = new();
        internal bool fpsCheckRunning = false;

        #region ProtectionPatches

        [HarmonyPatch(typeof(PlayerRepDamageMessage), "OnHandleMessage")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class AntiOneShotNonHost
        {
            static bool Prefix(ReceivedMessage received)
            {

                if (AreYouOPERATOR())
                {
                    if (antioneshot && !GamemodeManager.IsGamemodeStarted && !NetworkInfo.IsHost)
                    {
                        var data = received.ReadData<PlayerRepDamageData>();
                        var sender = received.Sender;

                        if (!sender.HasValue)
                            return true;

                        if (NetworkPlayerManager.TryGetPlayer(sender.Value, out var networkPlayer))
                        {
                            float damage = data.Attack.Attack.damage;
                            float maxHealth = networkPlayer.RigRefs.Health.max_Health;

                            MelonLogger.Msg($"Damage From {CleanedNAME(networkPlayer)} Damage : {damage} | Max Damage They Can Do : {maxHealth:F1}");

                            // HARD RULE: anything above max health is invalid
                            if (damage > maxHealth)
                                return false;
                        }
                    }
                }
                    return true;
               
            }
        }
        [HarmonyPatch(typeof(PlayerRepTeleportMessage), "OnHandleMessage")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class AntiModerationTeleport
        {
            static bool Prefix(ReceivedMessage received)
            {

                if (AntiModTP)
                {
                    return false;
                }


                return true;
            }
        }
        internal static class HidePlayerState
        {
            public static int CurrentCount = 5; // other players only
        }

        [HarmonyPatch(typeof(LobbyInfo), "get_MaxPlayers")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class HidePlayerList3
        {
            public static bool Prefix(LobbyInfo __instance, ref int __result)
            {
                if (!HIDEPLAYERLIST || __instance != LobbyInfoManager.LobbyInfo)
                    return true;

                __result = HidePlayerState.CurrentCount + 3; // + you
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerIDManager), "get_PlayerCount")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class HidePlayerList2
        {
            public static bool Prefix(ref int __result)
            {
                if (!HIDEPLAYERLIST)
                    return true;

                __result = HidePlayerState.CurrentCount + 1; // + you
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerList), "WritePlayers")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class HidePlayerList
        {
            public static bool Prefix(PlayerList __instance)
            {
                if (!HIDEPLAYERLIST)
                    return true;

                var localPlayer = new PlayerInfo
                {
                    Username = JR_YourNetworkPlayer().JR_Username(),
                    Nickname = JR_YourNetworkPlayer().JR_Nickname(),
                    PlatformID = JR_YourNetworkPlayer().JR_SteamID(),
                    Description = JR_YourNetworkPlayer().JR_Description(),
                    AvatarModID = JR_YourNetworkPlayer().PlayerID.Metadata.AvatarModID.GetValue(),
                    AvatarTitle = JR_YourNetworkPlayer().PlayerID.Metadata.AvatarTitle.GetValue(),
                    PermissionLevel = PermissionLevel.OWNER
                };

                var pool = PlayersOnline
                    .Where(p =>
                        p.PlatformID != localPlayer.PlatformID &&
                        !BanManager.BanList.Bans.Any(x => x.Player.PlatformID == p.PlatformID))
                    .GroupBy(p => p.PlatformID)
                    .Select(g => g.First())
                    .ToList();

                int count = Mathf.Min(HidePlayerState.CurrentCount, pool.Count);

                var randomPlayers = pool
                    .OrderBy(_ => UnityEngine.Random.value)
                    .Take(count)
                    .ToList();

                foreach (var p in randomPlayers)
                    p.PermissionLevel = PermissionLevel.DEFAULT;

                __instance.Players = new PlayerInfo[count + 1];
                __instance.Players[0] = localPlayer;

                for (int i = 0; i < count; i++)
                    __instance.Players[i + 1] = randomPlayers[i];

                return false;
            }
        }
 
        [HarmonyPatch(typeof(ModDownloadManager), nameof(ModDownloadManager.DeleteTemporaryDirectories))]
        internal static class NoDeleteHiddenOrPrivateMods
        {
            public static bool Prefix()
            {
                if (keephiddenmods)
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(ModDownloadManager), "get_ModsTempPath")]
        internal static class NoDeleteHiddenOrPrivateMods2
        {
            public static bool Prefix(ref string __result)
            {
                if (keephiddenmods)
                {
                  __result = Application.persistentDataPath + "/Mods";
                  return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(SpawnablesPanelView), nameof(SpawnablesPanelView.SelectItem))]
        internal static class DoubleClickSpawn
        {
            private static float lastClickTime = 0f;
            private const float doubleClickDelay = 0.35f;

            public static void Postfix()
            {
                if (!spawngunuialways || FusionProtectorPage ==null)
                    return;


                if (!JR_YourGetHand(WhichHand.Left).JR_HandGrabbedSpawnGun() &&
                        !JR_YourGetHand(WhichHand.Right).JR_HandGrabbedSpawnGun())
                {
                    float time = Time.time;

                    if (time - lastClickTime <= doubleClickDelay)
                    {
                        lastClickTime = 0f;


                        var selected = UIRig.Instance?.popUpMenu?.spawnablesPanelView?.selectedObject;
                        var spawnablebarcode = selected?.Barcode?.ID;

                        if (!string.IsNullOrEmpty(spawnablebarcode) && IsBarcodeInGame(spawnablebarcode))
                        {
                            var hand = Player.Head;
                            if (hand == null)
                                return;

                            SpawnIt(
                                spawnablebarcode,
                                hand.transform.position + hand.transform.forward *2 + hand.transform.up,
                                Quaternion.identity
                            );
                        }

                    }
                    else
                    {
                        lastClickTime = time;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(ConstraintCreateMessage), "OnHandleMessage")]
        internal static class BlockSpawnDespawnIncludeConstraints1
        {

            public static bool Prefix(ReceivedMessage received)
            {

                if (NetworkInfo.IsHost)
                {

                    NetworkPlayerManager.TryGetPlayer(received.Sender.Value, out var playerusingexploits);
                    if (blockedspawnies.Contains(playerusingexploits.JR_SteamID().ToString()))
                    {
                        return false;
                    }

                }

                return true;
            }

        }
        [HarmonyPatch(typeof(ConstraintDeleteMessage), "OnHandleMessage")]
        internal static class BlockSpawnDespawnIncludeConstraints2
        {

            internal static bool Prefix(ReceivedMessage received)
            {
                if (NetworkInfo.IsHost)
                {
                    NetworkPlayerManager.TryGetPlayer(received.Sender.Value, out var playerusingexploits);
                    if (blockedspawnies.Contains(playerusingexploits.JR_SteamID().ToString()))
                    {
                        return false;
                    }
                }
                return true;
            }

        }
        [HarmonyPatch(typeof(RigManager), nameof(RigManager.SwitchAvatar))]
        internal static class OnSwapAvatarPatch2
        {

            public static bool Prefix(RigManager __instance)
            {
                if (aviswitchprotection)
                {
                    if (NetworkInfo.IsHost || NetworkInfo.HasServer)
                    {
                        if (__instance == Player.RigManager)
                        {
                            var bodylognow = JR_BodyLog(Player.PhysicsRig).bodylogreturn;

                            if (bodylognow?.ballGrip.GetHand() == JR_YourGetHand(WhichHand.Left) || bodylognow?.ballGrip.GetHand() == JR_YourGetHand(WhichHand.Right))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }

        }
        [HarmonyPatch(typeof(RigManager), nameof(RigManager.SwapAvatarCrate))]
        internal static class OnSwapAvatarPatch3
        {
            public static bool _hasSkippedInitialSwap = false;

            public static bool Prefix(RigManager __instance, ref Barcode barcode)
            {
                if (aviswitchprotection)
                {
                    if (NetworkInfo.IsHost || NetworkInfo.HasServer)
                    {
                        if (__instance != Player.RigManager)
                            return true;

                        if (Player.RigManager == null)
                            return true;

                        if (Player.RigManager.avatar == null ||
                            Player.RigManager.physicsRig == null ||
                            Player.RigManager.physicsRig.leftHand == null ||
                            Player.RigManager.physicsRig.rightHand == null)
                            return true;

                        if (!_hasSkippedInitialSwap)
                        {
                            _hasSkippedInitialSwap = true;
                            return true;
                        }

                        var crateyboy = new AvatarCrateReference(barcode.ID);
                        var barcodenow = barcode.ID;



                        var bodylognow = JR_BodyLog(Player.PhysicsRig).bodylogreturn;

                        if (bodylognow?.ballGrip.GetHand() == JR_YourGetHand(WhichHand.Left) || bodylognow?.ballGrip.GetHand() == JR_YourGetHand(WhichHand.Right))
                        {
                            return true;
                        }
                        else
                        {

                        NotificationNow(
                            FusionProtectorInfo.ClientName,
                            $"Avatar Change Request : {crateyboy.Crate.name} | Author {crateyboy.Crate.Pallet.Author}",
                            NotificationType.INFORMATION,
                            3.5f,
                            true,
                            true,
                            () =>
                            {
                                _hasSkippedInitialSwap = false;
                                ChangeIntoAvi(barcodenow);
                            });
                          barcode = new AvatarCrateReference(Player.RigManager.AvatarCrate.Barcode.ID).Barcode;
                        }

                        
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(RigAvatarSetter), "OnSwapAvatar")]
        internal static class OnSwapAvatarPatch
        {

            public static void Postfix(RigAvatarSetter __instance, bool success)
            {
                if (!success) return;

                var referencesField = typeof(RigAvatarSetter)
                    .GetField("_references", BindingFlags.Instance | BindingFlags.NonPublic);
                var references = referencesField?.GetValue(__instance);
                var rigReferences = references as RigRefs;

                var playernow = NetworkPlayers()
                .FirstOrDefault(p => p.RigRefs?.RigManager == rigReferences.RigManager);


                var barcodeofavi = rigReferences.RigManager.AvatarCrate.Scannable.Barcode.ID;
                var reference = new AvatarCrateReference(barcodeofavi);
                var cratemodid = CrateFilterer.GetModID(reference.Crate.Pallet);


                if (playernow == null)
                    return;

                if (references != null)
                {      
                    FusionPermissions.FetchPermissionLevel(playernow.JR_SteamID(), out var PersonsLevel, out var therecolor);
           
                    if (PersonsLevel != PermissionLevel.OWNER && HostIsMe(playernow) || !HostIsMe(playernow))
                    {
                        if (warnavinow)
                        {
                            if (warnavilist.Contains(barcodeofavi))
                            {

                                NetworkSpawnerNotif(playernow, $"Warning Avatar : {StripColorTags(reference.Crate.name)} [{reference.Crate.Pallet.Author}]", NotificationType.WARNING, 2.5f);
                            }
                        }
                        if (blockedavifallbacks.Contains(barcodeofavi))
                        {
                            NetworkSpawnerNotif(playernow, $"Avatar Blocked {barcodeofavi.JR_BarcodeCrateName()}", NotificationType.ERROR, 2.5f);
                            playernow.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));
                        }
                        if (blockaviauthornow)
                        {

                            if (blockaviauthorlist.Contains(reference.Crate.Pallet.Author))
                            {
                                NetworkSpawnerNotif(playernow, $"Blocked Avatar Author : {reference.Crate.Pallet.Author}", NotificationType.ERROR, 1.5f);
                                playernow.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));

                            }
                        }
                        if (blockavipalletnow)
                        {

                            if (blockavipalletlist.Contains(StripColorTags(reference.Crate.Pallet.name)))
                            {
                                NetworkSpawnerNotif(playernow, $"Blocked Avatar Pallet : {StripColorTags(reference.Crate.Pallet.name)}", NotificationType.ERROR, 1.5f);
                                playernow.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));

                            }
                        }
                        if (globalblocklistnow)
                        {
                            if (SiteStuff.globalblocklistavatar.Contains(barcodeofavi))
                            {
                                if (globalblocklistnotification)
                                {
                                    NetworkSpawnerNotif(playernow, $"[FP] Global Blacklisted Avatar Blocked : {barcodeofavi}", NotificationType.ERROR, 1.5f);
                                    playernow.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));
                                }
                            }

                            if (SiteStuff.globalaviblocklistmodioid.Contains(cratemodid))
                            {
                                if (globalblocklistnotification)
                                {
                                    NetworkSpawnerNotif(playernow, $"[FP] Global Blacklisted Avatar Mod.IO : {cratemodid}", NotificationType.ERROR, 1.5f);
                                    playernow.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));
                                }
                            }

                            if (SiteStuff.globalaviblocklistpallet.Contains(StripColorTags(reference.Crate.Pallet.name)))
                            {
                                if (globalblocklistnotification)
                                {
                                    NetworkSpawnerNotif(playernow, $"[FP] Global Blacklisted Avatar Pallet : {barcodeofavi?.JR_BarcodePalletName()}", NotificationType.ERROR, 1.5f);
                                    playernow.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));
                                }
                            }

                            if (SiteStuff.globalaviblocklistauthor.Contains(reference.Crate.Pallet.Author))
                            {
                                if (globalblocklistnotification)
                                {
                                    NetworkSpawnerNotif(playernow, $"[FP] Global Blacklisted Avatar Author : {barcodeofavi?.JR_BarcodeAuthor()}", NotificationType.ERROR, 1.5f);
                                    playernow.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));
                                }
                            }
                        }
                        if (IsAvatarCrateExist(barcodeofavi) && SiteStuff.blockednsfw.Contains(barcodeofavi))
                        {
                            MelonLogger.Warning($"NSFW Protection\nReport User : {playernow?.JR_Nickname()}\nAvatar Pallet: {barcodeofavi.JR_BarcodePalletName()}");
                            NetworkSpawnerNotif(playernow, $"NSFW Protection\nReport Avatar Pallet! => {barcodeofavi.JR_BarcodePalletName()}");
                            SpawnEffects.CallDespawnEffect(playernow?.MarrowEntity);
                            playernow.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));
                        }


                    }
                }
            }
        }
        [HarmonyPatch(typeof(LocalHealth), "OnRespawned")]
        internal static class SpawnProtectionPatch
        {
            public static bool spawnProtection = false;
            public static IEnumerator SpawnProtection(float timeforit)
            {
                spawnProtection = true;
                yield return new WaitForSecondsRealtime(timeforit);
                spawnProtection = false;
            }
            public static void Postfix()
            {
                if (fullspawnprotection && !GamemodeManager.IsGamemodeStarted)
                {
                    MelonCoroutines.Start(SpawnProtection(spawnprotectiontimer));
                }
            }
        }
        [HarmonyPatch(typeof(LocalRagdoll), "KnockoutCoroutine")]
        internal static class SpawnProtectionPatch2
        {

            public static IEnumerator Postfix(IEnumerator __result)
            {
                while (__result.MoveNext())
                {
                    yield return __result.Current;
                }
                if (fullspawnprotection && !GamemodeManager.IsGamemodeStarted)
                {
                    MelonCoroutines.Start(SpawnProtectionPatch.SpawnProtection(5.0f));
                }
            }

        }
        [HarmonyPatch(typeof(PlayerSender), nameof(PlayerSender.SendPlayerDamage))]
        [HarmonyPatch(new[] { typeof(byte),typeof(Attack) })]
        internal static class SpawnProtectionPatch3
        {

            public static bool Prefix(byte target, Attack attack)
            {
                if (fullspawnprotection && !GamemodeManager.IsGamemodeStarted && SpawnProtectionPatch.spawnProtection)
                {
                    return false;
                }


                return true;
            }

        }
        [HarmonyPatch(typeof(PlayerSender), nameof(PlayerSender.SendPlayerDamage))]
        [HarmonyPatch(new[] { typeof(byte), typeof(Attack),typeof(PlayerDamageReceiver.BodyPart)})]
        internal static class SpawnProtectionPatch4
        {

            public static bool Prefix(byte target, Attack attack)
            {
                if (fullspawnprotection && !GamemodeManager.IsGamemodeStarted && SpawnProtectionPatch.spawnProtection)
                {
                    return false;
                }

                return true;
            }

        }
        [HarmonyPatch(typeof(RandomCodeGenerator), nameof(RandomCodeGenerator.GetString))]
        internal static class RandomCodeGeneratorPatch
        {
            private static readonly System.Collections.Generic.HashSet<string> ExistingFusionCodes = new();

            public static void Postfix(ref string __result)
            {
                if (HideFusionProtector)
                    return;

                if (!fusionprotectedlobby || !NetworkInfo.HasLayer)
                    return;


                NetworkLayerManager.Layer.Matchmaker?.RequestLobbies(result =>
                {
                    ExistingFusionCodes.Clear();

                    foreach (var lobby in result.Lobbies)
                    {
                        var code = lobby.Metadata.LobbyInfo.LobbyCode;

                        if (code.StartsWith("FP-"))
                            ExistingFusionCodes.Add(code);
                    }
                });

                // generate FP0-999 unique code
                string newCode;
                do
                {
                    newCode = "FP-" + UnityEngine.Random.Range(0, 1000);
                }
                while (ExistingFusionCodes.Contains(newCode));

                __result = newCode;
                ExistingFusionCodes.Add(newCode);
            }
        }
        [HarmonyPatch(typeof(DownloadNotifications), nameof(DownloadNotifications.SendDownloadNotification))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class LogDownloads
        {
          
            public static System.Collections.Generic.Dictionary<LobbyInfo, System.Collections.Generic.List<Pallet>> DownloadLogger = new();
            public static System.Collections.Generic.List<Pallet> DeleteThesOnLeave = new();
            static void Prefix(string palletTitle)
            {
                var manifests = AssetWarehouse.Instance?.GetPalletManifests()?.ToArray();
                if (manifests == null)
                    return;

                var pallet = manifests
                    .FirstOrDefault(m => m.Pallet != null && m.Pallet.Title == palletTitle)
                    ?.Pallet;

                if (pallet == null)
                    return;

                if (!DownloadLogger.TryGetValue(LobbyInfoManager.LobbyInfo, out var palletList))
                {
                    palletList = new System.Collections.Generic.List<Pallet>();
                    DownloadLogger[LobbyInfoManager.LobbyInfo] = palletList;
                }

                palletList.Remove(pallet);
                palletList.Insert(0, pallet);


                DeleteThesOnLeave.Remove(pallet);
                DeleteThesOnLeave.Insert(0, pallet);
                MelonCoroutines.Start(LoadAssetsEnum(randomizerslzonly,false));
            }
        }
        
        [HarmonyPatch(typeof(ConnectionSender), nameof(ConnectionSender.SendDisconnect))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class KickedAndBanned
        {
            public static void Postfix(ulong platformID, string reason = "")
            {
                if (reason == "Banned from Server")
                {
                    NotificationNow(FusionProtectorInfo.ClientName, $"{platformID} Got Banned!", NotificationType.WARNING, 3.0f);
                }

                if (reason == "Kicked from Server")
                {
                    NotificationNow(FusionProtectorInfo.ClientName, $"{platformID} Got Kicked!", NotificationType.WARNING, 3.0f);
                }

            }

        }
        
        [HarmonyPatch(typeof(NetworkHelper), nameof(NetworkHelper.PardonUser))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class PardonMessage
        {
            public static void Postfix(ulong longId)
            {
                NotificationNow(FusionProtectorInfo.ClientName, $"{longId} Got Unbanned!", NotificationType.SUCCESS, 3.0f);
            }
        }
        [HarmonyPatch(typeof(PageView), nameof(PageView.CoSummonAnimation))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class ColorRadialMenuCool
        {

            public static void Postfix()
            {

                if (!Bodylogradialcolors)
                    return;

                var popUpMenu = UIRig.Instance.popUpMenu;


                foreach (var itemnow in popUpMenu.radialPageView.buttons)
                {

                    int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[12], out int radialr);
                    int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[13], out int radialg);
                    int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[14], out int radialb);
                    int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[15], out int radiala);


                    var colorexact = ColorClasseyPoo.ConvertRGBAToUnityColor(radialr, radialg, radialb, radiala);

                    itemnow.color2 =  colorexact;
                    itemnow.textMesh.color = colorexact;
                    itemnow.icon.GetComponent<UnityEngine.CanvasRenderer>().SetColor(colorexact);

                }

                var rig = Player.PhysicsRig;
                if (rig == null) return;

                var bodyLog = JR_BodyLog(rig);
                var bodyReturn = bodyLog.bodylogreturn;
                if (bodyReturn == null) return;


                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[0], out int bodycolorr);
                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[1], out int bodycolorg);
                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[2], out int bodycolorb);
                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[3], out int bodycolora);
                bodyReturn.hologramTint = ColorClasseyPoo.ConvertRGBAToUnityColor(bodycolorr, bodycolorg, bodycolorb, bodycolora);



                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[4], out int bodyballcolorr);
                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[5], out int bodyballcolorg);
                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[6], out int bodyballcolorb);
                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[7], out int bodyballcolora);
                bodyReturn.transform.Find("spheregrip/Sphere/Art/GrabGizmo").GetComponent<UnityEngine.MeshRenderer>().material.color = ColorClasseyPoo.ConvertRGBAToUnityColor(bodyballcolorr, bodyballcolorg, bodyballcolorb, bodyballcolora);

                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[8], out int bodycolorliner);
                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[9], out int bodycolorlineg);
                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[10], out int bodycolorlineb);
                int.TryParse(BodyLogRadialMenuColorPreset.CurrentPreset[11], out int bodycolorlinea);

                var linecolornow = ColorClasseyPoo.ConvertRGBAToUnityColor(bodycolorliner, bodycolorlineg, bodycolorlineb, bodycolorlinea);

                bodyReturn.lineRenderer?.SetColors(linecolornow, linecolornow);


            }

        }
        [HarmonyPatch(typeof(CheatTool), "Start")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class CustomDevTools
        {
            public static bool Prefix(CheatTool __instance)
            {
                InstanceOfIt = __instance;

                try
                {
                    if (File.Exists(devitemscurrent))
                    {
                        var currentJson3 = File.ReadAllText(devitemscurrent);
                        var current3 = JsonConvert.DeserializeObject<CreateCheatToolsPreset>(currentJson3);
                        if (current3 != null)
                            CurrentPresetNow = current3;
                    }


                    var preset = CreateCheatToolsPreset.CurrentPresetNow;

                    var items = new[]
                    {
    preset.Item1,
    preset.Item2,
    preset.Item3,
    preset.Item4,
    preset.Item5
};

                    foreach (var item in items)
                    {
                        if (item.ModIoID is int modId && modId != -1)
                        {
                            if (!IsBarcodeInGame(item.BarcodeId))
                            {
                               DownloadModIOMod(modId,false);
                            }
                        }
                    }



                    InstanceOfIt.crates = new[]
                    {
                     new SpawnableCrateReference(CreateCheatToolsPreset.CurrentPresetNow.Item1.BarcodeId),
                     new SpawnableCrateReference(CreateCheatToolsPreset.CurrentPresetNow.Item2.BarcodeId),
                     new SpawnableCrateReference(CreateCheatToolsPreset.CurrentPresetNow.Item3.BarcodeId),
                     new SpawnableCrateReference(CreateCheatToolsPreset.CurrentPresetNow.Item4.BarcodeId),
                     new SpawnableCrateReference(CreateCheatToolsPreset.CurrentPresetNow.Item5.BarcodeId)
                    };
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Preset apply failed: {ex}");
                }

                return true;
            }
        }


        //bonelabsuppportdll patches
        internal static class CustomDevToolsFusion
        {
            public static bool Prefix(PopUpMenuView menu)
            {

                if (!NetworkSceneManager.IsLevelNetworked)
                {
                    return true;
                }

                var transform = menu.radialPageView.transform;


                void spawnnow(CreateCheatToolsPreset.Item itemnow)
                {
                    if (itemnow.ModIoID !=-1)
                    {
                        if (!IsBarcodeInGame(itemnow.BarcodeId))
                        {
                            DownloadModIOMod(itemnow.ModIoID, false);
                        }
                    }

                    var newspawnable = new Spawnable() { crateRef = new(itemnow.BarcodeId) };

                    if (localonlydevtools ? true : !itemnow.LocalSpawn)
                    {

                        var spawnableinfo = new NetworkAssetSpawner.SpawnRequestInfo()
                        {
                            Spawnable = newspawnable,
                            Position = transform.position,
                            Rotation = transform.rotation,
                            SpawnSource = EntitySource.Player

                        };

                        NetworkAssetSpawner.Spawn(spawnableinfo);
                    }
                    else
                    {
                        LocalAssetSpawner.Register(newspawnable);
                        LocalAssetSpawner.Spawn(newspawnable, transform.position, transform.rotation, callbackpoole =>
                        {
                        });
                    }
                }


                spawnnow(CreateCheatToolsPreset.CurrentPresetNow.Item1);
                spawnnow(CreateCheatToolsPreset.CurrentPresetNow.Item2);
                spawnnow(CreateCheatToolsPreset.CurrentPresetNow.Item3);
                spawnnow(CreateCheatToolsPreset.CurrentPresetNow.Item4);
                spawnnow(CreateCheatToolsPreset.CurrentPresetNow.Item5);
                return false;
            }

        }
        //this patches the lag lobby from other montana client users also it stops the effect completely for you imo its a stupid effect anyways might as well nop the entire thing lol
        internal static class BodyLogEffect
        {
            public static bool Prefix(ReceivedMessage received)
            {
                if (antibodylogeffect)
                {
                    return false;
                }
                return true;
            }
        }
        //


        [HarmonyPatch(typeof(PullCordDevice), nameof(PullCordDevice.SwapAvatar))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class BodylogSwapFunctionPatch
        {
            static bool Prefix()
            {
                if (AntiBodyLogGrief)
                {
                    var bodylognow = JR_BodyLog(Player.PhysicsRig).bodylogreturn;


                    if (bodylognow?.ballGrip.GetHand() == JR_YourGetHand(WhichHand.Left) || bodylognow?.ballGrip.GetHand() == JR_YourGetHand(WhichHand.Right))
                    {
                        return true;
                    }


                    return false;
                }


                return true;
            }

        }
        [HarmonyPatch(typeof(LevelRequestMessage), "OnHandleMessage")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class MakeOWNERSAutoChangeMap
        {

            public static bool Prefix(ReceivedMessage received)
            {
                var sender = received.Sender;

                if (!sender.HasValue)
                {
                    return false;
                }

                var data = received.ReadData<LevelRequestData>();
              
                var id = PlayerIDManager.GetPlayerID(sender.Value);

                if (id != null && id.TryGetDisplayName(out var name))
                {
                    if (ownerscanchangemap)
                    {
                        FusionPermissions.FetchPermissionLevel(id, out var permyXc, out _);
                        if (FusionPermissions.HasSufficientPermissions(permyXc, PermissionLevel.OWNER))
                        {
                            if (IsBarcodeInGame(data.Barcode))
                            {
                                SceneStreamer.Load(new Barcode(data.Barcode));
                            }
                            else
                            {
                                NotificationNow(FusionProtectorInfo.ClientName, $"You Don't Have This [{StripColorTags(data.Title)}]", NotificationType.WARNING, 3.0f);
                            }
                        }
                        else
                        {


                            LabFusion.UI.Popups.Notifier.Send(new LabFusion.UI.Popups.Notification()
                            {
                                Title = $"{StripColorTags(data.Title)} Load Request",
                                Message = new LabFusion.UI.Popups.NotificationText($"{name} has requested to load {StripColorTags(data.Title)}.", Color.yellow),

                                SaveToMenu = true,
                                ShowPopup = true,
                                OnAccepted = () =>
                                {
                                    SceneStreamer.Load(new Barcode(data.Barcode));
                                },
                            });
                        }
                    }
                    else
                    {
                        LabFusion.UI.Popups.Notifier.Send(new LabFusion.UI.Popups.Notification()
                        {
                            Title = $"{StripColorTags(data.Title)} Load Request",
                            Message = new LabFusion.UI.Popups.NotificationText($"{name} has requested to load {StripColorTags(data.Title)}.", Color.yellow),

                            SaveToMenu = true,
                            ShowPopup = true,
                            OnAccepted = () =>
                            {
                                SceneStreamer.Load(new Barcode(data.Barcode));
                            },
                        });
                    }

                }

                return false;
            }


        }
        [HarmonyPatch(typeof(Il2CppSLZ.VFX.DecalProjector), nameof(Il2CppSLZ.VFX.DecalProjector.Awake))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class AntiDecals
        {

            public static bool Prefix()
            {
                if (antidecal)
                {
                    return false;
                }
                return true;
            }


        }
        [HarmonyPatch(typeof(InteractableIcon), nameof(InteractableIcon.Awake))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class RemoveIconsProtect
        {

            public static bool Prefix()
            {
                if (grippy)
                {
                    return false;
                }
                return true;
            }


        }
        [HarmonyPatch(typeof(FusionPlayer), "CheckFloatingPoint")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class outofbounds
        {
            public static bool Prefix(ref bool ____brokeBounds)
            {


                var rm = RigData.Refs.RigManager;
                var position = rm.physicsRig.feet.transform.position;

                if (NetworkTransformManager.IsInBounds(position))
                {
                    return false;
                }

                outofboundslobbycode = LobbyInfoManager.LobbyInfo.LobbyCode;
                Physics.autoSimulation = false;
                LocalPlayer.TeleportToCheckpoint();
                ____brokeBounds = true;
                outofboundsnow = true;

                if (NetworkInfo.HasServer && !NetworkInfo.IsHost)
                {
                    NetworkHelper.Disconnect("Left Bounds");
                }

                SceneStreamer.Reload();
                return false;
            }
        }
        [HarmonyPatch(typeof(SpawnEffects), nameof(SpawnEffects.CallDespawnEffect))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class AntiDespawneffectnow
        {
            static bool Prefix()
            {
                if (antidespawneffect)
                {
                    return false;
                }


                return true;
            }

        }
        [HarmonyPatch(typeof(URLWhitelistManager), "IsURLWhitelisted")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class ModifyWhitelistJR
        {
            static bool Prefix(string url)
            {
                var list = DataSaver.ReadJsonFromText<URLWhitelist>(SiteStuff.custommediadoms);

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return true;
                }

                bool isLink = uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;

                if (!isLink)
                {
                    return true;
                }

                var domain = uri.Host;

                foreach (var whitelist in list.Whitelist)
                {
                    if (domain == whitelist.Domain)
                    {
                        return true;
                    }
                }

                return false;

            }

        }

        //this is to protect against misunderstood out of context clips and nsfw or other related links to help keep fusion more safe
        [HarmonyPatch(typeof(VideoPlayer))]
        internal static class MediaPlayerProtection
        {
            private static bool IsLinkBlocked(string url)
            {
                if (string.IsNullOrWhiteSpace(url))
                    return false;

                var base64url = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url));
                
                return SiteStuff.MediaPlayerProtectionnow.Any(p => p.Link == base64url);
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(VideoPlayer.Prepare))]
            [HarmonyPriority(int.MaxValue - 1)]
            public static bool PrepPrefix(VideoPlayer __instance)
            {
                if (__instance == null)
                    return true;


                if (disablemediaplayers)
                {
                    __instance.Stop();
                    return false;
                }

                if (!NetworkSceneManager.IsLevelNetworked)
                    return true;

                if (__instance.source == VideoSource.Url)
                {
                    string url = __instance.url;
               
                    if (string.IsNullOrEmpty(url))
                        return true;
 
                    if (IsLinkBlocked(url))
                    {
                       return false;
                    }

                    if (MEDIAPLAYERBLOCKERNOWList.Contains(url))
                    {
                      return false;
                    }


                    if (!URLWhitelistManager.IsURLWhitelisted(url))
                    {
                        return false;
                    }
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(VideoPlayer.url))]
            [HarmonyPriority(int.MaxValue - 1)]
            [HarmonyPatch(MethodType.Setter)]
            public static void SetItURLPrefix(ref string value)
            {
                if (disablemediaplayers)
                {
                    value = string.Empty;
                    return;
                }

                if (!NetworkSceneManager.IsLevelNetworked || string.IsNullOrEmpty(value))
                    return;


                if (mediaplayerprotection)
                {
                    if (!IsLinkBlocked(value))
                    {
                        if (!MediaPlayerLogs.Contains(value))
                        {
                            MediaPlayerLogs.Add(value);
                        }
                    }
                    string encodedValue = value;

                    var actuallink = SiteStuff.MediaPlayerProtectionnow.FirstOrDefault(p => p.Link.ToLower() == Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(encodedValue)).ToLower());

                    if (!IsLinkBlocked(value))
                    {
                        MelonLogger.Warning($"[Fusion Protector] Media Player Triggered : [{value}]");
                    }

                    if (IsLinkBlocked(value))
                    {
                        MelonLogger.Warning($"[Fusion Protector Media Protection] Blocked Media [{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))}]\nReason : {actuallink.Reason}");
                        NotificationNow(FusionProtectorInfo.ClientName, $"[Fusion Protector Media Protection] Blocked Media!\nReason : {actuallink.Reason}", NotificationType.WARNING, 4.0f);
                        value = string.Empty;
                        return;
                    }

                    if (MEDIAPLAYERBLOCKERNOWList.Contains(value))
                    {
                        NotificationNow(FusionProtectorInfo.ClientName, $"[Fusion Protector Media Protection] Blocked Media!", NotificationType.WARNING, 4.0f);
                        value = string.Empty;
                        return;
                    }

                }

                if (!URLWhitelistManager.IsURLWhitelisted(value))
                {
                    value = string.Empty;
                }
            }
        }

        //this works when u connect to lobby
        [HarmonyPatch(typeof(MenuLocation), "ApplyPlayerToElement")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class InGame
        {
            public static PermissionLevel TempLevel = PermissionLevel.DEFAULT;
            public static void SendSettingToServer(string serversetting, object valuedefault)
            {
                if (TimeReferences.TimeSinceStartup - OwnerServerSettingMessage._timeOfRequest <= OwnerServerSettingMessage._requestCooldown)
                {
                    return;
                }


                var ownerservdata = OwnerServerSettingData.Create(
PlayerIDManager.LocalSmallID,
serversetting,
valuedefault
);

                MessageRelay.RelayModule<OwnerServerSettingMessage, OwnerServerSettingData>(
            ownerservdata,
            CommonMessageRoutes.ReliableToServer
            );
            }
            public static bool Prefix(PlayerElement element, PlayerID player)
            {
                var username = TextFilter.SanitizeName(player.Metadata.Username.GetValue());

                element.UsernameElement.Title = username;

                element.NicknameElement.Title = "Nickname";
                element.NicknameElement.Value = TextFilter.SanitizeName(player.Metadata.Nickname.GetValue());

                element.NicknameElement.Interactable = false;
                element.NicknameElement.EmptyFormat = "No {0}";

                element.DescriptionElement.Title = "Description";
                element.DescriptionElement.Value = TextFilter.SanitizeName(player.Metadata.Description.GetValue());
                element.DescriptionElement.Interactable = false;
                element.DescriptionElement.EmptyFormat = "No {0}";

                var avatarTitle = player.Metadata.AvatarTitle.GetValue();
                var modId = player.Metadata.AvatarModID.GetValue();

                ElementIconHelper.SetProfileIcon(element, avatarTitle, modId);



                FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var selfLevel, out _);




                PermissionLevel level;
                Color color;

                FusionPermissions.FetchPermissionLevel(player.PlatformID, out level, out color);

                var user = PermissionList.PermittedUsers
                    .FirstOrDefault(u => u.Item1.Equals(player.PlatformID));

                if (user != null)
                {
                    level = user.Item3;
                }

                var activeLobbyInfo = LobbyInfoManager.LobbyInfo;

                if (player.IsMe)
                {
                    element.VolumeElement.gameObject.SetActive(false);
                }
                else
                {
                    var volumeElement = element.VolumeElement
                        .Cleared()
                        .WithTitle("Volume")
                        .WithIncrement(0.1f)
                        .WithLimits(0f, 2f);

                    volumeElement.gameObject.SetActive(true);

                    volumeElement.Value = ContactsList.GetContact(player).volume;
                    volumeElement.OnValueChanged += (v) =>
                    {
                        var contact = ContactsList.GetContact(player);
                        contact.volume = v;
                        ContactsList.UpdateContact(contact);
                    };
                }

                var permissionsElement = element.PermissionsElement
                    .Cleared()
                    .WithTitle(NetworkInfo.IsHost ? "Permissions" : "Your Server Permissions")
                    .WithColor(Color.yellow);

                permissionsElement.EnumType = typeof(PermissionLevel);
                permissionsElement.Value = NetworkInfo.IsHost ? level : user?.Item3 ?? permissionsElement.Value;

                permissionsElement.OnValueChanged += (v) =>
                {
                    FusionPermissions.TrySetPermission(player.PlatformID, username, (PermissionLevel)v);
                };

                permissionsElement.Interactable = !player.IsMe;

                LabFusion.Marrow.Proxies.StringElement platformIDElement;

                if (player.IsMe)
                {
                    platformIDElement = element.PlatformIDElement
                   .Cleared()
                   .WithTitle("[YOU] Steam ID")
                   .WithColor(Color.yellow)
                   .WithInteractability(false);
                }
                else
                {
                    platformIDElement = element.PlatformIDElement
                   .Cleared()
                   .WithTitle("Steam ID")
                   .WithColor(Color.yellow)
                   .WithInteractability(false);
                }
                platformIDElement.Value = player.PlatformID.ToString();


                element.ActionsElement.Clear();
                var actionsPage = element.ActionsElement.AddPage();

                if (!NetworkInfo.IsHost)
                {
                    actionsPage.AddElement<ButtonElement>($"Current Server Perms : {level}")
                    .WithColor(Color.cyan)
                    .WithInteractability(false);
                }
                if (NetworkHelper.IsBanned(player.PlatformID))
                {
                    actionsPage.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Banned From Your Lobby : YES")
                    .WithColor(Color.red)
                    .WithInteractability(false);
                }

                if (user != null)
                {
                    actionsPage.AddElement<LabFusion.Marrow.Proxies.ButtonElement>($"Permission In Your Server : {user.Item3}")
                    .WithColor(Color.red)
                    .WithInteractability(false);
                }

                InvokeWithType(typeof(MenuLocation), "AddModerationGroup", new object[] { activeLobbyInfo, actionsPage, player, selfLevel, level });

                if (player.PlatformID != SteamIdYours())
                {
                    var actionsPage2 = actionsPage.AddElement<GroupElement>("Protector Options");



                    if (NetworkInfo.IsHost)
                    {

                        actionsPage2.AddElement<FunctionElement>($"Kick For {kicktime} Mins")
                        .WithColor(Color.yellow)
                        .Do(() =>
                        {
                            if (NetworkPlayerManager.TryGetPlayer(player, out var playie))
                            {



                                var timernow = new TimedObject(playie.JR_SteamID(), kicktime);
                                NetworkHelper.KickUser(playie.PlayerID);
                            }
                        });


                        actionsPage2.AddElement<FunctionElement>($"Kick Until Server Restarts")
                        .WithColor(Color.yellow)
                        .Do(() =>
                        {
                          if (NetworkPlayerManager.TryGetPlayer(player, out var playie))
                          {


                                if (!kicktillrestart.Contains(playie.JR_SteamID()))
                                {
                                    kicktillrestart.Add(playie.JR_SteamID());
                                    NetworkHelper.KickUser(playie.PlayerID);
                                }
                          }
                        });


                    }


                    actionsPage2.AddElement<FunctionElement>("Add/Remove Avatar To Avatar Blocker")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {
                        if (NetworkPlayerManager.TryGetPlayer(player, out var playie))
                        {
                            if (playie?.JR_PlayersAvatarBarcodeID() != "c3534c5a-94b2-40a4-912a-24a8506f6c79")
                            {

                                ToggleAddRemoveFromFile(playie?.JR_PlayersAvatarBarcodeID(), blockedavifallbacks, avatarsblocked, FusionProtectorInfo.ClientName, $"Added {playie?.JR_PlayersAvatarBarcodeID().JR_BarcodeCrateName()} To Avatar Blocker!.", $"Removed {playie?.JR_PlayersAvatarBarcodeID().JR_BarcodeCrateName()} From Avatar Blocker!.");
                            }
                            else
                            {
                                NotificationNow(FusionProtectorInfo.ClientName, "Can't Do That!", NotificationType.ERROR, 2.5f);
                            }
                        }
                    });

                    actionsPage2.AddElement<FunctionElement>("Clone Avatar")
                   .WithColor(Color.yellow)
                   .Do(() =>
                   {
                       if (NetworkPlayerManager.TryGetPlayer(player, out var playie))
                       {
                           ChangeIntoAvi(playie.JR_PlayersAvatarBarcodeID());
                       }
                   });
                    actionsPage2.AddElement<FunctionElement>("Copy Avatar Details")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {
                        if (NetworkPlayerManager.TryGetPlayer(player, out var playie))
                        {
                            var spawnableavi = new SpawnableCrateReference(playie.JR_PlayersAvatarBarcodeID());

                            GUIUtility.systemCopyBuffer = $"Barcode : {spawnableavi.Barcode.ID}\nCrate : {StripColorTags(spawnableavi.Crate.name)} Author [{spawnableavi.Crate.Pallet.Author}]\nPallet It's In : {StripColorTags(spawnableavi.Crate.Pallet.name)}";
                        }
                    });
                    actionsPage2.AddElement<FunctionElement>("Local Protection Avi")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {
                        if (NetworkPlayerManager.TryGetPlayer(player, out var playie))
                        {
                            playie.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));
                            SpawnEffects.CallDespawnEffect(playie?.MarrowEntity);
                        }
                    });
                    actionsPage2.AddElement<FunctionElement>("UnBan/Ban From Your Lobbies")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {
                        if (!NetworkHelper.IsBanned(player.PlatformID))
                        {
                            BanInfo item = new()
                            {
                                Player = new PlayerInfo { Username = player.Metadata.Username.GetValue(), Nickname = player.Metadata.Nickname.GetValue(), PlatformID = player.PlatformID, Description = player.Metadata.Description.GetValue(), AvatarModID = player.Metadata.AvatarModID.GetValue(), AvatarTitle = player.Metadata.AvatarTitle.GetValue() },
                                Reason = $"Manually Banned [{FusionProtectorInfo.ClientName}]"
                            };
                            BanManager.BanList.Bans.RemoveAll((BanInfo info2) => info2.Player.PlatformID == player.PlatformID);
                            BanManager.BanList.Bans.Add(item);
                            DataSaver.WriteJsonToFile("bans.json", BanManager.BanList);

                            NotificationNow(FusionProtectorInfo.ClientName, "Banned Player", NotificationType.SUCCESS);
                        }
                        else
                        {
                            BanManager.Pardon(player.PlatformID);
                            NotificationNow(FusionProtectorInfo.ClientName, "UnBanned Player", NotificationType.SUCCESS);

                        }
                    });
                    actionsPage2.AddElement<FunctionElement>("Copy All Profile To Clipboard")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {


                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true
                        };

                        string nowplayerinfo = JsonSerializer.Serialize(player, options);
                        GUIUtility.systemCopyBuffer = nowplayerinfo;
                        NotificationNow(FusionProtectorInfo.ClientName, "Copied Players Entire Details To Clipboard", NotificationType.SUCCESS);
                    });
                    actionsPage2.AddElement<FunctionElement>("Copy Steam ID")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {
                        GUIUtility.systemCopyBuffer = player.PlatformID.ToString();
                        NotificationNow(FusionProtectorInfo.ClientName, "Copied Steam ID", NotificationType.SUCCESS);
                    });
                    actionsPage2.AddElement<FunctionElement>("Open Steam Profile")
                     .WithColor(Color.yellow)
                     .Do(() =>
                     {
                         CheckSteamID(player.PlatformID);
                     });

                    if (selfLevel == PermissionLevel.OWNER)
                    {
                        var actionsPage4 = actionsPage.AddElement<GroupElement>("Protector Owner Options");

                        actionsPage4.AddElement<FunctionElement>("Clear Constraints")
                        .WithColor(Color.yellow)
                        .Do(() =>
                        {
                            if (NetworkPlayerManager.TryGetPlayer(player, out var playie))
                            {
                                ClearConstraints(playie);
                            }
                        });

                        actionsPage4.AddElement<FunctionElement>("Teleport All")
                        .WithColor(Color.yellow)
                        .Do(() =>
                        {
                            if (FusionPermissions.HasSufficientPermissions(selfLevel, activeLobbyInfo.Teleportation))
                            {
                                foreach (var playerId in PlayerIDManager.PlayerIDs)
                                {
                                    if (playerId != PlayerIDManager.LocalSmallID)
                                        PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_ME, playerId);
                                }
                            }
                        });


                    }

                    var actionsPage3 = actionsPage.AddElement<GroupElement>(NetworkInfo.IsHost ? "Protector Server Options" : "Protector Host Only Options");

                    if (voicblocked.Contains(player.PlatformID.ToString()))
                        {
                            actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Voice Blocked : YES")
                            .WithColor(Color.yellow)
                            .WithInteractability(false);
                        }
                    if (blockedplatformids.Contains(player.PlatformID.ToString()))
                        {
                            actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Damage Blocked : YES")
                            .WithColor(Color.yellow)
                            .WithInteractability(false);
                        }
                    if (blockedspawnies.Contains(player.PlatformID.ToString()))
                    {
                        actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Spawn/Despawn Blocked : YES")
                        .WithColor(Color.yellow)
                        .WithInteractability(false);
                    }
                    if (blockmessages.Contains(player.PlatformID.ToString()))
                    {
                        actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Blocked Messages : YES")
                        .WithColor(Color.yellow)
                        .WithInteractability(false);
                    }
                    if (blockmovements.Contains(player.PlatformID.ToString()))
                    {
                        actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Disable Movement Syncing : YES")
                        .WithColor(Color.yellow)
                        .WithInteractability(false);
                    }

                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Block Player Messaging")
                    .WithColor(Color.yellow)
                    .Do(() =>
                        {
                          ToggleAddRemoveFromFile(player.PlatformID.ToString(), blockmessages, blockmessagingnowpath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} To Block Player Messaging [Server]!.", $"Removed {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} From Block Player Messaging [Server]!.",true);
                        });
                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Voice Blocker")
                    .WithColor(Color.yellow)
                    .Do(() =>
                        {
                         
                          ToggleAddRemoveFromFile(player.PlatformID.ToString(), voicblocked, voicepathblocked, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} To Voice Blocker [Server]!.", $"Removed {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} From Voice Blocker [Server]!.", true);
                            
                        });
                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Damage Blocker")
                    .WithColor(Color.yellow)
                    .Do(() =>
                       {
                         
                        ToggleAddRemoveFromFile(player.PlatformID.ToString(), blockedplatformids, DamageBlockPath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} To Damage Blocker [Server]!.", $"Removed {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} From Damage Blocker [Server]!.", true);
                           
                       });
                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Spawn/Despawn Blocker")
                    .WithColor(Color.yellow)
                    .Do(() =>
                      {
                   
                        ToggleAddRemoveFromFile(player.PlatformID.ToString(), blockedspawnies, ServerBlockSpawnPath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} To Server Spawn Blocker [Server]!.", $"Removed {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} From Server Spawn/Despawn Blocker [Server]!.", true);
                          
                      });
                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Disable Movement Syncing")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {

                        ToggleAddRemoveFromFile(player.PlatformID.ToString(), blockmovements, BlockMovementsPath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} To Server Disable Movement Syncing [Server]!.", $"Removed {CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} From Server Disable Movement Syncing [Server]!.", true);

                    });




                }
                return false;
            }
        }
        //happens without in game
        [HarmonyPatch(typeof(MenuMatchmaking), "ApplyPlayerToElement")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class WithoutInGame
        {
            public static bool Prefix(PlayerElement element, PlayerInfo info)
            {
                element.UsernameElement.Title = TextFilter.SanitizeName(info.Username);

                element.NicknameElement.Title = "Nickname";
                element.NicknameElement.Value = TextFilter.SanitizeName(info.Nickname);



                element.NicknameElement.EmptyFormat = "No {0}";

                element.DescriptionElement.Title = "Description";
                element.DescriptionElement.Value = TextFilter.SanitizeName(info.Description);
                element.DescriptionElement.EmptyFormat = "No {0}";

                element.PermissionsElement
                    .WithTitle("Permissions")
                    .WithColor(Color.yellow)
                    .WithInteractability(false);

                element.PermissionsElement.Value = info.PermissionLevel;
                element.PermissionsElement.EnumType = typeof(PermissionLevel);

                element.PlatformIDElement
                    .WithTitle("Steam ID")
                    .WithColor(Color.yellow)
                    .WithInteractability(false);

                element.ActionsElement.Clear();

                if (info.PlatformID != SteamIdYours())
                {
                    var actionsPage = element.ActionsElement.AddPage();



                    var user = PermissionList.PermittedUsers
                        .FirstOrDefault(u => u.Item1.Equals(info.PlatformID));

                    PermissionLevel level = user != null
                        ? user.Item3
                        : PermissionLevel.DEFAULT;

                    actionsPage.AddElement<LabFusion.Marrow.Proxies.ButtonElement>(
                        $"Permission In Your Server : {level}")
                        .WithColor(Color.red)
                        .WithInteractability(false);

                    var enumnow = actionsPage.AddElement<LabFusion.Marrow.Proxies.EnumElement>($"Your Server Permissions")
                      .WithColor(Color.red)
                      .WithInteractability(false);
                    enumnow.Value = level;
                    enumnow.EnumType = typeof(PermissionLevel);
                    enumnow.OnValueChanged += (v) =>
                    {
                        FusionPermissions.TrySetPermission(info.PlatformID, info.Username, (PermissionLevel)v);
                    };
                    enumnow.Interactable = true;








                    if (NetworkHelper.IsBanned(info.PlatformID))
                    {
                        actionsPage.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Banned From Your Lobby : YES")
                        .WithColor(Color.red)
                        .WithInteractability(false);
                    }

                    var actionsPage3 = actionsPage.AddElement<GroupElement>(NetworkInfo.IsHost ? "Protector Server Options" : "Protector Host Only Options");

                    if (voicblocked.Contains(info.PlatformID.ToString()))
                    {
                        actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Voice Blocked : YES")
                        .WithColor(Color.yellow)
                        .WithInteractability(false);
                    }
                    if (blockedplatformids.Contains(info.PlatformID.ToString()))
                    {
                        actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Damage Blocked : YES")
                        .WithColor(Color.yellow)
                        .WithInteractability(false);
                    }
                    if (blockedspawnies.Contains(info.PlatformID.ToString()))
                    {
                        actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Spawn/Despawn Blocked : YES")
                        .WithColor(Color.yellow)
                        .WithInteractability(false);
                    }
                    if (blockmessages.Contains(info.PlatformID.ToString()))
                    {
                        actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Blocked Messages : YES")
                        .WithColor(Color.yellow)
                        .WithInteractability(false);
                    }
                    if (blockmovements.Contains(info.PlatformID.ToString()))
                    {
                        actionsPage3.AddElement<LabFusion.Marrow.Proxies.ButtonElement>("Disable Movement Syncing : YES")
                        .WithColor(Color.yellow)
                        .WithInteractability(false);
                    }

                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Block Player Messaging")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {
                       
                            ToggleAddRemoveFromFile(info.PlatformID.ToString(), blockmessages, blockmessagingnowpath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(info.Nickname,info.Username)} To Block Player Messaging [Server]!.", $"Removed {CleanedNAME(info.Nickname, info.Username)} From Block Player Messaging!.", true);
                        
                    });


                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Voice Blocker")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {
                       
                            ToggleAddRemoveFromFile(info.PlatformID.ToString(), voicblocked, voicepathblocked, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(info.Nickname, info.Username)} To Voice Blocker [Server]!.", $"Removed {CleanedNAME(info.Nickname, info.Username)} From Voice Blocker!.", true);
                        
                    });


                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Damage Blocker")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {
                            ToggleAddRemoveFromFile(info.PlatformID.ToString(), blockedplatformids, DamageBlockPath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(info.Nickname, info.Username)} To Damage Blocker [Server]!.", $"Removed {CleanedNAME(info.Nickname, info.Username)} From Damage Blocker!.", true);
                        
                    });

                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Spawn/Despawn Blocker")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {
                        ToggleAddRemoveFromFile(info.PlatformID.ToString(), blockedspawnies, ServerBlockSpawnPath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(info.Nickname, info.Username)} To Server Spawn Blocker [Server]!.", $"Removed {CleanedNAME(info.Nickname, info.Username)} From Server Spawn/Despawn Blocker!.", true);
                    });

                    actionsPage3.AddElement<FunctionElement>("Add/Remove To Disable Movement Syncing")
                    .WithColor(Color.yellow)
                    .Do(() =>
                    {

                        ToggleAddRemoveFromFile(info.PlatformID.ToString(), blockmovements, BlockMovementsPath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(info.Nickname, info.Username)} To Server Disable Movement Syncing [Server]!.", $"Removed {CleanedNAME(info.Nickname, info.Username)} From Server Disable Movement Syncing!.", true);
                    });






                    var actionsPage2 = actionsPage.AddElement<GroupElement>("Protector Options");

                    actionsPage2.AddElement<FunctionElement>("UnBan/Ban From Your Lobbies")
                        .WithColor(Color.yellow)
                        .Do(() =>
                        {
                            if (!NetworkHelper.IsBanned(info.PlatformID))
                            {
                                BanInfo item = new()
                                {
                                    Player = new PlayerInfo
                                    {
                                        Username = info.Username,
                                        Nickname = info.Nickname,
                                        PlatformID = info.PlatformID,
                                        Description = info.Description,
                                        PermissionLevel = info.PermissionLevel,
                                        AvatarModID = info.AvatarModID,
                                        AvatarTitle = info.AvatarTitle
                                    },
                                    Reason = $"Manually Banned [{FusionProtectorInfo.ClientName}]"
                                };
                                BanManager.BanList.Bans.RemoveAll((BanInfo info2) => info2.Player.PlatformID == info.PlatformID);
                                BanManager.BanList.Bans.Add(item);
                                DataSaver.WriteJsonToFile("bans.json", BanManager.BanList);

                                NotificationNow(FusionProtectorInfo.ClientName, "Banned Player", NotificationType.SUCCESS);
                            }
                            else
                            {
                                BanManager.Pardon(info.PlatformID);
                                NotificationNow(FusionProtectorInfo.ClientName, "UnBanned Player", NotificationType.SUCCESS);
                            }
                        });

                    actionsPage2.AddElement<FunctionElement>("Copy All Profile To Clipboard")
                        .WithColor(Color.yellow)
                        .Do(() =>
                        {
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };

                            string nowplayerinfo = JsonSerializer.Serialize(info, options);
                            GUIUtility.systemCopyBuffer = nowplayerinfo;
                            NotificationNow(FusionProtectorInfo.ClientName, "Copied Players Entire Details To Clipboard", NotificationType.SUCCESS);
                        });

                    actionsPage2.AddElement<FunctionElement>("Copy Steam ID")
                        .WithColor(Color.yellow)
                        .Do(() =>
                        {
                            GUIUtility.systemCopyBuffer = info.PlatformID.ToString();
                            NotificationNow(FusionProtectorInfo.ClientName, "Copied Steam ID", NotificationType.SUCCESS);
                        });

                    actionsPage2.AddElement<FunctionElement>("Open Steam Profile")
                        .WithColor(Color.yellow)
                        .Do(() =>
                        {
                            CheckSteamID(info.PlatformID);
                        });

                }


                element.PlatformIDElement.Value = info.PlatformID.ToString();

                ElementIconHelper.SetProfileIcon(element, info.AvatarTitle, info.AvatarModID);

                element.VolumeElement.gameObject.SetActive(false);

                return false;
            }

        }

        //client exploit notifications system and blocker
        [HarmonyPatch(typeof(NativeMessageHandler), "Handle")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class ProtectionFromClients
        {
            public static string lastSentMessage;
            public static string steengthnoti;
            public static NetworkPlayer LastPlayer;
            public static System.Collections.Generic.Dictionary<ulong, float> lastSpawnTime = new();
            public static System.Collections.Generic.Dictionary<string, int> barcodeCounts = new();
            public static System.Collections.Generic.Dictionary<PlayerID, System.Collections.Generic.List<PlayerRepDamageData>> PlayerDamageLogs = new();

            public static Stopwatch CountForSpawnRequestTimeout = Stopwatch.StartNew();
            public static int SpamDetection;

            private class SpamState
            {
                public int Count;
                public Stopwatch Timer = Stopwatch.StartNew();
            }

            private static readonly ConcurrentDictionary<ulong, SpamState> SpawnSpam
                = new();


            private static readonly System.Collections.Generic.HashSet<byte> validTags = typeof(NativeMessageTag)
    .GetFields(BindingFlags.Public | BindingFlags.Static)
    .Select(f => (byte)f.GetValue(null))
    .ToHashSet();

            private static readonly System.Collections.Generic.HashSet<int> capturedUnknowns = new();


       
            
            public static bool ValidateAndSanitizeAvatarData(ref PlayerRepAvatarData playerRepAvatarData,NetworkPlayer go)
            {

            void FailAndSwap(string reason)
                {
                    if (go == null)
                        return;

                    if (Invalidstatsnow)
                    {
                        NotificationNowAlways(
                            FusionProtectorInfo.ClientName,
                            reason + $" From: {CleanedNAME(go)}",
                            NotificationType.WARNING,
                            3.5f,
                            true,
                            true
                        );
                    }

                    MelonLogger.Msg(reason + $" From: {CleanedNAME(go)}");

                    try
                    {
                        go.RigRefs?.RigManager?.SwapAvatarCrate(
                            new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79")
                        );
                    }
                    catch { }
                }

                if (go == null)
                    return false;

                if (playerRepAvatarData.Equals(default(PlayerRepAvatarData)))
                {
                    FailAndSwap("Blocked invalid avatar data (default struct)");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(playerRepAvatarData.Barcode))
                {
                    FailAndSwap("Blocked avatar data (empty barcode)");
                    return false;
                }

                if (playerRepAvatarData.Barcode == "00000-00000--00000-0")
                {
                    FailAndSwap("Blocked avatar data (invalid barcode)");
                    return false;
                }

                if (playerRepAvatarData.Stats.Equals(default(SerializedAvatarStats)))
                {
                    FailAndSwap("Blocked avatar data (default stats)");
                    return false;
                }

                object boxedStats = playerRepAvatarData.Stats;

                if (boxedStats == null)
                {
                    FailAndSwap("Blocked avatar data (null stats)");
                    return false;
                }

                var type = boxedStats.GetType();

                var fields = type.GetFields(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance
                );

                if (fields == null)
                {
                    FailAndSwap("Blocked avatar data (reflection failure)");
                    return false;
                }

                foreach (var field in fields)
                {
                    if (field == null || string.IsNullOrEmpty(field.Name))
                        continue;

                    if (field.Name == "vitality" || field.Name == "intelligence")
                        continue;

                    object rawValue = field.GetValue(boxedStats);

                    if (!field.FieldType.IsValueType && rawValue == null)
                    {
                        FailAndSwap($"Blocked avatar data (null field: {field.Name})");
                        return false;
                    }

                    if (field.FieldType == typeof(float))
                    {
                        float value;

                        try
                        {
                            value = Convert.ToSingle(rawValue);
                        }
                        catch
                        {
                            FailAndSwap($"Invalid stat detected: {field.Name} = {rawValue}");
                            return false;
                        }

                        if (float.IsNaN(value) || float.IsInfinity(value))
                        {
                            FailAndSwap($"Invalid stat detected: {field.Name} = {value}");
                            return false;
                        }

                        const float minValid = 0.001f;
                        const float maxValid = 15f;

                        if (value < minValid || value > maxValid)
                        {
                            FailAndSwap($"Invalid stat detected: {field.Name} = {value}");
                            return false;
                        }

                        return true;
                    }
                }

                playerRepAvatarData.Stats = (SerializedAvatarStats)boxedStats;

                return true;
            }
            public static IEnumerator RunNotificationThenKickProtection(PlayerID playernow, string message, float messagetime, Action CodeNow)
            {
                if (!HideFusionProtector)
                {
                    var data = SendNotificationData.Create(
    PlayerIDManager.LocalSmallID,
    message,
    "Fusion Protetor Kick/Ban System",
    messagetime
);

                    MessageRelay.RelayModule<SendNotificationMessage, SendNotificationData>(
                        data,
                        new MessageRoute(playernow.SmallID, NetworkChannel.Reliable)
                    );
                }
                yield return new WaitForSecondsRealtime(messagetime + 1f);

                CodeNow?.Invoke();
            }
            static bool Prefix(NativeMessageHandler __instance, ReceivedMessage received)
            {
                
                if (!validTags.Contains((byte)__instance.Tag) && capturedUnknowns.Add(__instance.Tag))
                {
                    MelonLogger.Msg($"Message That Does Not Exist In Native Tags [Perhaps A Client User Sending] : {__instance.Tag}");
                }
               



                if (__instance is SpawnResponseMessage || __instance is PlayerRepAvatarMessage)
                {
                    NetworkPlayerManager.TryGetPlayer(received.Sender.Value, out var LastPlayer);
                }
      

                if (AntiGrab && !GamemodeManager.IsGamemodeStarted)
                {
                    if (__instance is PlayerRepGrabMessage)
                    {
                        var data = received.ReadData<PlayerRepGrabData>();
                        if (data == null)
                            return true;

                        var grip = data.GetGrip();
                        if (grip == null || grip.gameObject == null || Player.RigManager == null)
                            return true;

                        var root = grip.gameObject.transform?.root?.gameObject;
                        if (root == null)
                            return true;

                        if (root == Player.RigManager.gameObject)
                            return false;
                    }
                }

                if (__instance is ConnectionRequestMessage || __instance is EntityDataRequestMessage || __instance is EntityZoneRegisterMessage)
                    return true;



                if (NetworkInfo.IsHost)
                {
                    try
                    {

                        if (blockexploitscompletely)
                        {
                            if (NetworkPlayerManager.TryGetPlayer(received.Sender.Value, out var playerusingexploits))
                            {

                                ulong platformId = received.PlatformID.Value;

                                FusionPermissions.FetchPermissionLevel(platformId, out var playerPermLevel, out _);
                                FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var selflevel, out _);


                                bool IsOwner()
                                {
                                    return OwnerCheckEnabled && playerPermLevel == PermissionLevel.OWNER;
                                }


                                if (!IsOwner())
                                {
                                    if (blockmovements.Contains(received.PlatformID.ToString()))
                                    {
                                        if (__instance is PlayerPoseUpdateMessage || __instance is PlayerRepGrabMessage || __instance is EntityPoseUpdateMessage)
                                        {
                                            return false;
                                        }
                                    }
                                }

                            
                                bool SpawnProtections(string barcode, bool spawnlogging = false)
                                {
                                    void spawnablekickfunc(string barcode)
                                    {
                                        if (SpawnablesKick.Contains(barcode))
                                        {
                                            MelonCoroutines.Start(RunNotificationThenKickProtection(playerusingexploits.PlayerID, $"Spawned A Spawnable Host Has On Auto Kick, Removed From Lobby! Barcode [{barcode.JR_BarcodeCrateName()}]", 3.0f, () =>
                                            {

                                                NetworkSpawnerNotif(playerusingexploits, $"{CleanedNAME(playerusingexploits)} Spawned A Spawnable Host Has On Auto Kick, Removed From Lobby! Barcode [{barcode.JR_BarcodeCrateName()}]", NotificationType.WARNING, 2.5f);

                                                NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                            }));
                                        }
                                    }

                                 

                                    var reference = new SpawnableCrateReference(barcode);
                                    string playerId = playerusingexploits.PlayerID.PlatformID.ToString().Trim();
                                    var cratemodid = CrateFilterer.GetModID(reference.Crate.Pallet);

                                    string palletName = StripColorTags(reference.Crate.name);
                                    string palletAuthor = reference.Crate.Pallet.Author;
                                    int modioID = CrateFilterer.GetModID(reference.Crate.Pallet);
                                    bool isAvatar = barcode.Contains(".Avatar.");

                                    var infox = (playerusingexploits.JR_Nickname(), playerusingexploits.JR_Username(), playerusingexploits.JR_SteamID(), palletName, palletAuthor, modioID, barcode, isAvatar);

                                    if (!SpawnLogs.Contains(infox))
                                        SpawnLogs.Add(infox);

                                    if (spawnlogsmelonlog)
                                    {
                                        MelonLogger.Warning(
                                            $"Player({CleanedNAME(playerusingexploits)} [{playerId}])\n" +
                                            $"Is Spawning\n" +
                                            $"(Pallet Name : {palletName} | Pallet Author : {palletAuthor})\n" +
                                            $"MOD IO # ID {modioID}\n" +
                                            $"Sending Spawnable Barcode ID : {barcode}"
                                        );
                                    }


                                    if (spawnlogging)
                                    {


                                        if (!PlayerSpawningStuff.TryGetValue(playerId, out var barcodes))
                                        {
                                            barcodes = new System.Collections.Generic.HashSet<string>();
                                            PlayerSpawningStuff[playerId] = barcodes;
                                        }

                                        barcodes.Add(barcode);
                                    }

                                    if (HostIsMe(playerusingexploits))
                                    {
                                        return true;
                                    }
                                    
                                    bool LimitItem(string barcodenow, int maxspawns, bool maxnotif = true)
                                    {
                                        if (!IsMagazine(reference))
                                            return true;

                                        int count = 0;

                                        foreach (var entity in NetworkEntities())
                                        {
                                            var barcode = entity.JR_GetMarrowEntity().JR_GetBarcodeID();

                                            if (!barcodeCounts.ContainsKey(barcode))
                                                barcodeCounts[barcode] = 0;

                                            barcodeCounts[barcode]++;

                                            if (barcode == barcodenow)
                                                count++;
                                        }

                                        int maxuntilblock = limitplayercount ? NetworkPlayers().Count : maxspawns;

                                        if (count >= maxuntilblock)
                                        {
                                            if (maxnotif)
                                            {
                                                NetworkSpawnerNotif(
                                                    playerusingexploits,
                                                    $"Max Server Limit For : {barcode.JR_BarcodeCrateName()}",
                                                    NotificationType.ERROR,
                                                    1.5f
                                                );
                                            }
                                            return false;
                                        }

                                        return true;
                                    }

                                    if (!IsOwner())
                                    {
                                        if (barcode == "SLZ.BONELAB.Core.Spawnable.GameplaySystems")
                                        {
                                            NetworkSpawnerNotif(playerusingexploits, "Tried To Delete Fusion UI [GameplaySystems] Auto Kicking...", NotificationType.WARNING, 3.5f);
                                            NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                        }


                                        if (spamspawnprevention)
                                        {
                                            var state = SpawnSpam.GetOrAdd(playerusingexploits.JR_SteamID(), _ => new SpamState());

                                            if (state.Timer.ElapsedMilliseconds >= antispamspawntimer)
                                            {
                                                int detected = Interlocked.Exchange(ref state.Count, 0);
                                                state.Timer.Restart();

                                                if (detected >= countbeforespam)
                                                {
                                                    if (notificationspamspawn)
                                                    {
                                                        NotificationNowAlways(
                                                            FusionProtectorInfo.ClientName,
                                                            $"{CleanedNAME(playerusingexploits)} Is Spamming Spawn",
                                                            NotificationType.WARNING,
                                                            2.5f
                                                        );
                                                    }

                                                    return false;
                                                }

                                            }
                                            Interlocked.Increment(ref state.Count);
                                        }


                                        //CAN DO WHEN NOT OR WHEN YOU ARE HOST
                                        //
                                        if (BLOCKAVATARSASSPAWNABLES)
                                        {
                                            if (IsAvatarCrateExist(barcode))
                                            {
                                                return false;
                                            }
                                        }
                                        if (IsSpawnableCrateExist(barcode) && SiteStuff.blockednsfw.Contains(barcode))
                                        {
                                            var playerName = playerusingexploits != null
                                                ? CleanedNAME(playerusingexploits)
                                                : "Unknown";

                                            string spawnableInfo =
                                                $"NSFW Protection\nReport Spawnable! => {StripColorTags(reference.Crate.name)} [{reference.Crate.Pallet.Author}]";

                                            var msg = playerusingexploits != null && HostIsMe(playerusingexploits)
                                                ? spawnableInfo
                                                : $"Person Doing : {playerName}\n{spawnableInfo}";

                                            MelonLogger.Warning(msg);

                                            NetworkSpawnerNotif(
                                                playerusingexploits,
                                                spawnableInfo,
                                                NotificationType.ERROR,
                                                1.5f
                                            );

                                            return false;
                                        }
                                        if (blockedspawnables)
                                        {
                                            if (BlockedSpawnables.Contains(barcode))
                                            {
                                                var playerName = playerusingexploits != null
                                                    ? CleanedNAME(playerusingexploits)
                                                    : "Unknown";

                                                string spawnableInfo = $"Blocked Spawnable : {StripColorTags(reference.Crate.name) + " [" + reference.Crate.Pallet.Author}]";



                                                if (blockspwnnotis)
                                                {
                                                    NetworkSpawnerNotif(playerusingexploits, spawnableInfo, NotificationType.ERROR, 1.5f);
                                                }
                                                else
                                                {
                                                    var msg = playerusingexploits != null && HostIsMe(playerusingexploits)
                                                    ? spawnableInfo
                                                    : $"Person Doing : {playerName}\n{spawnableInfo}";
                                                    MelonLogger.Msg(msg);
                                                }

                                                return false;
                                            }
                                        }
                                        if (blockedspawnies.Contains(playerId))
                                        {
                                            return false;
                                        }
                                        if (globalblocklistnow)
                                        {
                                            
                                            if (SiteStuff.globalblocklistavatar.Contains(barcode))
                                            {
                                                if (globalblocklistnotification)
                                                {
                                                    NetworkSpawnerNotif(playerusingexploits, $"[FP] Global Blacklisted Avatar Spawnable : {barcode}", NotificationType.ERROR, 1.5f);
                                                }
                                                return false;
                                            }

                                            if (SiteStuff.globalblocklistmodioid.Contains(cratemodid))
                                            {
                                                if (globalblocklistnotification)
                                                {
                                                    NetworkSpawnerNotif(playerusingexploits, $"[FP] Global Blacklisted Spawnable Mod.IO Mod : {cratemodid}", NotificationType.ERROR, 1.5f);
                                                }
                                                return false;
                                            }

                                            if (SiteStuff.globalblocklistspawnable.Contains(barcode))
                                            {
                                                if (globalblocklistnotification)
                                                {
                                                    NetworkSpawnerNotif(playerusingexploits, $"[FP] Global Blacklisted Spawnable : {barcode?.JR_BarcodeCrateName()}", NotificationType.ERROR, 1.5f);
                                                }
                                                return false;
                                            }

                                            if (SiteStuff.globalblocklistpallet.Contains(StripColorTags(reference.Crate.Pallet.name)))
                                            {
                                                if (globalblocklistnotification)
                                                {
                                                    NetworkSpawnerNotif(playerusingexploits, $"[FP] Global Blacklisted Spawnable Pallet : {barcode?.JR_BarcodePalletName()}", NotificationType.ERROR, 1.5f);
                                                }
                                                return false;
                                            }

                                            if (SiteStuff.globalblocklistauthor.Contains(reference.Crate.Pallet.Author))
                                            {
                                                if (globalblocklistnotification)
                                                {
                                                    NetworkSpawnerNotif(playerusingexploits, $"[FP] Global Blacklisted Spawnable Author : {barcode?.JR_BarcodeAuthor()}", NotificationType.ERROR, 1.5f);
                                                }
                                                return false;
                                            }
                                        }
                                        if (ModIDBlocker)
                                        {
                                            if (modidblocked.Contains(cratemodid.ToString()))
                                            {
                                                var playerName = playerusingexploits != null
                                                    ? CleanedNAME(playerusingexploits)
                                                    : "Unknown";

                                                string blockInfo = $"Mod ID Blocked : {cratemodid}";



                                                if (blockspwnnotis)
                                                {
                                                    NetworkSpawnerNotif(
                                                        playerusingexploits,
                                                        blockInfo,
                                                        NotificationType.ERROR,
                                                        1.5f
                                                    );
                                                }
                                                else
                                                {
                                                    var msg = playerusingexploits != null && HostIsMe(playerusingexploits)
                                                    ? blockInfo
                                                    : $"Person Doing : {playerName}\n{blockInfo}";
                                                    MelonLogger.Msg(msg);
                                                }

                                                return false;
                                            }
                                        }
                                        if (BlockPalletCompletely)
                                        {
                                            if (blockentirepallet.Contains(StripColorTags(reference.Crate.Pallet.name)))
                                            {
                                                var playerName = playerusingexploits != null
                                                    ? CleanedNAME(playerusingexploits)
                                                    : "Unknown";

                                                string palletInfo =
                                                    $"Blocked Entire Pallet : {StripColorTags(reference.Crate.name)} [{reference.Crate.Pallet.Author}]";



                                                if (blockspwnnotis)
                                                {
                                                    NetworkSpawnerNotif(
                                                        playerusingexploits,
                                                        palletInfo,
                                                        NotificationType.ERROR,
                                                        1.5f
                                                    );
                                                }
                                                else
                                                {
                                                    var msg = playerusingexploits != null && HostIsMe(playerusingexploits)
                                                    ? palletInfo
                                                    : $"Person Doing : {playerName}\n{palletInfo}";
                                                    MelonLogger.Msg(msg);
                                                }

                                                return false;
                                            }
                                        }
                                        if (BlockAuthorOfSpawnable)
                                        {
                                            if (blockentireauthor.Contains(reference.Crate.Pallet.Author))
                                            {
                                                var playerName = playerusingexploits != null
                                                    ? CleanedNAME(playerusingexploits)
                                                    : "Unknown";

                                                string authorInfo =
                                                    $"Blocked Entire Author : {StripColorTags(reference.Crate.name)} [{reference.Crate.Pallet.Author}]";



                                                if (blockspwnnotis)
                                                {
                                                    NetworkSpawnerNotif(
                                                        playerusingexploits,
                                                        authorInfo,
                                                        NotificationType.ERROR,
                                                        1.5f
                                                    );
                                                }
                                                else
                                                {
                                                    var msg = playerusingexploits != null && HostIsMe(playerusingexploits)
                                                    ? authorInfo
                                                    : $"Person Doing : {playerName}\n{authorInfo}";
                                                    MelonLogger.Msg(msg);
                                                }

                                                return false;
                                            }
                                        }
                                        if (warnedspawnables)
                                        {

                                            if (WarnedSpawnables.Contains(barcode))
                                            {
                                                NetworkSpawnerNotif(playerusingexploits, $"Warning Spawnable : {StripColorTags(reference.Crate.name)} [{reference.Crate.Pallet.Author}]", NotificationType.WARNING, 2.5f);
                                            }

                                        }
                                        //


                                        if (AntiDevManip)
                                        {

                                            if (barcode == "c1534c5a-c6a8-45d0-aaa2-2c954465764d")
                                            {
                                                return false;
                                            }

                                        }
                                        if (AntiLasereyes)
                                        {
                                            if (barcode == "BamBaeYoh.LaserEyes.Spawnable.LaserEyes")
                                            {
                                                return false;
                                            }
                                        }

                                        if (SpawnGunProtection)
                                        {
                                            if (!IsMagazine(reference))
                                            {
                                                switch (antimodguntypereal)
                                                {
                                                    case antimodguntype.AnySpawnGun:
                                                        if (barcode != "c1534c5a-5747-42a2-bd08-ab3b47616467" &&
                                                            barcode != "Doctor.AdvancedUtilityGun.Spawnable.AUG1" &&
                                                            barcode != "doge15567.PersonalSpawnguns.Spawnable.doge15567sPersonalSpawngun" &&
                                                            barcode != "doge15567.PersonalSpawnguns.Spawnable.SPAWNTEST")
                                                        {
                                                            bool leftHasSpawnGun = playerusingexploits.JR_GetHand(WhichHand.Left).JR_IsGrabbedSpawnGun();
                                                            bool rightHasSpawnGun = playerusingexploits.JR_GetHand(WhichHand.Right).JR_IsGrabbedSpawnGun();

                                                            if (!leftHasSpawnGun && !rightHasSpawnGun)
                                                            {
                                                                return false;
                                                            }
                                                        }
                                                        break;

                                                    case antimodguntype.DefaultBlue:
                                                        if (barcode != "c1534c5a-5747-42a2-bd08-ab3b47616467")
                                                        {
                                                            var left = playerusingexploits.JR_GetMarrowEntityInHand(WhichHand.Left);
                                                            var right = playerusingexploits.JR_GetMarrowEntityInHand(WhichHand.Right);

                                                            bool leftHasSpawnGunx = left != null && left.JR_GetBarcodeID() == "c1534c5a-5747-42a2-bd08-ab3b47616467";
                                                            bool rightHasSpawnGunx = right != null && right.JR_GetBarcodeID() == "c1534c5a-5747-42a2-bd08-ab3b47616467";

                                                            if (!leftHasSpawnGunx && !rightHasSpawnGunx)
                                                            {
                                                                return false;
                                                            }
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                        if (spawnableskickon)
                                        {
                                            if (SpawnablesKick.Contains(barcode))
                                            {
                                                MelonCoroutines.Start(RunNotificationThenKickProtection(playerusingexploits.PlayerID, $"Spawned A Spawnable Host Has On Auto Kick, Removed From Lobby! Barcode [{barcode.JR_BarcodeCrateName()}]", 3.0f, () =>
                                                {

                                                    NetworkSpawnerNotif(playerusingexploits, $"{CleanedNAME(playerusingexploits)} Spawned A Spawnable Host Has On Auto Kick, Removed From Lobby! Barcode [{barcode.JR_BarcodeCrateName()}]", NotificationType.WARNING, 2.5f);

                                                    NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                                }));
                                            }
                                        }
                                        if (spawnbypassprotection && !FusionPermissions.HasSufficientPermissions(playerPermLevel, LobbyInfoManager.LobbyInfo.DevTools))
                                        {
                                            if (!IsMagazine(reference))
                                                return false;

                                        }

                                        //these funcs has to be here because it filters through all the code above and this is the last funcs it hits
                                        
                                        if (hostonlyspawnlimiter)
                                        {
                                            if (spawnlimitline.TryGetValue(barcode.ToLowerInvariant(), out int maxlimit))
                                            {
                                                spawnablekickfunc(barcode);
                                                return LimitItem(barcode, maxlimit);
                                            }
                                        }                                        
                                        if (globalspawnlimitperitem)
                                        {
                                            spawnablekickfunc(barcode);

                                            return LimitItem(barcode, limitnowofglobal);
                                        }                                        
                                        if (spawnlimiternow)
                                        {
                                            if (!MostUsedItems(barcode))
                                            {
                                                spawnablekickfunc(barcode);

                                                var id = playerusingexploits.PlayerID.PlatformID;
                                                var now = Time.realtimeSinceStartup;

                                                if (lastSpawnTime.TryGetValue(id, out var last) && now - last < spawnlimitertimer)
                                                    return false;

                                                lastSpawnTime[id] = now;
                                            }
                                            else
                                            {
                                                spawnablekickfunc(barcode);

                                                return LimitItem(barcode, 15,false);
                                            }
                                        }
                                     
                                    }

                                    return true;
                                }

                                bool DespawnFuncs()
                                {
                                    despawnresponselogger.TryAdd(playerusingexploits.PlayerID, new());

                                    despawnresponselogger[playerusingexploits.PlayerID]
                                        .Add("DespawnResponseMessage triggered");


                                    if (blockedspawnies.Contains(playerusingexploits.JR_SteamID().ToString()))
                                    {
                                        return false;
                                    }
                                    if (DESPAWNPROTECTION)
                                    {
                                        if (!IsOwner())
                                        {


                                            bool leftHasSpawnGun = playerusingexploits.JR_GetHand(WhichHand.Left).JR_IsGrabbedSpawnGun();
                                            bool rightHasSpawnGun = playerusingexploits.JR_GetHand(WhichHand.Right).JR_IsGrabbedSpawnGun();

                                            if (!leftHasSpawnGun && !rightHasSpawnGun)
                                            {

                                                return false;
                                            }



                                        }
                                    }
                                    return true;
                                }
                                //if (__instance is ConstraintCreateMessage)
                                //{
                                //
                                //}
                                bool safeawaydistance(SerializedSpawnData data, string barcode,float safedistancemeter = 4.5f)
                                {
                                    bool isSafe = true;

                                    var isslzog = new SpawnableCrateReference(barcode);

                                    if (isslzog.Crate.Pallet.Author != "SLZ")
                                    {
                                        if (!IsMagazine(isslzog))
                                        {
                                            foreach (var gtc in NetworkPlayers())
                                            {
                                                if (gtc.JR_SmallID() != playerusingexploits.JR_SmallID())
                                                {
                                                    float distance = Vector3.Distance(
                                                        data.SerializedTransform.position,
                                                        gtc.RigRefs.Head.transform.position
                                                    );

                                                    if (distance < safedistancemeter)
                                                    {
                                                        isSafe = false;
                                                        break;
                                                    }
                                                }
                                            }


                                            if (!isSafe)
                                            {

                                                return false;

                                            }
                                        }
                                    }

                                    return isSafe;
                                }


                                if (__instance is ModInfoRequestMessage)
                                {
                                    string playerId = playerusingexploits.PlayerID.PlatformID.ToString();

                                    var modinforequest = received.ReadData<ModInfoRequestData>();
                                    if (modinforequest == null)
                                    {
                                        return true;
                                    }

                                    var barcodemodioid = modinforequest.Barcode;

                                    if (spawnbypassprotection && !FusionPermissions.HasSufficientPermissions(playerPermLevel, LobbyInfoManager.LobbyInfo.DevTools))
                                    {
                                        return false;
                                    }

                                    if (blockedspawnies.Contains(playerId))
                                    {

                                        return false;
                                    }

                                    if (spawnableskickon)
                                    {
                                        if (SpawnablesKick.Contains(barcodemodioid))
                                        {
                                            MelonCoroutines.Start(RunNotificationThenKickProtection(playerusingexploits.PlayerID, $"Spawned A Spawnable Host Has On Auto Kick, Removed From Lobby! Barcode [{barcodemodioid}]", 3.0f, () =>
                                        {

                                            NetworkSpawnerNotif(playerusingexploits, $"{CleanedNAME(playerusingexploits)} Spawned A Spawnable Host Has On Auto Kick, Removed From Lobby! Barcode [{barcodemodioid}]", NotificationType.WARNING, 2.5f);

                                            NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                        }));
                                        }
                                    }

                                    if (AntiLasereyes)
                                    {
                                        if (barcodemodioid == "BamBaeYoh.LaserEyes.Spawnable.LaserEyes")
                                        {
                                            return false;
                                        }
                                    }

                                    if (globalblocklistnow)
                                    {
                                        if (SiteStuff.globalblocklistspawnable.Contains(barcodemodioid))
                                        {
                                            if (globalblocklistnotification)
                                            {
                                                NetworkSpawnerNotif(playerusingexploits, $"[FP] Global Blacklisted Spawnable : {barcodemodioid}", NotificationType.ERROR, 1.5f);
                                            }
                                            return false;
                                        }
                                    }

                                    if (blockedspawnables)
                                    {
                                        if (BlockedSpawnables.Contains(barcodemodioid))
                                        {
                                            var playerName = playerusingexploits != null
                                                ? CleanedNAME(playerusingexploits)
                                                : "Unknown";

                                            string spawnableInfo = $"Blocked Spawnable : {barcodemodioid}";



                                            if (blockspwnnotis)
                                            {
                                                NetworkSpawnerNotif(playerusingexploits, spawnableInfo, NotificationType.ERROR, 1.5f);
                                            }
                                            else
                                            {
                                                var msg = playerusingexploits != null && HostIsMe(playerusingexploits)
                                                ? spawnableInfo
                                                : $"Person Doing : {playerName}\n{spawnableInfo}";
                                                MelonLogger.Msg(msg);
                                            }

                                            return false;
                                        }
                                    }

                                }
                                //this function is the one that syncs the download to all other client
                                if (__instance is ModInfoResponseMessage)
                                {
                                    var modinforesponse = received.ReadData<ModInfoResponseData>();
                                    if (modinforesponse == null)
                                    {
                                        return true;
                                    }

                                    var modidnow = modinforesponse.ModFile.File.ModID;

                                    if (spawnbypassprotection && !FusionPermissions.HasSufficientPermissions(playerPermLevel, LobbyInfoManager.LobbyInfo.DevTools))
                                    {
                                        return false;
                                    }

                                    string playerId = playerusingexploits.PlayerID.PlatformID.ToString();


                                    if (globalblocklistnow)
                                    {
                                        if (SiteStuff.globalblocklistmodioid.Contains(modidnow))
                                        {
                                            if (globalblocklistnotification)
                                            {
                                                NetworkSpawnerNotif(playerusingexploits, $"[FP] Global Blacklisted Mod.IO Mod Spawned: {modidnow}", NotificationType.ERROR, 1.5f);
                                            }
                                            return false;
                                        }
                                    }

                                    if (!IsOwner())
                                    {
                                        if (modidblocked.Contains(modidnow.ToString()))
                                        {
                                            NetworkSpawnerNotif(playerusingexploits, $"Mod ID Blocked : {modidnow}", NotificationType.ERROR, 1.5f);
                                            return false;
                                        }
                                    }

                                    if (blockedspawnies.Contains(playerId))
                                    {

                                        return false;
                                    }
                                }
                                if (__instance is DespawnResponseMessage)
                                {
                                    return DespawnFuncs();
                                }
                                if (__instance is DespawnRequestMessage)
                                {
                                    return DespawnFuncs();
                                }
                                if (__instance is SpawnRequestMessage)
                                {








                                    var spawndata = received.ReadData<SerializedSpawnData>();
                                    if (spawndata == null || string.IsNullOrEmpty(spawndata.Barcode))
                                    {
                                        MelonLogger.Warning("Blocked invalid spawnable barcode");
                                        return false;
                                    }




                                    //safe distance spawning basically doesnt allow close spawns if near a player

                                    if (safedistancespawning)
                                    {
                                        return safeawaydistance(spawndata, spawndata.Barcode);
                                    }



                                    if (ballplayersprevention)
                                    {
                                        if (spawndata.Barcode == "BaBaCorp.BaBasToybox.Spawnable.ReinforcedBall")
                                        {
                                            return safeawaydistance(spawndata, spawndata.Barcode);
                                        }
                                    }

                                    if (blindplayersprevention)
                                    {
                                        if (spawndata.Barcode == "SLZ.BONELAB.Core.Spawnable.ImpactVoid")
                                        {
                                            return safeawaydistance(spawndata, spawndata.Barcode);
                                        }
                                    }




                                    return SpawnProtections(spawndata.Barcode, true);
                                    



                                }
                                if (__instance is SpawnResponseMessage)
                                {
                                    var spawndataRESPONSE = received.ReadData<SpawnResponseData>();
                                    if (spawndataRESPONSE == null || string.IsNullOrEmpty(spawndataRESPONSE.SpawnData.Barcode))
                                    {
                                        MelonLogger.Warning("Blocked invalid spawnable barcode");
                                        return false;
                                    }

                                    return SpawnProtections(spawndataRESPONSE.SpawnData.Barcode, true);
                                }
                                if (__instance is PlayerRepDamageMessage)
                                {
                                    var damagedata = received.ReadData<PlayerRepDamageData>();


                                    if (damagedata == null || string.IsNullOrEmpty(damagedata.Attack.Attack.damage.ToString()))
                                    {
                                        MelonLogger.Warning($"Blocked invalid damage data");
                                        return false;
                                    }

                                    if (!PlayerDamageLogs.TryGetValue(playerusingexploits.PlayerID, out var list))
                                    {
                                        list = new System.Collections.Generic.List<PlayerRepDamageData>();
                                        PlayerDamageLogs[playerusingexploits.PlayerID] = list;
                                    }

                                    list.Add(damagedata);

                                    if (!IsOwner())
                                    {
                                        if (antioneshot)
                                        {
                                            float damage = damagedata.Attack.Attack.damage;
                                            float maxHealth = playerusingexploits.RigRefs.Health.max_Health;

                                            MelonLogger.Msg($"Damage From {CleanedNAME(playerusingexploits)} Damage : {damage} | Max Damage They Can Do : {maxHealth:F1}");

                                            if (damage > maxHealth)
                                                return false;
                                        }


                                        if (nodamageunlessweapons)
                                        {
                                            var leftHand = playerusingexploits.JR_GetHand(WhichHand.Left);
                                            var rightHand = playerusingexploits.JR_GetHand(WhichHand.Right);

                                            bool leftHasWeapon = leftHand.JR_GetGun() || leftHand.JR_GetMelee();
                                            bool rightHasWeapon = rightHand.JR_GetGun() || rightHand.JR_GetMelee();

                                            if (!leftHasWeapon || !rightHasWeapon)
                                            {
                                                return false;
                                            }
                                        }

                                        if (servermaxdamagethres)
                                        {
                                            if (float.TryParse(maxdamagethressy, out var floatofdamage))
                                            {
                                                if (damagedata.Attack.Attack.damage > floatofdamage)
                                                {
                                                    MelonLogger.Msg(CleanedNAME(playerusingexploits) + $" Passed The Max Damage Value [{floatofdamage}] Server Blocked It!");
                                                    return false;
                                                }
                                            }
                                        }




                                    }


                                    if (blockedplatformids.Contains(playerusingexploits.PlayerID.PlatformID.ToString()))
                                    {
                                        return false;
                                    }




                                    if (damagedata.Attack.Attack.damage == float.MaxValue)
                                    {
                                        NetworkSpawnerNotif(playerusingexploits, $"Kill Attempt By Client");
                                        return false;
                                    }
                                }
                                if (__instance is PlayerRepAvatarMessage)
                                {
                                    var data = received.ReadData<PlayerRepAvatarData>();

                                    if (!ProtectionFromClients.ValidateAndSanitizeAvatarData(ref data, playerusingexploits))
                                    {
                                        return false;
                                    }



                                    var reference = new AvatarCrateReference(data.Barcode);
                                    var cratemodid = CrateFilterer.GetModID(reference.Crate.Pallet);

                                    string playerId = playerusingexploits.PlayerID.PlatformID.ToString().Trim();
                                    string barcode = data.Barcode;

                                   
                                    var entry = (
                                    PlayerName: playerusingexploits.JR_Nickname(),
                                    Username: playerusingexploits.JR_Username(),
                                    PlatformID: playerusingexploits.PlayerID.PlatformID,
                                    PalletName: StripColorTags(reference.Crate.name) ?? string.Empty,
                                    PalletAuthor: reference.Crate.Pallet.Author ?? string.Empty,
                                    ModioID: CrateFilterer.GetModID(reference.Crate.Pallet),
                                    BarcodeID: data.Barcode
                                    );

                                    string message =
                                        $"Player: {CleanedNAME(playerusingexploits)} [{entry.PlatformID}]\n" +
                                        $"Switching Into:\n" +
                                        $"Pallet Name: {entry.PalletName}\n" +
                                        $"Pallet Author: {entry.PalletAuthor}\n" +
                                        $"MOD.IO ID: {entry.ModioID}\n" +
                                        $"Avatar Barcode ID: {entry.BarcodeID}";

                                    if (message != lastSentMessage)
                                    {
                                        lastSentMessage = message;
                                        MelonLogger.Warning(message);
                                    }


                                    if (HostIsMe(playerusingexploits))
                                    {
                                        return true;
                                    }


                                    if (!IsOwner())
                                    {

                                        if (statkicker)
                                        {

                                            if (StatsKickerPresets.CurrentPreset != null && StatsKickerPresets.CurrentPreset.Length >= 11)
                                            {
                                                var playerStats = new float[]
                                                {
        data.Stats.height,       
        data.Stats.massArm,      
        data.Stats.massChest,    
        data.Stats.massHead,     
        data.Stats.massLeg,      
        data.Stats.massPelvis,   
        data.Stats.massTotal,    
        data.Stats.speed,        
        data.Stats.strengthLower,
        data.Stats.strengthUpper,
        data.Stats.vitality     
                                                };

                                                string[] statNames = new string[]
                                                {
        "Height",
        "Mass Arm",
        "Mass Chest",
        "Mass Head",
        "Mass Leg",
        "Mass Pelvis",
        "Mass Total",
        "Speed",
        "Strength Lower",
        "Strength Upper",
        "Vitality"
                                                };

                                                for (int i = 0; i < 11; i++)
                                                {
                                                    if (float.TryParse(StatsKickerPresets.CurrentPreset[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var limit))
                                                    {
                                                        if (playerStats[i] >= limit)
                                                        {



                                                            MelonCoroutines.Start(RunNotificationThenKickProtection(playerusingexploits.PlayerID, $"Kick triggered by stat: {statNames[i]} | value: {playerStats[i]} | Limit: {limit}", 3.0f, () =>
                                                            {
                                                                NetworkSpawnerNotif(playerusingexploits, $"Kick triggered by stat: {statNames[i]} | Player value: {playerStats[i]} | Limit: {limit}", NotificationType.ERROR, 1.5f);
                                                                NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                                            }));

                                                            break;
                                                        }
                                                    }
                                                }
                                            }

                                        }

                                        if (avatarskickon)
                                        {
                                            if (AvatarsKick.Contains(data.Barcode))
                                            {

                                                MelonCoroutines.Start(RunNotificationThenKickProtection(playerusingexploits.PlayerID, $"Avatar You Switched Into Has Auto Kick | {data.Barcode.JR_BarcodeCrateName()}", 3.0f, () =>
                                                {
                                                    NetworkSpawnerNotif(playerusingexploits, $"Avatar Kicked | {data.Barcode.JR_BarcodeCrateName()}", NotificationType.ERROR, 2.5f);
                                                    NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                                }));
                                            }
                                        }

                                        if (kickifvitaly && float.IsInfinity(data.Stats.vitality))
                                        {

                                            MelonCoroutines.Start(RunNotificationThenKickProtection(playerusingexploits.PlayerID, $"Your Avatar Had Infinite Vitality You Was Kicked", 2.5f, () =>
                                            {
                                                NetworkSpawnerNotif(playerusingexploits, $"Infinite Vitality Kicked", NotificationType.ERROR, 2.5f);
                                                NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                            }));

                                        }

                                        if (strengththresprotection)
                                        {
                                            float lower = data.Stats.strengthLower;
                                            float upper = data.Stats.strengthUpper;

                                            if (lower > strengththreshnow)
                                            {
                                                float oldStrength = lower;
                                                data.Stats.strengthLower = 3.5f;

                                                if (strengthnotif)
                                                {
                                                    string stringy =
                                                        $"Strength Threshold Protection Triggered\n" +
                                                        $"User: {CleanedNAME(playerusingexploits)}\n" +
                                                        $"Old Strength: {oldStrength}\n" +
                                                        $"Threshold: {strengththreshnow}\n" +
                                                        $"Strength has been reset to: {data.Stats.strengthLower}";

                                                    if (steengthnoti != stringy)
                                                    {
                                                        steengthnoti = stringy;
                                                        MelonLogger.Warning(stringy);



                                                        NetworkSpawnerNotif(playerusingexploits, "Strength Threshold Protection: Strength was reset to a lower value.");

                                                    }
                                                }
                                            }

                                            if (upper > strengththreshnow)
                                            {
                                                float oldStrength = upper;

                                                data.Stats.strengthUpper = 3.5f;

                                                if (strengthnotif)
                                                {
                                                    string stringy =
                                                        $"Strength Threshold Protection Triggered\n" +
                                                        $"User: {CleanedNAME(playerusingexploits)}\n" +
                                                        $"Old Strength: {oldStrength}\n" +
                                                        $"Threshold: {strengththreshnow}\n" +
                                                        $"Strength has been reset to: {data.Stats.strengthUpper}";

                                                    if (steengthnoti != stringy)
                                                    {
                                                        steengthnoti = stringy;
                                                        MelonLogger.Warning(stringy);
                                                        NetworkSpawnerNotif(playerusingexploits, "Strength Threshold Protection: Strength was reset to a lower value.");

                                                    }
                                                }
                                            }

                                        }
                                    }

                                    // Avatar Kick System
                                    if (IsSpawnableCrateExist(data.Barcode) && SiteStuff.blockednsfw.Contains(data.Barcode))
                                    {
                                        MelonLogger.Warning(
                                            $"NSFW Protection\nReport User :{CleanedNAME(playerusingexploits)}\n" +
                                            $"Report Spawnable Pallet! => {data.Barcode}");
                                        NetworkSpawnerNotif(playerusingexploits, $"NSFW Protection!\n Tried Swapping To : {data.Barcode}");
                                        playerusingexploits.RigRefs.RigManager.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));
                                        SpawnEffects.CallDespawnEffect(playerusingexploits?.MarrowEntity);
                                        return false;
                                    }

                                    // Crash exploit
                                    //data.Stats.massArm = MathF.Max(0.5f, MathF.Min(data.Stats.massArm, 3000f));
                                    //data.Stats.massChest = MathF.Max(0.5f, MathF.Min(data.Stats.massChest, 3000f));
                                    //data.Stats.massHead = MathF.Max(0.5f, MathF.Min(data.Stats.massHead, 3000f));
                                    //data.Stats.massLeg = MathF.Max(0.5f, MathF.Min(data.Stats.massLeg, 3000f));
                                    //data.Stats.massPelvis = MathF.Max(0.5f, MathF.Min(data.Stats.massPelvis, 3000f));
                                    //data.Stats.massTotal = MathF.Max(0.5f, MathF.Min(data.Stats.massTotal, 3000f));
                                    
                                    foreach (var field in data.Stats.GetType().GetFields())
                                    {
                                        if (field.FieldType == typeof(float))
                                        {
                                            float value = (float)field.GetValue(data.Stats);
                                            value = MathF.Max(0.01f, MathF.Min(value, 3000f));
                                            field.SetValue(data.Stats, value);
                                        }
                                    }


                                    //Crash exploit
                                    if (data.Stats.massArm == float.MaxValue ||
                                        data.Stats.massChest == float.MaxValue ||
                                        data.Stats.massHead == float.MaxValue ||
                                        data.Stats.massLeg == float.MaxValue ||
                                        data.Stats.massPelvis == float.MaxValue ||
                                        data.Stats.massTotal == float.MaxValue)
                                    {
                                        MelonLogger.Warning(
                                            $"Prevented Kick/Crash Lobby\nOn Player : {CleanedNAME(playerusingexploits)} ({playerusingexploits.PlayerID.PlatformID})");
                                        NetworkSpawnerNotif(playerusingexploits, "Prevented Kick/Crash Lobby");
                                        return false;
                                    }

                                    //Delete voice exploit
                                    data.Stats.height = MathF.Max(data.Stats.height, 0.1f);

                                    //Remote Targeting Exploits
                                    if (!playerusingexploits.IsMe())
                                    {

                                        if (data.Stats.massArm == float.MaxValue ||
                                            data.Stats.massChest == float.MaxValue ||
                                            data.Stats.massHead == float.MaxValue ||
                                            data.Stats.massLeg == float.MaxValue ||
                                            data.Stats.massPelvis == float.MaxValue ||
                                            data.Stats.massTotal == float.MaxValue)
                                        {
                                            MelonLogger.Warning(
                                                $"Attempted To Crash/Kick Exploit\nExploiter :{CleanedNAME(playerusingexploits)} ({playerusingexploits.PlayerID.PlatformID})");
                                            NetworkSpawnerNotif(playerusingexploits, "Attempted To Crash/Kick Exploit");

                                            return false;
                                        }

                                        // No movement / GM stat exploits
                                        if (!playerusingexploits.PlayerID.Metadata.Loading.GetValue())
                                        {

                                            if (Iswithintwovalues(data.Stats.agility, 0f, 0.2f) ||
                                            Iswithintwovalues(data.Stats.speed, 0f, 0.2f))
                                            {
                                                MelonLogger.Warning(
                                                    $"(Maybe) Used No Moving Exploit\nExploiter :{CleanedNAME(playerusingexploits)} ({playerusingexploits.PlayerID.PlatformID})");
                                                NetworkSpawnerNotif(playerusingexploits, "(Maybe) Used No Moving Exploit");
                                                return false;
                                            }
                                        }
                                    }


                                }
                                if (__instance is PlayerVoiceChatMessage)
                                {
                                    if (voicblocked.Contains(received.PlatformID.Value.ToString()))
                                    {
                                        if (!IsOwner())
                                        {
                                            return false;
                                        }
                                    }
                                }


                                if (__instance is PlayerMetadataRequestMessage)
                                {

                                    if (playerusingexploits.IsMe())
                                        return true;

                                    var data = received.ReadData<PlayerMetadataData>();

                                    if (kickunincodenames)
                                    {
                                       
                                            if (data.Key == "Username")
                                            {
                                                if (ContainsInvisibleUnicode(playerusingexploits.Username))
                                                {
                                                    NotificationNow(FusionProtectorInfo.ClientName, $"{CleanedNAME(playerusingexploits)} Kicked For Invisible Unincode Username!.", NotificationType.WARNING, 3.5f);
                                                    NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                                }
                                            }
                                        
                                    }

                                    if (data.Key == "PermissionLevel")
                                    {
                                        if (data.Value == "OWNER" && playerPermLevel != PermissionLevel.OWNER)
                                        {
                                            NotificationNow(FusionProtectorInfo.ClientName,$"{CleanedNAME(playerusingexploits)} Kicked For Spoofing Permission Level [OWNER]!.",NotificationType.WARNING,3.5f);
                                            NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                        }

                                        if (data.Value == "OPERATOR" && playerPermLevel != PermissionLevel.OPERATOR)
                                        {
                                            NotificationNow(FusionProtectorInfo.ClientName, $"{CleanedNAME(playerusingexploits)} Kicked For Spoofing Permission Level [OPERATOR]!.", NotificationType.WARNING, 3.5f);
                                            NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                        }
                                        if (data.Value == "DEFAULT" && playerPermLevel != PermissionLevel.DEFAULT)
                                        {
                                            NotificationNow(FusionProtectorInfo.ClientName, $"{CleanedNAME(playerusingexploits)} Kicked For Spoofing Permission Level [DEFAULT]!.", NotificationType.WARNING, 3.5f);
                                            NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                        }

                                        if (data.Value == "GUEST" && playerPermLevel != PermissionLevel.GUEST)
                                        {
                                            NotificationNow(FusionProtectorInfo.ClientName, $"{CleanedNAME(playerusingexploits)} Kicked For Spoofing Permission Level [GUEST]!.", NotificationType.WARNING, 3.5f);
                                            NetworkHelper.KickUser(playerusingexploits.PlayerID);
                                        }
                                    }



                                    if (AntiAnimatedName)
                                    {
                                        if (data.Key is "Nickname" or "Username" or "Description")
                                        {
                                            return false;
                                        }
                                    }

                                }


                            }
                        }
                        return true;


                    }
                    catch
                    {
                        return true;
                    }
                }
                //else
               // {

               // }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerRepAvatarMessage), "OnHandleMessage")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class NonHostPlayerRepAvatarMessage
        {
            public static bool Prefix(ReceivedMessage received)
            {
              
                var data = received.ReadData<PlayerRepAvatarData>();

              
                if (!NetworkPlayerManager.TryGetPlayer(received.Sender.Value, out var player))
                {
                    MelonLogger.Warning("Failed to resolve sender player");
                    return false;
                }

                if (!ProtectionFromClients.ValidateAndSanitizeAvatarData(ref data, player))
                {
                    return false;
                }


                string playerId = player.PlayerID.PlatformID.ToString();

                if (!PlayeravatarStuff.TryGetValue(playerId, out var barcodes))
                {
                    barcodes = new System.Collections.Generic.HashSet<string>();
                    PlayeravatarStuff[playerId] = barcodes;
                }

                barcodes.Add(data.Barcode);

                return true;
            }
        }
        [HarmonyPatch(typeof(SpawnResponseMessage), "OnHandleMessage")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class NonHostSpawnResponseEditor
        {
            private static bool Prefix(ReceivedMessage received)
            {

                if (blockallspawnslocally)
                    return false;

                if (NetworkInfo.IsHost)
                    return true;

                if (!received.Sender.HasValue)
                    return true;

                var data = received.ReadData<SpawnResponseData>();
                
                if (data?.SpawnData?.Barcode == null)
                    return true;

                string barcode = data.SpawnData.Barcode;

                if (barcode == "SLZ.BONELAB.Core.Spawnable.GameplaySystems")
                {
                    return false;
                }


                var reference = new SpawnableCrateReference(barcode);
                if (reference?.Crate?.Pallet == null)
                    return true;

                NetworkPlayerManager.TryGetPlayer(received.Sender.Value, out var player);


                ulong platformID = player.PlayerID.PlatformID;
                string key = platformID.ToString();

                var crate = reference.Crate;
                var pallet = crate.Pallet;

                string palletName = StripColorTags(crate.name ?? string.Empty);
                string palletAuthor = pallet.Author ?? "Unknown";

                int modioID = CrateFilterer.GetModID(pallet);

                bool isAvatar = barcode.Contains(".Avatar.");

                var info = (
                    player.JR_Nickname() ?? "Unknown",
                    player.JR_Username() ?? "Unknown",
                    platformID,
                    palletName,
                    palletAuthor,
                    modioID,
                    barcode,
                    isAvatar
                );

                if (!PlayerSpawningStuff.TryGetValue(key, out var barcodes))
                    PlayerSpawningStuff[key] = barcodes = new System.Collections.Generic.HashSet<string>();

                barcodes.Add(barcode);

                if (!SpawnLogs.Contains(info))
                    SpawnLogs.Add(info);

                if (spawnlogsmelonlog)
                {
                    MelonLogger.Warning(
                        $"Player({CleanedNAME(player)} [{platformID}])\n" +
                        $"Is Spawning\n" +
                        $"(Pallet Name : {palletName} | Pallet Author : {palletAuthor})\n" +
                        $"MOD IO # ID {modioID}\n" +
                        $"Sending Spawnable Barcode ID : {barcode}"
                    );
                }

                if (globalblocklistnow)
                {
                    if (SiteStuff.globalblocklistavatar.Contains(barcode) ||
                        SiteStuff.globalblocklistspawnable.Contains(barcode))
                    {
                        if (globalblocklistnotification)
                            NetworkSpawnerNotif(player, $"[FP] Global Blacklisted Spawnable : {barcode}", NotificationType.ERROR, 1.5f);
                        return false;
                    }

                    if (SiteStuff.globalblocklistmodioid.Contains(modioID))
                    {
                        if (globalblocklistnotification)
                            NetworkSpawnerNotif(player, $"[FP] Global Blacklisted Spawnable Mod.IO Mod : {modioID}", NotificationType.ERROR, 1.5f);
                        return false;
                    }

                    if (SiteStuff.globalblocklistpallet.Contains(palletName))
                    {
                        if (globalblocklistnotification)
                            NetworkSpawnerNotif(player, $"[FP] Global Blacklisted Spawnable Pallet : {palletName}", NotificationType.ERROR, 1.5f);
                        return false;
                    }

                    if (SiteStuff.globalblocklistauthor.Contains(palletAuthor))
                    {
                        if (globalblocklistnotification)
                            NetworkSpawnerNotif(player, $"[FP] Global Blacklisted Spawnable Author : {palletAuthor}", NotificationType.ERROR, 1.5f);
                        return false;
                    }
                }

                if (spawnprotectionsnot_host)
                {
                    if (BLOCKAVATARSASSPAWNABLES && IsAvatarCrateExist(barcode))
                        return false;

                    if (IsSpawnableCrateExist(barcode) && SiteStuff.blockednsfw.Contains(barcode))
                    {
                        string msg = $"NSFW Protection\nReport Spawnable! => {palletName} [{palletAuthor}]";
                        MelonLogger.Warning(msg);
                        NetworkSpawnerNotif(player, msg, NotificationType.ERROR, 1.5f);
                        return false;
                    }

                    if (blockedspawnables && BlockedSpawnables.Contains(barcode))
                    {
                        if (blockspwnnotis)
                            NetworkSpawnerNotif(player, $"Blocked Spawnable : {palletName} [{palletAuthor}]", NotificationType.ERROR, 1.5f);
                        else
                            MelonLogger.Msg($"Person Doing : {CleanedNAME(player)}\nBlocked Spawnable : {palletName} [{palletAuthor}]");

                        return false;
                    }

                    if (blockedspawnies.Contains(key))
                        return false;

                    if (ModIDBlocker && modidblocked.Contains(modioID.ToString()))
                    {
                        NetworkSpawnerNotif(player, $"Mod ID Blocked : {modioID}", NotificationType.ERROR, 1.5f);
                        return false;
                    }

                    if (BlockPalletCompletely && blockentirepallet.Contains(palletName))
                    {
                        if (blockspwnnotis)
                            NetworkSpawnerNotif(player, $"Blocked Entire Pallet : {palletName} [{palletAuthor}]", NotificationType.ERROR, 1.5f);

                        return false;
                    }

                    if (BlockAuthorOfSpawnable && blockentireauthor.Contains(palletAuthor))
                    {
                        if (blockspwnnotis)
                            NetworkSpawnerNotif(player, $"Blocked Entire Author : {palletName} [{palletAuthor}]", NotificationType.ERROR, 1.5f);
                        else
                            MelonLogger.Msg($"Person Doing : {CleanedNAME(player)}\nBlocked Entire Author : {palletName} [{palletAuthor}]");

                        return false;
                    }

                    if (warnedspawnables && WarnedSpawnables.Contains(barcode))
                    {
                        NetworkSpawnerNotif(player, $"Warning Spawnable : {palletName} [{palletAuthor}]", NotificationType.WARNING, 2.5f);
                    }

                }



                return true;
            }
        }
        [HarmonyPatch(typeof(ModInfoResponseMessage), "OnHandleMessage")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class NonHostModInfoResponseEditor
        {
            private static bool Prefix(ReceivedMessage received)
            {
                    
                if (blockallspawnslocally)
                    return false;

                if (!NetworkInfo.IsHost)
                {
                    var data = received.ReadData<ModInfoResponseData>();
                    if (data?.ModFile?.File == null)
                        return true;

                    var modId = data.ModFile.File.ModID;

                    NetworkPlayerManager.TryGetPlayer(received.Sender.Value, out var player);

                    if (globalblocklistnow &&
                        SiteStuff.globalblocklistmodioid.Contains(modId))
                    {
                        if (globalblocklistnotification)
                            NetworkSpawnerNotif(player, $"[FP] Global Blacklisted Mod.IO Mod Spawned: {modId}", NotificationType.ERROR, 1.5f);

                        return false;
                    }

                    if (ModIDBlocker && modidblocked.Contains(modId.ToString()))
                    {
                        NetworkSpawnerNotif(player, $"Mod ID Blocked : {modId}", NotificationType.ERROR, 1.5f);
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(PlayerRepDamageMessage), "OnHandleMessage")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class NonHostPlayerRepDamageEditor
        {
            private static bool Prefix(ReceivedMessage received)
            {
                if (!NetworkInfo.IsHost)
                {
                    var data = received.ReadData<PlayerRepDamageData>();

                    if (data?.Attack?.Attack == null)
                        return true;

                    if (data.Attack.Attack.damage == float.MaxValue)
                    {
                        if (NetworkPlayerManager.TryGetPlayer(received.Sender.Value, out var player))
                            NetworkSpawnerNotif(player, "Kill Attempt By Client");

                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(LobbyInfo), "get_NameTags")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class SpoofNameTags
        {
            private static bool Prefix(ref bool __result)
            {
                if (forcenametagson)
                {
                    __result = true;

                    return false;
                }
                return true;
            }

        }

        [HarmonyPatch(typeof(SteamNetworkLayer), "OnPlayerJoin")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class PlayerJoinLogger
        {
            private static void Postfix(PlayerID id)
            {

                if (id != null && !JoinLogger.Any(p => p?.PlatformID == id.PlatformID))
                {
                    JoinLogger.Add(new PlayerInfo
                    {
                        Nickname = id.Metadata.Nickname.GetValue(),
                        Username = id.Metadata.Username.GetValue(),
                        PlatformID = id.PlatformID,
                        AvatarModID = id.Metadata.AvatarModID.GetValue(),
                        AvatarTitle = id.Metadata.AvatarTitle.GetValue(),
                        Description = id.Metadata.Description.GetValue()
                    });
                }

            }

        }
        [HarmonyPatch(typeof(ConnectionRequestMessage), "OnHandleMessage")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class ConnectionBlocker
        {
            private static bool Prefix(ReceivedMessage received)
            {

                var requestInfo = received.ReadData<ConnectionRequestData>();
                var platformId = received.PlatformID;

                if (kicktillrestart.Contains(platformId.Value))
                {
                    ConnectionSender.SendConnectionDeny(platformId.Value, string.Empty);
                    return false;
                }

                if (TimedObject.ActiveTimers.TryGetValue(platformId, out var timer))
                {
                    if (!timer.IsExpired())
                    {
                        ConnectionSender.SendConnectionDeny(platformId.Value, string.Empty);
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Health), nameof(Health.TAKEDAMAGE))]
        internal static class RealGodModeScrubs
        {
            static bool Prefix()
            {
                if (SpawnProtectionPatch.spawnProtection)
                {
                    return false;
                }

                if (godmode && AreYouOWNER())
                {
                    if (!GamemodeManager.IsGamemodeStarted)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        [HarmonyPatch(typeof(LocalRagdoll), "KnockoutCoroutine")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class RealKnockOutScrubs
        {
            public static bool Prefix()
            {

                if (antiknockout && AreYouOWNER())
                {
                    if (!GamemodeManager.IsGamemodeStarted)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        [HarmonyPatch(typeof(AmmoInventory), nameof(AmmoInventory.RemoveCartridge))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class RealInfiniteAmmoScrubs
        {
            static void Postfix(AmmoInventory __instance, CartridgeData cartridge, int count)
            {
                if (unlammo)
                {
                    if (!GamemodeManager.IsGamemodeStarted)
                    {


                        __instance.AddCartridge(cartridge, count);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverBegin))]
        internal static class ForceGrabDisabler
        {
            static bool Prefix()
            {
                return !forcegrabdisablernow;
            }
        }

        [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverEnd))]
        internal static class ForceGrabDisabler2
        {
            static bool Prefix()
            {
                return !forcegrabdisablernow;
            }
        }
        [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverUpdate))]
        internal static class ForceGrabDisabler3
        {
            static bool Prefix()
            {
                return !forcegrabdisablernow;
            }
        }

        [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnStartAttach))]
        internal static class ForceGrabDisabler4
        {
            static bool Prefix()
            {
                return !forcegrabdisablernow;
            }
        }

        [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnForcePullComplete))]
        internal static class ForceGrabDisabler5
        {
            static bool Prefix()
            {
                return !forcegrabdisablernow;
            }
        }

        [HarmonyPatch(typeof(WindBuffetSFX), nameof(WindBuffetSFX.Awake))]
        internal static class AudioPatch1
        {
            static bool Prefix()
            {

                return !disablewindsfx;

            }
        }


        [HarmonyPatch(typeof(RigVoiceSource), "get_MinMicrophoneDistance")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class ProxRemoverPart1
        {
            static bool Prefix(ref float __result)
            {
                if (removeproxchat)
                {
                    __result = 9999999f;

                    return false;
                }
                return true;
            }

        }
        [HarmonyPatch(typeof(RigVoiceSource), "get_MaxMicrophoneDistance")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class ProxRemoverPart2
        {
            static bool Prefix(ref float __result)
            {
                if (removeproxchat)
                {
                    __result = 9999999f;
                    return false;
                }
                return true;
            }

        }
        [HarmonyPatch(typeof(RigVoiceSource), "UpdateOcclusion")]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class ProxRemoverPart3
        {
            static bool Prefix()
            {
                if (removeproxchat)
                {
                    return false;
                }
                return true;
            }

        }


        //remove masterlist
        [HarmonyPatch(typeof(TrustedListManager), nameof(TrustedListManager.VerifyPlayer))]
        [HarmonyPatch(new[] { typeof(ulong), typeof(string) })]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class FusionRemoveDaMastas
        {
            static bool Prefix(ref TrustedStatus __result)
            {
                __result = TrustedStatus.None;
                return false;
            }
        }
       
        
        //removes masterlist stuff cause its stupid and no one should have power except you in your lobby fp is made to protect and gives the user more ability to protect themselfs from trolls
        [HarmonyPatch(typeof(TrustedListManager), nameof(TrustedListManager.VerifyPlayer))]
        [HarmonyPatch(new[] { typeof(TrustedPlayer[]), typeof(ulong),typeof(string) })]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class FusionRemoveDaMastas2
        {
            static bool Prefix(ref TrustedStatus __result)
            {
                __result = TrustedStatus.None;
                return false;
            }
        }
        [HarmonyPatch(typeof(MasterPermissionsManager), nameof(MasterPermissionsManager.IsMaster))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class FusionRemoveDaMastas3
        {
            static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }
        //

        //remove and enable fusions global ban list
        [HarmonyPatch(typeof(GlobalBanManager), nameof(GlobalBanManager.GetBanInfo))]
        [HarmonyPriority(int.MaxValue - 1)]
        internal static class Patch_GetBanInfo
        {
            static bool Prefix(ref GlobalBanInfo __result)
            {
                if (REMOVEDGLOBALBANLIST)
                {
                    __result = null;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GlobalBanManager), nameof(GlobalBanManager.IsBanned))]
        [HarmonyPriority(int.MaxValue - 1)]
        [HarmonyPatch(new[] { typeof(LobbyInfo) })]
        internal static class Patch_IsBanned_Lobby
        {
            static bool Prefix(ref bool __result)
            {
                if (REMOVEDGLOBALBANLIST)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GlobalBanManager), nameof(GlobalBanManager.IsBanned))]
        [HarmonyPriority(int.MaxValue - 1)]
        [HarmonyPatch(new[] { typeof(PlatformInfo) })]
        internal static class Patch_IsBanned_Platform
        {
            static bool Prefix(ref bool __result)
            {
                if (REMOVEDGLOBALBANLIST)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        //

        #endregion

        #region Functions

        internal enum antimodguntype
        {
            DefaultBlue,
            AnySpawnGun,
        }
        internal enum SpawnableSearchType
        {
            Spawn,
            CopyDetailsToClipboard,
            UnFavoriteAndFavorite,
            DespawnAllOfThis,
            SetInSpawnGun

        }
        internal enum AvatarSearchType
        {
            ChangeInto,
            CopyDetailsToClipboard,
            SetBodyLog

        }
        internal enum DespawnerAll
        {
            NoFilter,
            Guns,
            Melees,
            Npcs,
            EverythingButGuns,
            EverythingButMelees,
            EverythingButNpcs,
            NetworkProps,
            AllNotButtonsLeverCircuits

        }
        internal enum handnow
        {
            Left,
            Right
        }
        internal enum WhichHand
        {
            Both,
            Left,
            Right
        }
        internal enum RandomizerType
        {
            AllAvatars,
            AllNPCs,
            AllSpawnables,
            NoTagsSpawnables,
            AllWeapons,
            AllMelees,
            AllRifle,
            AllSMG,
            AllRanged,
            AllPistol,
            AllShotgun,
            AllSniper,
            AllBlunt,
            AllBlade,
            AllKnife,


        }
        internal enum Slots
        {
            HolsterLeft,
            HolsterRight,
            BackLeft,
            BackRight,
            BottomRight,
            Head
        }

        internal static void CreateDevToolPreset(string titleOfPreset,Item item1,Item item2,Item item3,Item item4,Item item5)
        {
            bool exists = false;

            foreach (var t in CreateCheatToolsPreset.CheatPresets)
            {
                if (string.Equals(t.TitleOfPreset, titleOfPreset, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                CreateCheatToolsPreset.CheatPresets.Add(
                    new CreateCheatToolsPreset(
                        titleOfPreset,
                        item1,
                        item2,
                        item3,
                        item4,
                        item5
                    )
                );

                CreateCheatToolsPreset.SavePresets();

                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    $"Added Preset {titleOfPreset}!",
                    NotificationType.SUCCESS,
                    2.5f
                );
            }
            else
            {
                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    "This Preset Name Exists Already!",
                    NotificationType.WARNING,
                    2.5f
                );
            }
        }
        internal static void CreateBodyLogPage(string pagename, string slot1 = "c3534c5a-94b2-40a4-912a-24a8506f6c79", string slot2 = "c3534c5a-94b2-40a4-912a-24a8506f6c79", string slot3 = "c3534c5a-94b2-40a4-912a-24a8506f6c79", string slot4 = "c3534c5a-94b2-40a4-912a-24a8506f6c79", string slot5 = "c3534c5a-94b2-40a4-912a-24a8506f6c79", string slot6 = "c3534c5a-94b2-40a4-912a-24a8506f6c79")
        {

            bool exists = false;

            foreach (var t in BodyLogPage.BodyLogPages)
            {
                if (string.Equals(t.TitleOfPreset, pagename, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                BodyLogPage.BodyLogPages.Add(new BodyLogPage(pagename, slot1,
                    slot2,
                    slot3,
                    slot4,
                    slot5,
                    slot6));
                BodyLogPage.SavePresets();
                NotificationNow(FusionProtectorInfo.ClientName,
$"Added Preset {pagename}!",
NotificationType.SUCCESS,
2.5f);
            }
            else
            {
                NotificationNow(FusionProtectorInfo.ClientName,
                    "This Preset Name Exists Already!",
                    NotificationType.WARNING,
                    2.5f);
            }

        }
        internal static string DataToJsonString(object listorsomething)
        {
            string returnvalue = "";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };
            returnvalue = JsonSerializer.Serialize(listorsomething, options);

            return returnvalue;
        }
        internal static void SaveTXTFunc()
        {
            if (logrecentlymet)
            {
                File.WriteAllText(RECNETLYMETLOGGED, DataToJsonString(JoinLogger));
            }
            if (logmediaplayer)
            {
                File.WriteAllText(MEDIAPLAYERLOGS, DataToJsonString(MediaPlayerLogs));
            }
            if (loglobbiessince)
            {
                File.WriteAllText(LOBBIESLOGGEDSINCE, DataToJsonString(CachedLobbies));
            }
            if (logplayersince)
            {
                File.WriteAllText(PLAYERSLOGGEDSINCE, DataToJsonString(PlayersOnline));
            }
        }
        internal static bool MostUsedItems(string barcode)
        {
            return SiteStuff.mostusedexcl.Contains(barcode);
        }
        internal static void DeleteModioMod(Pallet PalletNow, bool notif = true)
        {
            if (CrateFilterer.GetModID(PalletNow) != -1)
            {

                var (folder, manif, fullpal, manny) = GetPalletFolder(PalletNow?.name);

                if (!string.IsNullOrEmpty(folder) && System.IO.Directory.Exists(folder))
                {
                    UnloadPallet(PalletNow);
                    System.IO.Directory.Delete(folder, true);
                    System.IO.File.Delete(manif);
                    if (notif)
                        NotificationNow(FusionProtectorInfo.ClientName, $"Deleted {PalletNow?.name}.", NotificationType.ERROR, 3.0f);
                }
            }
        }
        internal static void UnloadPallet(Pallet pallet)
        {
            if (pallet == null)
                return;

            var bundles = pallet._packedAssets;
            if (bundles != null)
            {
                foreach (var bundle in bundles)
                {
                    bundle?.marrowAsset.UnloadAsset(true);
                }
            }

            AssetWarehouse.Instance.UnloadPallet(pallet);

            MelonLogger.Msg($"Unloaded pallet: {pallet.Title}");
        }
        internal static (string folderpath, string manifestpath, string fullpallet, PalletManifest palletm) GetPalletFolder(string palletTitle, bool openSelectionPc = false)
        {
            var manifests = AssetWarehouse.Instance?.GetPalletManifests()?.ToArray();
            if (manifests == null)
                return (null, null,null,null);

            var pallet = manifests
                .FirstOrDefault(m => m.Pallet != null && m.Pallet.Title == palletTitle);

            if (pallet?.PalletPath == null)
                return (null,null,null, null);
            
            string folderPath = Path.GetDirectoryName(pallet.PalletPath);
            if (openSelectionPc && System.IO.Directory.Exists(folderPath))
            {
                Process.Start("explorer.exe", $"/select,\"{folderPath}\"");
                NotificationNow(FusionProtectorInfo.ClientName,$"Opened {palletTitle} download folder.",NotificationType.SUCCESS,2f);
            }

            return (folderPath, pallet.ManifestPath,pallet.PalletPath, pallet);
        }
        internal static IEnumerator OnStartOfGame()
        {   
            if (homeworldsnow)
            {
                var defaultBarcode = "c2534c5a-80e1-4a29-93ca-f3254d656e75";
                var barcode = defaultBarcode;

                if (File.Exists(homeworldnow))
                {
                    var text = File.ReadAllText(homeworldnow);
                    if (IsBarcodeInGame(text))
                        barcode = text;
                }

                SceneStreamer.Load(new LevelCrateReference(barcode).Barcode);
            }

            yield return new WaitForSeconds(15.0f);

            MelonCoroutines.Start(SiteStuff.UpdateSites());
        }
        internal static IEnumerator RunAfterBuild()
        {
            yield return new WaitForSeconds(5.0f);
            ManuallySave(false);

            if (disablewindsfx)
            {
                var icons = GameObject.FindObjectsOfType<WindBuffetSFX>();
                foreach (var icon in icons)
                {
                    icon.windBuffetClip = null;
                    icon._buffetSrc = null;
                }
            }

            if (grippy)
            {
                var icons = GameObject.FindObjectsOfType<InteractableIcon>();
                foreach (var icon in icons)
                {
                    icon.IconSize = 0f;
                    icon.scaledIconSize = 0f;
                }
            }

            if (infiniteinventory)
            {
                NotificationNow(FusionProtectorInfo.ClientName, "Stored Current Inventory!\nRe-Enable For Storing New Stuff!", NotificationType.SUCCESS, 2.0f);
                StoreInventoryItems();
            }


            if (!HideFusionProtector)
            {
                ModuleMessageManager.RegisterHandler<SendNotificationMessage>();
                ModuleMessageManager.RegisterHandler<ProtectorPingMessage>();
            }
            ModuleMessageManager.RegisterHandler<OwnerServerSettingMessage>();
            ModuleMessageManager.RegisterHandler<SendGameModeOverMessage>();
            ModuleMessageManager.RegisterHandler<SendBodyLogMessage>();
            ModuleMessageManager.RegisterHandler<SendModIDMessage>();
            ModuleMessageManager.RegisterHandler<ShareBodyLogPageMessage>();
            ModuleMessageManager.RegisterHandler<ShareDevToolPresetMessage>();
            ModuleMessageManager.RegisterHandler<SendBitMessage>();
            ModuleMessageManager.RegisterHandler<SendBase64FileMessage>();


        }
        internal static ulong SteamIdYours()
        {
            if (!Steamworks.SteamClient.IsValid || !Steamworks.SteamClient.IsLoggedOn)
                return 0;

            return Steamworks.SteamClient.SteamId.Value;
        }
        internal static void WatchFileChanges(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            FileSystemWatcher watcher = new(directory)
            {
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            watcher.Changed += (sender, e) =>
            {
               
                ReloadList();
                PermissionList.ReadFile();
            };

            watchers.Add(watcher);
        }
        internal static void NetworkSpawnerNotif(NetworkPlayer playerusingexploits, string messageforit, NotificationType typenow = NotificationType.ERROR, float notificationtime = 1.5f, bool savetothemenu = true)
        {

            var playerName = playerusingexploits != null ? CleanedNAME(playerusingexploits) : "Unknown";
            var msg = playerusingexploits.PlayerID.IsHost ?  messageforit : $"Person Doing : {playerName}\n{messageforit}";

            NotificationNow(FusionProtectorInfo.ClientName, msg, typenow, notificationtime, true, savetothemenu);
     
            var entry = (
            Nickname: playerusingexploits.JR_Nickname(),
            Username: playerusingexploits.JR_Username(),
            PlatformId: playerusingexploits.PlayerID.PlatformID.ToString(),
            ExploitType: messageforit
            );

            if (!ClientExploitLogs.Contains(entry))
            {
                ClientExploitLogs.Add(entry);
            }
        }
        internal static void EditFusionPreferences(string valuetoedit, object valuetochange)
        {
            var cat = MelonPreferences.GetCategory("BONELAB Fusion");
            if (cat != null)
            {
                var targetEntry = cat.Entries.FirstOrDefault(e => e.DisplayName == valuetoedit);
                if (targetEntry != null)
                {
                    targetEntry.BoxedValue = valuetochange;
                    cat.SaveToFile();
                }
            }
        }
        internal IEnumerator FPSCheck()
        {
            fpsCheckRunning = true;

            float badTime = 0f;

            for (float t = 0f; t < 10f; t += Time.deltaTime)
            {
                if (fps <= fpslimit)
                {
                    badTime += Time.deltaTime;

                    if (badTime >= 6f)
                    {
                        DespawnAll(DespawnerTimerz);
                        break;
                    }
                }
                else
                {
                    badTime = 0f;
                }

                yield return null;
            }

            fpsCheckRunning = false;
        }
        internal static void TryPatchit(string dll, string typeName, string methodName, MethodInfo prefix, MethodInfo postfix)
        {
            var asm = System.AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == dll);

            if (asm == null)
                return;

            var type = asm.GetType(typeName);
            if (type == null)
                return;

            var method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method == null)
                return;

            harmony.Patch(
                method,
                prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                postfix: postfix != null ? new HarmonyMethod(postfix) : null
            );

        }
        internal static bool IsMagazine(SpawnableCrateReference reffy)
        {
            if (reffy == null || reffy.Crate == null)
                return false;

            var crate = reffy.Crate;
            var name = crate.name.ToLower();
            var barcode = crate.Barcode?.ID.ToLower();
            
            for (int i = 0; i < crate.Tags.Count; i++)
            {
                crate.Tags[i] = crate.Tags[i]?.ToLowerInvariant();
            }

            var tags = crate.Tags;

            bool matches =
    name.Contains(" mag ") ||
    name.EndsWith(" mag") ||
    name.StartsWith("mag ") ||
    name.StartsWith("mag_") ||
    name.Contains("magazine") ||
    name.EndsWith(" mag") ||
    name.EndsWith("_mag") ||
    name.StartsWith("cartridge") ||
    name.Contains("cartridge") ||
    name.EndsWith(" shells") ||
    name.StartsWith("cartridge - ") ||
    

    tags.Contains("mag") ||
    tags.Contains("magazine") ||
    tags.Contains("magazines") ||
    tags.Contains("cartridge") ||
   

    barcode.EndsWith("mag") ||
    barcode.EndsWith("cartridge") ||
    barcode.StartsWith("cartridge") ||
    barcode.Contains("cartridge");



            return matches;
        }
        internal static bool IsMagazine(Crate reffy)
        {
            if (reffy == null)
                return false;

            var crate = reffy;
            var name = crate.name.ToLower();
            var barcode = crate.Barcode?.ID.ToLower();

            for (int i = 0; i < crate.Tags.Count; i++)
            {
                crate.Tags[i] = crate.Tags[i]?.ToLowerInvariant();
            }

            var tags = crate.Tags;

            bool matches =
    name.Contains(" mag ") ||
    name.EndsWith(" mag") ||
    name.StartsWith("mag ") ||
    name.StartsWith("mag_") ||
    name.Contains("magazine") ||
    name.EndsWith(" mag") ||
    name.EndsWith("_mag") ||
    name.StartsWith("cartridge") ||
    name.Contains("cartridge") ||
    name.EndsWith(" shells") ||
    name.StartsWith("cartridge - ") ||


    tags.Contains("mag") ||
    tags.Contains("magazine") ||
    tags.Contains("magazines") ||
    tags.Contains("cartridge") ||


    barcode.EndsWith("mag") ||
    barcode.EndsWith("cartridge") ||
    barcode.StartsWith("cartridge") ||
    barcode.Contains("cartridge");



            return matches;
        }
        internal static (PullCordDevice bodylogreturn, UnityEngine.MeshRenderer Outerring) JR_BodyLog(PhysicsRig PlayerTodo)
        {
            var physicsRig = PlayerTodo;
            if (physicsRig != null)
            {
                var right = physicsRig.m_elbowRt?.Find("BodyLogSlot/BodyLog");
                Transform rightMeshT = physicsRig.m_elbowRt?.Find("BodyLogSlot/BodyLog/BodyLog/BodyLog");
                UnityEngine.MeshRenderer rightMesh = rightMeshT != null ? rightMeshT.GetComponent<UnityEngine.MeshRenderer>() : null;

                if (right != null)
                {
                    var device = right.GetComponent<PullCordDevice>();
                    if (device != null)
                        return (device, rightMesh);
                }

                var left = physicsRig.m_elbowLf?.Find("BodyLogSlot/BodyLog");
                Transform leftMeshT = physicsRig.m_elbowLf?.Find("BodyLogSlot/BodyLog/BodyLog/BodyLog");
                UnityEngine.MeshRenderer leftMesh = leftMeshT != null ? leftMeshT.GetComponent<UnityEngine.MeshRenderer>() : null;

                if (left != null)
                {
                    var device = left.GetComponent<PullCordDevice>();
                    if (device != null)
                        return (device, leftMesh);
                }
            }

            return (null, null);
        }
        internal static NetworkPlayer JR_YourNetworkPlayer()
        {
            return LocalPlayer.GetNetworkPlayer();
        }
        internal static System.Collections.Generic.HashSet<NetworkEntity> NetworkEntities()
        {
            return NetworkEntityManager.IDManager?.RegisteredEntities?.EntityIDLookup?.Keys?
                .Where(p =>
                {
                    var entity = p.JR_GetMarrowEntity();
                    return entity != null &&
                           !entity.JR_IsNetPlayer() &&
                           entity.JR_GetBarcodeID() != "Lakatrazz.FusionContent.Spawnable.NameTag" && p != JR_YourNetworkPlayer().NetworkEntity;
                })
                .ToHashSet();
        }
        internal static System.Collections.Generic.HashSet<NetworkPlayer> NetworkPlayers(bool excludeMe = false, bool excludeMeAndHost = false)
        {
            return LabFusion.Entities.NetworkPlayer.Players
                .Where(p =>
                {
                    if (p == null || !p.PlayerID.IsValid)
                        return false;

                    if (excludeMe && p.IsMe())
                        return false;

                    if (excludeMeAndHost && (p.IsMe() || p.PlayerID.IsHost))
                        return false;

                    return true;
                })
                .OrderByDescending(p => p.PlayerID.IsHost)
                .ThenBy(p =>
                {
                    return StripColorTags(string.IsNullOrEmpty(p.Username) ? "" : p.Username).Trim();
                }, StringComparer.OrdinalIgnoreCase)
                .ToHashSet();
        }
        internal static string BodySlot(Slots slot)
        {
            return slot switch
            {
                Slots.HolsterLeft => "SideLf",
                Slots.HolsterRight => "SideRt",
                Slots.BackLeft => "BackLf",
                Slots.BackRight => "BackRt",
                Slots.BottomRight => "BackCt",
                Slots.Head => "HeadSlot",
                _ => string.Empty
            };
        }
        internal static Hand JR_YourGetHand(WhichHand hand)
        {
            var rig = Player.RigManager?.physicsRig;
            if (rig == null)
                return null;

            return hand switch
            {
                WhichHand.Left => rig.leftHand,
                WhichHand.Right => rig.rightHand,
                _ => null
            };
        }
        internal static IEnumerator DumpPalletsCoroutine()
        {
            if (_isDumpRunning)
            {
                MelonLogger.Error("⚠️ Crate dump is already running!");
                yield break;
            }

            _isDumpRunning = true;
            MelonLogger.Warning("🔍 Starting crate dump...");

            StringBuilder sb = new();

            var pallets = AssetWarehouse.Instance.GetPallets();
            int batchSize = 150;
            int processedCrates = 0;

            int totalCrates = 0;
            foreach (var pallet in pallets)
                if (pallet.Crates != null)
                    totalCrates += pallet.Crates.Count;

            foreach (var pallet in pallets)
            {
                sb.AppendLine($"[Pallet] {StripColorTags(pallet.Title)} (Author: {pallet.Author})");

                if (pallet.Crates == null) continue;

                foreach (var cratey in pallet.Crates)
                {
                    if (cratey.Tags == null) continue;

                    sb.AppendLine($"  └── [Crate] {StripColorTags(cratey.Title)}");
                    sb.AppendLine($"      └── Barcode: {cratey.Barcode.ID}");

                    var tagsSet = new System.Collections.Generic.HashSet<string>();
                    foreach (var tag in cratey.Tags)
                        tagsSet.Add(tag);

                    if (tagsSet.Count > 0)
                        sb.AppendLine($"      └── Tags: {string.Join(", ", tagsSet)}");

                    processedCrates++;

                    if (processedCrates % batchSize == 0)
                    {
                        int barLength = 30;
                        float progress = (float)processedCrates / totalCrates;
                        int filled = (int)(progress * barLength);
                        string bar = $"[{new string('#', filled)}{new string('-', barLength - filled)}] {progress:P1}";
                        MelonLogger.Warning($"⏳ Dumping crates... {bar}");

                        yield return new WaitForSeconds(0.1f);
                    }
                }

                sb.AppendLine();
            }

            MelonLogger.Warning("⏳ Dumping crates... [##############################] 100%");

            try
            {
                File.WriteAllText(PalletDumpLocation, sb.ToString());
                MelonLogger.Warning($"✅ Crate dump written to: {PalletDumpLocation}");

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c start \"\" \"{PalletDumpLocation}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                });
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to write pallet dump: {ex.Message}");
            }

            _isDumpRunning = false;
        }
        internal static void CheckSteamID(ulong steamid)
        {
            string steamProfileUrl = $"https://steamcommunity.com/profiles/{steamid}";
            try
            {

                OpenPageNow(steamProfileUrl);
                NotificationNow(FusionProtectorInfo.ClientName, "Opened Steam profile in browser.", NotificationType.SUCCESS);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Failed to open URL: {ex.Message}");
                NotificationNow(FusionProtectorInfo.ClientName, "Failed to open profile link.", NotificationType.ERROR);
            }
        }
        internal static void NotificationNow(string Title, string Message, NotificationType Type, float length = 1.0f, bool showtitle = true, bool savetomenu = false, System.Action Accept = default, System.Action Decline = default)
        {
            if (donotdisturb)
                return;
            LabFusion.UI.Popups.Notifier.Cancel(LabFusion.UI.Popups.Notifier.CurrentNotification);
            LabFusion.UI.Popups.Notifier.Send(new LabFusion.UI.Popups.Notification
            {
                PopupLength = length,
                Title = Title,
                Message = Message,
                ShowPopup = showtitle,
                SaveToMenu = savetomenu,
                OnAccepted = Accept,
                OnDeclined = Decline,
                Type = Type,
                Tag = FusionProtectorInfo.ClientName
            });
        }

        //this bypasses the do not disturb
        internal static void NotificationNowAlways(string Title, string Message, NotificationType Type, float length = 1.0f, bool showtitle = true, bool savetomenu = false, System.Action Accept = default, System.Action Decline = default)
        {
          
            LabFusion.UI.Popups.Notifier.Cancel(LabFusion.UI.Popups.Notifier.CurrentNotification);
            LabFusion.UI.Popups.Notifier.Send(new LabFusion.UI.Popups.Notification
            {
                PopupLength = length,
                Title = Title,
                Message = Message,
                ShowPopup = showtitle,
                SaveToMenu = savetomenu,
                OnAccepted = Accept,
                OnDeclined = Decline,
                Type = Type,
                Tag = FusionProtectorInfo.ClientName
            });
        }
        internal static (object result, System.Type returnType) InvokeWithType(System.Type classToAccess, string functionToInvoke, object[] functionParameters)
        {
            var method = AccessTools.Method(classToAccess, functionToInvoke);
            if (method == null)
                return (null, null);

            var result = method.Invoke(null, functionParameters);
            return (result, method.ReturnType);
        }
        internal static void ToggleAddRemoveFromFile(string item, System.Collections.Generic.HashSet<string> listToUse,string filePath,string notificationTitle,string addedMessage,string removedMessage,bool notifications = true)
        {
            if (string.IsNullOrWhiteSpace(item))
                return;


            bool removed = listToUse.Remove(item);

            if (removed)
            {
                if (notifications)
                    NotificationNow(notificationTitle, removedMessage, NotificationType.ERROR, 3.0f);
            }
            else
            {
                listToUse.Add(item);

                if (notifications)
                    NotificationNow(notificationTitle, addedMessage, NotificationType.SUCCESS, 3.0f);
            }

            File.WriteAllLines(filePath, listToUse);
        }
        internal static (string left, string right) ParseLine(string line)
        {
            var linetrim = line.Trim();
            var parts = linetrim.Split(':');

            var left = parts.Length > 0 ? parts[0].Trim() : linetrim;
            var right = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            return (left, right);
        }
        internal static string JR_YourAvatarBarcodeID()
        {
            return Player.RigManager?.AvatarCrate?.Barcode?.ID ?? "NULL";
        }
        internal static void ChangeIntoAvi(string avibarcode)
        {
            if (IsBarcodeInGame(avibarcode))
            {
                if (JR_YourAvatarBarcodeID() == avibarcode)
                    return;


                var CrateRed = new AvatarCrateReference(avibarcode);

                Player.RigManager.SwapAvatarCrate(CrateRed.Barcode);

                DataManager.ActiveSave.PlayerSettings.CurrentAvatar = CrateRed.Barcode.ID;
                DataManager.TrySaveActiveSave(SaveFlags.Complete);

                var crate = CrateRed.Crate;
                if (crate != null)
                {
                    LocalPlayer.Metadata.AvatarTitle.SetValue(crate.Title);
                    LocalPlayer.Metadata.AvatarModID.SetValue(CrateFilterer.GetModID(crate.Pallet));
                }
            }
        }
        internal static bool ContainsInvisibleUnicode(string text)
        {
            foreach (char c in text)
            {
                var category = Char.GetUnicodeCategory(c);

                if (category == UnicodeCategory.Format ||
                    category == UnicodeCategory.Control ||
                    category == UnicodeCategory.OtherNotAssigned)
                {
                    return true;
                }
            }

            return false;
        }
        internal enum SearchMethod
        {
            CrateNames,
            BarcodeIDNames,
            PalletName,
            PalletAuthor
        }
        internal enum SearchMethodType
        {
            Level,
            Spawnable,
            Avatar
        }
        internal class SearchHistoryEntry
        {
            public string SearchText;
            public SearchMethod Method;
            public SearchMethodType Type;

            public bool IsAvatar { get; private set; }
            public bool IsSpawnable { get; private set; }
            public bool IsLevelCrate { get; private set; }

            public SearchHistoryEntry(string searchText, SearchMethod method, SearchMethodType type)
            {
                SearchText = searchText;
                Method = method;
                Type = type;

                IsAvatar = type == SearchMethodType.Avatar;
                IsSpawnable = type == SearchMethodType.Spawnable;
                IsLevelCrate = type == SearchMethodType.Level;
            }
        }
        internal static System.Collections.Generic.List<SearchHistoryEntry> SearchHistorynow = new();
        internal static IEnumerator Search(string search, Page results, SearchMethod SearchMethodNow, SearchMethodType SpawnableType, System.Action<string> FunctionPress)
        {
            if (isSearching)
            {
                NotificationNow(FusionProtectorInfo.ClientName, "[Searcher] Is Running Please Wait Till It Says Completed!", NotificationType.SUCCESS, 3.0f);
                yield break;
            }

            isSearching = true;
            int count = 0;
            results?.RemoveAll();

            string searchLower = search.ToLower();
            var pallets = AssetWarehouse.Instance.GetPallets();

            int batchSize = 100;
            int processed = 0;

            foreach (var pallet in pallets)
            {
                if (SearchMethodNow == SearchMethod.PalletName &&
                    pallet.name.ToLower().Contains(searchLower))
                {
                    int validCount = 0;
                    foreach (var crate in pallet.Crates)
                    {
                        bool valid = false;
                        if (SpawnableType == SearchMethodType.Avatar)
                            valid = CrateFilterer.GetCrate<AvatarCrate>(crate.Barcode) != null;
                        if (SpawnableType == SearchMethodType.Level)
                            valid = CrateFilterer.GetCrate<LevelCrate>(crate.Barcode) != null;
                        if (SpawnableType == SearchMethodType.Spawnable)
                            valid = CrateFilterer.GetCrate<SpawnableCrate>(crate.Barcode) != null;

                        if (valid)
                            validCount++;
                    }

                    if (validCount > 0) 
                    {
                        Page palletPage = results?.CreatePage("+ " + pallet.name, Color.green);

                        foreach (var crate in pallet.Crates)
                        {
                            bool valid = false;
                            if (SpawnableType == SearchMethodType.Avatar)
                                valid = CrateFilterer.GetCrate<AvatarCrate>(crate.Barcode) != null;
                            if (SpawnableType == SearchMethodType.Level)
                                valid = CrateFilterer.GetCrate<LevelCrate>(crate.Barcode) != null;
                            if (SpawnableType == SearchMethodType.Spawnable)
                                valid = CrateFilterer.GetCrate<SpawnableCrate>(crate.Barcode) != null;

                            if (!valid)
                                continue;

                            string id = crate.Barcode.ID;
                            palletPage?.CreateFunction(crate.name, Color.green, () => { FunctionPress?.Invoke(id); });
                            count++;
                        }
                    }
                }

                if (SearchMethodNow == SearchMethod.PalletAuthor &&
                    !string.IsNullOrEmpty(pallet.Author) &&
                    pallet.Author.ToLower().Contains(searchLower))
                {
                    int validCount = 0;
                    foreach (var crate in pallet.Crates)
                    {
                        bool valid = false;
                        if (SpawnableType == SearchMethodType.Avatar)
                            valid = CrateFilterer.GetCrate<AvatarCrate>(crate.Barcode) != null;
                        if (SpawnableType == SearchMethodType.Level)
                            valid = CrateFilterer.GetCrate<LevelCrate>(crate.Barcode) != null;
                        if (SpawnableType == SearchMethodType.Spawnable)
                            valid = CrateFilterer.GetCrate<SpawnableCrate>(crate.Barcode) != null;

                        if (valid)
                            validCount++;
                    }

                    if (validCount > 0)
                    {
                        Page palletPage = results?.CreatePage($"+ {pallet.name} ({pallet.Author})", Color.green);

                        foreach (var crate in pallet.Crates)
                        {
                            bool valid = false;
                            if (SpawnableType == SearchMethodType.Avatar)
                                valid = CrateFilterer.GetCrate<AvatarCrate>(crate.Barcode) != null;
                            if (SpawnableType == SearchMethodType.Level)
                                valid = CrateFilterer.GetCrate<LevelCrate>(crate.Barcode) != null;
                            if (SpawnableType == SearchMethodType.Spawnable)
                                valid = CrateFilterer.GetCrate<SpawnableCrate>(crate.Barcode) != null;

                            if (!valid)
                                continue;

                            string id = crate.Barcode.ID;
                            palletPage?.CreateFunction(crate.name, Color.green, () => { FunctionPress?.Invoke(id); });
                            count++;
                        }
                    }
                }

                foreach (var crate in pallet.Crates)
                {
                    bool valid = false;
                    if (SpawnableType == SearchMethodType.Avatar)
                        valid = CrateFilterer.GetCrate<AvatarCrate>(crate.Barcode) != null;
                    if (SpawnableType == SearchMethodType.Level)
                        valid = CrateFilterer.GetCrate<LevelCrate>(crate.Barcode) != null;
                    if (SpawnableType == SearchMethodType.Spawnable)
                        valid = CrateFilterer.GetCrate<SpawnableCrate>(crate.Barcode) != null;

                    if (!valid)
                        continue;

                    string crateName = crate.name.ToLower();
                    string barcodeLower = crate.Barcode.ID.ToLower();
                    string id = crate.Barcode.ID;

                    if (SearchMethodNow == SearchMethod.CrateNames &&
                        (crateName.Contains(searchLower) || crateName.StartsWith(searchLower)))
                    {
                        results?.CreateFunction(crate.name, Color.green, () => { FunctionPress?.Invoke(id); });
                        count++;
                    }

                    if (SearchMethodNow == SearchMethod.BarcodeIDNames &&
                        (barcodeLower.Contains(searchLower) || barcodeLower.StartsWith(searchLower)))
                    {
                        results?.CreateFunction(crate.name, Color.green, () => { FunctionPress?.Invoke(id); });
                        count++;
                    }

                    processed++;
                    if (processed >= batchSize)
                    {
                        processed = 0;
                        yield return null;
                    }
                }
            }

            MelonLogger.Msg($"[Searcher] Completed Found {count} Results.");
            NotificationNow(FusionProtectorInfo.ClientName, $"[Searcher] Completed Found {count} Results.", NotificationType.SUCCESS, 3.0f);
            var searchhistorynow = new SearchHistoryEntry(search, SearchMethodNow, SpawnableType);
            if (!SearchHistorynow.Contains(searchhistorynow))
            {
                SearchHistorynow.Add(new SearchHistoryEntry(search, SearchMethodNow, SpawnableType));
            }


            isSearching = false;
        }

        internal static BaseController RightController()
        {
            var rig = Player.RigManager;
            if (rig == null)
                return null;

            var controllerRig = rig.ControllerRig;
            if (controllerRig == null)
                return null;

            return controllerRig.rightController;
        }
        internal static BaseController LeftController()
        {
            var rig = Player.RigManager;
            if (rig == null)
                return null;

            var controllerRig = rig.ControllerRig;
            if (controllerRig == null)
                return null;

            return controllerRig.leftController;
        }
        internal static bool FullLoadedNow()
        {
            if (!NetworkInfo.HasServer)
                return false;

            if (!NetworkInfo.HasLayer)
                return false;

            if (!FusionSceneManager.HasTargetLoaded() && !FusionSceneManager.IsDelayedLoading())
                return false;

            if (!RigData.HasPlayer)
                return false;

            return true;
        }
        internal static string GetRandomByType(RandomizerType type)
        {
            var rand = new Random();

            return type switch
            {
                RandomizerType.AllAvatars => AvatarsStored.Count > 0
                    ? AvatarsStored.ElementAt(rand.Next(AvatarsStored.Count)).Barcode.ID
                    : null,

                RandomizerType.AllSpawnables => SpawnablesStored.Count > 0
                    ? SpawnablesStored.OrderBy(x => x).ElementAt(rand.Next(SpawnablesStored.Count))
                    : null,

                RandomizerType.NoTagsSpawnables => NoTagSpawnables.Count > 0
                    ? NoTagSpawnables.OrderBy(x => x).ElementAt(rand.Next(NoTagSpawnables.Count))
                    : null,

                RandomizerType.AllNPCs => AllNPCStored.Count > 0
                    ? AllNPCStored.OrderBy(x => x).ElementAt(rand.Next(AllNPCStored.Count))
                    : null,

                RandomizerType.AllWeapons => AllWeaponsStored.Count > 0
                    ? AllWeaponsStored.OrderBy(x => x).ElementAt(rand.Next(AllWeaponsStored.Count))
                    : null,

                RandomizerType.AllMelees => MeleeStored.Count > 0
                    ? MeleeStored.OrderBy(x => x).ElementAt(rand.Next(MeleeStored.Count))
                    : null,

                RandomizerType.AllRifle => GunRiflesStored.Count > 0
                    ? GunRiflesStored.OrderBy(x => x).ElementAt(rand.Next(GunRiflesStored.Count))
                    : null,

                RandomizerType.AllSMG => GunSMGStored.Count > 0
                    ? GunSMGStored.OrderBy(x => x).ElementAt(rand.Next(GunSMGStored.Count))
                    : null,

                RandomizerType.AllRanged => GunRangedStored.Count > 0
                    ? GunRangedStored.OrderBy(x => x).ElementAt(rand.Next(GunRangedStored.Count))
                    : null,

                RandomizerType.AllPistol => GunPistolStored.Count > 0
                    ? GunPistolStored.OrderBy(x => x).ElementAt(rand.Next(GunPistolStored.Count))
                    : null,

                RandomizerType.AllShotgun => GunShotgunStored.Count > 0
                    ? GunShotgunStored.OrderBy(x => x).ElementAt(rand.Next(GunShotgunStored.Count))
                    : null,

                RandomizerType.AllSniper => GunSniperStored.Count > 0
                    ? GunSniperStored.OrderBy(x => x).ElementAt(rand.Next(GunSniperStored.Count))
                    : null,

                RandomizerType.AllBlunt => MeleeStoredBlunt.Count > 0
                    ? MeleeStoredBlunt.OrderBy(x => x).ElementAt(rand.Next(MeleeStoredBlunt.Count))
                    : null,

                RandomizerType.AllBlade => MeleeStoredBlade.Count > 0
                    ? MeleeStoredBlade.OrderBy(x => x).ElementAt(rand.Next(MeleeStoredBlade.Count))
                    : null,

                RandomizerType.AllKnife => MeleeStoredKnife.Count > 0
                    ? MeleeStoredKnife.OrderBy(x => x).ElementAt(rand.Next(MeleeStoredKnife.Count))
                    : null,

                _ => null,
            };
        }
        internal static Poolee SpawnIt(string BARCODE, Vector3 Position, Quaternion rotation, bool localonly = false)
        {
            if (localonly || !NetworkInfo.HasServer)
            {
                var spawnable = new Spawnable { crateRef = new SpawnableCrateReference(BARCODE) };
                Poolee spawnyc = null;
                LocalAssetSpawner.Register(spawnable);
                LocalAssetSpawner.Spawn(spawnable, Position, rotation, callbackpoole =>
                {
                    spawnyc = callbackpoole;
                });
                return spawnyc;
            }
            else
            {
                FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var selfLevel, out _);
                if (FusionPermissions.HasSufficientPermissions(selfLevel, LobbyInfoManager.LobbyInfo.DevTools))
                {
                    var spawnable = new Spawnable { crateRef = new SpawnableCrateReference(BARCODE) };
                    var info = new SpawnRequestInfo
                    {
                        Spawnable = spawnable,
                        Position = Position,
                        Rotation = rotation,
                        SpawnSource = EntitySource.Player,
                        SpawnEffect = true,
                        SpawnCallback = _ => { }
                    };

                    Spawn(info);
                    return null;
                }
                else
                {
                    NotificationNow(FusionProtectorInfo.ClientName, "Invalid Permissions!", NotificationType.ERROR, 3.0f);
                    return null;
                }
            }
        }
        internal static string StripColorTags(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return Regex.Replace(input, "<.*?>", string.Empty, RegexOptions.Singleline);
        }
        internal static void FindPlayersLobbyFromPlayerSteamID(ulong steamId, System.Action<bool, PlayerInfo> onResult)
        {
            var matchmaker = NetworkLayerManager.Layer?.Matchmaker;

            if (matchmaker == null)
            {
                onResult?.Invoke(false, null);
                return;
            }

            matchmaker.RequestLobbies(info =>
            {
                foreach (var lobby in info.Lobbies)
                {
                    var players = lobby.Metadata.LobbyInfo?.PlayerList?.Players;
                    if (players == null)
                        continue;

                    foreach (var player in players)
                    {
                        if (player != null && player.PlatformID == steamId)
                        {
                            onResult?.Invoke(true, player);
                            return;
                        }
                    }
                }

                onResult?.Invoke(false, new PlayerInfo
                {
                    AvatarModID = -1,
                    Description = "Player Not On Fusion Publically!",
                    AvatarTitle = "Player Not On Fusion Publically!",
                    Nickname = "Player Not On Fusion Publically!",
                    Username = "Player Not On Fusion Publically!"
                });
            });
        }
        internal static void FindPlayersLobbyFromPlayerName(string searchName,System.Collections.Generic.Dictionary<LobbyInfo, System.Collections.Generic.List<PlayerInfo>> results,Action<bool> onResult)
        {
            var matchmaker = NetworkLayerManager.Layer?.Matchmaker;

            if (matchmaker == null || string.IsNullOrWhiteSpace(searchName))
            {
                onResult?.Invoke(false);
                return;
            }

            string search = searchName.Trim().ToLowerInvariant();
            results.Clear();

            matchmaker.RequestLobbies(lobbiesInfo =>
            {
                foreach (var lobbyWrapper in lobbiesInfo.Lobbies)
                {
                    var lobbyInfo = lobbyWrapper.Metadata.LobbyInfo;
                    var players = lobbyInfo?.PlayerList?.Players;

                    if (players == null)
                        continue;

                    foreach (var player in players)
                    {
                        if (player == null)
                            continue;

                        bool matched = false;

                        if (!string.IsNullOrWhiteSpace(player.Nickname))
                        {
                            string nickname = player.Nickname.Trim().ToLowerInvariant();
                            if (nickname.Contains(search))
                                matched = true;
                        }

                        if (!matched && !string.IsNullOrWhiteSpace(player.Username))
                        {
                            string username = player.Username.Trim().ToLowerInvariant();
                            if (username.Contains(search))
                                matched = true;
                        }

                        if (matched)
                        {
                            if (!results.TryGetValue(lobbyInfo, out var playerList))
                            {
                                playerList = new System.Collections.Generic.List<PlayerInfo>();
                                results[lobbyInfo] = playerList;
                            }

                            playerList.Add(player);
                        }
                    }
                }

                onResult?.Invoke(results.Count > 0);
            });
        }
        internal static bool IsAvatarCrateExist(string barcode)
        {
            var avatarRef = new AvatarCrateReference(barcode);
            if (avatarRef.Crate != null)
            {
                var pallet = avatarRef.Crate.Pallet;
                return pallet != null && pallet.Barcode != null;
            }
            return false;
        }
        internal static bool IsLevelCrateExist(string barcode)
        {
            var avatarRef = new LevelCrateReference(barcode);
            if (avatarRef.Crate != null)
            {
                var pallet = avatarRef.Crate.Pallet;
                return pallet != null && pallet.Barcode != null;
            }
            return false;
        }
        internal static bool IsSpawnableCrateExist(string barcode)
        {
            var avatarRef = new SpawnableCrateReference(barcode);
            if (avatarRef.Crate != null)
            {
                var pallet = avatarRef.Crate.Pallet;
                return pallet != null && pallet.Barcode != null;
            }
            return false;
        }
        internal static bool IsBarcodeInGame(string barcode)
        {
            var spawnableRef = new SpawnableCrateReference(barcode);
            if (spawnableRef.Crate != null)
            {
                var pallet = spawnableRef.Crate.Pallet;
                return pallet != null && pallet.Barcode != null;
            }

            var levelRef = new LevelCrateReference(barcode);
            if (levelRef.Crate != null)
            {
                var pallet = levelRef.Crate.Pallet;
                return pallet != null && pallet.Barcode != null;
            }

            var avatarRef = new AvatarCrateReference(barcode);
            if (avatarRef.Crate != null)
            {
                var pallet = avatarRef.Crate.Pallet;
                return pallet != null && pallet.Barcode != null;
            }

            return false;
        }
        internal static async Task<(NetworkEntity NetworkEntityReturn, Spawnable SpawnableReturn, GameObject GameObjectReturn, InteractableHost InteractableHostReturn)> SpawnersSpawner(string barcodenow, Vector3 location, Quaternion rotation, bool effect = true, bool manuallyInvisToYouOnly = false, bool copyLocationToClipboard = false)
        {
            if (!IsBarcodeInGame(barcodenow))
                return (null, null, null, null);

            var tcs = new TaskCompletionSource<(NetworkEntity, Spawnable, GameObject, InteractableHost)>();
            var spawnable = new Spawnable { crateRef = new SpawnableCrateReference(barcodenow) };

            if (NetworkInfo.HasServer)
            {
                var request = new NetworkAssetSpawner.SpawnRequestInfo
                {
                    Spawnable = spawnable,
                    Position = location,
                    Rotation = Player.Head.rotation,
                    SpawnCallback = new System.Action<SpawnCallbackInfo>(info =>
                    {
                        if (manuallyInvisToYouOnly)
                            info.Entity.JR_GetMarrowEntity()?.gameObject.DestroyNow();

                        var host = info.Entity.JR_GetMarrowEntity()?.GetComponent<InteractableHost>();
                        tcs.TrySetResult((info.Entity, spawnable, info.Spawned, host));
                    }),
                    SpawnEffect = effect,
                    SpawnSource = EntitySource.Player
                };

                NetworkAssetSpawner.Spawn(request);


                if (copyLocationToClipboard)
                {
                    GUIUtility.systemCopyBuffer =
                        $"Title: {StripColorTags(request.Spawnable.crateRef.Scannable.Title)}\n" +
                        $"Barcode: {request.Spawnable.crateRef.Barcode.ID}\n" +
                        $"Location: {request.Position}\n" +
                        $"Rotation: {request.Rotation}";
                }
            }
            else
            {
                LocalAssetSpawner.Register(spawnable);
                LocalAssetSpawner.Spawn(spawnable, location, rotation, callback =>
                {
                    var host = callback.gameObject.GetComponent<MarrowEntity>().GetComponent<InteractableHost>();
                    tcs.TrySetResult((null, spawnable, callback.gameObject, host));
                });

                if (copyLocationToClipboard)
                {
                    GUIUtility.systemCopyBuffer =
                        $"Title: {StripColorTags(spawnable.crateRef.Scannable.Title)}\n" +
                        $"Barcode: {spawnable.crateRef.Barcode.ID}\n" +
                        $"Location: {location}\n" +
                        $"Rotation: {rotation}";
                }
            }

            return await tcs.Task;
        }
        internal static IEnumerator LoadAssetsEnum(bool randomizerslzonly, bool enableLogging = true)
        {
           
            if (AvatarsStored.Count > 0)
            {
                AvatarsStored.Clear();
                if (enableLogging) MelonLogger.Warning("Cleared AvatarsStored");
                yield return null;
            }

            if (SpawnablesStored.Count > 0)
            {
                SpawnablesStored.Clear();
                if (enableLogging) MelonLogger.Warning("Cleared SpawnablesStored");
                yield return null;
            }

            if (NoTagSpawnables.Count > 0)
            {
                NoTagSpawnables.Clear();
                if (enableLogging) MelonLogger.Warning("Cleared NoTagSpawnables");
                yield return null;
            }

            if (AllNPCStored.Count > 0)
            {
                AllNPCStored.Clear();
                if (enableLogging) MelonLogger.Warning("Cleared AllNPCStored");
                yield return null;
            }

            if (AllWeaponsStored.Count > 0)
            {
                AllWeaponsStored.Clear();
                if (enableLogging) MelonLogger.Warning("Cleared AllWeaponsStored");
                yield return null;
            }

            if (MeleeStored.Count > 0)
            {
                MeleeStored.Clear();
                if (enableLogging) MelonLogger.Warning("Cleared MeleeStored");
                yield return null;
            }

            if (GunRiflesStored.Count > 0) { GunRiflesStored.Clear(); yield return null; }
            if (GunSMGStored.Count > 0) { GunSMGStored.Clear(); yield return null; }
            if (GunRangedStored.Count > 0) { GunRangedStored.Clear(); yield return null; }
            if (GunPistolStored.Count > 0) { GunPistolStored.Clear(); yield return null; }
            if (GunShotgunStored.Count > 0) { GunShotgunStored.Clear(); yield return null; }
            if (GunSniperStored.Count > 0) { GunSniperStored.Clear(); yield return null; }

            if (MeleeStoredBlunt.Count > 0) { MeleeStoredBlunt.Clear(); yield return null; }
            if (MeleeStoredBlade.Count > 0) { MeleeStoredBlade.Clear(); yield return null; }
            if (MeleeStoredKnife.Count > 0) { MeleeStoredKnife.Clear(); yield return null; }

            var pallets = AssetWarehouse.Instance.GetPallets();

            int crateCounter = 0;

            foreach (var pallet in pallets)
            {
                if (randomizerslzonly && pallet.Author != "SLZ")
                    continue;

                foreach (var crate in pallet.Crates)
                {
                    string id = crate.Barcode.ID;
                    var tags = crate.Tags;

                    if (IsLevelCrateExist(id))
                        LevelStored.Add(id);

                    if (IsSpawnableCrateExist(id))
                        SpawnablesStored.Add(id);

                    if (IsAvatarCrateExist(id))
                        AvatarsStored.Add(new AvatarCrateReference(id));

                    int tagCount = tags.Count;

                    if (tagCount == 0)
                        NoTagSpawnables.Add(id);

                    for (int i = 0; i < tagCount; i++)
                    {
                        switch (tags[i])
                        {
                            case "Weapon":
                                AllWeaponsStored.Add(id);
                                break;

                            case "NPC":
                                AllNPCStored.Add(id);
                                break;

                            case "Rifle":
                                GunRiflesStored.Add(id);
                                break;

                            case "SMG":
                                GunSMGStored.Add(id);
                                break;

                            case "Ranged":
                                GunRangedStored.Add(id);
                                break;

                            case "Pistol":
                                GunPistolStored.Add(id);
                                break;

                            case "Shotgun":
                                GunShotgunStored.Add(id);
                                break;

                            case "Sniper":
                                GunSniperStored.Add(id);
                                break;

                            case "Melee":
                                MeleeStored.Add(id);
                                break;

                            case "Blunt":
                                MeleeStoredBlunt.Add(id);
                                break;

                            case "Blade":
                                MeleeStoredBlade.Add(id);
                                break;

                            case "Knife":
                                MeleeStoredKnife.Add(id);
                                break;
                        }
                    }

                    crateCounter++;

                    if (crateCounter % 50 == 0)
                        yield return null;
                }
            }

          
            if (enableLogging)
            {
                MelonLogger.Warning($"Fusion Protector Loaded Assets");
                MelonLogger.Warning($"------------------------------");

                MelonLogger.Warning($"All Levels Loaded : [{LevelStored.Count}]");
                MelonLogger.Warning($"All Avatars Loaded : [{AvatarsStored.Count}]");
                MelonLogger.Warning($"All Spawnables Loaded : [{SpawnablesStored.Count}]");
                MelonLogger.Warning($"All No Tags Spawnables Loaded : [{NoTagSpawnables.Count}]");

                MelonLogger.Warning($"All NPCs Loaded : [{AllNPCStored.Count}]");
                MelonLogger.Warning($"All Weapons Loaded : [{AllWeaponsStored.Count}]");
                MelonLogger.Warning($"All Melees Loaded : [{MeleeStored.Count}]");

                MelonLogger.Warning($"Weapons [Rifle] Loaded : [{GunRiflesStored.Count}]");
                MelonLogger.Warning($"Weapons [SMG] Loaded : [{GunSMGStored.Count}]");
                MelonLogger.Warning($"Weapons [Ranged] Loaded : [{GunRangedStored.Count}]");
                MelonLogger.Warning($"Weapons [Pistol] Loaded : [{GunPistolStored.Count}]");
                MelonLogger.Warning($"Weapons [Shotgun] Loaded : [{GunShotgunStored.Count}]");
                MelonLogger.Warning($"Weapons [Sniper] Loaded : [{GunSniperStored.Count}]");

                MelonLogger.Warning($"Melees [Blunt] Loaded : [{MeleeStoredBlunt.Count}]");
                MelonLogger.Warning($"Melees [Blade] Loaded : [{MeleeStoredBlade.Count}]");
                MelonLogger.Warning($"Melees [Knife] Loaded : [{MeleeStoredKnife.Count}]");

                MelonLogger.Warning($"------------------------------");
            }
        }
        internal static void OpenPageNow(string linknow)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = linknow,
                UseShellExecute = true
            });
        }
        internal static System.Collections.Generic.IEnumerable<string> GetLoggedSettingsLines()
        {
            foreach (var (entry, _, _, _) in PageEx.boolslogged)
                yield return entry;
            foreach (var (entry, _, _, _) in PageEx.floatslogged)
                yield return entry;
            foreach (var (entry, _, _, _) in PageEx.intslogged)
                yield return entry;
            foreach (var (entry, _, _, _) in PageEx.enumvaluelogged)
                yield return entry;
            foreach (var (entry, _, _, _) in PageEx.stringslogged)
                yield return entry;
        }
        internal static async Task SaveSettingsAsync(bool notify = true)
        {
            try
            {
                var lines = GetLoggedSettingsLines().ToArray();
                await File.WriteAllLinesAsync(Main.ProtectorSettings, lines);

                if (notify)
                {
                    MelonLogger.Warning("Saved Settings!");
                    NotificationNow(FusionProtectorInfo.ClientName, "Manually Saved Settings!", NotificationType.SUCCESS);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to save settings: {ex}");
            }
        }
        internal static async void ManuallySave(bool notify = true)
        {
            await SaveSettingsAsync(notify);
        }
        internal static void DespawnNow(NetworkEntity entity)
        {
            if (AreYouOWNER())
            {
                NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
                {
                    EntityID = entity.ID,
                    DespawnEffect = false,
                });
            }
        }
        internal static void DespawnAll(DespawnerAll filter, bool localOnly = false)
        {
            if (!localOnly)
            {
                if (!AreYouOWNER())
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        "Invalid Permissions! [Owner Required!]",
                        NotificationType.ERROR,
                        3.0f);
                    return;
                }

                if (dropallbefore)
                {
                    foreach (var player in NetworkPlayers(true))
                    {
                        var slots = player?.RigRefs?.RigSlots;
                        if (slots == null) continue;

                        foreach (var slot in slots)
                            slot?.DropWeapon();
                    }
                }
            }

            foreach (var entity in NetworkEntities())
            {
                if (!PassesFilter(entity, filter))
                    continue;

                if (localOnly)
                    entity.JR_GetMarrowEntity()?.gameObject.DestroyNow();
                else
                    DespawnNow(entity);
            }
        }
        private static bool PassesFilter(NetworkEntity entity, DespawnerAll filter)
        {
            switch (filter)
            {
                case DespawnerAll.NoFilter:
                    return true;

                case DespawnerAll.Guns:
                    return entity.JR_IsGun();

                case DespawnerAll.Melees:
                    return entity.JR_IsMelee();

                case DespawnerAll.Npcs:
                    return entity.JR_IsNPC();

                case DespawnerAll.EverythingButGuns:
                    return !entity.JR_IsGun();

                case DespawnerAll.EverythingButMelees:
                    return !entity.JR_IsMelee();

                case DespawnerAll.EverythingButNpcs:
                    return !entity.JR_IsNPC();

                case DespawnerAll.NetworkProps:
                    {
                        var prop = entity.GetExtender<NetworkProp>();
                        var poolee = entity.GetExtender<PooleeExtender>();
                        return prop != null && poolee != null;
                    }

                case DespawnerAll.AllNotButtonsLeverCircuits:
                    {
                        var marrow = entity.JR_GetMarrowEntity();
                        if (marrow == null) return false;

                        var go = marrow.gameObject;

                        bool isButton =
                            go.GetComponentInChildren<Il2CppSLZ.Marrow.VoidLogic.ButtonNode>(true) ||
                            go.GetComponentInParent<Il2CppSLZ.Marrow.VoidLogic.ButtonNode>();

                        bool isLever =
                            go.GetComponentInChildren<Il2CppSLZ.Marrow.Circuits.HingeController>(true) ||
                            go.GetComponentInParent<Il2CppSLZ.Marrow.Circuits.HingeController>();

                        bool isCircuit =
                            go.GetComponentInChildren<CircuitSocket>(true) ||
                            go.GetComponentInParent<CircuitSocket>();

                        return !(isButton || isLever || isCircuit);
                    }

                default:
                    return true;
            }
        }
        private static bool _isLookingUpMod = false;
        internal static IEnumerator ModioInfo(int modIOID, Action<ModCallbackInfo> onFinished)
        {
            if (_isLookingUpMod)
            {
                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    $"Already looking up a mod please WAIT!",
                    NotificationType.WARNING
                );
                yield break;
            }

            _isLookingUpMod = true;

            NotificationNow(
                FusionProtectorInfo.ClientName,
                $"Reading mod info for ID {modIOID}... please wait.",
                NotificationType.INFORMATION
            );

            ModCallbackInfo infoNow = default;
            bool finished = false;

            ModTransaction transaction = new()
            {
                ModFile = new ModIOFile(modIOID),
                Callback = Callback
            };

            void Callback(DownloadCallbackInfo info)
            {
                if (info.Result != ModResult.SUCCEEDED)
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        "The content failed to install! Make sure you are logged into mod.io in VoidG114 or BONELAB Hub!",
                        NotificationType.WARNING
                    );
                }
            }

            ModIOFile modFile = transaction.ModFile;

            ModIOManager.GetMod(modFile.ModID, OnRequestedMod);

            void OnRequestedMod(ModCallbackInfo info)
            {
                infoNow = info;
                finished = true;

                if (info.Result == ModResult.SUCCEEDED)
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        $"Mod found: {info.Data.NameID}",
                        NotificationType.INFORMATION
                    );
                }
                else
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        "Failed to retrieve mod info.",
                        NotificationType.WARNING
                    );
                }
            }

            while (!finished)
                yield return null;

            _isLookingUpMod = false;

            onFinished?.Invoke(infoNow);
        }
        internal static void DownloadModIOMod(int modIOID, bool noti = true)
        {
            ModTransaction transaction = new ModTransaction()
            {
                ModFile = new ModIOFile(modIOID),
                Callback = Callback
            };

            ModIODownloader.EnqueueDownload(transaction);

            if (noti)
            {
                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    "Wait Until You See Installed Notification Then Press Whatever You Pressed AGAIN!",
                    NotificationType.WARNING,
                    6.0f
                );

            }

            void Callback(DownloadCallbackInfo info)
            {
                if (info.Result != ModResult.SUCCEEDED)
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        "The Content failed to install! Make sure you are logged into mod.io in VoidG114 or BONELAB Hub!",
                        NotificationType.WARNING
                    );
                }
            }
        }
        internal static void AllFusionProtectorLobbies()
        {
            fppubs.RemoveAll();

            if (!NetworkInfo.HasLayer)
                return;

            NetworkLayerManager.Layer.Matchmaker?.RequestLobbies(lobbyQueryResult =>
            {
                foreach (var fusionLobby in lobbyQueryResult.Lobbies)
                {
                    var fusionLobbyInfo = fusionLobby.Metadata.LobbyInfo;

                    if (fusionLobbyInfo.LobbyCode.StartsWith("FP-"))
                    {
                        if (fusionLobbyInfo.Privacy != ServerPrivacy.PUBLIC)
                            continue;

                        if (fusionLobbyInfo.PlayerCount >= fusionLobbyInfo.MaxPlayers)
                            continue;

                        var lobbyNamePrefix = string.IsNullOrEmpty(fusionLobbyInfo.LobbyName)
                            ? $"{fusionLobbyInfo.LobbyHostName}'s Server"
                            : $"[{fusionLobbyInfo.LobbyName}] ";

                        var lobbyLabel =
                            $"{lobbyNamePrefix}[{fusionLobbyInfo.LobbyHostName}] Join | " +
                            $"{fusionLobbyInfo.PlayerCount}/{fusionLobbyInfo.MaxPlayers} | " +
                            $"{fusionLobbyInfo.LevelTitle}";

                        var fusionLobbyPage =
                            fppubs.CreatePage(
                                $"+ {lobbyNamePrefix}[{fusionLobbyInfo.LobbyHostName}]",
                                Color.green
                            );

                        fusionLobbyPage.CreateFunction(
                            lobbyLabel,
                            Color.yellow,
                            () =>
                            {
                                if (CrateFilterer.HasCrate<LevelCrate>(
                                        new Barcode(fusionLobbyInfo.LevelBarcode)))
                                {
                                    NetworkHelper.JoinServerByCode(fusionLobbyInfo.LobbyCode);
                                }
                                else
                                {
                                    DownloadModIOMod(fusionLobbyInfo.LevelModID, false);
                                    NotificationNow(
                                        FusionProtectorInfo.ClientName,
                                        "Wait Until You See Installed Notification Then Click Join Again!",
                                        NotificationType.WARNING,
                                        5.0f
                                    );
                                }
                            }
                        );

                        var fusionPlayerListPage =
                            fusionLobbyPage.CreatePage("+ Players in Lobby", Color.green);

                        foreach (var fusionPlayer in fusionLobbyInfo.PlayerList.Players)
                        {
                            fusionPlayerListPage.CreateFunction(
                                $"[{CleanedNAME(fusionPlayer.Nickname, fusionPlayer.Username)}] [{fusionPlayer.PlatformID}]",
                                Color.green,
                                () =>
                                {
                                    GUIUtility.systemCopyBuffer = fusionPlayer.PlatformID.ToString();
                                    NotificationNow(
                                        FusionProtectorInfo.ClientName,
                                        "Copied Steam ID To Clipboard",
                                        NotificationType.SUCCESS,
                                        3.0f
                                    );
                                }
                            );
                        }
                    }
                }
            });
        }
        internal static void FriendLobbies()
        {
            OnlineFriends.RemoveAll();

            if (!NetworkInfo.HasLayer)
                return;
            int friends = 0;
            
            NetworkLayerManager.Layer.Matchmaker?.RequestLobbies(info =>
            {
                foreach (var lobby in info.Lobbies)
                {
                    var lobbyInfo = lobby.Metadata.LobbyInfo;

                    if (lobbyInfo?.PlayerList?.Players == null)
                        continue;

                    foreach (var player in lobbyInfo.PlayerList.Players)
                    {
                        var friend = NetworkHelper.IsFriend(player.PlatformID);
                        if (!friend)
                            continue;
                        friends++;

                        if (lobbyInfo.Privacy is not (ServerPrivacy.PUBLIC or ServerPrivacy.PRIVATE))
                            continue;
                        if (lobbyInfo.PlayerCount >= lobbyInfo.MaxPlayers)
                            continue;


                        var label = $"[{lobbyInfo.LobbyName}] [{lobbyInfo.LobbyHostName}] " +
                                    $"Join | {lobbyInfo.PlayerCount} / {lobbyInfo.MaxPlayers} | {lobbyInfo.LevelTitle}";

                        var PLAYERSFRIENDS = OnlineFriends.CreatePage($" + [{player.Nickname}] [{player.Username}] [{player.PlatformID}]", Color.yellow);

                        PLAYERSFRIENDS?.CreateFunction(label, Color.white,
                            () =>
                            {

                                if (CrateFilterer.HasCrate<LevelCrate>(new Barcode(lobbyInfo.LevelBarcode)))
                                {
                                    NetworkHelper.JoinServerByCode(lobbyInfo.LobbyCode);
                                }
                                else
                                {
                                    DownloadModIOMod(lobbyInfo.LevelModID, false);
                                    NotificationNow(FusionProtectorInfo.ClientName, "Wait Untill You See Installed Notification Then Click Join Again!", NotificationType.WARNING, 5.0f);

                                }
                            });

                        var playersinlobby = PLAYERSFRIENDS?.CreatePage("+ Players in Lobby", Color.yellow);
                        foreach (var playernow in lobby.Metadata.LobbyInfo.PlayerList.Players)
                        {

                            playersinlobby?.CreateFunction($"[{playernow.Nickname}] [{playernow.Username}] [{playernow.PlatformID}]", Color.white, () =>
                            {
                                GUIUtility.systemCopyBuffer = playernow.PlatformID.ToString();
                                NotificationNow(FusionProtectorInfo.ClientName, "Copied Steam ID To Clipboard", NotificationType.SUCCESS, 3.0f);
                            }
                                );

                        }

                    }
                }

            });
            if (friends != 0)
                NotificationNowAlways(FusionProtectorInfo.ClientName,"Friends Online : "+ friends.ToString(),NotificationType.SUCCESS,3.5f);
        }
        internal static void AllLobbies()
        {
            pubs.RemoveAll();

            if (!NetworkInfo.HasLayer)
                return;

            NetworkLayerManager.Layer.Matchmaker?.RequestLobbies(info =>
            {
                foreach (var lobby in info.Lobbies)
                {
                    var lobbyInfo = lobby.Metadata.LobbyInfo;

                    if (lobbyInfo.Privacy != ServerPrivacy.PUBLIC)
                        continue;

                    if (lobbyInfo.PlayerCount >= lobbyInfo.MaxPlayers)
                        continue;

                    var lobbyPrefix = string.IsNullOrEmpty(lobbyInfo.LobbyName) ? $"{lobbyInfo.LobbyHostName}'s Server" : $"[{lobbyInfo.LobbyName}] ";

                    var label = $"{lobbyPrefix}[{lobbyInfo.LobbyHostName}] Join | {lobbyInfo.PlayerCount}/{lobbyInfo.MaxPlayers} | {lobbyInfo.LevelTitle}";

                    var PUBBIES = pubs.CreatePage($"+ {lobbyPrefix}[{lobbyInfo.LobbyHostName}]", Color.green);


                    PUBBIES.CreateFunction(label, Color.yellow,
                        () =>
                        {

                            if (CrateFilterer.HasCrate<LevelCrate>(new Barcode(lobbyInfo.LevelBarcode)))
                            {
                                NetworkHelper.JoinServerByCode(lobbyInfo.LobbyCode);
                            }
                            else
                            {
                                DownloadModIOMod(lobbyInfo.LevelModID, false);
                                NotificationNow(FusionProtectorInfo.ClientName, "Wait Untill You See Installed Notification Then Click Join Again!", NotificationType.WARNING, 5.0f);

                            }
                        });

                    var playersinlobby = PUBBIES.CreatePage("+ Players in Lobby", Color.green);
                    foreach (var playernow in lobby.Metadata.LobbyInfo.PlayerList.Players)
                    {

                        playersinlobby.CreateFunction($"[{playernow.Nickname}] [{playernow.Username}] [{playernow.PlatformID}]",
                                Color.green,
                                () =>
                                {

                                    GUIUtility.systemCopyBuffer = playernow.PlatformID.ToString();
                                    NotificationNow(FusionProtectorInfo.ClientName, "Copied Steam ID To Clipboard", NotificationType.SUCCESS, 3.0f);
                                }
                            );

                    }
                }
            });
        }
        internal static bool Iswithintwovalues(float valuetocheck, float min, float max)
        {
            if (valuetocheck >= min && valuetocheck <= max)
            {
                return true;
            }
            return false;
        }
        internal static void ClearConstraints(NetworkPlayer Netty)
        {
            if (!Netty.PlayerID.IsValid)
            {
                return;
            }

            try
            {
                foreach (ConstraintTracker componentsInChild in Netty.RigRefs.RigManager.physicsRig.GetComponentsInChildren<ConstraintTracker>())
                {
                    componentsInChild.DeleteConstraint();
                }
            }
            catch
            {

            }
        }
        internal static void HolsterHiderAll(NetworkPlayer playerTodo, bool activeNow = false)
        {
            var rig = playerTodo != null
                ? playerTodo.RigRefs?.RigManager?.physicsRig
                : Player.RigManager?.physicsRig;

            if (rig == null) return;

            void Toggle(Transform root, string path)
            {
                if (root == null) return;
                var t = root.Find(path);
                if (t == null) return;
                var mr = t.GetComponent<UnityEngine.MeshRenderer>();
                mr?.gameObject.SetActive(activeNow);
            }

            Toggle(rig.m_spine?.transform, "SideRt/prop_handGunHolster/strap_geo");
            Toggle(rig.m_spine?.transform, "SideRt/prop_handGunHolster/handgunHolster_geo");
            Toggle(rig.m_spine?.transform, "SideLf/prop_handGunHolster/strap_geo");
            Toggle(rig.m_spine?.transform, "SideLf/prop_handGunHolster/handgunHolster_geo");
            Toggle(rig.m_pelvis?.transform, "BeltLf1/InventoryAmmoReceiver/Holder");
            Toggle(rig.m_pelvis?.transform, "BeltRt1/InventoryAmmoReceiver/Holder");
            Toggle(rig.m_pelvis?.transform, "BackCt/prop_pouch");
        }
        internal static void StoreInventoryItems()
        {
            if (!FullLoadedNow())
                return;

            weaponsInInventory.Clear();

            var inventory = Player.RigManager?.inventory;
            if (inventory == null)
                return;

            var allSlots = inventory.bodySlots.Concat(inventory.specialItems);


            var headSlot = Player.RigManager.physicsRig.m_head.transform
                ?.Find("HeadSlotContainer/WeaponReciever_01")
                ?.GetComponent<InventorySlotReceiver>();

            if (headSlot != null)
            {
                var weapon = headSlot._slottedWeapon;
                var barcode = weapon?.interactableHost?.marrowEntity?._poolee?._SpawnableCrate_k__BackingField?.Barcode?.ID;

                if (!string.IsNullOrEmpty(barcode))
                {
                    weaponsInInventory[headSlot] = (barcode, "HeadSlot");
                }
            }

            foreach (var slot in allSlots)
            {
                var receiver = slot?.inventorySlotReceiver;
                if (receiver == null)
                    continue;

                var weapon = receiver._slottedWeapon;
                var barcode = weapon?.interactableHost?.marrowEntity?._poolee?._SpawnableCrate_k__BackingField?.Barcode?.ID;

                if (!string.IsNullOrEmpty(barcode))
                {
                    string slotName = slot.name ?? "UnknownSlot";
                    weaponsInInventory[receiver] = (barcode, slotName);
                }
            }
        }
        internal static void SpawnInventoryRefresh()
        {
            if (!infiniteinventory || weaponsInInventory == null || weaponsInInventory.Count == 0)
                return;

            if (!AreYouOWNER())
                return;


            var inventory = Player.RigManager?.inventory;
            if (inventory == null || !FullLoadedNow())
                return;

            foreach (var kvp in weaponsInInventory)
            {
                var receiver = kvp.Key;
                var barcode = kvp.Value.Barcode;
                var slotnamey = kvp.Value.SlotName;

                if (receiver == null)
                    continue;

                if (!infiniteinvall)
                {
                    if (BodySlot(SlotsNowReal) == slotnamey)
                    {
                        if (receiver._slottedWeapon == null)
                        {
                            receiver.SpawnInSlotAsync(new Barcode(barcode));
                        }
                    }
                }
                else
                {
                    if (receiver._slottedWeapon == null)
                    {
                        receiver.SpawnInSlotAsync(new Barcode(barcode));
                    }
                }
            }
        }
        internal static void ChangeBodyLogAvatarSlot(int slotindex, string avatarbarcode, bool notification = true)
        {
            if (IsBarcodeInGame(avatarbarcode))
            {
                if (DataManager.ActiveSave.PlayerSettings.FavoriteAvatars == null)
                {
                    DataManager.ActiveSave.PlayerSettings.FavoriteAvatars = new Il2CppSystem.Collections.Generic.List<string>();
                    for (int i = 0; i < 6; i++)  // 0 to 5
                    {
                        DataManager.ActiveSave.PlayerSettings.FavoriteAvatars.Add("EMPTY");
                    }
                }


                DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[slotindex -1] = avatarbarcode;
                if (notification)
                {
                    NotificationNowAlways(FusionProtectorInfo.ClientName, $"Changed {slotindex} Slot And Saved!", NotificationType.SUCCESS);
                }
                DataManager.TrySaveActiveSave(SaveFlags.Complete);
                JR_BodyLog(Player.PhysicsRig).bodylogreturn.LoadFavoriteAvatars();
                JR_BodyLog(Player.PhysicsRig).bodylogreturn.BodyMallUpdate();
            }
            else
            {
                NotificationNowAlways(FusionProtectorInfo.ClientName, "This Does Not Exist In Your Game Install This Mod For It To Exist...", NotificationType.ERROR, 3.0f);
            }
        }
        internal static bool IfDontHaveInstallThenDo(string barcode, int modioID, bool notification = false)
        {
            if (!CrateFilterer.HasCrate<SpawnableCrate>(new Barcode(barcode)))
            {

                DownloadModIOMod(modioID, notification);
                return false;
            }


            return true;
        }
        internal static string CleanedNAME(NetworkPlayer playernow)
        {
            var nickname = StripColorTags(playernow?.JR_Nickname());
            var username = playernow?.JR_Username();
            return string.IsNullOrWhiteSpace(nickname) ? username : nickname + " | "+username;
        }
        internal static string CleanedNAME(string nicknamex, string usernamex)
        {
            var nickname = StripColorTags(nicknamex);
            return string.IsNullOrWhiteSpace(nicknamex) ? usernamex : nicknamex + " | "+usernamex;
        }
        internal static IEnumerator KeepLoadOut()
        {
            yield return new WaitForSecondsRealtime(2.5f);

            FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var selfLevel, out _);

            if (FusionPermissions.HasSufficientPermissions(selfLevel, LobbyInfoManager.LobbyInfo.DevTools))
            {

                var page = new InventoryPage("Current", InventoryPage.CurrentPreset);

                page.LoadIntoPlayer(false);
                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    "Loaded Inventory Into Player!",
                    NotificationType.SUCCESS,
                    3.5f
                );
            }
            else
            {
                NotificationNow(FusionProtectorInfo.ClientName,"Invalid Permissions.",NotificationType.ERROR,3.5f);
            }
        }
        internal static void SetBarCodeToSpawnGun(string barcode)
        {
            foreach (var hand in new[] { WhichHand.Left, WhichHand.Right })
            {
                var yourHand = JR_YourGetHand(hand);
                if (yourHand.JR_IsGrabbedSpawnGun())
                {
                    var spawnGun = yourHand.JR_HandGrabbedSpawnGun();
                    var crateRef = new SpawnableCrateReference(barcode);

                    spawnGun._selectedCrate = crateRef.Crate;
                    spawnGun.SetPreviewMesh();

                    SpawnEffects.CallDespawnEffect(JR_YourGetHand(hand)?.JR_GetMarrowEntity());
                    SpawnEffects.CallSpawnEffect(JR_YourGetHand(hand)?.JR_GetMarrowEntity());

                }
            }
        }
        internal static void SendMessageToHost(string messagenow)
        {
            if (HideFusionProtector)
                return;
            
            if (JR_YourNetworkPlayer().PlayerID.IsHost)
                return;

            var data = SendNotificationData.Create(
PlayerIDManager.LocalSmallID,
CleanedNAME(JR_YourNetworkPlayer()) + $"\n{messagenow}",
FusionProtectorInfo.ClientName,
3.0f
);

            MessageRelay.RelayModule<SendNotificationMessage, SendNotificationData>(data, CommonMessageRoutes.ReliableToServer);

        }
        internal static void SendPlayerMessage(NetworkPlayer PlayerTodo, string message, float messagetime, byte playerssmallid, string messageonrecv = "Message sent. If they have Fusion Protector, they’ll receive it.", Action OnAcceptNow = null)
        {
            if (HideFusionProtector)
            {
                NotificationNow(FusionProtectorInfo.ClientName, "Can't Use This While Hiding Fusion Protector.", NotificationType.ERROR, 3.0f);
                return;
            }
            


            if (TimeReferences.TimeSinceStartup - SendNotificationMessage._timeOfRequest <= SendNotificationMessage._requestCooldown)
            {
             return;
            }


            var senderId = PlayerTodo.JR_SmallID();


            NotificationNow(
                FusionProtectorInfo.ClientName,
                $"Sending message to {CleanedNAME(PlayerTodo)}...",
                NotificationType.WARNING);

            var data = SendNotificationData.Create(
                PlayerIDManager.LocalSmallID,
                TextFilter.SanitizeName(message),
                string.IsNullOrWhiteSpace(JR_YourNetworkPlayer().JR_Nickname())
                    ? JR_YourNetworkPlayer().JR_Username() + " Says"
                    : JR_YourNetworkPlayer().JR_Nickname() + " Says",
                messagetime
            );

            MessageRelay.RelayModule<SendNotificationMessage, SendNotificationData>(
                data,
                new MessageRoute(playerssmallid, NetworkChannel.Reliable)
            );

            NotificationNow(
                FusionProtectorInfo.ClientName,
                messageonrecv,
                NotificationType.SUCCESS,
                5.0f, true, true, OnAcceptNow);
        }
        internal static void PlayersListNow()
        {
            playermessages.RemoveAll();

            foreach (var player in NetworkPlayers())
            {
                var username = StripColorTags(CleanedNAME(player)) ?? "Unknown Player";

                var isHost = player.PlayerID.IsHost;
                var hostTag = isHost ? "[HOST] " : "";
                var color = isHost ? Color.cyan : Color.yellow;

                player.PlayerID.TryGetPermissionLevel(out var permLevel);

                var permTag = permLevel switch
                {
                    PermissionLevel.OWNER => "[OWN]",
                    PermissionLevel.GUEST => "[GST]",
                    PermissionLevel.DEFAULT => "[DEF]",
                    PermissionLevel.OPERATOR => "[OP]",
                    _ => ""
                };

                var pagemessagingnow = playermessages.CreatePage($"+ {hostTag}{CleanedNAME(player)} {permTag}", color);

                pagemessagingnow.CreateString("Send Code Mod Bas64 Raw Link", Color.yellow, "Link Here...", (stringy) =>
                {
                    if (TimeReferences.TimeSinceStartup - SendBase64FileMessage._timeOfRequest <= SendBase64FileMessage._requestCooldown)
                    {
                        return;
                    }

                    var data = SendBase64FileData.Create(PlayerIDManager.LocalSmallID, stringy, SendBase64FileMessage.codemodname);
                    MessageRelay.RelayModule<SendBase64FileMessage, SendBase64FileData>(data, new MessageRoute(player.JR_SmallID(), NetworkChannel.Reliable));
                });

                pagemessagingnow.CreateString("Send Code Mod .dll Name", Color.yellow, SendBase64FileMessage.codemodname, (stringy) =>
                {
                    SendBase64FileMessage.codemodname = stringy;
                });


                if (bitsending)
                {
                    pagemessagingnow.CreateString("Send Player Bits", Color.yellow,"100", (stringy) =>
                    {
                        if (TimeReferences.TimeSinceStartup - SendBitMessage._timeOfRequest <= SendBitMessage._requestCooldown)
                        {
                            return;
                        }

                        if (int.TryParse(stringy, out var intynow))
                        {
                            if (PointItemManager.GetBitCount() >= intynow)
                            {
                                var data = SendBitData.Create(PlayerIDManager.LocalSmallID, intynow);

                                MessageRelay.RelayModule<SendBitMessage, SendBitData>(data, new MessageRoute(player.JR_SmallID(), NetworkChannel.Reliable));
                                PointItemManager.DecrementBits(data.bits);

                            }
                            else
                            {
                                NotificationNowAlways(FusionProtectorInfo.ClientName, $"Not Enough Bits!", NotificationType.WARNING, 3.5f);
                            }
                        }
                    });
                }








                if (playermessaging)
                {
                    pagemessagingnow.Logsettingsfloat("Send Message Popup Length", Color.green, ref messgfloattime, 1, 1, 5, floatnow =>
                    {
                        messgfloattime = floatnow;
                    });
                    pagemessagingnow.LogsettingsString("Send Message", Color.yellow, ref messagenowplayer, (stringy) =>
                    {
                        messagenowplayer = stringy;
                        SendPlayerMessage(player, messagenowplayer, messgfloattime, player.JR_SmallID());
                    });
                }

                if (bodylogsending)
                {
                    pagemessagingnow.CreateFunction("Send Player Your Bodylog", Color.yellow, () =>
                    {
                        if (TimeReferences.TimeSinceStartup - SendModIDMessage._timeOfRequest <= SendModIDMessage._requestCooldown)
                        {
                            return;
                        }

                        if (DataManager.ActiveSave.PlayerSettings.FavoriteAvatars == null)
                        {
                            DataManager.ActiveSave.PlayerSettings.FavoriteAvatars = new Il2CppSystem.Collections.Generic.List<string>();
                            for (int i = 0; i < 6; i++)
                            {
                                DataManager.ActiveSave.PlayerSettings.FavoriteAvatars.Add("EMPTY");
                            }
                        }

                        var idofalot1 = CrateFilterer.GetModID(new AvatarCrateReference(DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[0]).Crate.Pallet);
                        var idofalot2 = CrateFilterer.GetModID(new AvatarCrateReference(DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[1]).Crate.Pallet);
                        var idofalot3 = CrateFilterer.GetModID(new AvatarCrateReference(DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[2]).Crate.Pallet);
                        var idofalot4 = CrateFilterer.GetModID(new AvatarCrateReference(DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[3]).Crate.Pallet);
                        var idofalot5 = CrateFilterer.GetModID(new AvatarCrateReference(DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[4]).Crate.Pallet);
                        var idofalot6 = CrateFilterer.GetModID(new AvatarCrateReference(DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[5]).Crate.Pallet);


                        var data = SendBodyLogData.Create(
                        PlayerIDManager.LocalSmallID,
                        DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[0],
                        DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[1],
                        DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[2],
                        DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[3],
                        DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[4],
                        DataManager.ActiveSave.PlayerSettings.FavoriteAvatars[5],
                        idofalot1,
                        idofalot2,
                        idofalot3,
                        idofalot4,
                        idofalot5,
                        idofalot6);

                        MessageRelay.RelayModule<SendBodyLogMessage, SendBodyLogData>(data, new MessageRoute(player.JR_SmallID(), NetworkChannel.Reliable));
                        NotificationNowAlways(FusionProtectorInfo.ClientName, $"Sent Bodylog To {CleanedNAME(player)}", NotificationType.WARNING, 3.5f);





                    });
                }

                if (modidsending)
                {
                    pagemessagingnow.CreateString("Send Player Mod.IO ID#", Color.yellow, "4158753", (stringh) =>
                    {
                        if (TimeReferences.TimeSinceStartup - SendModIDMessage._timeOfRequest <= SendModIDMessage._requestCooldown)
                        {
                            return;
                        }

                        if (int.TryParse(stringh, out var dn))
                        {

                            var data = SendModIDData.Create(
                                   PlayerIDManager.LocalSmallID, dn);

                            MessageRelay.RelayModule<SendModIDMessage, SendModIDData>(data, new MessageRoute(player.JR_SmallID(), NetworkChannel.Reliable));
                            NotificationNowAlways(FusionProtectorInfo.ClientName, $"Sent Mod.IO {stringh} To {CleanedNAME(player)}", NotificationType.WARNING, 3.5f);


                        }
                    });
                }
              




            }
        }
        internal static string BarcodeInHand()
        {
            var hand = handnowreal == handnow.Left ? WhichHand.Left : WhichHand.Right;
            var entity = JR_YourGetHand(hand)?.JR_GetMarrowEntity();
            return entity != null ? entity.JR_GetBarcodeID() : string.Empty;
        }
        internal static void Owneroptionsonly()
        {

            var servsettings = OwnerOnlyPg.CreatePage("+ Server Settings", Color.green);
            servsettings.CreateFunction("Switch To Sandbox", Color.yellow, () =>
            {
                if (TimeReferences.TimeSinceStartup - SendGameModeOverMessage._timeOfRequest <= SendGameModeOverMessage._requestCooldown)
                {
                    return;
                }
                var ownerservdata = SendGameModeOverData.Create(PlayerIDManager.LocalSmallID, "sandbox");
                MessageRelay.RelayModule<SendGameModeOverMessage, SendGameModeOverData>(
                    ownerservdata,
                    CommonMessageRoutes.ReliableToServer
                );
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.34+");
            servsettings.CreateFunction("Switch To Team Deathmatch", Color.yellow, () =>
            {
                if (TimeReferences.TimeSinceStartup - SendGameModeOverMessage._timeOfRequest <= SendGameModeOverMessage._requestCooldown)
                {
                    return;
                }
                var ownerservdata = SendGameModeOverData.Create(PlayerIDManager.LocalSmallID, "Lakatrazz.Team Deathmatch");
                MessageRelay.RelayModule<SendGameModeOverMessage, SendGameModeOverData>(
                    ownerservdata,
                    CommonMessageRoutes.ReliableToServer
                );
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.34+");
            servsettings.CreateFunction("Switch To Deathmatch", Color.yellow, () =>
            {
                if (TimeReferences.TimeSinceStartup - SendGameModeOverMessage._timeOfRequest <= SendGameModeOverMessage._requestCooldown)
                {
                    return;
                }
                var ownerservdata = SendGameModeOverData.Create(PlayerIDManager.LocalSmallID, "Lakatrazz.Deathmatch");
                MessageRelay.RelayModule<SendGameModeOverMessage, SendGameModeOverData>(
                    ownerservdata,
                    CommonMessageRoutes.ReliableToServer
                );
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.34+");
            servsettings.CreateFunction("Switch To Juggernaut", Color.yellow, () =>
            {
                if (TimeReferences.TimeSinceStartup - SendGameModeOverMessage._timeOfRequest <= SendGameModeOverMessage._requestCooldown)
                {
                    return;
                }
                var ownerservdata = SendGameModeOverData.Create(PlayerIDManager.LocalSmallID, "Lakatrazz.Juggernaut");
                MessageRelay.RelayModule<SendGameModeOverMessage, SendGameModeOverData>(
                    ownerservdata,
                    CommonMessageRoutes.ReliableToServer
                );
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.34+");
            servsettings.CreateFunction("Switch To Entangled", Color.yellow, () =>
            {
                if (TimeReferences.TimeSinceStartup - SendGameModeOverMessage._timeOfRequest <= SendGameModeOverMessage._requestCooldown)
                {
                    return;
                }
                var ownerservdata = SendGameModeOverData.Create(PlayerIDManager.LocalSmallID, "Lakatrazz.Entangled");
                MessageRelay.RelayModule<SendGameModeOverMessage, SendGameModeOverData>(
                    ownerservdata,
                    CommonMessageRoutes.ReliableToServer
                );
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.34+");
            servsettings.CreateFunction("Switch To Smash Bones", Color.yellow, () =>
            {
                if (TimeReferences.TimeSinceStartup - SendGameModeOverMessage._timeOfRequest <= SendGameModeOverMessage._requestCooldown)
                {
                    return;
                }
                var ownerservdata = SendGameModeOverData.Create(PlayerIDManager.LocalSmallID, "Lakatrazz.Smash Bones");
                MessageRelay.RelayModule<SendGameModeOverMessage, SendGameModeOverData>(
                    ownerservdata,
                    CommonMessageRoutes.ReliableToServer
                );
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.34+");
            servsettings.CreateFunction("Switch To Hide And Seek", Color.yellow, () =>
            {
                if (TimeReferences.TimeSinceStartup - SendGameModeOverMessage._timeOfRequest <= SendGameModeOverMessage._requestCooldown)
                {
                    return;
                }
                var ownerservdata = SendGameModeOverData.Create(PlayerIDManager.LocalSmallID, "Lakatrazz.Hide And Seek");
                MessageRelay.RelayModule<SendGameModeOverMessage, SendGameModeOverData>(
                    ownerservdata,
                    CommonMessageRoutes.ReliableToServer
                );
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.34+");
            servsettings.CreateFunction("NameTags ON/OFF", Color.yellow, () =>
            {
                InGame.SendSettingToServer("NameTags", !LobbyInfoManager.LobbyInfo.NameTags);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");
            servsettings.CreateFunction("VoiceChat ON/OFF", Color.yellow, () =>
            {
                InGame.SendSettingToServer("VoiceChat", !LobbyInfoManager.LobbyInfo.VoiceChat);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");
            servsettings.CreateFunction("Mortality ON/OFF", Color.yellow, () =>
            {
                InGame.SendSettingToServer("Mortality", !LobbyInfoManager.LobbyInfo.Mortality);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");
            servsettings.CreateFunction("Friendly Fire ON/OFF", Color.yellow, () =>
            {
                InGame.SendSettingToServer("Friendly Fire", !LobbyInfoManager.LobbyInfo.FriendlyFire);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");
            servsettings.CreateFunction("Knockout ON/OFF", Color.yellow, () =>
            {
                InGame.SendSettingToServer("Knockout", !LobbyInfoManager.LobbyInfo.Knockout);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");
            servsettings.CreateFunction("Player Constraining ON/OFF", Color.yellow, () =>
            {
                InGame.SendSettingToServer("Player Constraining", !LobbyInfoManager.LobbyInfo.PlayerConstraining);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");

            var enummy = servsettings.LogsettingsEnum("Permission Level To Set", Color.yellow, ref TempLevelNow, enabled =>
            {
                TempLevelNow = (PermissionLevel)enabled;
            });
            servsettings.CreateFunction("Set Permission : Dev Tools", Color.yellow, () =>
            {
                InGame.SendSettingToServer("Dev Tools", enummy.Value);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");
            servsettings.CreateFunction("Set Permission : Constrainer", Color.yellow, () =>
            {
                InGame.SendSettingToServer("Constrainer", enummy.Value);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");
            servsettings.CreateFunction("Set Permission : Custom Avatars", Color.yellow, () =>
            {
                InGame.SendSettingToServer("Custom Avatars", enummy.Value);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");
            servsettings.CreateFunction("Set Permission : Teleportation", Color.yellow, () =>
            {
                InGame.SendSettingToServer("Teleportation", enummy.Value);
            }).SetTooltip("Only Works If Host Has Fusion Protector At Least 1.48.20+");


            OwnerOnlyPg.Logsettings("Despawn Dead NPC's", Color.cyan, ref despawndeadnpcs, enabled =>
            {

                despawndeadnpcs = enabled;
            });
            fpsdespawn = OwnerOnlyPg.CreatePage("+ FPS Despawner", Color.green);
            fpsdespawn.Logsettingsfloat("FPS Limit", Color.green, ref fpslimit, 1, 1, 15, intnow =>
            {
                fpslimit = intnow;
            });
            fpsdespawn.LogsettingsEnum("FPS Despawn Filter", Color.yellow, ref DespawnerTimerz, enabled =>
            {
                DespawnerTimerz = (DespawnerAll)enabled;
            });
            fpsdespawn.Logsettings("FPS Despawner", Color.cyan, ref fpsdesapwner, enabled =>
            {
                fpsdesapwner = enabled;
            });

            AISpawnersPage = OwnerOnlyPg.CreatePage("+ Spawners", Color.green);

            var infinv = OwnerOnlyPg.CreatePage("+ Infinite Inventory", Color.green);
            infinv.LogsettingsEnum("Infinite Slot", Color.yellow, ref SlotsNowReal, enabled =>
            {
                SlotsNowReal = enabled;
            });
            infinv.Logsettings("Infinite Inventory", Color.cyan, ref infiniteinventory, enabled =>
            {
                if (!infiniteinventory && enabled)
                {
                    NotificationNow(FusionProtectorInfo.ClientName, "Stored Current Inventory!\nRe-Enable For Storing New Stuff!", NotificationType.SUCCESS, 2.0f);
                    StoreInventoryItems();
                }
                infiniteinventory = enabled;
            });
            infinv.Logsettings("Infinite Inventory All Slots", Color.cyan, ref infiniteinvall, enabled =>
            {
                infiniteinvall = enabled;
            });

            OwnerOnlyPg.CreateFunction("Teleport All", Color.yellow, () =>
            {

                FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var selflevel, out var colly);
                if (FusionPermissions.HasSufficientPermissions(selflevel, LobbyInfoManager.LobbyInfo.Teleportation))
                { 
                    SendMessageToHost("Teleported Everyone");

                    foreach (var playerId in PlayerIDManager.PlayerIDs)
                    {
                        if (playerId != PlayerIDManager.LocalSmallID)
                            PermissionSender.SendPermissionRequest(PermissionCommandType.TELEPORT_TO_ME, playerId);
                    }
                }
            });
            OwnerOnlyPg.LogsettingsEnum("Despawn All Filter", Color.yellow, ref DespawnerAllReal, enabled =>
            {
                DespawnerAllReal = (DespawnerAll)enabled;
            });
            OwnerOnlyPg.CreateFunction("Despawn All", Color.yellow, () =>
            {

               
                    if (AreYouOWNER())
                    {
                        SendMessageToHost("Despawned Everything");
                        DespawnAll(DespawnerAllReal);
                    }
                
           
                
            });
            OwnerOnlyPg.Logsettings("God Mode", Color.cyan, ref godmode, enabled => { godmode = enabled; });
            OwnerOnlyPg.Logsettings("Anti Knockout", Color.cyan, ref antiknockout, enabled => { antiknockout = enabled; });
            OwnerOnlyPg.Logsettings("Anti Dev Manipulator", Color.cyan, ref AntiDevManip, enabled =>
            {
                if (!AntiDevManip && enabled)
                {
                    foreach (var devmanippy in NetworkEntities())
                    {
                        if (devmanippy.JR_GetMarrowEntity().JR_GetBarcodeID() == "c1534c5a-c6a8-45d0-aaa2-2c954465764d")
                        {
                            DespawnNow(devmanippy);
                        }
                    }
                }
                AntiDevManip = enabled;
            });
            OwnerOnlyPg.Logsettings("Anti Lasereyes", Color.cyan, ref AntiLasereyes, enabled =>
            {
                if (!AntiLasereyes && enabled)
                {
                    foreach (var laser in NetworkEntities())
                    {
                        if (laser.JR_GetMarrowEntity().JR_GetBarcodeID() == "BamBaeYoh.LaserEyes.Spawnable.LaserEyes")
                        {
                            DespawnNow(laser);
                        }
                    }
                }

                AntiLasereyes = enabled;
            });


        }
        internal static void OPERATORoptions()
        {

            OPERATORPG.Logsettings("Anti One Shot", Color.cyan, ref antioneshot, enabled => { antioneshot = enabled; });


            OPERATORPG.CreateFunction("Clone Weapon In Left Hand", Color.yellow, () =>
            {
                var leftHandEntity = JR_YourGetHand(WhichHand.Left)?.JR_GetMarrowEntity();
                var leftBarcode = leftHandEntity?.JR_GetBarcodeID();

                if (!string.IsNullOrEmpty(leftBarcode) && IsBarcodeInGame(leftBarcode))
                {
                    SpawnIt(leftBarcode, (Vector3)(JR_YourGetHand(WhichHand.Left)?.transform.position + JR_YourGetHand(WhichHand.Left)?.transform.forward + JR_YourGetHand(WhichHand.Left)?.transform.up), Quaternion.identity);
                }
            });
            OPERATORPG.CreateFunction("Clone Weapon In Right Hand", Color.yellow, () =>
            {
                var rightHandEntity = JR_YourGetHand(WhichHand.Right)?.JR_GetMarrowEntity();
                var rightBarcode = rightHandEntity?.JR_GetBarcodeID();

                if (!string.IsNullOrEmpty(rightBarcode) && IsBarcodeInGame(rightBarcode))
                {
                    SpawnIt(
                        rightBarcode,
                        (Vector3)(JR_YourGetHand(WhichHand.Right)?.transform.position
                                  + JR_YourGetHand(WhichHand.Right)?.transform.forward
                                  + JR_YourGetHand(WhichHand.Right)?.transform.up),
                        Quaternion.identity
                    );
                }
            });
            OPERATORPG.Logsettings("Dashing", Color.cyan, ref dashingnow, enabled => { dashingnow = enabled; });
            OPERATORPG.Logsettings("Double Jump", Color.cyan, ref doublejumpnow, enabled => { doublejumpnow = enabled; });
            OPERATORPG.Logsettings("Air Control", Color.cyan, ref Aircontrolnow, enabled => { Aircontrolnow = enabled; });
            OPERATORPG.Logsettings("Anti Self Constraints", Color.cyan, ref selfconstraint, enabled => { selfconstraint = enabled; });
        }
        internal static void Hostonlyoptions() 
        {
            HOSTONLYPGE.Logsettings("Hide Playerlist From Lobbybrowser", Color.white, ref HIDEPLAYERLIST, enabled =>
            {
                if (!HIDEPLAYERLIST && enabled)
                {
                    if (NetworkInfo.IsHost)
                    {
                        NetworkHelper.Disconnect();
                        NotificationNowAlways(FusionProtectorInfo.ClientName, "Disconnected Lobby Restart It To Have Playerlist Protection", NotificationType.SUCCESS, 5.0f);
                    }
                }




                HIDEPLAYERLIST = enabled;
            });

            AltpreventPG = HOSTONLYPGE.CreatePage("+ Player Options [PO]", Color.green);
            

            AltpreventPG.Logsettings("Enable Player Options", Color.cyan, ref disablesteamreading, enabled =>
            {
                disablesteamreading = enabled;
            });


            AltpreventPG.Logsettings("[PO] Spoof Profile Notification", Color.cyan, ref spooferprofiledetection, enabled =>
            {
                spooferprofiledetection = enabled;
            });
            AltpreventPG.Logsettings("[PO] Clone Exploit Notifications", Color.cyan, ref clonedetector, enabled =>
            {
                clonedetector = enabled;
            });
            AltpreventPG.Logsettings("[PO] Kick Private Steam Accounts", Color.cyan, ref privatekicksteam, enabled =>
            {
                privatekicksteam = enabled;
            });
            
            AltpreventPG.Logsettings("[PO] Alt Remover", Color.cyan, ref AltRemov, enabled =>
            {
                AltRemov = enabled;
            });
            AltpreventPG.Logsettings("[PO] Alt Removal Ban Instead", Color.cyan, ref baninsteadalt, enabled =>
            {
                baninsteadalt = enabled;
            });
            AltpreventPG.Logsettings("[PO] Alt Teleport To You", Color.cyan, ref teleportaltacctoyou, enabled =>
            {
                teleportaltacctoyou = enabled;
            });




            statskick = HOSTONLYPGE.CreatePage("+ Stat Kicker Presets", Color.green);
            statskick.LogsettingsString("Preset Name", Color.yellow, ref statskickernows, (stringy) =>
            {
                statskickernows = stringy;
            });
            statskick.CreateFunction("Add Preset", Color.yellow, () =>
            {
                bool exists = false;

                foreach (var t in StatsKickerPresets.StatsKickerPresetz)
                {
                    if (string.Equals(t.TitleOfPreset, statskickernows, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    StatsKickerPresets.StatsKickerPresetz.Add(new StatsKickerPresets(statskickernows, "1000", "1000", "1000", "1000", "1000", "1000", "1000", "1000", "1000", "1000", "1000"));
                    StatsKickerPresets.SavePresets();
                    NotificationNow(FusionProtectorInfo.ClientName,
    $"Added Preset {statskickernows}!",
    NotificationType.SUCCESS,
    2.5f);
                }
                else
                {
                    NotificationNow(FusionProtectorInfo.ClientName,
                        "This Preset Name Exists Already!",
                        NotificationType.WARNING,
                        2.5f);
                }
            });
            statskicknow = statskick.CreatePage("+ Active Presets", Color.green);
            statskick.Logsettings("Stat Kicker", Color.cyan, ref statkicker, enabled =>
            {
                statkicker = enabled;
            });

            spawnlimitersz = HOSTONLYPGE.CreatePage("+ Spawn Limits", Color.green);
            spawnlimitersz.Logsettings("Item Limit To Player Count", Color.cyan, ref limitplayercount, enabled =>
            {
                limitplayercount = enabled;
            });

            spawnlimitersz.CreateFunction("Add Per Item Limit Right Hand Item", Color.yellow, () =>
            {
                var id = JR_YourGetHand(WhichHand.Right).JR_GetMarrowEntity().JR_GetBarcodeID();

                if (IsBarcodeInGame(id))
                {
                    var lines = File.Exists(spawnlimitshostonly)
                        ? File.ReadAllLines(spawnlimitshostonly).ToList()
                        : new System.Collections.Generic.List<string>();

                    string existingLine = lines.FirstOrDefault(l => l.StartsWith(id + ":"));
                    string oldValue = null;

                    if (existingLine != null)
                    {
                        oldValue = existingLine.Split(':')[1];

                        if (oldValue == limithostonly.ToString())
                        {
                            lines.Remove(existingLine);
                            File.WriteAllLines(spawnlimitshostonly, lines);

                            spawnlimitlinelist.Remove(existingLine);

                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                $"Removed {id.JR_BarcodeCrateName()} Limit [{limithostonly}] From Spawn Limiter!",
                                NotificationType.ERROR,
                                3f
                            );

                            return;
                        }

                        lines.Remove(existingLine);
                    }

                    string newLine = $"{id}:{limithostonly}";
                    lines.Add(newLine);

                    File.WriteAllLines(spawnlimitshostonly, lines);

                    spawnlimitlinelist.Remove(existingLine);
                    spawnlimitlinelist.Add(newLine);

                    if (oldValue != null)
                    {
                        NotificationNow(
                            FusionProtectorInfo.ClientName,
                            $"Updated {id.JR_BarcodeCrateName()} Limit [{oldValue} → {limithostonly}]",
                            NotificationType.SUCCESS,
                            3f
                        );
                    }
                    else
                    {
                        NotificationNow(
                            FusionProtectorInfo.ClientName,
                            $"Added {id.JR_BarcodeCrateName()} Limit [{limithostonly}] To Spawn Limiter!",
                            NotificationType.SUCCESS,
                            3f
                        );
                    }
                }
            });
            spawnlimitersz.CreateFunction("Add Per Item Limit Left Hand Item", Color.yellow, () =>
            {
                var id = JR_YourGetHand(WhichHand.Left).JR_GetMarrowEntity().JR_GetBarcodeID();

                if (IsBarcodeInGame(id))
                {
                    var lines = File.Exists(spawnlimitshostonly)
                        ? File.ReadAllLines(spawnlimitshostonly).ToList()
                        : new System.Collections.Generic.List<string>();

                    string existingLine = lines.FirstOrDefault(l => l.StartsWith(id + ":"));
                    string oldValue = null;

                    if (existingLine != null)
                    {
                        oldValue = existingLine.Split(':')[1];

                        if (oldValue == limithostonly.ToString())
                        {
                            lines.Remove(existingLine);
                            File.WriteAllLines(spawnlimitshostonly, lines);

                            spawnlimitlinelist.Remove(existingLine);

                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                $"Removed {id.JR_BarcodeCrateName()} Limit [{limithostonly}] From Spawn Limiter!",
                                NotificationType.ERROR,
                                3f
                            );

                            return;
                        }

                        lines.Remove(existingLine);
                    }

                    string newLine = $"{id}:{limithostonly}";
                    lines.Add(newLine);

                    File.WriteAllLines(spawnlimitshostonly, lines);

                    spawnlimitlinelist.Remove(existingLine);
                    spawnlimitlinelist.Add(newLine);

                    if (oldValue != null)
                    {
                        NotificationNow(
                            FusionProtectorInfo.ClientName,
                            $"Updated {id.JR_BarcodeCrateName()} Limit [{oldValue} → {limithostonly}]",
                            NotificationType.SUCCESS,
                            3f
                        );
                    }
                    else
                    {
                        NotificationNow(
                            FusionProtectorInfo.ClientName,
                            $"Added {id.JR_BarcodeCrateName()} Limit [{limithostonly}] To Spawn Limiter!",
                            NotificationType.SUCCESS,
                            3f
                        );
                    }
                }
            });
            spawnlimitersz.Logsettingsint("Per Item Limit", Color.green, ref limithostonly, 1, 1, 100, intnow =>
            {
                limithostonly = intnow;
            });

            spawnlimitersz.Logsettings("Per Item Spawn Limiter", Color.cyan, ref hostonlyspawnlimiter, enabled =>
            {
                hostonlyspawnlimiter = enabled;
            });


            spawnlimitersz.Logsettingsint("Global Per Item Limit", Color.green, ref limitnowofglobal, 1, 1, 100, intnow =>
            {
                limitnowofglobal = intnow;
            });
            spawnlimitersz.Logsettings("Global Per Item Spawn Limiter", Color.cyan, ref globalspawnlimitperitem, enabled =>
            {
                globalspawnlimitperitem = enabled;
            });


            HOSTONLYPGE.Logsettings("No Damage Unless Weapons In Hand", Color.cyan, ref nodamageunlessweapons, enabled =>
            {
                nodamageunlessweapons = enabled;
            });

            HOSTONLYPGE.LogsettingsString("Max Damage Threshold Value", Color.yellow, ref maxdamagethressy, (stringy) =>
            {
                if (float.TryParse(stringy, out var floatofdamage))
                {
                    maxdamagethressy = stringy;
                    NotificationNow(FusionProtectorInfo.ClientName, $"Set Value {maxdamagethressy}", NotificationType.SUCCESS, 2.0f);

                }
                else
                {
                    NotificationNow(FusionProtectorInfo.ClientName, "Failed Needs To Be A Number!!!!!!!!!!!", NotificationType.ERROR, 2.0f);
                }
            });

            HOSTONLYPGE.Logsettings("Max Damage Threshold", Color.cyan, ref servermaxdamagethres, enabled =>
            {
                servermaxdamagethres = enabled;
            });



            HOSTONLYPGE.Logsettings("No Name Change", Color.cyan, ref AntiAnimatedName, enabled => { AntiAnimatedName = enabled; });


            HOSTONLYPGE.Logsettings("Kick If Vitality Infinite", Color.cyan, ref kickifvitaly, enabled => { kickifvitaly = enabled; });
            HOSTONLYPGE.Logsettings("Avatar Kick If Change Into", Color.cyan, ref avatarskickon, enabled => { avatarskickon = enabled; });
            HOSTONLYPGE.Logsettings("Kick If Spawned", Color.cyan, ref spawnableskickon, enabled => { spawnableskickon = enabled; });


            HOSTONLYPGE.Logsettings("Kick Weird/Invisible Usernames", Color.cyan, ref kickunincodenames, enabled => { kickunincodenames = enabled; });


            HOSTONLYPGE.LogsettingsEnum("Anti Modded Spawn Guns Type", Color.yellow, ref antimodguntypereal, enabled =>
            {
                antimodguntypereal = (antimodguntype)enabled;
            });
            HOSTONLYPGE.Logsettings("Anti Modded Spawn Guns", Color.cyan, ref SpawnGunProtection, enabled =>
            {
                SpawnGunProtection = enabled;
            });
            HOSTONLYPGE.Logsettings("Owner Perms Bypass Some Protections", Color.cyan, ref OwnerCheckEnabled, enabled =>
            {
                OwnerCheckEnabled = enabled;
            });
            HOSTONLYPGE.Logsettings("Anti Spam Spawning", Color.cyan, ref spamspawnprevention, enabled =>
            {
                spamspawnprevention = enabled;
            });
            HOSTONLYPGE.Logsettingsint("Anti Spam Spawn Limit", Color.green, ref countbeforespam, 1, 1, 20, intnow =>
            {
                countbeforespam = intnow;
            });
            HOSTONLYPGE.Logsettingsint("Anti Spam Spawn Timer", Color.green, ref antispamspawntimer, 1, 1, 10000, intnow =>
            {
                antispamspawntimer = intnow;
            });

            HOSTONLYPGE.Logsettings("Anti Crash Spawn Delay", Color.cyan, ref spawnlimiternow, enabled =>
            {
                spawnlimiternow = enabled;
            });
            HOSTONLYPGE.Logsettingsfloat("Spawn Delay", Color.green, ref spawnlimitertimer, 0.1f, 0.1f, 5.0f, floatnow =>
            {
                spawnlimitertimer = floatnow;
            });

            HOSTONLYPGE.Logsettings("Owners Can Change Map", Color.cyan, ref ownerscanchangemap, enabled =>
            {
                ownerscanchangemap = enabled;
            });
            HOSTONLYPGE.Logsettings("Owners Can Change Server Settings", Color.cyan, ref OWNERSCANCHANGESERVER, enabled =>
            {
                OWNERSCANCHANGESERVER = enabled;
            });
            HOSTONLYPGE.Logsettings("Owners Can Change Server Gamemode", Color.cyan, ref ownerscanchangegamemode, enabled =>
            {
                ownerscanchangegamemode = enabled;
            });
            HOSTONLYPGE.Logsettings("Spawn Bypass Protection", Color.cyan, ref spawnbypassprotection, enabled =>
            {
                spawnbypassprotection = enabled;
            });
            HOSTONLYPGE.Logsettings("Despawn Protection", Color.cyan, ref DESPAWNPROTECTION, enabled =>
            {
                DESPAWNPROTECTION = enabled;
            });

            HOSTONLYPGE.Logsettings("Prevent Balling Players", Color.cyan, ref ballplayersprevention, enabled =>
            {
                ballplayersprevention = enabled;
            });
            HOSTONLYPGE.Logsettings("Prevent Blinding Players", Color.cyan, ref blindplayersprevention, enabled =>
            {
                blindplayersprevention = enabled;
            });
            HOSTONLYPGE.Logsettings("Kick Username Spoofers", Color.cyan, ref AutoKickSpoofers, enabled =>
            {
                AutoKickSpoofers = enabled;
            });
            HOSTONLYPGE.Logsettings("Safe Distance Spawning", Color.cyan, ref safedistancespawning, enabled =>
            {
                safedistancespawning = enabled;
            }).SetTooltip("this makes it so if anyone is within a few meters of a player they cannot spawn they have to backup to spawn stuff");
            HOSTONLYPGE.Logsettings("Strength Threshold", Color.green, ref strengththresprotection, enabled =>
            {
                strengththresprotection = enabled;
            });
            HOSTONLYPGE.Logsettingsfloat("Strength Threshold Value", Color.cyan, ref strengththreshnow, 10.0f, 150f, 2000.0f, (value) =>
            {

                strengththreshnow = value;
            });






        }
        internal static bool AreYouOWNER()
        {
            if (!NetworkInfo.HasServer) 
                return true;
            FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var xc, out _);
            return xc == PermissionLevel.OWNER;
        }
        internal static bool AreYouOPERATOR()
        {
            if (!NetworkInfo.HasServer) 
                return true;
            FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var xc, out _);
            return xc == PermissionLevel.OPERATOR || xc == PermissionLevel.OWNER;
        }
        internal static bool HostIsMe(NetworkPlayer playerNow)
        {
            return playerNow != null &&
                   playerNow.PlayerID != null &&
                   playerNow.PlayerID.IsHost &&
                   playerNow.IsMe();
        }
        internal static void ReloadList()
        {

            TeleporterManager.LoadTeleporters();
            CreateCheatToolsPreset.LoadPresets();
            BodyLogPage.LoadPresets();
            InventoryPage.LoadAllPresets();
            BodyLogRadialMenuColorPreset.LoadPresets();
            StatsKickerPresets.LoadPresets();
            FusionProfilePresets.LoadPresets();


            BlockedSpawnables.Clear();
            foreach (var line in File.ReadAllLines(BlockedSpawnablesPath).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                BlockedSpawnables.Add(line);

            }
            WarnedSpawnables.Clear();
            foreach (var line in File.ReadAllLines(WarnedSpawnablesPath).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                WarnedSpawnables.Add(line);
            }
            SpawnablesKick.Clear();
            foreach (var line in File.ReadAllLines(SpawnablesKickPath).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                SpawnablesKick.Add(line);
            }
            AvatarsKick.Clear();
            foreach (var line in File.ReadAllLines(AvatarsKickPath).Where(l => !string.IsNullOrWhiteSpace(l)))
            {

                AvatarsKick.Add(line);
            }
            blockedplatformids.Clear();
            foreach (var line in File.ReadAllLines(DamageBlockPath).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                blockedplatformids.Add(line);

            }
            CustomAvFav.Clear();
            CustomAvFavref.Clear();
            foreach (var line in File.ReadAllLines(AvatarCustomFav).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                CustomAvFav.Add(line);
                CustomAvFavref.Add(new AvatarCrateReference(line));
            }

            CustomSpawnFav.Clear();
            CustomSpawnFavref.Clear();
            foreach (var line in File.ReadAllLines(SpawnableCustomFav).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                CustomSpawnFav.Add(line);
                CustomSpawnFavref.Add(new SpawnableCrateReference(line));
            }
            blockedspawnies.Clear();
            foreach (var line in File.ReadAllLines(ServerBlockSpawnPath).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                blockedspawnies.Add(line);
            }
            voicblocked.Clear();
            foreach (var line in File.ReadAllLines(voicepathblocked).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                voicblocked.Add(line);
            }

            spawnlimitlinelist.Clear();
            spawnlimitline.Clear();
            foreach (var line in File.ReadAllLines(spawnlimitshostonly).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                spawnlimitlinelist.Add(line);
                var trimmed = line.Trim();
                var parts = trimmed.Split(new[] { ':' }, 2);
                var left = parts[0].Trim().ToLowerInvariant().ToString();
                var right = parts[1].Trim();

                if (int.TryParse(right, out int maxlimit))
                {
                    spawnlimitline.Add(left, maxlimit);
                }
            }

            blockedavifallbacks.Clear();
            foreach (var line in File.ReadAllLines(avatarsblocked).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                blockedavifallbacks.Add(line);
            }
            modidblocked.Clear();
            foreach (var line in File.ReadAllLines(ModIDBLOCKSPATH).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                modidblocked.Add(line);
            }
            blockmessages.Clear();
            foreach (var line in File.ReadAllLines(blockmessagingnowpath).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                blockmessages.Add(line);
            }
            blockentireauthor.Clear();
            foreach (var line in File.ReadAllLines(blockauthornowlist).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                blockentireauthor.Add(line);
            }
            blockentirepallet.Clear();
            foreach (var line in File.ReadAllLines(blockpalletnowlist).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                blockentirepallet.Add(line);
            }
            blockavipalletlist.Clear();
            foreach (var line in File.ReadAllLines(BlockPalletAviNowp).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                blockavipalletlist.Add(line);
            }
            blockaviauthorlist.Clear();
            foreach (var line in File.ReadAllLines(BlockAviAuthorNowp).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                blockaviauthorlist.Add(line);
            }
            warnavilist.Clear();
            foreach (var line in File.ReadAllLines(WarnAvisNow).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                warnavilist.Add(line);
            }
            MEDIAPLAYERBLOCKERNOWList.Clear();
            foreach (var line in File.ReadAllLines(MEDIAPLAYERBLOCKERNOW).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                MEDIAPLAYERBLOCKERNOWList.Add(line);
            }


            blockmovements.Clear();
            foreach (var line in File.ReadAllLines(BlockMovementsPath).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                blockmovements.Add(line);
            }


            if (File.Exists(InventoryPage.PresetsFileCurrent))
            {
                string jsonDefault = File.ReadAllText(InventoryPage.PresetsFileCurrent);
                var loadedCurrent = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(jsonDefault);
                if (loadedCurrent != null)
                    InventoryPage.CurrentPreset = loadedCurrent;
            }

            if (File.Exists(devitemscurrent))
            {
                var currentJson3 = File.ReadAllText(devitemscurrent);
                var current3 = JsonConvert.DeserializeObject<CreateCheatToolsPreset>(currentJson3);
                if (current3 != null)
                    CurrentPresetNow = current3;
            }



        }
        internal static void RemoveSoundGrip(Grip gruppy)
        {
            var grippyobj = gruppy.gameObject.transform.root.gameObject;

            if (grippyobj.JR_IsWeapon() || grippyobj.JR_IsMelee())
            {
                foreach (var source in grippyobj.gameObject.GetComponentsInChildren<AudioSource>(true))
                {
                    source.Stop();
                    source.mute = true;
                }
                foreach (var source in grippyobj.gameObject.GetComponentsInChildren<AudioSource>(true))
                {
                    source.enabled = false;
                }
                foreach (var gunSfx in grippyobj.GetComponentsInChildren<GunSFX>(true))
                {
                    gunSfx.enabled = false;
                }

                foreach (var gravGunSfx in grippyobj.GetComponentsInChildren<GravGunSFX>(true))
                {
                    gravGunSfx.enabled = false;
                }
            }
        }

        #endregion

        internal enum MenuSections
        {
            SelfCat,
            SpawnLogs,
            AvatarSwitchLogs,
            SpawnableSearcher,
            AvatarSearcher,
            CustomAviFav,
            CustomSpawnableFav,
            PlayerInformation,
            SceneEntities,
            ServerHistory,
            FusionBanManager,
            DownloadLogger,
            MediaPlayerLogger,
            JoinLoggerNow,
            DamageLogger,
            NetworkLogger,
            PlayerSeriStats,
            PlayerSearch,
            MODIOSEARCHER
        }
        public override void OnGUI()
        {
            if (NetworkInfo.HasLayer && !HelperMethods.IsLoading())
            {
                if (!show)
                    return;

                var windowWidth = 1000;
                var windowHeight = 1000;

                var centerX = (Screen.width - windowWidth) / 2;
                var centerY = (Screen.height - windowHeight) / 2;

                GUI.Window(
                    8888,
                    new Rect(centerX, centerY, windowWidth, windowHeight),
                    (GUI.WindowFunction)ProtectorUI,
                    $"{FusionProtectorInfo.ClientName} v{FusionProtectorInfo.Version} | [Press Y To Close/Open | ← → Scroll Left & Right ]"
                );
                GUI.BringWindowToFront(8888);

                var pagesWidth = 320;
                var pagesHeight = 1000;
                var gap = 10;
                var pagesX = centerX - pagesWidth - gap;
                var pagesY = centerY;

                GUI.Window(
                    7777,
                    new Rect(pagesX, pagesY, pagesWidth, pagesHeight),
                    (GUI.WindowFunction)PagesMenu,
                    $"{FusionProtectorInfo.ClientName} Pages"
                );
                GUI.BringWindowToFront(7777);
            }
        }
        internal static void PagesMenu(int windowID)
        {

            const float baseX = 10f;
            const float baseY = 20f;
            const float buttonWidth = 300f;
            const float buttonHeight = 30f;
            const float buttonSpacing = 35f;
            const float scrollSpeed = 20f;


            if (Event.current != null)
            {
                switch (Event.current.type)
                {
                    case EventType.ScrollWheel:
                        pscroll += Event.current.delta.y * scrollSpeed;
                        break;
                    case EventType.KeyDown:
                        if (Event.current.keyCode == KeyCode.UpArrow) pscroll -= scrollSpeed;
                        if (Event.current.keyCode == KeyCode.DownArrow) pscroll += scrollSpeed;
                        break;
                }

                pscroll = Mathf.Max(0f, pscroll);
            }

            GUIStyle buttonStyle = new(GUI.skin.button) { richText = true };

            float y = baseY - pscroll;

            void CreatePageButton(string label, System.Action onClick)
            {
                Rect rect = new(baseX, y, buttonWidth, buttonHeight);
                if (GUI.Button(rect, label, buttonStyle))
                {
                    onClick?.Invoke();
                    scrollX = 0f;
                    scroll = 0f;
                }
                y += buttonSpacing;
            }

            GUI.Label(new Rect(baseX, y, buttonWidth, buttonHeight), "Pages :");
            y += buttonSpacing;

            if (NetworkInfo.HasLayer)
            {
                CreatePageButton("Fusion Lookup", () => PageNow = MenuSections.PlayerSearch);
            }



            if (NetworkInfo.HasServer)
            {
                CreatePageButton("Network Entities", () =>
                {
                    PageNow = MenuSections.SceneEntities;

                    ListNetworkEntities = NetworkEntities().ToList();
                });
                CreatePageButton("Players Information", () => PageNow = MenuSections.PlayerInformation);
                CreatePageButton("Server History", () => PageNow = MenuSections.ServerHistory);

            }

            CreatePageButton("Main Options", () => PageNow = MenuSections.SelfCat);
            
            CreatePageButton("Lobby Spawn Logs", () => PageNow = MenuSections.SpawnLogs);
            if (NetworkInfo.IsHost)
            {         
                CreatePageButton("Network Message Logger", () => PageNow = MenuSections.NetworkLogger);

                CreatePageButton("Damage Information", () => PageNow = MenuSections.DamageLogger);
                CreatePageButton("Player Avatar Stats", () => PageNow = MenuSections.PlayerSeriStats);

            }

            CreatePageButton("Recently Met Players", () => PageNow = MenuSections.JoinLoggerNow);



            CreatePageButton("Mod.IO Searchup", () => PageNow = MenuSections.MODIOSEARCHER);

            CreatePageButton("Mod.IO Download Logger", () => PageNow = MenuSections.DownloadLogger);

            CreatePageButton("Media Player Logger", () => PageNow = MenuSections.MediaPlayerLogger);

            CreatePageButton("Avatar Logs", () => PageNow = MenuSections.AvatarSwitchLogs);
            CreatePageButton("Spawnable Searcher", () => PageNow = MenuSections.SpawnableSearcher);
            CreatePageButton("Avatar Searcher", () => PageNow = MenuSections.AvatarSearcher);
            CreatePageButton("Custom Avatar Favorites", () => PageNow = MenuSections.CustomAviFav);
            CreatePageButton("Custom Spawnable Favorites", () => PageNow = MenuSections.CustomSpawnableFav);
            CreatePageButton("Fusion Ban Manager", () => PageNow = MenuSections.FusionBanManager);


        }
        internal static void ProtectorUI(int windowID)
        {
            float baseX = 10f;
            float baseY = 20f;

            if (Event.current != null)
            {
                if (Event.current.type == EventType.ScrollWheel)
                {
                    scroll += Event.current.delta.y * 20f;
                    if (Event.current.alt) scrollX += Event.current.delta.y * 40f;
                }

                if (Event.current.type == EventType.KeyDown)
                {
                    if (Event.current.keyCode == KeyCode.UpArrow) scroll -= 20f;
                    if (Event.current.keyCode == KeyCode.DownArrow) scroll += 20f;
                    if (Event.current.keyCode == KeyCode.LeftArrow) scrollX -= 40f;
                    if (Event.current.keyCode == KeyCode.RightArrow) scrollX += 40f;
                }

                scroll = Mathf.Max(0f, scroll);
                scrollX = Mathf.Max(0f, scrollX);
            }

            GUIStyle buttonStyle = new(GUI.skin.button) { richText = true };

            float y = baseY - scroll;
            float xOffset = baseX - scrollX;

            void CreateBoolOptions(ref bool value, string label, System.Action off = null, System.Action on = null, float spacing = 35f)
            {
                Rect r = new(xOffset, y, 300, 30);
                bool prev = value;
                value = GUI.Toggle(r, value, label);
                y += spacing;

                if (value != prev)
                {
                    if (value) on?.Invoke();
                    else off?.Invoke();
                }
            }
            void CreateSliderOptions(ref float val, string label, float def, float min, float max, System.Action<float> onApply, float spacing = 35f)
            {
                Rect r = new(xOffset, y, 300, 25);
                GUI.Label(r, $"{label} Value [{Mathf.Round(val)}/{Mathf.Round(max)}]");
                y += spacing;

                Rect sliderRect = new Rect(xOffset, y, 300, 25);
                val = GUI.HorizontalSlider(sliderRect, val, min, max);
                y += spacing;

                float applyVal = val;

                Rect btnRect = new Rect(xOffset, y, 300, 30);
                if (GUI.Button(btnRect, $"Apply {label}", buttonStyle))
                {
                    onApply?.Invoke(applyVal);
                }
                y += spacing;
            }
            void CreateTextbox(ref string text, string label, float spacing = 35f, float widthcustom = 300)
            {
                Rect r = new(xOffset, y, 300, 25);
                GUI.Label(r, label);
                y += spacing;

                Rect box = new Rect(xOffset, y, widthcustom, 25);
                text = GUI.TextField(box, text);
                y += spacing;
            }
            void CreateOptionButton(string label, System.Action act, float spacing = 35f,float width = 300f,float height = 30f)
            {
                Rect r = new(xOffset, y, width, height);
                if (GUI.Button(r, label, buttonStyle))
                {
                    act?.Invoke();
                }
                y += spacing;
            }

            void CreateLabel(string label, float width = 300)
            {
                GUI.Label(new Rect(xOffset, y, width, 30), label);
                y += 35f;
            }

            void CreateSpacer(string title, float spacing = 35f)
            {
                Rect r = new Rect(xOffset, y, 300, 25);
                GUI.Label(r, title + " :");
                GUI.Box(new Rect(r.x, r.y + 20f, 300, 1), GUIContent.none);
                y += spacing;
            }
            void AddSection(float offtoadd)
            {
                xOffset += offtoadd;
                y = baseY - scroll;
            }
            void FreezeScrolling()
            {
                y = 20;
            }

            void ServerInfoNow(LobbyInfo StoredInformation)
            {
                if (StoredInformation != null)
                {

                    CreateLabel("Lobby Code : " + StoredInformation.LobbyCode, 1000);
                    CreateLabel("Host Name : " + StoredInformation.LobbyHostName, 1000);
                    CreateLabel("Level Title : " + StoredInformation.LevelTitle, 1000);
                    CreateLabel("Level Barcode : " + StoredInformation.LevelBarcode, 1000);
                    CreateLabel("Level Mod IO #ID : " + StoredInformation.LevelModID, 1000);
                    CreateLabel("Lobby ID : " + StoredInformation.LobbyID, 1000);

                    CreateLabel("Players :");
                    foreach (var rplayerinfonow in StoredInformation.PlayerList.Players)
                    {
                        CreateLabel($" Nickname : {StripColorTags(rplayerinfonow.Nickname)} | Username : {rplayerinfonow.Username} | Steam ID : {rplayerinfonow.PlatformID}", 1000);
                    }

                    if (StoredInformation.Privacy == ServerPrivacy.PUBLIC)
                    {
                        CreateOptionButton("Join Server", () => { NetworkHelper.JoinServerByCode(StoredInformation.LobbyCode); });
                    }
                    CreateOptionButton("Copy Lobby Details To Clipboard", () =>
                    {
                        GUIUtility.systemCopyBuffer = DataToJsonString(StoredInformation);
                        NotificationNowAlways(FusionProtectorInfo.ClientName,"Copied Lobby Info To Clipboard!",NotificationType.SUCCESS,3.5f);
                    });
                    CreateOptionButton("Download Map", () => { DownloadModIOMod(StoredInformation.LevelModID); });


                }
                else
                {
                    CreateLabel("No lobby selected.");
                }
            }
            void bloockwarnkickspawnables(SpawnableCrateReference serresult)
            {
                if (serresult == null || serresult.Crate == null || serresult.Crate.Barcode == null) return;

                var id = serresult.Crate.Barcode.ID;
                var pallet = serresult.Crate.Pallet;
                var palletname = StripColorTags(serresult.Crate.Pallet.name);
                var palletauthor = serresult.Crate.Pallet.Author;

                CreateLabel("Selected Spawnable Information : " + StripColorTags(serresult.Crate.name) + " | " + palletauthor, 700);
                CreateLabel($"Mod IO : {CrateFilterer.GetModID(serresult.Crate.Pallet)}", 600);
                CreateLabel($"Barcode ID : {id}", 600);
                CreateLabel($"Pallet Author : {palletauthor}", 600);
                CreateLabel($"Pallet Name : {palletname}", 600);
                CreateLabel(SpawnablesKick != null && SpawnablesKick.Contains(id) ? "Kick If Spawned : YES" : "Kick If Spawned : NO");
                CreateLabel(BlockedSpawnables != null && BlockedSpawnables.Contains(id) ? "Blocked Spawn : YES" : "Blocked Spawn : NO");
                CreateLabel(WarnedSpawnables != null && WarnedSpawnables.Contains(id) ? "Warn Spawn : YES" : "Warn Spawn : NO");
                CreateLabel(blockentirepallet != null && blockentirepallet.Contains(palletname) ? "Blocked Pallet : YES" : "Blocked Pallet : NO");
                CreateLabel(blockentireauthor != null && blockentireauthor.Contains(palletauthor) ? "Blocked Author : YES" : "Blocked Author : NO");
                CreateLabel(modidblocked != null && modidblocked.Contains(CrateFilterer.GetModID(pallet).ToString()) ? "Blocked Mod.IO Mod : YES" : "Blocked Mod.IO Mod : NO");
                CreateLabel(DataManager.ActiveSave.PlayerSettings.FavoriteSpawnables.Contains(id) ? "Favorited Spawnable : YES" : "Favorited Spawnable : NO");

                CreateOptionButton("Open Mod Download Folder", () =>
                {
                    GetPalletFolder(serresult.Crate.Pallet.name, true);
                });

                CreateOptionButton("Despawn All Of This Item", () =>
                {
                    foreach (var netentity in NetworkEntities())
                    {
                        var marrow = netentity?.JR_GetMarrowEntity();
                        if (marrow != null && marrow.JR_GetBarcodeID() == id)
                        {
                            netentity.JR_Despawn();
                        }
                    }
                    NotificationNow(FusionProtectorInfo.ClientName, $"Despawned Everything Matching {id.JR_BarcodeCrateName()}", NotificationType.SUCCESS, 3.5f);
                });

                CreateOptionButton("Force Delete All Of This Locally", () =>
                {
                    foreach (var netentity in NetworkEntities())
                    {
                        var marrow = netentity?.JR_GetMarrowEntity();
                        if (marrow != null && marrow.JR_GetBarcodeID() == id)
                        {
                            marrow.gameObject.DestroyNow();
                        }
                    }
                    NotificationNow(FusionProtectorInfo.ClientName, $"Force Deleted Locally Everything Matching {id.JR_BarcodeCrateName()}", NotificationType.SUCCESS, 3.5f);
                });

                CreateOptionButton("Spawn This", () =>
                {
                    var hand = JR_YourGetHand(WhichHand.Left);
                    if (hand != null)
                    {
                        SpawnIt(id, hand.transform.position + hand.transform.forward + hand.transform.up, Quaternion.identity);
                    }
                });

                CreateOptionButton("Add/Remove Block Author Of Spawnable", () =>
                {
                    ToggleAddRemoveFromFile(palletauthor, blockentireauthor, blockauthornowlist, FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodeAuthor()} To Blocked Authors!.", $"Removed {id.JR_BarcodeAuthor()} From Blocked Authors!.", true);
                });

                CreateOptionButton("Add/Remove Block Pallet Completely", () =>
                {
                    ToggleAddRemoveFromFile(palletname, blockentirepallet, blockpalletnowlist, FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodePalletName()} To Blocked Pallets!.", $"Removed {id.JR_BarcodePalletName()} From Blocked Pallets!.", true);
                });

                CreateOptionButton("Add/Remove Block This Mod.IO Mod Completely", () =>
                {
                    ToggleAddRemoveFromFile(CrateFilterer.GetModID(pallet).ToString(), modidblocked, ModIDBLOCKSPATH, FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodePalletName()} To Blocked Mod.IO Mod!.", $"Removed {id.JR_BarcodePalletName()} From Blocked Mod.IO Mod!.", true);
                });

                CreateOptionButton("Copy Details To Clipboard", () =>
                {
                    GUIUtility.systemCopyBuffer =
                        $"Spawnable Searcher Information : {pallet?.name}\n" +
                        $"Mod IO : {CrateFilterer.GetModID(pallet)}\n" +
                        $"Barcode ID : {id}\n" +
                        $"Pallet Author : {pallet?.Author}\n" +
                        $"Pallet Name : {pallet?.name}";

                    NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                });

                CreateOptionButton("Copy Barcode To Clipboard", () =>
                {
                    GUIUtility.systemCopyBuffer = id;

                    NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                });

                CreateOptionButton("Add/Remove Kick If Spawned", () =>
                {
                    ToggleAddRemoveFromFile(id, SpawnablesKick, SpawnablesKickPath, FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodeCrateName()} To Kick If Spawned!.", $"Removed {id.JR_BarcodeCrateName()} From Kick If Spawned!.");
                });

                CreateOptionButton("Block/UnBlock This Spawnable", () =>
                {
                    ToggleAddRemoveFromFile(id, BlockedSpawnables, BlockedSpawnablesPath, FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodeCrateName()} To Blocked Spawnables!.", $"Removed {id.JR_BarcodeCrateName()} From Blocked Spawnables!.");
                });

                CreateOptionButton("Warn/UnWarn This Spawnable", () =>
                {
                    ToggleAddRemoveFromFile(id, WarnedSpawnables, WarnedSpawnablesPath, FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodeCrateName()} To Warn Spawnables!.", $"Removed {id.JR_BarcodeCrateName()} From Warn Spawnables!.");
                });

                CreateOptionButton("Un/Favorite Spawnable", () =>
                {
                    var favorites = DataManager.ActiveSave?.PlayerSettings?.FavoriteSpawnables;
                    if (favorites == null) return;

                    if (!favorites.Contains(id))
                    {
                        favorites.Add(id);
                        DataManager.TrySaveActiveSave(SaveFlags.Complete);
                        NotificationNow(FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodeCrateName()} To SaveGame Favorites!\nReload Level For Effect!", NotificationType.SUCCESS, 4.0f);
                    }
                    else
                    {
                        favorites.Remove(id);
                        DataManager.TrySaveActiveSave(SaveFlags.Complete);
                        NotificationNow(FusionProtectorInfo.ClientName, $"Removed {id.JR_BarcodeCrateName()} From SaveGame Favorites!\nReload Level For Effect!", NotificationType.WARNING,4.0f);
                    }
                });

                CreateOptionButton("Add/Remove Custom Favorites [Spawnable]", () =>
                {
                    ToggleAddRemoveFromFile(id, CustomSpawnFav, SpawnableCustomFav, FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodeCrateName()} To Custom Spawnable Favorites!.", $"Removed {id.JR_BarcodeCrateName()} From Custom Spawnable Favorites!.");
                });

                CreateOptionButton("Delete Mod Completely", () =>
                {

                        DeleteModioMod(serresult.Crate.Pallet);
                });


            }
            void blockavatarfunctions(AvatarCrateReference avatarresult)
            {
                if (avatarresult == null || avatarresult.Crate == null || avatarresult.Crate.Barcode == null) return;

                var id = avatarresult.Crate.Barcode.ID;
                var pallet = avatarresult.Crate.Pallet;
                var palletname = StripColorTags(avatarresult.Crate.Pallet.name);
                var palletauthor = avatarresult.Crate.Pallet.Author;





                CreateLabel($"Mod IO : {CrateFilterer.GetModID(pallet)}", 600);
                CreateLabel($"Barcode ID : {id}", 600);
                CreateLabel($"Pallet Author : {palletauthor}", 600);
                CreateLabel($"Pallet Name : {palletname}", 600);
                
                
                CreateOptionButton("Open Mod Download Folder", () =>
                {
                    GetPalletFolder(avatarresult.Crate.Pallet.name, true);
                });

                CreateOptionButton("Change Into Avatar", () =>
                {
                    ChangeIntoAvi(id);
                });

                CreateOptionButton("Copy Details To Clipboard", () =>
                {
                    GUIUtility.systemCopyBuffer =
                        $"Avatar Information : {palletname}\n" +
                        $"Mod IO : {CrateFilterer.GetModID(pallet)}\n" +
                        $"Barcode ID : {id}\n" +
                        $"Pallet Author : {palletauthor}\n" +
                        $"Pallet Name : {palletname}";

                    NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                });

                CreateOptionButton("Copy Barcode To Clipboard", () =>
                {
                    GUIUtility.systemCopyBuffer = id;

                    NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                });

                CreateOptionButton("Add/Remove Kick If Changed Into", () =>
                {
                    ToggleAddRemoveFromFile(id, AvatarsKick, AvatarsKickPath, FusionProtectorInfo.ClientName, $"Added {id} To Kick If Changed Into!.", $"Removed {id} From Kick If Changed Into!.");
                });
                CreateOptionButton("Add/Remove Avatar To Avatar Blocker", () =>
                {
                    if (id != "c3534c5a-94b2-40a4-912a-24a8506f6c79")
                    {

                        ToggleAddRemoveFromFile(id, blockedavifallbacks, avatarsblocked, FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodeCrateName()} To Avatar Blocker!.", $"Removed {id.JR_BarcodeCrateName()} From Avatar Blocker!.");
                    }
                    else
                    {
                        NotificationNow(FusionProtectorInfo.ClientName, "Can't Do That!", NotificationType.ERROR, 2.5f);
                    }
                });

                CreateOptionButton("Add/Remove Block Avatar Pallet", () =>
                {
                    ToggleAddRemoveFromFile(palletname, blockavipalletlist, BlockPalletAviNowp, FusionProtectorInfo.ClientName, $"Added {id.JR_BarcodePalletName()} To Block Avatar Pallet!.", $"Removed {id.JR_BarcodePalletName()} From Block Avatar Pallet!.", true);
                });
                CreateOptionButton("Add/Remove Block Avatar Author", () =>
                {
                    ToggleAddRemoveFromFile(palletauthor, blockaviauthorlist, BlockAviAuthorNowp, FusionProtectorInfo.ClientName, $"Added {palletauthor} To Block Avatar Author!.", $"Removed {palletauthor} From Block Avatar Author!.", true);
                });
                CreateOptionButton("Add/Remove Warn Avatar Change", () =>
                {
                    ToggleAddRemoveFromFile(id, warnavilist, WarnAvisNow, FusionProtectorInfo.ClientName, $"Added {id} To Warn Avatar Change!.", $"Removed {id} From Warn Avatar Change!.");
                });

                CreateOptionButton("Add/Remove Custom Favorites [Avatar]", () =>
                {
                    ToggleAddRemoveFromFile(id, CustomAvFav, AvatarCustomFav, FusionProtectorInfo.ClientName, $"Added {id} To Custom Avatar Favorites!.", $"Removed {id} From Custom Avatar Favorites!.");

                });
                CreateOptionButton("Delete Mod Completely", () =>
                {
                    DeleteModioMod(pallet);
                });
            }


            if (PageNow != PreviousPage)
            {
                scroll = 0f;
                y = 20f;
                PreviousPage = PageNow;
            }
            else
            {
                y = baseY - scroll;
            }


            switch (PageNow)
            {
                //MenuSections.MODIOSEARCHER
                case MenuSections.MODIOSEARCHER:

                    CreateTextbox(ref modiosearchernow, "Mod.IO Searcher :");
                    CreateOptionButton("Search For Mod....", () =>
                    {
                        if (int.TryParse(modiosearchernow,out int modnowint))
                        {
                            MelonCoroutines.Start(ModioInfo(modnowint, (info) =>
                            {
                                moddyinfostored = info;
                            }));
                        }
                    });

                    if (!string.IsNullOrEmpty(moddyinfostored.Data.NameID))
                    {
                        AddSection(310);
                        CreateLabel("Mod.IO Info :");

                        CreateLabel("NameID :" + moddyinfostored.Data.NameID, 1000);
                        CreateLabel("ID :" + moddyinfostored.Data.ID, 1000);
                        CreateLabel("Mature :" + moddyinfostored.Data.Mature, 1000);
                        CreateLabel("MaturityOption :" + moddyinfostored.Data.MaturityOption, 1000);
                        CreateLabel("ThumbnailUrl :" + moddyinfostored.Data.ThumbnailUrl,1000);
                        
                        CreateOptionButton("Copy Mod.IO Info To Clipboard", () =>
                        {
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                IncludeFields = true
                            };

                            string modinfonowc = JsonSerializer.Serialize(moddyinfostored, options);
                            GUIUtility.systemCopyBuffer = modinfonowc;
                            NotificationNowAlways(FusionProtectorInfo.ClientName, "Copied Mod.IO Info To Clipboard!", NotificationType.SUCCESS, 3.5f);
                        });

                        CreateOptionButton("Add/Remove Block This Mod.IO Mod Completely", () =>
                        {
                            ToggleAddRemoveFromFile(moddyinfostored.Data.ID.ToString(), modidblocked, ModIDBLOCKSPATH, FusionProtectorInfo.ClientName, $"Added {moddyinfostored.Data.NameID} To Blocked Mod.IO Mod!.", $"Removed {moddyinfostored.Data.NameID} From Blocked Mod.IO Mod!.", true);
                        });
                        CreateOptionButton("Download Mod", () =>
                        {
                            DownloadModIOMod(moddyinfostored.Data.ID);
                        });
                    }



                    break;


                case MenuSections.PlayerSearch:

                 // CreateTextbox(ref searchfusionuser, "");
                 //
                 // CreateOptionButton("Search Fusion For Player", () =>
                 // {
                 //     FindPlayersLobbyFromPlayerName(searchfusionuser, foundPlayers, success =>{});
                 // });

                //foreach (var kvp in foundPlayers)
                //{
                //    var lobbyplayerisin = kvp.Key;
                //    var plyer = kvp.Value;
                //
                //
                //    foreach (var xonu in plyer)
                //    {
                //        CreateOptionButton(
                //            CleanedNAME(StripColorTags(xonu.Nickname), xonu.Username),
                //            () => { StoredRefForLookup = xonu;
                //                StoredRefForLookupLobby = lobbyplayerisin;
                //            }
                //        );
                //    }
                //}

                    CreateLabel("Lobbies Logged Since Fusion Login : "+ CachedLobbies.Count.ToString());
                    CreateTextbox(ref searchedloggedlobbies, "Search Through");

                    foreach (var lobbies in CachedLobbies)
                    {
                        if (StripColorTags(lobbies.Metadata.LobbyInfo.LobbyHostName).ToLower().Contains(searchedloggedlobbies.ToLower()) || StripColorTags(lobbies.Metadata.LobbyInfo.LobbyName).ToLower().Contains(searchedloggedlobbies.ToLower()))
                        {
                            if (lobbies.Metadata.LobbyInfo.Privacy == ServerPrivacy.PUBLIC)
                            {
                                string lobbyText =
                                    $"<color=yellow><b>{StripColorTags(lobbies.Metadata.LobbyInfo.LobbyName)}</b></color>\n" +
                                    $"Host: <color=cyan><b>{lobbies.Metadata.LobbyInfo.LobbyHostName}</b></color>\n" +
                                    $"Players: <color=lime><b>{lobbies.Metadata.LobbyInfo.PlayerCount}/{lobbies.Metadata.LobbyInfo.MaxPlayers}</b></color>\n" +
                                    $"Map: <color=orange><b>{lobbies.Metadata.LobbyInfo.LevelTitle}</b></color>\n" +
                                    $"Privacy: <color=white><b>{lobbies.Metadata.LobbyInfo.Privacy}</b></color>";

                                CreateOptionButton(lobbyText, () =>
                                {

                                    var options = new JsonSerializerOptions
                                    {
                                        WriteIndented = true,
                                        IncludeFields = true
                                    };

                                    string lobbfinfo = JsonSerializer.Serialize(lobbies, options);
                                    GUIUtility.systemCopyBuffer = lobbfinfo;
                                    NotificationNowAlways(FusionProtectorInfo.ClientName, "Lobby Info Copied To Clipboard!", NotificationType.SUCCESS, 3.5f);




                                }, 95f, 300, 90);
                            }
                        }
                    }
                   
                    AddSection(310);
                 
                    CreateLabel("Players Logged Since Fusion Login : " + PlayersOnline.Count.ToString());
                    CreateTextbox(ref searchedloggedPLAYER, "Search Through");

                    foreach (var playerfus in PlayersOnline)
                    {
                        if (StripColorTags(playerfus.Username).ToLower().Contains(searchedloggedPLAYER.ToLower()) || StripColorTags(playerfus.Nickname).ToLower().Contains(searchedloggedPLAYER.ToLower()))
                        {
                            CreateOptionButton(CleanedNAME(playerfus.Nickname, playerfus.Username), () =>
                        {
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                IncludeFields = true
                            };

                            string plyin = JsonSerializer.Serialize(playerfus, options);
                            GUIUtility.systemCopyBuffer = plyin;
                            NotificationNowAlways(FusionProtectorInfo.ClientName, "Players Info Copied To Clipboard!", NotificationType.SUCCESS, 3.5f);

                        });
                        }
                    }

                    break;
                case MenuSections.JoinLoggerNow:

                    CreateLabel("Recently Met Players :" + JoinLogger.Count);

                    foreach (var logger in JoinLogger)
                    {
                        if (logger == null)
                            continue;

                        string nickname = logger.Nickname ?? "Unknown";
                        string username = logger.Username ?? "Unknown";

                        CreateOptionButton(
                            CleanedNAME(nickname, username),
                            () =>
                            {
                                StoredNowplayerrrr = logger;
                            });
                    }

                    FreezeScrolling();
                    AddSection(310);

                    string selectedPlayerName = "None";

                    if (StoredNowplayerrrr != null)
                    {
                        string nickname = StoredNowplayerrrr.Nickname ?? "Unknown";
                        string username = StoredNowplayerrrr.Username ?? "Unknown";

                        selectedPlayerName = CleanedNAME(nickname, username);
                    }

                    CreateLabel("Player Options: " + selectedPlayerName);

                    if (StoredNowplayerrrr != null)
                    {
                        CreateOptionButton("Open Steam Profile", () =>
                        {
                            if (!string.IsNullOrEmpty(StoredNowplayerrrr.PlatformID.ToString()))
                                CheckSteamID(StoredNowplayerrrr.PlatformID);
                        });

                        CreateOptionButton("Copy Details To Clipboard", () =>
                        {
                            GUIUtility.systemCopyBuffer =
                                $"Nickname : {StoredNowplayerrrr.Nickname ?? "Unknown"}\n" +
                                $"Username : {StoredNowplayerrrr.Username ?? "Unknown"}\n" +
                                $"SteamID : {StoredNowplayerrrr.PlatformID}\n" +
                                $"Description : {StoredNowplayerrrr.Description ?? "None"}\n" +
                                $"Avatar Mod.io #ID : {StoredNowplayerrrr.AvatarModID}";

                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                "Copied player information to clipboard",
                                NotificationType.INFORMATION,
                                2.0f
                            );
                        });

                        CreateOptionButton("Ban/Unban From Your Lobby", () =>
                        {
                            if (string.IsNullOrEmpty(StoredNowplayerrrr.PlatformID.ToString()))
                                return;

                            if (NetworkHelper.IsBanned(StoredNowplayerrrr.PlatformID))
                            {
                                BanManager.Pardon(StoredNowplayerrrr.PlatformID);

                                NotificationNow(
                                    FusionProtectorInfo.ClientName,
                                    "UnBanned Player",
                                    NotificationType.SUCCESS
                                );
                                return;
                            }

                            BanManager.BanList.Bans.RemoveAll(b => b.Player.PlatformID == StoredNowplayerrrr.PlatformID);

                            BanManager.BanList.Bans.Add(new BanInfo
                            {
                                Player = StoredNowplayerrrr,
                                Reason = $"Manually Banned [{FusionProtectorInfo.ClientName}]"
                            });

                            DataSaver.WriteJsonToFile("bans.json", BanManager.BanList);

                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                "Banned Player",
                                NotificationType.SUCCESS
                            );
                        });
                    }

                    break;

                case MenuSections.SelfCat:
                    CreateLabel("Main Options :");

                    CreateOptionButton("Disconnect From Server", () =>
                    {
                        NetworkHelper.Disconnect();
                    });
                    CreateOptionButton("Exit Game", () =>
                    {
                        Application.Quit(0);
                    });
                    CreateOptionButton("Reload Level", () =>
                    {
                        if (!NetworkInfo.IsHost)
                        {
                            NetworkHelper.Disconnect();
                        }
                        SceneStreamer.Reload();

                    });
                    CreateOptionButton("Despawn All [Owner Only]", () =>
                    {
                        DespawnAll(DespawnerAll.NoFilter);
                    });
                    CreateOptionButton("Copy Lobby Details To Clipboard", () =>
                    {
                        GUIUtility.systemCopyBuffer = DataToJsonString(LobbyInfoManager.LobbyInfo);
                        NotificationNowAlways(FusionProtectorInfo.ClientName, "Copied Lobby Info To Clipboard!", NotificationType.SUCCESS, 3.5f);

                    });
                    CreateOptionButton("Reload Assests", () =>
                    {
                        MelonCoroutines.Start(LoadAssetsEnum(randomizerslzonly));
                        
                    });
                    CreateTextbox(ref MODIOINT, "Download MOD.IO #ID : ");
                    CreateOptionButton("Download MOD.IO #ID", () =>
                    {
                        if (int.TryParse(MODIOINT, out var NOWYI))
                        {
                            DownloadModIOMod(NOWYI);
                        }
                    });
                    CreateOptionButton("Teleport To Spawn", () =>
                    {
                        LocalPlayer.TeleportToCheckpoint();
                    });
                    CreateTextbox(ref avichnge, "Change Into Avatar : ");
                    CreateOptionButton("Change Into Avatar", () =>
                    {
                        ChangeIntoAvi(avichnge);
                    });
                    CreateOptionButton("Cancel Notifications", () =>
                    {
                        LabFusion.UI.Popups.Notifier.CancelAll();
                    });



                    break;
                case MenuSections.SpawnLogs:
                    CreateLabel("Lobby Spawn Logs :");

                    CreateTextbox(ref SpawnLogsSearcher, "Lobby Spawn Log Searcher :");
                    foreach (var (PlayerName, Username, PlatformID, PalletName, PalletAuthor, ModioID, BarcodeID, isspawnableavi) in SpawnLogs)
                    {
                        string MODIOID = "";

                        if (ModioID == -1)
                        {
                            MODIOID = "[SLZ]";
                        }
                        else
                        {
                            MODIOID = $"[#ID : {ModioID}]";
                        }

                        if (string.IsNullOrWhiteSpace(SpawnLogsSearcher))
                        {
                            if (!isspawnableavi)
                            {
                                CreateOptionButton($"{PalletName} | {MODIOID}", () =>
                                {
                                    SpawnLogsRef = (PlayerName, Username, PlatformID.ToString(), PalletName, PalletAuthor, ModioID.ToString(), BarcodeID, isspawnableavi.ToString());
                                });
                            }
                        }
                        else
                        {
                            if (PalletName.ToLower().Contains(SpawnLogsSearcher.ToLower()))
                            {
                                if (!isspawnableavi)
                                {
                                    CreateOptionButton($"{PalletName} | {MODIOID}", () =>
                                    {
                                        SpawnLogsRef = (PlayerName, Username, PlatformID.ToString(), PalletName, PalletAuthor, ModioID.ToString(), BarcodeID, isspawnableavi.ToString());
                                    });
                                }
                            }
                        }
                    }
                    AddSection(310);
                    CreateLabel("Avatar Spawnable Logs :");
                    CreateTextbox(ref aviSpawnLogsSearcher, "Avatar Spawnable Logs Searcher :");

                    foreach (var (PlayerName, Username, PlatformID, PalletName, PalletAuthor, ModioID, BarcodeID, isspawnableavi) in SpawnLogs)
                    {
                        string MODIOID = "";

                        if (ModioID == -1)
                        {
                            MODIOID = "[SLZ]";
                        }
                        else
                        {
                            MODIOID = $"[#ID : {ModioID}]";
                        }

                        if (string.IsNullOrWhiteSpace(aviSpawnLogsSearcher))
                        {


                            if (isspawnableavi)
                            {
                                CreateOptionButton($"{PalletName} | {MODIOID}", () =>
                                {
                                    SpawnLogsRef = (PlayerName, Username, PlatformID.ToString(), PalletName, PalletAuthor, ModioID.ToString(), BarcodeID, isspawnableavi.ToString());
                                });
                            }
                        }

                        else
                        {
                            if (PalletName.ToLower().Contains(aviSpawnLogsSearcher.ToLower()))
                            {
                                if (isspawnableavi)
                                {
                                    CreateOptionButton($"{PalletName} | {MODIOID}", () =>
                                    {
                                        SpawnLogsRef = (PlayerName, Username, PlatformID.ToString(), PalletName, PalletAuthor, ModioID.ToString(), BarcodeID, isspawnableavi.ToString());
                                    });
                                }
                            }
                        }

                    }

                    AddSection(310);
                    FreezeScrolling();
                    if (SpawnLogsRef.PlayerName != null)
                    {
                        bloockwarnkickspawnables(new SpawnableCrateReference(SpawnLogsRef.BarcodeID));

                        if (NetworkInfo.IsHost)
                        {


                            CreateLabel($"First Player Who Spawned Username : {SpawnLogsRef.Username}", 600);
                            CreateLabel($"First Player Who Spawned Nickname : {StripColorTags(SpawnLogsRef.PlayerName)}", 600);
                            CreateLabel($"First Player Who Spawned Steam ID : {SpawnLogsRef.PlatformID}", 600);
                        }
                    }
                    else
                    {
                        CreateLabel("Nothing selected", 700);

                    }




                    break;
                case MenuSections.AvatarSwitchLogs:

                    CreateTextbox(ref aviseachnow, "Avatar Switch Logs Searcher");

                    var search = aviseachnow?.Trim().ToLower();

                    foreach (var kvp in PlayeravatarStuff)
                    {
                        string playerId = kvp.Key;
                        var barcodes = kvp.Value;

                        foreach (var barcode in barcodes)
                        {
                            var reference = new AvatarCrateReference(barcode);

                            if (reference?.Crate?.Pallet == null)
                                continue;

                            string crateName = StripColorTags(reference.Crate.name).ToLower() ?? "";
                            string author = reference.Crate.Pallet.Author.ToLower() ?? "";
                            string barcodeLower = barcode?.ToLower() ?? "";

                            bool matchesSearch =
                                string.IsNullOrWhiteSpace(search) ||
                                crateName.ToLower().Contains(search) ||
                                author.ToLower().Contains(search) ||
                                barcodeLower.Contains(search);

                            if (!matchesSearch)
                                continue;

                            if (ulong.TryParse(playerId, out var playsid))
                            {
                                var playerstoredha = NetworkPlayer.Players
                                    .FirstOrDefault(anyx => anyx.PlayerID.PlatformID == playsid);

                                CreateOptionButton($"{crateName} | {author} [{playerId}]", () =>
                                {
                                    AvatarSwitchyNow = (
                                        string.IsNullOrEmpty(playerstoredha?.JR_Nickname()) ? "" : playerstoredha.JR_Nickname(),
                                        string.IsNullOrEmpty(playerstoredha?.JR_Username()) ? "" : playerstoredha.JR_Username(),
                                        playerId,
                                        StripColorTags(crateName),
                                        author,
                                        CrateFilterer.GetModID(reference.Crate.Pallet).ToString(),
                                        barcode
                                    );
                                });
                            }
                        }
                    }


                    AddSection(310);
                    FreezeScrolling();

                    if (AvatarSwitchyNow.PlatformID != null)
                    {
                        if (NetworkInfo.IsHost)
                        {
                            CreateLabel($"First Player Who Spawned Username : {AvatarSwitchyNow.Username}", 600);
                            CreateLabel($"First Player Who Spawned Nickname : {StripColorTags(AvatarSwitchyNow.PlayerName)}", 600);
                            CreateLabel($"First Player Who Spawned Steam ID : {AvatarSwitchyNow.PlatformID}", 600);
                        }
                        CreateOptionButton("Copy Details To Clipboard", () =>
                        {
                            GUIUtility.systemCopyBuffer = $"Spawnable Log Information : {AvatarSwitchyNow.PalletName}\n" +
        $"Player Who Spawned Username : {AvatarSwitchyNow.Username}\n" +
        $"Player Who Spawned Nickname : {AvatarSwitchyNow.PlayerName}\n" +
        $"Mod IO : {AvatarSwitchyNow.ModioID}\n" +
        $"Barcode ID : {AvatarSwitchyNow.BarcodeID}\n" +
        $"Pallet Author : {AvatarSwitchyNow.PalletAuthor}\n" +
        $"Pallet Name : {StripColorTags(AvatarSwitchyNow.PalletName)}\n" +
        $"Player Who Spawned Steam ID : {AvatarSwitchyNow.PlatformID}";
                            NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                        });

                        CreateOptionButton("Copy Barcode To Clipboard", () =>
                        {
                            GUIUtility.systemCopyBuffer = AvatarSwitchyNow.BarcodeID;

                            NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                        });

                        CreateLabel($"Avatar Switch Log Information : {AvatarSwitchyNow.PalletName}", 600);
                        blockavatarfunctions(new AvatarCrateReference(AvatarSwitchyNow.BarcodeID));
                    }
                    else
                    {
                        CreateLabel("Nothing selected", 700);
                    }
                    break;
                case MenuSections.SpawnableSearcher:


                    void SpawnerRESULTS(string switchname)
                    {
                        MelonCoroutines.Start(LoadAssetsEnum(randomizerslzonly,false));
                        SpawnablerResultsNow.Clear();

                        var palletList = AssetWarehouse.Instance?.GetPallets();
                        if (palletList == null) return;

                        foreach (var palletNow in palletList)
                        {
                            if (palletNow?.Crates == null)
                                continue;

                            foreach (var crates in palletNow.Crates)
                            {
                                if (crates == null)
                                    continue;

                                if (!IsMagazine(crates))
                                {
                                    string searchLower = SpawnableSearchies?.ToLower() ?? "";

                                    string crateName = "";
                                    switch (switchname)
                                    {

                                        case "cratename":
                                            crateName = StripColorTags(crates.name?.ToLower()) ?? "";
                                            break;


                                        case "palletname":
                                            crateName = StripColorTags(crates.Pallet.name?.ToLower()) ?? "";
                                            break;


                                        case "barcode":
                                            crateName = StripColorTags(crates.Barcode.ID?.ToLower()) ?? "";
                                            break;

                                    }

                                    if (crateName.Contains(searchLower))
                                    {
                                      SpawnablerResultsNow.Add(crates.Barcode?.ID);
                                    }
                                }
                            }
                        }
                        
                        currentPage = 0;

                    }



                    FreezeScrolling();
                    CreateTextbox(ref SpawnableSearchies, "Spawnable Searcher :");

                    CreateOptionButton("Find Crate Name Results", () =>
                    {
                        SpawnerRESULTS("cratename");
                    });
                    CreateOptionButton("Find Pallet Name Results", () =>
                    {
                        SpawnerRESULTS("palletname");
                    });

                    CreateOptionButton("Find Barcode Name Results", () =>
                    {
                        SpawnerRESULTS("barcode");
                    });

                    CreateLabel("Search Results : " + SpawnablerResultsNow.Count);


                    int totalPages = Mathf.CeilToInt(SpawnablerResultsNow.Count / (float)itemsPerPage);
                    int startIndex = currentPage * itemsPerPage;
                    int endIndex = Mathf.Min(startIndex + itemsPerPage, SpawnablerResultsNow.Count);

                    for (int i = startIndex; i < endIndex; i++)
                    {
                        var barcodeid = SpawnablerResultsNow[i];
                        var spawnyreference = new SpawnableCrateReference(barcodeid);

                        if (spawnyreference?.Crate?.Pallet != null)
                        {
                            string displayName = StripColorTags(spawnyreference.Crate.name) + " | " + spawnyreference.Crate.Pallet.Author;
                            CreateOptionButton(displayName, () =>
                            {
                                serresult = spawnyreference;
                            });
                        }
                    }

                    if (currentPage > 0)
                        CreateOptionButton($"Previous Page", () => { currentPage--; });

                    if (currentPage < totalPages - 1)
                        CreateOptionButton($"Next Page", () => { currentPage++; });

                    AddSection(310);
                    FreezeScrolling();

                    if (serresult?.Crate?.Pallet != null)
                    {
                        bloockwarnkickspawnables(serresult);
                    }
                    else
                    {
                        CreateLabel("Nothing selected", 700);

                    }

                    break;
                case MenuSections.AvatarSearcher:

                    FreezeScrolling();


                    CreateTextbox(ref AvatarSearchies, "Avatar Searcher :");

                    CreateOptionButton("Find Results", () =>
                    {
                        MelonCoroutines.Start(LoadAssetsEnum(randomizerslzonly,false));

                        AvisResultsNow.Clear();

                        if (AvatarsStored != null)
                        {
                            foreach (var avi in AvatarsStored)
                            {
                                if (avi?.Crate?.name == null)
                                    continue;

                                if (StripColorTags(avi.Crate.name).ToLower().Contains(AvatarSearchies.ToLower()))
                                {
                                    AvisResultsNow.Add(avi.Barcode.ID);
                                }
                            }
                        }

                        currentPage2 = 0;
                    });

                    int resultsCount = AvisResultsNow?.Count ?? 0;
                    CreateLabel("Search Results : " + resultsCount);

                    int totalPages2 = Mathf.CeilToInt(resultsCount / (float)itemsPerPage);
                    int startIndex2 = currentPage2 * itemsPerPage;
                    int endIndex2 = Mathf.Min(startIndex2 + itemsPerPage, resultsCount);

                    if (AvisResultsNow != null)
                    {
                        for (int i = startIndex2; i < endIndex2; i++)
                        {
                            string barcodeid = AvisResultsNow[i];
                            if (string.IsNullOrEmpty(barcodeid))
                                continue;

                            var spawnyreference = new AvatarCrateReference(barcodeid);

                            var crate = spawnyreference?.Crate;
                            var pallet = crate?.Pallet;

                            if (crate != null && pallet != null)
                            {
                                string safeName = StripColorTags(crate.name ?? "Unknown Name");
                                string safeAuthor = pallet.Author ?? "Unknown Author";

                                string displayName = safeName + " | " + safeAuthor;

                                CreateOptionButton(displayName, () =>
                                {
                                    avirez = spawnyreference;
                                });
                            }
                        }
                    }

                    if (currentPage2 > 0)
                        CreateOptionButton("Previous Page", () => { currentPage2--; });

                    if (currentPage2 < totalPages2 - 1)
                        CreateOptionButton("Next Page", () => { currentPage2++; });


                    AddSection(310);
                    FreezeScrolling();


                    var selectedCrate = avirez?.Crate;
                    var selectedPallet = selectedCrate?.Pallet;

                    if (selectedCrate?.name != null)
                    {
                        string safeName = StripColorTags(selectedCrate.name);
                        string safeAuthor = selectedPallet?.Author ?? "Unknown Author";
                        string safePalletName = selectedPallet?.name ?? "Unknown Pallet";
                        string barcode = selectedCrate.Barcode?.ID ?? "Unknown ID";

                        CreateLabel($"Selected Avatar Information : {safeName} | {safeAuthor}", 700);
                        blockavatarfunctions(new AvatarCrateReference(barcode));



                    }
                    else
                    {
                        CreateLabel("Nothing selected", 700);
                    }

                    break;
                case MenuSections.CustomAviFav:
                    CreateTextbox(ref avifavsearcher, "Custom Avatar Favorites Searcher :");

                    if (CustomAvFavref != null)
                    {
                        foreach (var avi in CustomAvFavref)
                        {
                            var crate = avi?.Crate;
                            if (crate?.name == null)
                                continue;

                            string name = crate.name;
                            string searchx = avifavsearcher ?? string.Empty;

                            bool matches = string.IsNullOrWhiteSpace(searchx)
                                           || name.Contains(searchx, StringComparison.OrdinalIgnoreCase);

                            if (matches)
                            {
                                CreateOptionButton(StripColorTags(name), () => { avifavorite = avi; });
                            }
                        }
                    }

                    AddSection(310);
                    FreezeScrolling();

                    var selectedAvix = avifavorite;
                    var selectedCratex = selectedAvix?.Crate;
                    var selectedPalletx = selectedCratex?.Pallet;
                    var selectedBarcodex = selectedCratex?.Barcode?.ID;

                    if (selectedCratex != null && selectedPalletx != null)
                    {
                        blockavatarfunctions(new AvatarCrateReference(selectedBarcodex));
                    }
                    else
                    {
                        CreateLabel("Nothing selected", 700);
                    }

                    break;
                case MenuSections.CustomSpawnableFav:

                    CreateTextbox(ref spawnablefavsearcher, "Custom Spawnable Favorites Searcher :");

                    if (CustomSpawnFavref != null)
                    {
                        foreach (var avi in CustomSpawnFavref)
                        {
                            var crate = avi?.Crate;
                            if (crate?.name == null)
                                continue;

                            string name = crate.name;
                            string searchc = spawnablefavsearcher ?? string.Empty;

                            bool matches = string.IsNullOrWhiteSpace(searchc)
                                || name.Contains(searchc, StringComparison.OrdinalIgnoreCase);

                            if (matches)
                            {
                                CreateOptionButton(StripColorTags(name), () =>
                                {
                                    spawnyfavorite = avi;
                                });
                            }
                        }
                    }

                    AddSection(310);
                    FreezeScrolling();

                    var skibbidyPick = spawnyfavorite;

                    if (skibbidyPick?.Crate != null)
                    {
                        bloockwarnkickspawnables(skibbidyPick);
                    }
                    else
                    {
                        CreateLabel("Nothing selected", 700);
                    }

                    break;
                case MenuSections.PlayerInformation:
                    CreateLabel("Players : ");

                    var netPlayers = NetworkPlayers();
                    if (netPlayers != null)
                    {
                        foreach (var player in netPlayers)
                        {
                            if (player == null) continue;

                            CreateOptionButton(player.PlayerID.IsHost ? "[HOST] " + CleanedNAME(player) : CleanedNAME(player), () =>
                            {
                                storeynow = player;
                                var pid = storeynow?.PlayerID;
                                var platformStr = pid?.PlatformID.ToString().Trim();

                                if (NetworkInfo.IsHost)
                                {
                                    playersspawnrefs.Clear();
                              
                                    if (platformStr != null && PlayerSpawningStuff != null && PlayerSpawningStuff.TryGetValue(platformStr, out var barcodes))
                                    {
                                        if (barcodes != null)
                                        {
                                            foreach (var barcode in barcodes)
                                            {
                                                if (barcode != null)
                                                    playersspawnrefs.Add(new SpawnableCrateReference(barcode));
                                            }
                                        }
                                    }
                                }
                                
                                
                                playersavatarrefs.Clear();
                                if (platformStr != null && PlayeravatarStuff != null && PlayeravatarStuff.TryGetValue(platformStr, out var barcodesx))
                                {
                                    if (barcodesx != null)
                                    {
                                        foreach (var barcode in barcodesx)
                                        {
                                            if (barcode != null)
                                                playersavatarrefs.Add(new AvatarCrateReference(barcode));
                                        }
                                    }
                                }
                            });
                        }
                    }

                    AddSection(310);
                    FreezeScrolling();

                    if (storeynow == null)
                    {
                        CreateLabel("No valid player selected.", 700);
                        break;
                    }

                    CreateLabel("Player Options : ");

                    if (NetworkInfo.IsHost)
                    {
                        var steamidnow = storeynow.PlayerID.PlatformID.ToString();
                        
                        CreateLabel(blockedspawnies.Contains(steamidnow) ? "Server Spawn/Despawn Blocked : YES" : "Server Spawn/Despawn Blocked : NO",700);
                        CreateLabel(voicblocked.Contains(steamidnow) ? "Server Muted : YES" : "Server Muted : NO",700);
                        CreateLabel(blockedplatformids.Contains(steamidnow)? "Server Damage Blocked : YES": "Server Damage Blocked : NO",700);
                        CreateLabel(blockmessages.Contains(steamidnow)? "Server Block Players Messaging : YES" : "Server Block Players Messaging : NO", 700);
                        CreateLabel(blockmovements.Contains(steamidnow) ? "Server Disable Movement Syncing : YES" : "Server Disable Movement Syncing : NO", 700);

                        CreateOptionButton("Add/Remove To Server Block Player Messaging", () =>
                        {
                            var steamStr = storeynow?.JR_SteamID().ToString();
                            if (steamStr != null)
                            {
                                ToggleAddRemoveFromFile(steamStr, blockmessages, blockmessagingnowpath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(storeynow)} To Block Player Messaging [Server]!.", $"Removed {CleanedNAME(storeynow)} From Block Player Messaging [Server]!.", true);

                            }
                        });
                        CreateOptionButton("Add/Remove To Server Voice Blocker", () =>
                        {
                            var steamStr = storeynow?.JR_SteamID().ToString();
                            if (steamStr != null)
                            {
                                ToggleAddRemoveFromFile(steamStr, voicblocked, voicepathblocked, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(storeynow)} To Voice Blocker [Server]!.", $"Removed {CleanedNAME(storeynow)} From Voice Blocker [Server]!.", true);

                            }
                        });
                        CreateOptionButton("Add/Remove To Server Damage Blocker", () =>
                        {
                            var steamStr = storeynow?.JR_SteamID().ToString();
                            if (steamStr != null)
                            {
                                ToggleAddRemoveFromFile(steamStr, blockedplatformids, DamageBlockPath, FusionProtectorInfo.ClientName,
                                    $"Added {SpawnLogsRef.BarcodeID} To Damage Blocker!.",
                                    $"Removed {SpawnLogsRef.BarcodeID} From Damage Blocker!.", true);
                            }
                        });
                        CreateOptionButton("Add/Remove To Server Spawn/Despawn Blocker", () =>
                        {
                            var steamStr = storeynow?.JR_SteamID().ToString();
                            if (steamStr != null)
                            {
                                ToggleAddRemoveFromFile(steamStr, blockedspawnies, ServerBlockSpawnPath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(storeynow)} To Server Spawn/Despawn Blocker [Server]!.", $"Removed {CleanedNAME(storeynow)} From Server Spawn/Despawn Blocker [Server]!.", true);

                            }
                        });
                        CreateOptionButton("Add/Remove To Server Disable Movement Syncing", () =>
                        {
                            var steamStr = storeynow?.JR_SteamID().ToString();
                            if (steamStr != null)
                            {
                                ToggleAddRemoveFromFile(steamStr, blockmovements, BlockMovementsPath, FusionProtectorInfo.ClientName, $"Added {CleanedNAME(storeynow)} To Server Disable Movement Syncing [Server]!.", $"Removed {CleanedNAME(storeynow)} From Server Disable Movement Syncing [Server]!.", true);
                            }
                        });

                    }

                    FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var permyXc, out _);
                    if (FusionPermissions.HasSufficientPermissions(permyXc, PermissionLevel.OWNER))
                    {
                        CreateOptionButton("[Owner] Clear Constraints", () =>
                        {
                            if (storeynow != null)
                                ClearConstraints(storeynow);
                        });
                    }

                    CreateOptionButton("Add/Remove Avatar To Avatar Blocker", () =>
                    {
                        if (storeynow?.JR_PlayersAvatarBarcodeID() != "c3534c5a-94b2-40a4-912a-24a8506f6c79")
                        {

                            ToggleAddRemoveFromFile(storeynow?.JR_PlayersAvatarBarcodeID(), blockedavifallbacks, avatarsblocked, FusionProtectorInfo.ClientName, $"Added {storeynow?.JR_PlayersAvatarBarcodeID().JR_BarcodeCrateName()} To Avatar Blocker!.", $"Removed {storeynow?.JR_PlayersAvatarBarcodeID().JR_BarcodeCrateName()} From Avatar Blocker!.");
                        }
                        else
                        {
                            NotificationNow(FusionProtectorInfo.ClientName, "Can't Do That!", NotificationType.ERROR, 2.5f);
                        }
                    });


                    CreateOptionButton("Clone Avatar", () =>
                    {
                        var avi = storeynow?.JR_PlayersAvatarBarcodeID();
                        if (avi != null)
                            ChangeIntoAvi(avi);
                    });





                    CreateOptionButton("Copy Avatar Details", () =>
                    {
                        var avi = storeynow?.JR_PlayersAvatarBarcodeID();
                        if (avi != null)
                        {
                            var spawnableavi = new SpawnableCrateReference(avi);
                            GUIUtility.systemCopyBuffer = $"Barcode : {spawnableavi?.Barcode?.ID}\nCrate : {spawnableavi?.Crate?.name} Author [{spawnableavi?.Crate?.Pallet?.Author}]\nPallet It's In : {spawnableavi?.Crate?.Pallet?.name}";
                        }
                    });

                    CreateOptionButton("Local Protection Avi", () =>
                    {
                        var rm = storeynow?.RigRefs?.RigManager;
                        if (rm != null)
                        {
                            rm.SwapAvatarCrate(new Barcode("c3534c5a-94b2-40a4-912a-24a8506f6c79"));
                            if (storeynow?.MarrowEntity != null)
                                SpawnEffects.CallDespawnEffect(storeynow.MarrowEntity);
                        }
                    });

                    CreateOptionButton("UnBan/Ban From Your Lobbies", () =>
                    {
                        var pid = storeynow?.PlayerID;
                        if (pid == null) return;

                        if (!NetworkHelper.IsBanned(pid.PlatformID))
                        {
                            BanInfo item = new()
                            {
                                Player = new PlayerInfo
                                {
                                    Username = pid.Metadata?.Username?.GetValue(),
                                    Nickname = pid.Metadata?.Nickname?.GetValue(),
                                    PlatformID = pid.PlatformID,
                                    Description = pid.Metadata?.Description?.GetValue(),
                                    AvatarModID = pid.Metadata.AvatarModID.GetValue(),
                                    AvatarTitle = pid.Metadata?.AvatarTitle?.GetValue()
                                },
                                Reason = $"Manually Banned [{FusionProtectorInfo.ClientName}]"
                            };

                            if (BanManager.BanList?.Bans != null)
                            {
                                BanManager.BanList.Bans.RemoveAll(info2 => info2?.Player?.PlatformID == pid.PlatformID);
                                BanManager.BanList.Bans.Add(item);
                                DataSaver.WriteJsonToFile("bans.json", BanManager.BanList);
                            }
                            NotificationNow(FusionProtectorInfo.ClientName, "Banned Player", NotificationType.SUCCESS);
                        }
                        else
                        {
                            BanManager.Pardon(pid.PlatformID);
                            NotificationNow(FusionProtectorInfo.ClientName, "UnBanned Player", NotificationType.SUCCESS);
                        }
                    });

                    CreateOptionButton("Copy All Profile To Clipboard", () =>
                    {
                        if (storeynow != null)
                        {


                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };

                            string nowplayerinfo = JsonSerializer.Serialize(storeynow.PlayerID, options);
                            GUIUtility.systemCopyBuffer = nowplayerinfo;
                            NotificationNow(FusionProtectorInfo.ClientName, "Copied Players Entire Details To Clipboard", NotificationType.SUCCESS);

                        }
                    });

                    CreateOptionButton("Copy Steam ID", () =>
                    {
                        var sid = storeynow?.PlayerID?.PlatformID;
                        if (sid != null)
                        {
                            GUIUtility.systemCopyBuffer = sid.ToString();
                            NotificationNow(FusionProtectorInfo.ClientName, "Copied Steam ID", NotificationType.SUCCESS);
                        }
                    });

                    CreateOptionButton("Open Steam Profile", () =>
                    {
                        CheckSteamID(storeynow.PlayerID.PlatformID);
                    });

                    var stats = storeynow?.JR_SerializedAvatarStats();
                    if (stats != null)
                    {
                        CreateLabel("Current Health : " + storeynow.RigRefs.Health.curr_Health.ToString("F0") + "/"+ storeynow.RigRefs.Health.max_Health.ToString("F0"));
                        CreateLabel("Current Death Time : " + storeynow.RigRefs.Health.currDeathTime.ToString("F0"));
                    }
                    AddSection(310);

                    string playerInfoHeader = (string.IsNullOrWhiteSpace(storeynow.JR_Nickname())
                        ? storeynow.JR_Username()
                        : StripColorTags(storeynow.JR_Nickname()) + " | " + storeynow.JR_Username()) ?? "Player";

                    if (storeynow.PlayerID?.IsValid == true)
                    {
                        CreateLabel(playerInfoHeader + " Information : ");

                        testholder[0] = storeynow.JR_PlayersAvatarBarcodeID() ?? "";
                        CreateTextbox(ref testholder[0], "Players Avatar : ", 35);
                        blockavatarfunctions(new AvatarCrateReference(storeynow.JR_PlayersAvatarBarcodeID()));


                        testholder[1] = storeynow.JR_SteamID().ToString() ?? "";
                        CreateTextbox(ref testholder[1], "Steam ID : ", 35);

                        testholder[2] = storeynow.JR_GetHand(WhichHand.Left)?.JR_GetMarrowEntity()?.JR_GetBarcodeID() ?? "";
                        CreateTextbox(ref testholder[2], "Left Handed Item : ", 35);
                        bloockwarnkickspawnables(new SpawnableCrateReference(storeynow.JR_GetHand(WhichHand.Left)?.JR_GetMarrowEntity()?.JR_GetBarcodeID()));
                        testholder[3] = storeynow.JR_GetHand(WhichHand.Right)?.JR_GetMarrowEntity()?.JR_GetBarcodeID() ?? "";
                        CreateTextbox(ref testholder[3], "Right Handed Item : ", 35);
                        bloockwarnkickspawnables(new SpawnableCrateReference(storeynow.JR_GetHand(WhichHand.Right)?.JR_GetMarrowEntity()?.JR_GetBarcodeID()));
                    }

                    AddSection(310);


                    if (NetworkInfo.IsHost)
                    {
                        CreateLabel(playerInfoHeader + " Spawns : ");

                        if (playersspawnrefs.Count == 0)
                        {
                            CreateLabel("No spawnables found for this player.", 700);
                        }
                        else
                        {
                            playerinfospawnlogs = playerinfospawnlogs ?? "";
                            CreateTextbox(ref playerinfospawnlogs, "Search Spawn Logs : ", 35);

                            foreach (var barcode in playersspawnrefs)
                            {
                                if (barcode?.Crate == null) continue;

                                string cname = StripColorTags(barcode.Crate.name) ?? "";
                                if (cname.ToLower().Contains(playerinfospawnlogs.ToLower()))
                                {
                                    CreateOptionButton(cname + " | " + (barcode.Crate.Pallet?.Author ?? "Unknown"), () =>
                                    {
                                        playersspawnrefsstored = barcode;
                                    });
                                }
                            }
                        }

                        AddSection(310);
                        FreezeScrolling();

                        if (playersspawnrefsstored?.Crate != null)
                        {
                            bloockwarnkickspawnables(playersspawnrefsstored);
                        }
                        else
                        {
                            CreateLabel("Nothing selected", 700);
                        }

                        AddSection(310);
                        FreezeScrolling();
                    }


                    CreateLabel(playerInfoHeader + " Avatar Switch Logs : ");

                    if (playersavatarrefs.Count == 0)
                    {
                        CreateLabel("No avatars found for this player.", 700);
                    }
                    else
                    {
                        playerinfoavatarswitch = playerinfoavatarswitch ?? "";
                        CreateTextbox(ref playerinfoavatarswitch, "Search Avatar Switch Logs : ", 35);

                        foreach (var barcode in playersavatarrefs)
                        {
                            if (barcode?.Crate == null) continue;

                            string cname = StripColorTags(barcode.Crate.name) ?? "";
                            if (cname.ToLower().Contains(playerinfoavatarswitch.ToLower()))
                            {
                                CreateOptionButton(cname + " | " + (barcode.Crate.Pallet?.Author ?? "Unknown"), () =>
                                {
                                    playersavatarrefsstored = barcode;
                                });
                            }
                        }
                    }

                    AddSection(310);
                    FreezeScrolling();

                    if (playersavatarrefsstored?.Crate != null)
                    {
                        var crate = playersavatarrefsstored.Crate;
                        CreateLabel("Selected Avatar Information : " + StripColorTags(crate.name ?? "") + " | " + (crate.Pallet?.Author ?? "Unknown"), 700);

                        CreateLabel($"Mod IO : {CrateFilterer.GetModID(crate.Pallet)}", 600);
                        CreateLabel($"Barcode ID : {crate.Barcode?.ID}", 600);
                        CreateLabel($"Pallet Author : {crate.Pallet?.Author}", 600);
                        CreateLabel($"Pallet Name : {crate.Pallet?.name}", 600);

                        CreateOptionButton("Copy Details To Clipboard", () =>
                        {
                            GUIUtility.systemCopyBuffer = $"Avatar Information : {crate.Pallet?.name}\nMod IO : {crate.Barcode?.ID}\nBarcode ID : {crate.Barcode?.ID}\nPallet Author : {crate.Pallet?.Author}\nPallet Name : {crate.Pallet?.name}";
                            NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                        });
                        CreateOptionButton("Copy Barcode To Clipboard", () =>
                        {
                            GUIUtility.systemCopyBuffer = crate.Barcode?.ID;

                            NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                        });
                        CreateOptionButton("Change Into Avatar", () =>
                        {
                            var id = crate.Barcode?.ID;
                            if (id != null)
                                ChangeIntoAvi(id);
                        });

                        CreateOptionButton("Add/Remove Kick If Changed Into", () =>
                        {
                            var id = crate.Barcode?.ID;
                            if (id != null)
                                ToggleAddRemoveFromFile(id, AvatarsKick, AvatarsKickPath, FusionProtectorInfo.ClientName,
                                    $"Added {id} To Kick If Changed Into!.",
                                    $"Removed {id} From Kick If Changed Into!.");
                        });

                        CreateOptionButton("Add/Remove Custom Favorites [Avatar]", () =>
                        {
                            var id = crate.Barcode?.ID;
                            if (id != null)
                                ToggleAddRemoveFromFile(id, CustomAvFav, AvatarCustomFav, FusionProtectorInfo.ClientName,
                                    $"Added {id} To Custom Avatar Favorites!.",
                                    $"Removed {id} From Custom Avatar Favorites!.");
                        });
                    }
                    else
                    {
                        CreateLabel("Nothing selected", 700);
                    }

                    break;
                case MenuSections.SceneEntities:

                    FreezeScrolling();
                    CreateLabel("Network Entities : " + ListNetworkEntities.Count);

                    CreateTextbox(ref netentities, "Network Entities Searcher :");

                    var filteredEntities = NetworkEntities()
                        .Where(e =>
                        {
                            var r = new SpawnableCrateReference(e.JR_GetMarrowEntity().JR_GetBarcodeID());
                            return r?.Crate?.Pallet != null &&
                                   StripColorTags(r.Crate.name).ToLower().Contains(netentities.ToLower());
                        })
                        .ToList();

                    int totalPages3 = Mathf.CeilToInt(filteredEntities.Count / (float)itemsPerPage);
                    int startIndex3 = currentPage3 * itemsPerPage;
                    int endIndex3 = Mathf.Min(startIndex3 + itemsPerPage, filteredEntities.Count);

                    for (int i = startIndex3; i < endIndex3; i++)
                    {
                        var barcodeid = filteredEntities[i];
                        var spawnyreference = new SpawnableCrateReference(barcodeid.JR_GetMarrowEntity().JR_GetBarcodeID());

                        string displayName = StripColorTags(spawnyreference.Crate.name) + " | " + spawnyreference.Crate.Pallet.Author;
                        CreateOptionButton(displayName, () =>
                        {
                            nettyspawnedc = barcodeid;
                        });
                    }

                    if (currentPage3 > 0)
                        CreateOptionButton("Previous Page", () => { currentPage3--; });

                    if (currentPage3 < totalPages3 - 1)
                        CreateOptionButton("Next Page", () => { currentPage3++; });

                    AddSection(310);
                    FreezeScrolling();

                    CreateLabel("Network Entity Options : ");


                    if (nettyspawnedc.JR_GetMarrowEntity() != null)
                    {
                        CreateOptionButton("Despawn This", () =>
                        {
                            DespawnNow(nettyspawnedc);
                        });

                        CreateOptionButton("Force Delete This", () =>
                        {

                            var marrow = nettyspawnedc?.JR_GetMarrowEntity();
                            if (marrow != null)
                            {
                                marrow.gameObject.DestroyNow();
                                NotificationNow(FusionProtectorInfo.ClientName, $"Force Deleted Locally {marrow.JR_GetBarcodeID().JR_BarcodeCrateName()}", NotificationType.SUCCESS, 3.5f);
                            }

                        });



                        CreateOptionButton("Spawn This", () =>
                        {
                            SpawnIt(nettyspawnedc.JR_GetMarrowEntity().JR_GetBarcodeID(), JR_YourGetHand(WhichHand.Left).transform.position + JR_YourGetHand(WhichHand.Left).transform.forward + JR_YourGetHand(WhichHand.Left).transform.up, Quaternion.identity);
                        });



                        AddSection(310);
                        FreezeScrolling();

                        bloockwarnkickspawnables(new SpawnableCrateReference(nettyspawnedc.JR_GetMarrowEntity().JR_GetBarcodeID()));
                    }

                    break;

                case MenuSections.PlayerSeriStats:
                    CreateLabel("Players Avatar Stats Information :");
                  
                        foreach (var playei in NetworkPlayers())
                        {
                            CreateOptionButton(CleanedNAME(playei), () =>
                            {
                                storedplz = playei;
                            });
                        }

  if (storedplz !=null)
                    {
                        foreach (var field in storedplz.JR_SerializedAvatarStats().GetType().GetFields())
                        {
                            if (field.FieldType == typeof(float))
                            {
                                float value = (float)field.GetValue(storedplz.JR_SerializedAvatarStats());

                                CreateLabel(field.Name + " :" + value);

                            }
                        }
                    }
                    break;


                case MenuSections.DamageLogger:
                    CreateLabel("Damage Information :");

                    CreateTextbox(ref damagelogsearcher, "Damage Information Searcher :");

                    foreach (var kvp in ProtectionFromClients.PlayerDamageLogs)
                    {
                        var attackerId = kvp.Key;
                        var damageList = kvp.Value;

                        if (!NetworkPlayerManager.TryGetPlayer(attackerId, out var attackerPlayer) || attackerPlayer == null)
                            continue;

                        string attackerName = StripColorTags(CleanedNAME(attackerPlayer.JR_Nickname(), attackerPlayer.JR_Username())).ToLower();
                        string searchTerm = damagelogsearcher.ToLower();

                        if (!attackerName.Contains(searchTerm))
                            continue;

                        // Group the damage by damage value
                        var grouped = damageList
                            .Where(d => d?.Attack?.Attack != null)
                            .GroupBy(d => d.Attack.Attack.damage);

                        foreach (var group in grouped)
                        {
                            var damageValue = group.Key;
                            var count = group.Count();

                            CreateLabel(
                                $"Attacker: {StripColorTags(CleanedNAME(attackerPlayer.JR_Nickname(), attackerPlayer.JR_Username()))} | " +
                                $"Damage: {damageValue} x{count}",
                                1000
                            );
                        }
                    }
                    break;



                case MenuSections.NetworkLogger:
                    CreateLabel("Network Message Logger :");

                    foreach (var (player, logs) in despawnresponselogger)
                    {
                        CreateLabel(
                            $"Despawn Request/Response Message Called : " +
                            $"{CleanedNAME(player.Metadata.Nickname.GetValue(), player.Metadata.Username.GetValue())} ({player.PlatformID}) " +
                            $"x{logs.Count}",
                            2000
                        );
                    }

                    break;
                //add onto this
                case MenuSections.ServerHistory:

                    if (ServerHistorys != null)
                    {
                        foreach (var lobbyInfoNow in ServerHistorys)
                        {
                            if (lobbyInfoNow == null || string.IsNullOrWhiteSpace(lobbyInfoNow.LobbyCode)) continue; // skip null entries

                            var serverName = string.IsNullOrWhiteSpace(lobbyInfoNow.LobbyName)
    ? $"{lobbyInfoNow.LobbyHostName}'s Server"
    : lobbyInfoNow.LobbyName;

                            CreateOptionButton(serverName, () =>
                            {
                                storedlobby = lobbyInfoNow;
                            });
                        }
                    }

                    AddSection(310);
                    FreezeScrolling();

                    CreateLabel("Lobby History Information : ");

                    ServerInfoNow(storedlobby);
                    break;
                case MenuSections.FusionBanManager:

                    CreateLabel("Fusion Ban Manager : ");

                    CreateOptionButton("Export Ban List To Clipboard", () =>
                    {
                        var settings = new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            FloatFormatHandling = FloatFormatHandling.Symbol,
                            Culture = CultureInfo.InvariantCulture,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        };

                        GUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(BanManager.BanList.Bans, settings);
                        NotificationNow(FusionProtectorInfo.ClientName, $"Exported Ban List To Clipboard", NotificationType.WARNING, 3.5f);
                    });
                    CreateOptionButton("Import Bans From Clipboard", () =>
                    {
                        var json = GUIUtility.systemCopyBuffer;
                        if (string.IsNullOrEmpty(json))
                            return;

                        var importedList = JsonConvert.DeserializeObject<BanList>(json);
                        if (importedList?.Bans == null || importedList.Bans.Count == 0)
                            return;

                        var existingIds = new System.Collections.Generic.HashSet<ulong>();
                        foreach (var ban in BanManager.BanList.Bans)
                            existingIds.Add(ban.Player.PlatformID);

                        var counter = 0;

                        foreach (var imported in importedList.Bans)
                        {
                            if (imported?.Player == null)
                                continue;

                            if (existingIds.Add(imported.Player.PlatformID))
                            {
                                BanManager.BanList.Bans.Add(imported);
                                counter++;
                            }
                        }

                        NotificationNow(
                            FusionProtectorInfo.ClientName,
                            $"Imported New Bans [{counter}]",
                            NotificationType.WARNING,
                            3.5f
                        );

                        DataSaver.WriteJsonToFile("bans.json", BanManager.BanList);
                    });

                    CreateLabel("Bans Count :" + BanManager.BanList.Bans.Count.ToString());

                    CreateTextbox(ref bansearcher, "Bans Searcher :");

                    foreach (var ban in BanManager.BanList.Bans)
                    {
                        var nickname2 = StripColorTags(ban.Player.Nickname);
                        var username2 = ban.Player.Username;


                        if (string.IsNullOrWhiteSpace(bansearcher.ToLower()))
                        {

                            CreateOptionButton(
                                    string.IsNullOrWhiteSpace(nickname2)
                                        ? username2
                                        : nickname2 + " | " + username2,
                                    () =>
                                    {
                                        storedban = ban;
                                    });
                        }

                        else
                        {
                            if (username2.ToLower().Contains(bansearcher.ToLower()) || nickname2.ToLower().Contains(bansearcher.ToLower()))
                            {
                                CreateOptionButton(
                                 string.IsNullOrWhiteSpace(nickname2)
                                     ? username2
                                     : nickname2 + " | " + username2,
                                 () =>
                                 {
                                     storedban = ban;
                                 });
                            }
                        }

                    }

                    AddSection(310);

                    if (storedban != null)
                    {
                        var nickname = StripColorTags(storedban.Player.Nickname);
                        var username = storedban.Player.Username;

                        var labelText = string.IsNullOrWhiteSpace(nickname)
                            ? $"Selected Player : {username}"
                            : $"Selected Player : {nickname} | {username}";

                        CreateLabel(labelText);

                        CreateOptionButton("UN-BAN", () =>
                        {
                            NetworkHelper.PardonUser(storedban.Player.PlatformID);
                        });

                        CreateOptionButton("Open Steam Page", () =>
                        {
                            CheckSteamID(storedban.Player.PlatformID);
                        });


                        CreateOptionButton("Copy Ban Details To Clipboard", () =>
                        {
                            var settings = new JsonSerializerSettings
                            {
                                Formatting = Formatting.Indented,
                                FloatFormatHandling = FloatFormatHandling.Symbol,
                                Culture = CultureInfo.InvariantCulture,
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            };

                            GUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(storedban, settings);
                            NotificationNow(FusionProtectorInfo.ClientName, $"Copied Ban Info To Clipboard For Player {labelText}", NotificationType.WARNING, 3.5f);
                        });

                    }
                    else
                    {

                        CreateLabel("Nothing");
                    }
                    break;
                case MenuSections.DownloadLogger:
                    {
                        CreateLabel("Mod.IO Download Logger : ");

                        if (LogDownloads.DownloadLogger == null)
                            break;

                        foreach (var entry in LogDownloads.DownloadLogger)
                        {
                            LobbyInfo lobby = entry.Key;
                            System.Collections.Generic.List<Pallet> pallets = entry.Value;

                            foreach (var pallet in pallets)
                            {
                                if (pallet == null)
                                    continue;

                                var title = string.IsNullOrWhiteSpace(pallet.Title)
                                    ? "Unknown Mod"
                                    : StripColorTags(pallet.Title);

                                var modId = CrateFilterer.GetModID(pallet);

                                var capturedPallet = pallet;
                                var capturedLobby = lobby;

                                CreateOptionButton($"{title} | Mod.IO #ID {modId}",() =>{
                                        modiostorednow = capturedPallet;
                                        lobbyinfofrominstall = capturedLobby;
                                        //OpenPageNow($"https://mod.io/search/mods/{modId}");
                                    }
                                );
                            }
                        }


                        AddSection(310);
                        CreateLabel("Mod.IO Options : "+ modiostorednow.name);
                        var id = modiostorednow.Barcode;
                        var palletname = StripColorTags(modiostorednow.name);
                        var palletauthor = modiostorednow.Author;
                        CreateLabel($"Barcode ID : {id}", 600);
                        CreateLabel($"Pallet Author : {palletauthor}", 600);
                        CreateLabel($"Pallet Name : {palletname}", 600);
                  
                        CreateLabel(modidblocked != null && modidblocked.Contains(CrateFilterer.GetModID(modiostorednow).ToString()) ? "Blocked Mod.IO Mod : YES" : "Blocked Mod.IO Mod : NO");
                        CreateLabel(blockentirepallet != null && blockentirepallet.Contains(modiostorednow.name) ? "Blocked Pallet : YES" : "Blocked Pallet : NO");

                        CreateOptionButton("Copy Details To Clipboard", () =>
                        {
                            GUIUtility.systemCopyBuffer =
                                $"Spawnable Searcher Information : {modiostorednow?.name}\n" +
                                $"Mod IO : {CrateFilterer.GetModID(modiostorednow)}\n" +
                                $"Pallet Author : {modiostorednow?.Author}\n" +
                                $"Pallet Name : {modiostorednow?.name}";

                            NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                        });
                        CreateOptionButton("Copy Barcode To Clipboard", () =>
                        {
                            GUIUtility.systemCopyBuffer = id.ID;

                            NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
                        });

                        CreateOptionButton("Open Mod Download Folder", () =>
                        {
                            GetPalletFolder(modiostorednow?.name,true);

                        });

                        CreateOptionButton($"Open Page Mod.IO #ID {CrateFilterer.GetModID(modiostorednow)}",() =>
                        {

                            OpenPageNow($"https://mod.io/search/mods/{CrateFilterer.GetModID(modiostorednow)}");

                        });

                        CreateOptionButton("Add/Remove Block Pallet Completely", () =>
                        {
                            ToggleAddRemoveFromFile(modiostorednow.name, blockentirepallet, blockpalletnowlist, FusionProtectorInfo.ClientName, $"Added {modiostorednow.name} To Blocked Pallets!.", $"Removed {modiostorednow.name} From Blocked Pallets!.", true);
                        });

                        CreateOptionButton("Add/Remove Block This Mod.IO Mod Completely", () =>
                        {
                            ToggleAddRemoveFromFile(CrateFilterer.GetModID(modiostorednow).ToString(), modidblocked, ModIDBLOCKSPATH, FusionProtectorInfo.ClientName, $"Added {modiostorednow.name} To Blocked Mod.IO Mod!.", $"Removed {modiostorednow.name} From Blocked Mod.IO Mod!.", true);
                        });

                        CreateOptionButton("Delete Mod Completely", () =>
                        {
                            DeleteModioMod(modiostorednow);
                        });

                        if (lobbyinfofrominstall != null)
                        {
                            AddSection(310);
                            CreateLabel("Mod.IO Downloads In Server Information : " + modiostorednow.name);

                            ServerInfoNow(lobbyinfofrominstall);


                            LogDownloads.DownloadLogger.TryGetValue(lobbyinfofrominstall, out var pall);

                            CreateLabel("Mod.IO Downloaded In Server Information : " + pall.Count.ToString());

                            CreateOptionButton("Delete All Mods From Server", () =>
                            {
                                foreach (var entry in LogDownloads.DownloadLogger)
                                {
                                    LobbyInfo lobby = entry.Key;


                                    if (lobby == lobbyinfofrominstall)
                                    {
                                        System.Collections.Generic.List<Pallet> pallets = entry.Value;

                                        foreach (var pallet in pallets)
                                        {
                                            DeleteModioMod(pallet,false);
                                        }
                                        NotificationNow(FusionProtectorInfo.ClientName, $"Deleted {pallets.Count.ToString()} Mods From This Server!", NotificationType.SUCCESS, 6.5f);

                                    }

                                }

                            });
                        }


                    }
                    break;
                case MenuSections.MediaPlayerLogger:
                 CreateLabel("Media Player Logger : " + MediaPlayerLogs.Count.ToString());

                    foreach (var medialink in MediaPlayerLogs)
                    {
                        CreateOptionButton(medialink, () =>
                        {
                            storedmedia = medialink;
                        });
                    }
                    if (!string.IsNullOrEmpty(storedmedia))
                    {
                        AddSection(310);
                        CreateLabel("Media Player Logger Options : " + storedmedia);

                        CreateLabel(MEDIAPLAYERBLOCKERNOWList != null && MEDIAPLAYERBLOCKERNOWList.Contains(storedmedia) ? "Media Player Blocked : YES" : "Media Player Blocked : NO");

                        CreateOptionButton("Add/Remove Media Player Blocker", () =>
                        {
                            ToggleAddRemoveFromFile(storedmedia, MEDIAPLAYERBLOCKERNOWList, MEDIAPLAYERBLOCKERNOW, FusionProtectorInfo.ClientName, $"Added {storedmedia} To Media Player Blocker!.", $"Removed {storedmedia} From Media Player Blocker!.", true);
                        });

                        CreateOptionButton("Copy Link To Clipboard", () =>
                        {
                            GUIUtility.systemCopyBuffer = storedmedia;
                            NotificationNow(FusionProtectorInfo.ClientName, $"Copied {storedmedia} To Clipboard!", NotificationType.SUCCESS, 2f);
                        });

                    }


                    break;
            
            
            
            
            }

        }
        public override void OnUpdate()
        {
            if (blockentireauthornow != null)
            {
                if (NetworkInfo.HasLayer)
                {
                    if (Input.GetKeyDown(KeyCode.Y))
                        show = !show;
                }

                if (!HelperMethods.IsLoading() && RigData.HasPlayer)
                {

                    if (spawngunuialways)
                    {
                        if (!spawngunatleastonce && (JR_YourGetHand(WhichHand.Left).JR_HandGrabbedSpawnGun() || JR_YourGetHand(WhichHand.Right).JR_HandGrabbedSpawnGun()))
                        {
                            spawngunatleastonce = true;
                        }

                        if (spawngunatleastonce)
                        {
                            UIRig.Instance.popUpMenu.AddSpawnMenu();
                        }
                    }

                    if (preventnotificationlag && MenuNotifications.SavedNotifications.Count > 400)
                    {
                        MenuNotifications.ClearNotifications();
                        LabFusion.UI.Popups.Notifier.Cancel(FusionProtectorInfo.ClientName);
                        NotificationNow(FusionProtectorInfo.ClientName, "Cleared Notifications To Prevent Lag!", NotificationType.WARNING, 2.5f, true, true);

                    }


                    foreach (var play in NetworkPlayers())
                    {
                        if (play.PlayerID.Metadata.Loading.GetValue())
                            continue;

                        var stored = StoredJoinPlayers
                            .FirstOrDefault(pml => pml.PlatformID == play.JR_SteamID());

                        if (stored == null)
                            continue;

                        string currentUsername = play.JR_Username() ?? "Unknown";
                        ulong platformID = stored.PlatformID;

                        if (LastKnownUsernames.TryGetValue(platformID, out var lastUsername))
                        {
                            if (lastUsername != currentUsername)
                            {
                                if (spoofedusernameusername)
                                {
                                    NotificationNowAlways(FusionProtectorInfo.ClientName, $"Person Doing : {CleanedNAME(play)}\n[Spoofed Username] '{lastUsername}' -> '{currentUsername}'", NotificationType.WARNING, 3.5f, true, true, () => { CheckSteamID(platformID); });
                                }
                                LastKnownUsernames[platformID] = currentUsername;

                                if (AutoKickSpoofers)
                                {
                                    FusionPermissions.FetchPermissionLevel(SteamIdYours(), out var selfLevel, out _);
                                    if (FusionPermissions.HasSufficientPermissions(selfLevel, LobbyInfoManager.LobbyInfo.Kicking))
                                    {
                                        NetworkHelper.KickUser(play.PlayerID);
                                    }
                                }
                            }
                        }
                        else
                        {
                            LastKnownUsernames[platformID] = currentUsername;
                        }
                    }

                    if (showammoalways)
                    {
                        Player.UIRig.uiHud.SHOWAMMO(UI_HUD.AmmoDisplayLocation.Head);
                        Player.UIRig.uiHud.HEADHUDFOLLOW(true);
                    }

                    if (bodylog)
                    {
                        JR_BodyLog(Player.PhysicsRig).bodylogreturn?.ballArt.gameObject.SetActive(false);
                        JR_BodyLog(Player.PhysicsRig).Outerring.gameObject.SetActive(false);
                    }


                    if (!GamemodeManager.IsGamemodeStarted)
                    {



                        if (personalspace)
                        {
                            foreach (var player in NetworkPlayers(true))
                            {
                                var avatar = player?.RigRefs?.RigManager?.avatar?.gameObject;
                                var feet = Player.PhysicsRig?.m_head?.transform;

                                avatar?.SetActive(!(avatar?.IsTooClose(feet, personalspacevalue) ?? false));
                            }
                        }

                        if (autorunnow)
                        {
                            InvokeWithType(typeof(SmashBones), "ApplyAutoRun", new object[] { Player.RigManager });
                        }



                        if (NetworkInfo.IsHost)
                        {
                            if (AntiDevManip)
                            {
                                foreach (var devmanippy in NetworkEntities())
                                {
                                    var entity = devmanippy?.JR_GetMarrowEntity();
                                    if (entity?.JR_GetBarcodeID() == "c1534c5a-c6a8-45d0-aaa2-2c954465764d")
                                    {
                                        DespawnNow(devmanippy);
                                    }
                                }
                            }

                            if (AntiLasereyes)
                            {
                                foreach (var laser in NetworkEntities())
                                {
                                    if (laser.JR_GetMarrowEntity().JR_GetBarcodeID() == "BamBaeYoh.LaserEyes.Spawnable.LaserEyes")
                                    {
                                        DespawnNow(laser);
                                    }
                                }
                            }
                        }


                        // Owner-only stuff
                        if (AreYouOWNER())
                        {
                            if (despawndeadnpcs)
                            {
                                foreach (var entity in NetworkEntities())
                                {
                                    if (entity?.JR_IsNPC() == true && entity?.JR_GetNPCAIBrain()?.isDead == true)
                                    {
                                        entity?.JR_Despawn();
                                    }
                                }
                            }

                            if (fpsdesapwner && fps <= fpslimit && !fpsCheckRunning)
                            {
                                MelonCoroutines.Start(FPSCheck());
                            }

                            if (godmode)
                            {
                                Player.RigManager?.health?.ResetHits();
                            }
                        }

                        // No permission restrictions
                        if (TeleportThresHold)
                        {
                            var rigManager = Player.RigManager;
                            var physicsRig = rigManager?.physicsRig;
                            if (physicsRig != null)
                            {
                                Vector3 velocity = physicsRig._wholeBodyVelocity_k__BackingField;
                                float speed = velocity.magnitude;

                                if (speed >= speedthreshold)
                                {
                                    LocalPlayer.TeleportToCheckpoint();
                                }
                            }
                        }



                        var rightController = RightController();
                        var leftController = LeftController();
                        if (rightController != null && leftController != null && rightController.GetBButton() && leftController.GetBButton())
                        {
                            timertodo++;
                            if (!tpback10seconds)
                            {

                                if (timertodo >= (fps < 20 ? 30 : 150))
                                {
                                    LocalPlayer.TeleportToCheckpoint();
                                    NotificationNow(FusionProtectorInfo.ClientName, "Emergency Escape To Spawn!", NotificationType.WARNING, 2.0f);
                                    timertodo = 0;
                                }
                            }
                            else
                            {
                                if (timertodo >= (fps < 20 ? 30 : 150))
                                {
                                    LocalPlayer.TeleportToPosition(Seconds10back);
                                    NotificationNow(FusionProtectorInfo.ClientName, $"Emergency Escape {timerfoeesa} Seconds Back!", NotificationType.WARNING, 2.0f);
                                    timertodo = 0;
                                }
                            }
                        }
                        else if (rightController == null || !rightController.GetBButton())
                        {
                            timertodo = 0;
                        }

                        if (rightController != null && leftController != null && rightController.GetAButton() && leftController.GetAButton())
                        {
                            timeravisafe++;
                            if (timeravisafe >= (fps < 20 ? 30 : 250))
                            {
                                ChangeIntoAvi("c3534c5a-94b2-40a4-912a-24a8506f6c79");
                                NotificationNow(FusionProtectorInfo.ClientName, "Emergency Avatar!", NotificationType.WARNING, 2.0f);
                                timeravisafe = 0;
                            }
                        }
                        else if (rightController == null || !rightController.GetAButton())
                        {
                            timeravisafe = 0;
                        }


                        if (NetworkInfo.IsHost)
                        {
                            if (rightController != null && leftController != null && rightController.GetThumbStick() && leftController.GetThumbStick())
                            {
                                timerreloadlevel++;
                                if (timerreloadlevel >= (fps < 20 ? 50 : 700))
                                {
                                    SceneStreamer.Reload();
                                    NotificationNow(FusionProtectorInfo.ClientName, "Emergency Reload Level!", NotificationType.WARNING, 2.0f);
                                    timerreloadlevel = 0;
                                }
                            }
                            else if (rightController == null || !rightController.GetThumbStick())
                            {
                                timerreloadlevel = 0;
                            }
                        }


                        // Operator or higher options
                        if (AreYouOPERATOR())
                        {

                            if (dashingnow)
                            {
                                InvokeWithType(typeof(SmashBones), "ApplyDashing", new object[] { Player.RigManager });
                            }
                            if (doublejumpnow)
                            {
                                InvokeWithType(typeof(SmashBones), "ApplyDoubleJump", new object[] { Player.RigManager });
                            }
                            if (Aircontrolnow)
                            {
                                if (!JR_YourGetHand(WhichHand.Left).JR_IsGrabbedNimbusGun() && !JR_YourGetHand(WhichHand.Right).JR_IsGrabbedNimbusGun())
                                {
                                    InvokeWithType(typeof(SmashBones), "ApplyAirControl", new object[] { Player.RigManager });
                                }
                            }
                            if (selfconstraint)
                            {
                                LocalPlayer.ClearConstraints();
                            }

                        }


                        if (AntiGravityChange)
                        {
                            Physics.gravity = new Vector3(0f, -9.81f, 0f); //sets to earth on loop
                        }
                    }
                }
            }
        }
        public override void OnApplicationStart()
        {
            if (!System.IO.Directory.Exists(FPSavetxt))
                System.IO.Directory.CreateDirectory(FPSavetxt);


            if (!System.IO.Directory.Exists(FusionProtectorFiles))
                System.IO.Directory.CreateDirectory(FusionProtectorFiles);

            if (!File.Exists(RECNETLYMETLOGGED))
            {
                File.WriteAllText(RECNETLYMETLOGGED, "");
            }
            if (!File.Exists(MEDIAPLAYERLOGS))
            {
                File.WriteAllText(MEDIAPLAYERLOGS, "");
            }
            if (!File.Exists(LOBBIESLOGGEDSINCE))
            {
                File.WriteAllText(LOBBIESLOGGEDSINCE, "");
            }
            if (!File.Exists(PLAYERSLOGGEDSINCE))
            {
                File.WriteAllText(PLAYERSLOGGEDSINCE, "");
            }

            if (!File.Exists(BlockMovementsPath))
            {
                File.WriteAllText(BlockMovementsPath, "");
            }
            if (!File.Exists(WarnAvisNow))
            {
                File.WriteAllText(WarnAvisNow, "");
            }
            if (!File.Exists(BlockAviAuthorNowp))
            {
                File.WriteAllText(BlockAviAuthorNowp, "");
            }
            if (!File.Exists(BlockPalletAviNowp))
            {
                File.WriteAllText(BlockPalletAviNowp, "");
            }

            if (!File.Exists(homeworldnow))
            {
                File.WriteAllText(homeworldnow, "c2534c5a-80e1-4a29-93ca-f3254d656e75");
            }
            if (!File.Exists(BlockedSpawnablesPath))
            {
                File.WriteAllText(BlockedSpawnablesPath, "");
            }
            if (!File.Exists(WarnedSpawnablesPath))
            {
                File.WriteAllText(WarnedSpawnablesPath, "");
            }
            if (!File.Exists(ProtectorSettings))
            {
                File.WriteAllText(ProtectorSettings, "");
            }
            if (!File.Exists(SpawnablesKickPath))
            {
                File.WriteAllText(SpawnablesKickPath, "");
            }
            if (!File.Exists(AvatarsKickPath))
            {
                File.WriteAllText(AvatarsKickPath, "");
            }
            if (!File.Exists(SpawnableCustomFav))
            {
                File.WriteAllText(SpawnableCustomFav, "");
            }
            if (!File.Exists(AvatarCustomFav))
            {
                File.WriteAllText(AvatarCustomFav, "");
            }
            if (!File.Exists(MEDIAPLAYERBLOCKERNOW))
            {
                File.WriteAllText(MEDIAPLAYERBLOCKERNOW, "");
            }
            if (!File.Exists(DamageBlockPath))
            {
                File.WriteAllText(DamageBlockPath, "");
            }
            if (!File.Exists(CreateCheatToolsPreset.devitems))
            {
                File.WriteAllText(CreateCheatToolsPreset.devitems, "");
            }
            if (!File.Exists(CreateCheatToolsPreset.devitemscurrent))
            {
                File.WriteAllText(CreateCheatToolsPreset.devitemscurrent, "{\r\n  \"TitleOfPreset\": \"Default\",\r\n  \"Item1\": {\r\n    \"BarcodeId\": \"c1534c5a-5747-42a2-bd08-ab3b47616467\",\r\n    \"SpawnableName\": \"Spawn Gun\",\r\n    \"LocalSpawn\": false\r\n  },\r\n  \"Item2\": {\r\n    \"BarcodeId\": \"c1534c5a-6b38-438a-a324-d7e147616467\",\r\n    \"SpawnableName\": \"Nimbus Gun\",\r\n    \"LocalSpawn\": false\r\n  },\r\n  \"Item3\": {\r\n    \"BarcodeId\": \"Empty\",\r\n    \"SpawnableName\": \"Empty\",\r\n    \"LocalSpawn\": false\r\n  },\r\n  \"Item4\": {\r\n    \"BarcodeId\": \"Empty\",\r\n    \"SpawnableName\": \"Empty\",\r\n    \"LocalSpawn\": false\r\n  },\r\n  \"Item5\": {\r\n    \"BarcodeId\": \"Empty\",\r\n    \"SpawnableName\": \"Empty\",\r\n    \"LocalSpawn\": false\r\n  }\r\n}");
            }
            if (!File.Exists(BodyLogPage.Bodylogpages))
            {
                File.WriteAllText(BodyLogPage.Bodylogpages, "");
            }
            if (!File.Exists(TeleporterManager.teleportmanager))
            {
                File.WriteAllText(TeleporterManager.teleportmanager, "");
            }
            if (!File.Exists(InventoryPage.PresetsFile))
            {
                File.WriteAllText(InventoryPage.PresetsFile, "");
            }
            if (!File.Exists(InventoryPage.PresetsFileCurrent))
            {
                File.WriteAllText(InventoryPage.PresetsFileCurrent, "");
            }
            if (!File.Exists(ModIDBLOCKSPATH))
            {
                File.WriteAllText(ModIDBLOCKSPATH, "");
            }
            if (!File.Exists(voicepathblocked))
            {
                File.WriteAllText(voicepathblocked, "");
            }
            if (!File.Exists(ServerBlockSpawnPath))
            {
                File.WriteAllText(ServerBlockSpawnPath, "");
            }
            if (!File.Exists(blockmessagingnowpath))
            {
                File.WriteAllText(blockmessagingnowpath, "");
            }
            if (!File.Exists(BodyLogRadialMenuColorPreset.ColorsCurrent))
            {
                File.WriteAllText(BodyLogRadialMenuColorPreset.ColorsCurrent, "[ \r\n   \"0\",\r\n  \"255\",\r\n  \"0\",\r\n  \"255\",\r\n    \"0\",\r\n  \"255\",\r\n    \"0\",\r\n  \"255\",\r\n    \"0\",\r\n  \"255\",\r\n    \"0\",\r\n  \"255\",\r\n    \"0\",\r\n  \"255\",\r\n    \"0\",\r\n  \"255\",\r\n]");
            }
            if (!File.Exists(BodyLogRadialMenuColorPreset.ColorsPresets))
            {
                File.WriteAllText(BodyLogRadialMenuColorPreset.ColorsPresets, "");
            }
            if (!File.Exists(StatsKickerPresets.StatsKickerCurrent))
            {
                File.WriteAllText(StatsKickerPresets.StatsKickerCurrent, "[\r\n  \"1000\",\r\n  \"1000\",\r\n  \"1000\",\r\n  \"1000\",\r\n  \"1000\",\r\n  \"1000\",\r\n  \"1000\",\r\n  \"1000\",\r\n  \"1000\",\r\n  \"1000\",\r\n  \"1000\"\r\n]");
            }
            if (!File.Exists(StatsKickerPresets.StatsKickerPresetsNow))
            {
                File.WriteAllText(StatsKickerPresets.StatsKickerPresetsNow, "");
            }
            if (!File.Exists(FusionProfilePresets.PresetsPath))
            {
                File.WriteAllText(FusionProfilePresets.PresetsPath, "");
            }
            if (!File.Exists(avatarsblocked))
            {
                File.WriteAllText(avatarsblocked, "");
            }
            if (!File.Exists(spawnlimitshostonly))
            {
                File.WriteAllText(spawnlimitshostonly, "");
            }
            if (!File.Exists(blockpalletnowlist))
            {
                File.WriteAllText(blockpalletnowlist, "");
            }
            if (!File.Exists(blockauthornowlist))
            {
                File.WriteAllText(blockauthornowlist, "");
            }
            ReloadList();
        }
        public override void OnInitializeMelon()
        {
            
            //montana client detection tried to do my best 
            string modsFolder = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods");
            string targetDll = Path.Combine(modsFolder, "MontanaClient.dll");
            string oldtargetDll = Path.Combine(modsFolder, "Client.dll");
            //


            if (File.Exists(targetDll) || File.Exists(oldtargetDll))
            {
                MelonLogger.Warning("Detected a client in Mods folder. Quitting application...");
                Application.Quit();
            }

            System.Collections.Generic.List<string> filesToWatch = new()
            {
            spawnlimitshostonly,
            avatarsblocked,
            homeworldnow,
            PalletDumpLocation,
            BlockedSpawnablesPath,
            WarnedSpawnablesPath,
            SpawnablesKickPath,
            AvatarsKickPath,
            SpawnableCustomFav,
            blockpalletnowlist,
            blockauthornowlist,
            AvatarCustomFav,
            DamageBlockPath,
            ServerBlockSpawnPath,
            blockmessagingnowpath,
            ModIDBLOCKSPATH,
            voicepathblocked,
            WarnAvisNow,
            BlockAviAuthorNowp,
            BlockPalletAviNowp,
            permissionshere,
            MEDIAPLAYERBLOCKERNOW
            };

            foreach (var file in filesToWatch)
            {
                WatchFileChanges(file);
            }




            SteamNetworkLayer.OnLoggedInEvent += delegate
            {



                try
                {
                    if (SteamIdYours() == 0)
                    {

                        MelonLogger.Warning($"Stored Original Steam ID : {SteamIdYours()}");
                        SiteStuff.GlobalBanChecking();
                    }


                    if (AvatarsStored.Count == 0)
                    {

                        new SimpleTimer(() =>
                        {
                            if (!NetworkInfo.HasLayer)
                                return;

                            var layer = NetworkLayerManager.Layer;
                            if (layer == null)
                                return;

                            var matchmaker = layer.Matchmaker;
                            if (matchmaker == null)
                                return;

                            var searchBar = MenuMatchmaking.SearchBarElement;
                            var browserPage = MenuMatchmaking.BrowserPage;

                            if (searchBar == null || browserPage == null)
                                return;

                            if (searchBar.gameObject == null || browserPage.gameObject == null)
                                return;

                            if (searchBar.gameObject.active || browserPage.gameObject.active)
                                return;
                           
                            matchmaker.RequestLobbies((info) =>
                            {
                                if (info.Lobbies == null || !info.Lobbies.Any())
                                    return;

                                foreach (var lobby in info.Lobbies)
                                {
                                    var title = lobby.Metadata.LobbyInfo.LobbyID;
                                    var code = lobby.Metadata.LobbyInfo.LobbyHostName;

                                    if (!CachedLobbies.Any(x =>
                                        x.Metadata.LobbyInfo.LobbyID == title &&
                                        x.Metadata.LobbyInfo.LobbyHostName == code))
                                    {
                                        CachedLobbies.Add(lobby);
                                    }

                                    foreach (var p in lobby.Metadata.LobbyInfo.PlayerList.Players)
                                    {
                                        if (PlayersOnline.Any(x => x.PlatformID == p.PlatformID))
                                            continue;

                                        PlayersOnline.Add(new PlayerInfo
                                        {
                                            Username = StripColorTags(p.Username),
                                            PlatformID = p.PlatformID,
                                            Nickname = p.Nickname,
                                            Description = p.Description,
                                            PermissionLevel = p.PermissionLevel,
                                            AvatarModID = p.AvatarModID,
                                            AvatarTitle = p.AvatarTitle
                                        });

                                    }
                                }
                            });
                        }, 0).Start(true, 6f);

                        new SimpleTimer(() =>
                        {
                            if (autosavenow)
                            {
                                ManuallySave(false);
                            }
                        }, 0).Start(true, 7f);

                        new SimpleTimer(() =>
                        {
                            if (SteamIdYours() != 0)
                            {
                                SiteStuff.GlobalBanChecking();
                            }
                        }, 0).Start(true, 180f);

                        EmergencyEscapetimer = new SimpleTimer(() =>
                        {
                            if (!tpback10seconds) return;

                            var feetTransform = Player.RigManager?.physicsRig?.feet?.transform;
                            if (feetTransform == null) return;

                            Seconds10back = feetTransform.position;
                        }, timerfoeesa).Start(true, timerfoeesa);

                        new SimpleTimer(() =>
                        {
                            if (Time.unscaledDeltaTime > 0f)
                                fps = 1f / Time.unscaledDeltaTime;
                        }, 0).Start(true, 0.1f);

                        DespawnAllTimera = new SimpleTimer(() =>
                        {
                            if (!DespawnAllTimer || HelperMethods.IsLoading()) return;
                            if (!NetworkInfo.HasServer) return;

                            DespawnAll(DespawnerTimerAllReal);
                        }, DespawnAllTimerMins).Start();

                        new SimpleTimer(() =>
                        {
                            if (hideholstersplayers)
                            {
                                var players = NetworkPlayers(true);
                                if (players != null)
                                {
                                    foreach (var player in players)
                                    {
                                        if (player == null) continue;
                                        try { HolsterHiderAll(player, false); } catch { }
                                    }
                                }
                            }

                            if (hideholsters)
                            {
                                try { HolsterHiderAll(null, false); } catch { }
                            }

                            if (bodylogplayers)
                            {
                                var players = NetworkPlayers(true);
                                if (players != null)
                                {
                                    foreach (var playernow in players)
                                    {
                                        if (playernow == null) continue;
                                        var rig = playernow.RigRefs?.RigManager?.physicsRig;
                                        if (rig == null) continue;
                                        var bodyLog = JR_BodyLog(rig).bodylogreturn;
                                        var outRing = JR_BodyLog(rig).Outerring;
                                        if (bodyLog != null)
                                        {
                                            try
                                            {
                                                bodyLog.ballLine?.gameObject?.SetActive(false);
                                                bodyLog.ballArt?.gameObject?.SetActive(false);
                                            }
                                            catch { }
                                        }
                                        outRing?.gameObject?.SetActive(false);
                                    }
                                }
                            }
                        }, 0).Start(true, 5f);

                        new SimpleTimer(() =>
                        {
                            SpawnInventoryRefresh();
                        }, 0).Start(true, 0.5f);

                        new SimpleTimer(() =>
                        {
                            if (!unlammo) return;
                            if (GamemodeManager.IsGamemodeStarted) return;
                            if (HelperMethods.IsLoading()) return;


                            LocalInventory.SetAmmo(10000);

                        }, 0).Start(true, 2f);

                        MelonCoroutines.Start(LoadAssetsEnum(randomizerslzonly));

                        CreateProtectorUI();
                        originalbodylog = DataManager.ActiveSave.PlayerSettings.FavoriteAvatars;
                        var sb = new System.Text.StringBuilder();

                        for (int i = 0; i < originalbodylog.Count; i++)
                        {
                            var item = originalbodylog[i];
                            sb.Append(item ?? "NULL");

                            if (i < originalbodylog.Count - 1)
                                sb.Append(", ");
                        }

                        MelonLogger.Warning("Stored Original Bodylog: " + sb.ToString());

                        originalprofiledetails = new PlayerInfo
                        {
                            Username = LocalPlayer.Metadata.Username.GetValue(),
                            Nickname = LocalPlayer.Metadata.Nickname.GetValue(),
                            Description = LocalPlayer.Metadata.Description.GetValue(),
                            AvatarModID = LocalPlayer.Metadata.AvatarModID.GetValue(),
                            AvatarTitle = LocalPlayer.Metadata.AvatarTitle.GetValue(),
                            PermissionLevel = 0,
                            PlatformID = SteamIdYours()
                        };

                        MelonLogger.Warning(
    "Stored Original Profile Details:\n" +
    $"Username: {originalprofiledetails.Username}\n" +
    $"Nickname: {originalprofiledetails.Nickname}\n" +
    $"Description: {originalprofiledetails.Description}\n" +
    $"AvatarModID: {originalprofiledetails.AvatarModID}\n" +
    $"AvatarTitle: {originalprofiledetails.AvatarTitle}\n" +
    $"PermissionLevel: {originalprofiledetails.PermissionLevel}\n" +
    $"PlatformID: {originalprofiledetails.PlatformID}"
);
                    }
                }
                catch
                {
                }


            };
            //NETWORKPLAYER COUNT CHECK IS TO ONLY RUN THESE BLOCKS OF BASED ON THE COMMENTS BELOW

            //WORKS WHEN CREATE SERVER LOAD IN & SINGLE PLAYER LOADING LEVELS
            Hooking.OnLevelLoaded+= delegate
            {


                if (NetworkPlayers().Count == 0)
                {
                    ProtectionFromClients.PlayerDamageLogs.Clear();
                 
                    if (spawngunuialways)
                    {
                        spawngunatleastonce = false;
                    }
                    if (aviswitchprotection)
                    {
                        OnSwapAvatarPatch3._hasSkippedInitialSwap = false;
                    }
                    PageNow = MenuSections.SelfCat;
                    if (KEEPLOADOUTINVENTORY)
                    {
                      MelonCoroutines.Start(KeepLoadOut());
                    }

                 

                    if (modomatonload)
                    {
                        SpawnIt("Atlas.96.ModOMat.Spawnable.ModOMatPortable",
                          Player.RigManager.physicsRig.feet.transform.position +
                          Player.RigManager.physicsRig.feet.transform.forward,
                          Quaternion.identity, true);
                    }



                    Seconds10back = Player.RigManager.physicsRig.feet.transform.position;
                }

            };

            //WORKS LOADING INTO FUSION SERVER AND FUSION LEVEL LOADING
            MultiplayerHooking.OnTargetLevelLoaded += delegate
            {

                
                if (NetworkPlayers().Count > 0)
                {
                    ProtectionFromClients.PlayerDamageLogs.Clear();

                    if (spawngunuialways)
                    {
                        spawngunatleastonce = false;
                    }
                    if (aviswitchprotection)
                    {
                        OnSwapAvatarPatch3._hasSkippedInitialSwap = false;
                    }
                    Seconds10back = Player.RigManager.physicsRig.feet.transform.position;

                    StoredJoinPlayers.Clear();
                    if (!NetworkInfo.IsHost)
                    {
                        foreach (var play in NetworkPlayers())
                        {
                            var activeone = new SpoofChecker
                            {
                                PlatformID = play.PlayerID.PlatformID,
                                Username = play.PlayerID.Metadata.Username.GetValue()
                            };

                            StoredJoinPlayers.Add(activeone);

                        }
                    }



                    PageNow = MenuSections.SelfCat;
                    if (KEEPLOADOUTINVENTORY)
                    {
                        MelonCoroutines.Start(KeepLoadOut());

                    }


                    rejoinlastserver = LobbyInfoManager.LobbyInfo.LobbyCode;
                    MelonLogger.Warning($"Stored Lobby Code For Rejoining : {rejoinlastserver}");
                    if (modomatonload)
                    {
                        SpawnIt("Atlas.96.ModOMat.Spawnable.ModOMatPortable",
                          Player.RigManager.physicsRig.feet.transform.position +
                          Player.RigManager.physicsRig.feet.transform.forward,
                          Quaternion.identity, true);
                    }

                }
            };

            MultiplayerHooking.OnDisconnected += delegate
            {
                kicktillrestart.Clear();
                SaveTXTFunc();
                PlayersListNow();
                playersavatarrefs.Clear();
                playersspawnrefs.Clear();
                ProtectionFromClients.lastSpawnTime = new();
                LabFusion.UI.Popups.Notifier.Cancel(FusionProtectorInfo.ClientName);
                PageNow = MenuSections.SelfCat;

                if (cleandisconnect)
                {
                    foreach (var poolee in Resources.FindObjectsOfTypeAll<Poolee>())
                    {
                        var root = poolee?.transform?.root;
                        if (root == null)
                            continue;

                        var marrowEntity = root.GetComponent<MarrowEntity>();
                        if (marrowEntity != null)
                        {
                            marrowEntity.Despawn();
                            continue;
                        }

                        var marrowBody = root.GetComponent<MarrowBody>();
                        marrowBody?.Entity.Despawn();
                    }
                }

                StoredJoinPlayers.Clear();

                if (DeleteLastLobbyMods && LogDownloads.DeleteThesOnLeave.Count > 0)
                {
                    foreach (var pallet in LogDownloads.DeleteThesOnLeave)
                    {
                        DeleteModioMod(pallet,false);
                    }
                    NotificationNow(FusionProtectorInfo.ClientName,$"Deleted {LogDownloads.DeleteThesOnLeave.Count.ToString()} Mods From Last Lobby!",NotificationType.WARNING,6.5f);

                    LogDownloads.DeleteThesOnLeave.Clear();
                }
                ProtectionFromClients.PlayerDamageLogs.Clear();
                despawnresponselogger.Clear();
            };
            MultiplayerHooking.OnPlayerJoined += delegate
            {
                ProtectionFromClients.lastSpawnTime = new();
                PlayersListNow();
                SaveTXTFunc();
            };
            MultiplayerHooking.OnPlayerLeft += delegate
            {
                SaveTXTFunc();
                ProtectionFromClients.lastSpawnTime = new();
                PlayersListNow();

                var currentPlatformIDs = NetworkPlayers()
               .Select(p => p.PlayerID.PlatformID)
               .ToHashSet();

                StoredJoinPlayers.RemoveAll(p => !currentPlatformIDs.Contains(p.PlatformID));
            };
            NetworkSceneManager.OnPlayerLoadedIntoLevel += (now, hh) =>
            {
                if (NetworkPlayerManager.TryGetPlayer(now, out var joinednetty))
                    {



                    var activeone = new SpoofChecker
                    {
                        PlatformID = now.PlatformID,
                        Username = now.Metadata.Username.GetValue()
                    };

                    StoredJoinPlayers.Add(activeone);

                    _ = SiteStuff.AltPrevention(now.PlatformID);



                }

            };
            MultiplayerHooking.OnTargetLevelLoaded += delegate
            {
              
                ServerHistorys.Add(LobbyInfoManager.LobbyInfo);
              
                if (!HideFusionProtector)
                {

                    if (!NetworkInfo.IsHost)
                    {

                        var protectionData = ProtectorPingData.Create(PlayerIDManager.LocalSmallID, FusionProtectorInfo.Version);
                        MessageRelay.RelayModule<ProtectorPingMessage, ProtectorPingData>(
                            protectionData,
                            CommonMessageRoutes.ReliableToServer
                    );
                    }
                }

            };
            MultiplayerHooking.OnJoinedServer += delegate
            {

                playersavatarrefs.Clear();
                playersspawnrefs.Clear();
                PlayerSpawningStuff.Clear();

                MelonCoroutines.Start(SiteStuff.UpdateSites());

                if (clientexploitclearonnewserver)
                {
                    ProtectionLogs.RemoveAll();
                    ClientExploitLogs.Clear();
                }
                if (spawnlogexploitclearonnewserver)
                {
                    SpawnLogs.Clear();
                }
                if (switchlogexploitclearonnewserver)
                {
                    PlayeravatarStuff.Clear();
                }
            };
            Menu.OnPageOpened += pageaction =>
            {
                void RemoveFromFile(string itemToRemove, string path, System.Collections.Generic.HashSet<string> listofstring)
                {
                    listofstring.Remove(itemToRemove);
                    File.WriteAllLines(path, listofstring);
                }

                void PopulatePage(string filePath, System.Collections.Generic.HashSet<string> list, Page page)
                {
                    page.RemoveAll();

                    if (list.Count > 0)
                        NotificationNow(FusionProtectorInfo.ClientName, $"Amount :{list.Count}", NotificationType.WARNING, 2.0f);

                    foreach (var item in list)
                    {
                        page.CreateFunction(item, Color.yellow, () =>
                        {    
                            NotificationNow(FusionProtectorInfo.ClientName, $"Removed {item}", NotificationType.SUCCESS);
                            RemoveFromFile(item, filePath, list);
                            Menu.OpenPage(page);
                        });
                    }
                    
                }

                var montana = Page.Root.GetChildPage("Montana Client");
                var montanaold = Page.Root.GetChildPage("Client");

                var detectedClient = montana ?? montanaold;

                if (detectedClient != null)
                {
                    Page.Root.Name = $"Uninstall Your Client [{detectedClient.Name}]!";
                    Page.Root.RemoveAll();
                }



                if (pageaction == OnlineFriends)
                {
                    FriendLobbies();
                }

                if (pageaction == levelhistory)
                {
                    levelhistory.RemoveAll();
                    int countnnow = 0;

                    foreach (var searchy in SearchHistorynow)
                    {
                        if (searchy.IsLevelCrate)
                        {
                            countnnow++;
                            levelhistory.CreateFunction(searchy.SearchText, Color.white, () =>
                        {
                            MelonCoroutines.Start(Search(
        searchy.SearchText,
        levelresults,
        searchy.Method,
        SearchMethodType.Level,
        (barcode) =>
        {
            var nowy = new LevelCrateReference(barcode);
            SceneStreamer.Load(nowy.Barcode);
        }));
                        });
                        }
                    }
                    NotificationNowAlways(FusionProtectorInfo.ClientName, $"Total Searches {countnnow}", NotificationType.WARNING, 3.5f);

                }

                if (pageaction == avisearchhistory)
                {
                    avisearchhistory.RemoveAll();
                    void AvatarsearcherLessCode(string barcode)
                    {
                        switch (AvatarSearchTypeReal)
                        {
                            case AvatarSearchType.ChangeInto:
                                ChangeIntoAvi(barcode);
                                break;

                            case AvatarSearchType.CopyDetailsToClipboard:
                                var referecny = new AvatarCrateReference(barcode);
                                GUIUtility.systemCopyBuffer =
                                    $"Barcode ID : {referecny.Barcode.ID}\n" +
                                    $"Pallet Name : {StripColorTags(referecny.Crate.Pallet.name)}\n" +
                                    $"Pallet Author : {referecny.Crate.Pallet.Author}";
                                break;

                            case AvatarSearchType.SetBodyLog:
                                ChangeBodyLogAvatarSlot(bodylogindex, barcode, true);
                                break;
                        }
                    }
                    int countnnow = 0;

                    foreach (var searchy in SearchHistorynow)
                    {
                        if (searchy.IsAvatar)
                        {
                            countnnow++;
                            avisearchhistory.CreateFunction(searchy.SearchText, Color.white, () =>
                        {
                            MelonCoroutines.Start(Search(
        searchy.SearchText,
        aviresults,
        searchy.Method,
        SearchMethodType.Avatar,
        (barcode) =>
        {
            AvatarsearcherLessCode(barcode);

        }));
                        });
                        }
                    }
                    
                    NotificationNowAlways(FusionProtectorInfo.ClientName, $"Total Searches {countnnow}", NotificationType.WARNING, 3.5f);

                }

                if (pageaction == spawnablehistory)
                {
                    void SpawnerFuncLessCode(string barcode)
                    {
                        switch (spawnablesrchtype)
                        {
                            case SpawnableSearchType.Spawn:

                                SpawnIt(
                                    barcode,
                                    JR_YourGetHand(WhichHand.Left).transform.position +
                                    JR_YourGetHand(WhichHand.Left).transform.forward +
                                    JR_YourGetHand(WhichHand.Left).transform.up,
                                    Quaternion.identity);

                                break;

                            case SpawnableSearchType.CopyDetailsToClipboard:

                                GUIUtility.systemCopyBuffer = barcode;
                                NotificationNow(
                                    FusionProtectorInfo.ClientName,
                                    $"Copied Barcode To Clipboard! {barcode}",
                                    NotificationType.WARNING,
                                    3.0f);

                                break;

                            case SpawnableSearchType.UnFavoriteAndFavorite:

                                if (!DataManager.ActiveSave.PlayerSettings.FavoriteSpawnables.Contains(barcode))
                                {
                                    DataManager.ActiveSave.PlayerSettings.FavoriteSpawnables.Add(barcode);
                                    DataManager.TrySaveActiveSave(SaveFlags.Complete);

                                    NotificationNow(
                                        FusionProtectorInfo.ClientName,
                                        $"Added {barcode} To SaveGame Favorites!",
                                        NotificationType.SUCCESS);
                                }
                                else
                                {
                                    DataManager.ActiveSave.PlayerSettings.FavoriteSpawnables.Remove(barcode);
                                    DataManager.TrySaveActiveSave(SaveFlags.Complete);

                                    NotificationNow(
                                        FusionProtectorInfo.ClientName,
                                        $"Removed {barcode} From SaveGame Favorites!",
                                        NotificationType.SUCCESS);
                                }

                                break;

                            case SpawnableSearchType.DespawnAllOfThis:

                                foreach (var netentity in NetworkEntities())
                                {
                                    if (netentity.JR_GetMarrowEntity().JR_GetBarcodeID() == barcode)
                                    {
                                        netentity.JR_Despawn();
                                    }
                                }

                                NotificationNow(
                                    FusionProtectorInfo.ClientName,
                                    $"Despawned Everything Matching {StripColorTags(new SpawnableCrateReference(barcode).Crate.name)}",
                                    NotificationType.SUCCESS,
                                    3.5f);

                                break;

                            case SpawnableSearchType.SetInSpawnGun:

                                SetBarCodeToSpawnGun(barcode);

                                break;
                        }
                    }
                    int countnnow =0;
                    spawnablehistory.RemoveAll();
                    foreach (var searchy in SearchHistorynow)
                    {
                        if (searchy.IsSpawnable)
                        {
                            countnnow++;
                            spawnablehistory.CreateFunction(searchy.SearchText, Color.white, () =>
                            {
                                MelonCoroutines.Start(Search(
                searchy.SearchText,
                spawnableresults,
                searchy.Method,
                SearchMethodType.Spawnable,
                (barcode) =>
                {
                    SpawnerFuncLessCode(barcode);

                }));
                            });


                        }


                    }


                    NotificationNowAlways(FusionProtectorInfo.ClientName,$"Total Searches {countnnow}",NotificationType.WARNING,3.5f);
                }

                if (pageaction == permissioneditornow)
                {
                    permissioneditornow.RemoveAll();

                    foreach (var perm in PermissionList.PermittedUsers)
                    {
                        ulong id = perm.Item1;
                        string username = perm.Item2;
                        PermissionLevel level = perm.Item3;

                        string cleanedname = new string(username
                            .Where(c => !char.IsControl(c))
                            .ToArray())
                            .Trim();

                        var nameofUser = string.IsNullOrWhiteSpace(cleanedname)
                            ? id.ToString()
                            : cleanedname;

                        permissioneditornow.CreateEnum(nameofUser, Color.yellow, level, enabled =>
                        {
                            permlevel = (PermissionLevel)enabled;
                            PermissionList.SetPermission(id, username, permlevel);
                        });
                    }
                }

                if (pageaction == WarnedSpawnablesnow)
                {
                    PopulatePage(WarnedSpawnablesPath, WarnedSpawnables, WarnedSpawnablesnow);
                }

                if (pageaction == modidblockednow)
                {
                    PopulatePage(ModIDBLOCKSPATH, modidblocked, modidblockednow);
                }

                if (pageaction == BlockedSpawnablesnow)
                {
                    PopulatePage(BlockedSpawnablesPath, BlockedSpawnables, BlockedSpawnablesnow);
                }

                if (pageaction == SpawnablesKicknow)
                {
                    PopulatePage(SpawnablesKickPath, SpawnablesKick, SpawnablesKicknow);
                }

                if (pageaction == blockentirepalletnow)
                {
                    PopulatePage(blockpalletnowlist, blockentirepallet, blockentirepalletnow);
                }

                if (pageaction == blockentireauthornow)
                {
                    PopulatePage(blockauthornowlist, blockentireauthor, blockentireauthornow);
                }

                if (pageaction == playerjoinlogsnow)
                {
                    playerjoinlogsnow.RemoveAll();

                    playerjoinlogsnow.CreateFunction("Copy All Players To Clipboard", Color.green, () =>
                    { 
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        GUIUtility.systemCopyBuffer = JsonSerializer.Serialize(JoinLogger, options);
                    });
                    foreach (var logger in JoinLogger)
                    {
                        string pageTitle = $"+ [{CleanedNAME(logger.Nickname, logger.Username)}] ({logger.PlatformID})";
                        var page = playerjoinlogsnow.CreatePage(pageTitle, Color.yellow);

                        page.CreateFunction("Open Steam Profile", Color.yellow, () =>
                        {
                            CheckSteamID(logger.PlatformID);
                        });

                        page.CreateFunction("Copy Details To Clipboard", Color.yellow, () =>
                        {
                            GUIUtility.systemCopyBuffer =
                                $"Nickname : {logger.Nickname}\n" +
                                $"Username : {logger.Username}\n" +
                                $"SteamID : {logger.PlatformID}\n" +
                                $"Description : {logger.Description}\n" +
                                $"Avatar Mod.io #ID : {logger.AvatarModID}";

                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                "Copied player information to clipboard",
                                NotificationType.INFORMATION,
                                2.0f
                            );
                        });

                        page.CreateFunction("Ban/Unban From Your Lobby", Color.yellow, () =>
                        {
                            if (NetworkHelper.IsBanned(logger.PlatformID))
                            {
                                BanManager.Pardon(logger.PlatformID);
                                NotificationNow(
                                    FusionProtectorInfo.ClientName,
                                    "UnBanned Player",
                                    NotificationType.SUCCESS
                                );
                                return;
                            }

                            BanManager.BanList.Bans.RemoveAll(b => b.Player.PlatformID == logger.PlatformID);

                            BanManager.BanList.Bans.Add(new BanInfo
                            {
                                Player = new PlayerInfo
                                {
                                    Username = logger.Username,
                                    Nickname = logger.Nickname,
                                    PlatformID = logger.PlatformID,
                                    Description = logger.Description,
                                    AvatarModID = logger.AvatarModID,
                                    AvatarTitle = logger.AvatarTitle
                                },
                                Reason = $"Manually Banned [{FusionProtectorInfo.ClientName}]"
                            });

                            DataSaver.WriteJsonToFile("bans.json", BanManager.BanList);

                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                "Banned Player",
                                NotificationType.SUCCESS
                            );
                        });
                    }
                }
                if (pageaction == AISpawnersPage)
                {
                    AISpawnersPage.RemoveAll();
                    var allspawners = AISpawnersPage.CreatePage("+ All Spawners", Color.yellow);
                    allspawners.CreateFunction("Pause All", Color.yellow, () =>
                    {
                        foreach (var spawnernow in NPCSpawnersNow)
                        {
                            spawnernow.Pause(false);
                        }
                        NotificationNow(FusionProtectorInfo.ClientName, "Paused All Spawners", NotificationType.SUCCESS, 2f);
                    });
                    allspawners.CreateFunction("Resume All", Color.yellow, () =>
                    {
                        foreach (var spawnernow in NPCSpawnersNow)
                        {
                            spawnernow.Resume(false);
                        }
                        NotificationNow(FusionProtectorInfo.ClientName, "Resumed All Spawners", NotificationType.SUCCESS, 2f);
                    });
                    allspawners.CreateFunction("Erase All", Color.yellow, () =>
                    {
                        BoneLib.BoneMenu.Menu.DisplayDialog("Are you sure?", "This will erase all your spawners!", null, () =>
                        {
                            foreach (var spawnernow in NPCSpawnersNow)
                            {
                                spawnernow.StopAndClear(false);
                            }
                            NPCSpawnersNow.Clear();
                            Menu.OpenPage(AISpawnersPage);
                            NotificationNow(FusionProtectorInfo.ClientName, "Erased All Spawners", NotificationType.SUCCESS, 2f);
                        });
                    });
                    allspawners.CreateFunction("Kill All In All Spawners", Color.yellow, () =>
                    {
                        foreach (var spawnernow in NPCSpawnersNow)
                        {
                            spawnernow.KillAllInSpawner();
                        }
                        NotificationNow(FusionProtectorInfo.ClientName, "Kill All NPCs In All Spawners", NotificationType.SUCCESS, 2f);

                    });
                    allspawners.Logsettingsint("Max Entities", Color.green, ref maxentrefresh, 1, 1, 100, intnow =>
                    {
                        maxentrefresh = intnow;

                        foreach (var aispawnernow in NPCSpawnersNow)
                        {
                            aispawnernow.UpdateMaxSpawns(maxentrefresh);

                        }
                    });
                    allspawners.Logsettingsfloat("Timer Seconds", Color.green, ref Timerrefresh, 1, 1, 100000, intnow =>
                    {
                        Timerrefresh = intnow;

                        foreach (var aispawnernow in NPCSpawnersNow)
                        {
                           
                            aispawnernow.UpdateSpawnInterval(Timerrefresh);

                        }
                    });

                    var presetties = AISpawnersPage.CreatePage("+ Preset Spawners", Color.yellow);
                    var customspawner = AISpawnersPage.CreatePage("+ Custom Spawner", Color.yellow);

                    customspawner.Logsettingsint("Max Entities", Color.green, ref cmaxentrefresh, 1, 1, 100, intnow =>
                    {
                        cmaxentrefresh = intnow;
                    });
                    customspawner.Logsettingsfloat("Timer Seconds", Color.green, ref cTimerrefresh, 1, 1, 100000, intnow =>
                    {
                        cTimerrefresh = intnow;
                    });
                    customspawner.LogsettingsString("Barcode To Create Spawner", Color.yellow, ref customspwner, (stringy) =>
                    {
                        customspwner = stringy;
                    });
                    customspawner.CreateFunction("Add Custom Spawner ^", Color.yellow, () =>
                    {
                        if (IsBarcodeInGame(customspwner.Trim()))
                        {
                            var spawnernowha = new AISpawner((netentity) =>
                            {
                             
                            }, cTimerrefresh)
                            {
                                eachspawnisrandom = false
                            };
                            spawnernowha.Start(customspwner.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh);
                            NPCSpawnersNow.Add(spawnernowha);
                        }
                    });
                    customspawner.CreateFunction("Added Left Hand Item As Spawner", Color.yellow, () =>
                    {
                        var barynow = JR_YourGetHand(WhichHand.Left).JR_GetMarrowEntity().JR_GetBarcodeID();
                        if (IsBarcodeInGame(barynow))
                        {
                            var spawnernowha = new AISpawner((netentity) =>
                            {
                              
                            }, cTimerrefresh)
                            {
                                eachspawnisrandom = false
                            };
                            spawnernowha.Start(barynow, Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh);
                            NPCSpawnersNow.Add(spawnernowha);
                        }
                    });
                    customspawner.CreateFunction("Added Right Hand Item As Spawner", Color.yellow, () =>
                    {
                        var barynow = JR_YourGetHand(WhichHand.Right).JR_GetMarrowEntity().JR_GetBarcodeID();
                        if (IsBarcodeInGame(barynow))
                        {
                            var spawnernowha = new AISpawner((netentity) =>
                            {
                               
                            }, cTimerrefresh)
                            {
                                eachspawnisrandom = false
                            };
                            spawnernowha.Start(barynow, Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh);
                            NPCSpawnersNow.Add(spawnernowha);
                        }
                    });

                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All NPCS]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                         
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllNPCs);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Avatars]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                            
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllAvatars);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Spawnables]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                           
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllSpawnables);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Weapons]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                          
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllWeapons);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All No Tag Spawnables]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                           
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.NoTagsSpawnables);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Blades]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                          
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllBlade);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Blunts]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                          
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllBlunt);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Knifes]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                         
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllKnife);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Melees]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                            
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllMelees);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Pistols]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                           
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllPistol);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Ranged]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                           
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllRanged);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Rifles]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                          
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllRifle);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Shotguns]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                          
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllShotgun);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All SMGS]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                        
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllSMG);
                        NPCSpawnersNow.Add(spawnernowha);
                    });
                    customspawner.CreateFunction("Spawn Randomizing Spawner! [All Snipers]", Color.yellow, () =>
                    {
                        var spawnernowha = new AISpawner((netentity) =>
                        {
                         
                        }, cTimerrefresh)
                        {
                            eachspawnisrandom = true
                        };
                        spawnernowha.Start(GUIUtility.systemCopyBuffer.Trim(), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh, RandomizerType.AllSniper);
                        NPCSpawnersNow.Add(spawnernowha);
                    });

                    var randomsps = customspawner.CreatePage("+ Random Spawner", Color.yellow);

                    randomsps.CreateFunction("NPC Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {
                          
                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllNPCs), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("Avatar Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {
                            
                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllAvatars), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("Melee Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {
                          
                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllMelees), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("Spawnable Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {
                           
                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllSpawnables), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("No Tag Spawnable Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {
                          
                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.NoTagsSpawnables), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("Weapon Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllWeapons), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("[Weapon] Pistol Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllPistol), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("[Weapon] Ranged Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllRanged), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("[Weapon] Rifle Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllRifle), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("[Weapon] Shotgun Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllShotgun), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("[Weapon] SMG Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllSMG), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("[Weapon] Sniper Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllSniper), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("[Melee] Blade Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllBlade), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("[Melee] Blunt Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllBlunt), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });
                    randomsps.CreateFunction("[Melee] Knife Spawner!", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start(GetRandomByType(RandomizerType.AllKnife), Player.PhysicsRig.m_footLf.transform.position, cmaxentrefresh));
                    });

                    presetties.CreateFunction("[Ammo] Ammo Box Light", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start("c1534c5a-683b-4c01-b378-6795416d6d6f", Player.PhysicsRig.m_footLf.transform.position, 10));
                    });
                    presetties.CreateFunction("[Ammo] Ammo Box Medium", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start("c1534c5a-57d4-4468-b5f0-c795416d6d6f", Player.PhysicsRig.m_footLf.transform.position, 10));
                    });
                    presetties.CreateFunction("[Ammo] Ammo Box Heavy", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start("c1534c5a-97a9-43f7-be30-6095416d6d6f", Player.PhysicsRig.m_footLf.transform.position, 10));
                    });

                    presetties.CreateFunction("[NPC] Nullbodys", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start("c1534c5a-d82d-4f65-89fd-a4954e756c6c", Player.PhysicsRig.m_footLf.transform.position, 10));
                    });
                    presetties.CreateFunction("[NPC] Crablets", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start("c1534c5a-4583-48b5-ac3f-eb9543726162", Player.PhysicsRig.m_footLf.transform.position, 10));
                    });
                    presetties.CreateFunction("[NPC] Omni Projector Hazmats", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start("c1534c5a-7c6d-4f53-b61c-e4024f6d6e69", Player.PhysicsRig.m_footLf.transform.position, 10));
                    });
                    presetties.CreateFunction("[NPC] Security Guards", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start("SLZ.BONELAB.Content.Spawnable.NPCSecurityGuard", Player.PhysicsRig.m_footLf.transform.position, 10));
                    });
                    presetties.CreateFunction("[NPC] Skeleton Steel", Color.yellow, () =>
                    {
                        NPCSpawnersNow.Add(new AISpawner((netentity) =>
                        {

                        }, cTimerrefresh).Start("c1534c5a-a750-44ca-9730-b487536b656c", Player.PhysicsRig.m_footLf.transform.position, 10));
                    });




                    foreach (var aispawnernow in NPCSpawnersNow)
                    {


                        var levelcratetemp = new LevelCrateReference(aispawnernow.SpawnerMapLockedTo);
                        var maplockedpg = AISpawnersPage.CreatePage("+ " + StripColorTags(levelcratetemp.Scannable.Title), Color.yellow);

                        var npcused = maplockedpg.CreatePage($"+ {aispawnernow.NameOfUsed} | ID : {aispawnernow.InstanceID}", Color.yellow);


                        npcused.CreateFunction($"Change Spawners Spawnable [Clipboard]", Color.yellow, () =>
                        {
                            if (!aispawnernow.eachspawnisrandom)
                            {
                                var clippytext = GUIUtility.systemCopyBuffer.Trim();
                                if (IsBarcodeInGame(clippytext))
                                {
                                    aispawnernow.BarcodeID = clippytext ?? throw new System.ArgumentNullException(nameof(clippytext));
                                    var crateRef = new SpawnableCrateReference(clippytext);
                                    aispawnernow.NameOfUsed = StripColorTags(crateRef?.Crate?.Title) ?? "Unknown";
                                    Menu.OpenPage(AISpawnersPage);
                                    NotificationNow(FusionProtectorInfo.ClientName, $"[Spawner {aispawnernow.InstanceID}] Changed Spawners Spawnable!", NotificationType.SUCCESS, 2.0f);

                                }
                                else
                                {
                                    NotificationNow(FusionProtectorInfo.ClientName, $"[Spawner {aispawnernow.InstanceID}] Barcode '{aispawnernow.BarcodeID}' does not exist in game or Mod for it is not instlled!. Spawner not started.", NotificationType.ERROR, 3f);
                                }
                            }
                            else
                            {
                                NotificationNow(FusionProtectorInfo.ClientName, $"[Spawner {aispawnernow.InstanceID}] Is a randomizing spawner this is actally pointless so no....", NotificationType.ERROR, 3f);

                            }
                        });
                        npcused.CreateFunction($"On/Off Spawn Only If All Dead", Color.yellow, () =>
                        {
                            aispawnernow.onlyspawnifalldead = !aispawnernow.onlyspawnifalldead;
                            NotificationNow(FusionProtectorInfo.ClientName, $"+ Spawn Only If All Dead : {aispawnernow.onlyspawnifalldead} | Spawner ID : {aispawnernow.InstanceID}", NotificationType.SUCCESS, 2.0f);
                        });
                        npcused.CreateFunction($"On/Off Despawn Dead", Color.yellow, () =>
                        {
                            aispawnernow.despawndeads = !aispawnernow.despawndeads;
                            NotificationNow(FusionProtectorInfo.ClientName, $"+ Despawn Dead : {aispawnernow.despawndeads} | Spawner ID : {aispawnernow.InstanceID}", NotificationType.SUCCESS, 2.0f);
                        });
                        npcused.CreateFunction($"Kill All In Spawner", Color.yellow, () =>
                        {
                            aispawnernow.KillAllInSpawner();
                        });
                        npcused.CreateFunction($"Pause Spawner", Color.yellow, () =>
                        {

                            aispawnernow.Pause();
                        });
                        npcused.CreateFunction($"Resume Spawner", Color.yellow, () =>
                        {

                            aispawnernow.Resume();
                        });
                        npcused.CreateFunction($"Remove Spawner", Color.yellow, () =>
                        {

                            aispawnernow.StopAndClear(false);
                            NPCSpawnersNow.Remove(aispawnernow);
                            Menu.OpenPage(AISpawnersPage);
                            NotificationNow(FusionProtectorInfo.ClientName, $"Removed Spawner {aispawnernow.NameOfUsed} Spawner ID : {aispawnernow.InstanceID}", NotificationType.SUCCESS, 3f);

                        });
                        npcused.Logsettingsint("Max Entities", Color.green, ref vmaxentrefresh, 1, 1, 100, intnow =>
                        {
                            vmaxentrefresh = intnow;

                            aispawnernow.UpdateMaxSpawns(vmaxentrefresh);
                        });
                        npcused.Logsettingsfloat("Timer Seconds", Color.green, ref vTimerrefresh, 1, 1, 100000, intnow =>
                        {
                            vTimerrefresh = intnow;
                            aispawnernow.UpdateSpawnInterval(vTimerrefresh);

                        });
                    }
                }
                if (pageaction == playermessages)
                {
                    PlayersListNow();
                }
                if (pageaction == PlayersOnlinePage && NetworkInfo.HasLayer)
                {
                    var matchmaker = NetworkLayerManager.Layer.Matchmaker;
                    if (matchmaker != null)
                    {
                        PlayersOnlinePage.RemoveAll();
                        PlayersOnlines.Clear();

                        if (!MenuMatchmaking.SearchBarElement.gameObject.active && !MenuMatchmaking.BrowserPage.gameObject.active)
                        {
                            matchmaker.RequestLobbies(info =>
                            {
                                if (info.Lobbies != null && info.Lobbies.Any())
                                {
                                    foreach (var lobby in info.Lobbies)
                                    {
                                        foreach (var p in lobby.Metadata.LobbyInfo.PlayerList.Players)
                                        {
                                            if (PlayersOnlines.Any(x => x.PlatformID == p.PlatformID))
                                                continue;

                                            PlayersOnlines.Add(new PlayerInfo
                                            {
                                                Username = StripColorTags(p.Username),
                                                PlatformID = p.PlatformID,
                                                Nickname = p.Nickname,
                                                Description = p.Description,
                                                PermissionLevel = p.PermissionLevel,
                                                AvatarModID = p.AvatarModID,
                                                AvatarTitle = p.AvatarTitle
                                            });
                                        }
                                    }

                                    foreach (var playersonlineline in PlayersOnlines)
                                    {
                                        var name = string.IsNullOrEmpty(playersonlineline.Nickname)
                                            ? playersonlineline.Username
                                            : playersonlineline.Nickname;

                                        PlayersOnlinePage.CreateFunction($"+ {name}", Color.green, () =>
                                        {
                                            if (!NetworkHelper.IsBanned(playersonlineline.PlatformID))
                                            {
                                                BanInfo item = new()
                                                {
                                                    Player = new PlayerInfo { Username = playersonlineline.Username, Nickname = playersonlineline.Nickname, PlatformID = playersonlineline.PlatformID, Description = playersonlineline.Description, AvatarModID = playersonlineline.AvatarModID, AvatarTitle = playersonlineline.AvatarTitle },
                                                    Reason = $"Manually Banned [{FusionProtectorInfo.ClientName}]"
                                                };
                                                BanManager.BanList.Bans.RemoveAll((BanInfo info2) => info2.Player.PlatformID == playersonlineline.PlatformID);
                                                BanManager.BanList.Bans.Add(item);
                                                DataSaver.WriteJsonToFile("bans.json", BanManager.BanList);

                                                NotificationNow(FusionProtectorInfo.ClientName, "Banned Player", NotificationType.SUCCESS);
                                            }
                                            else
                                            {
                                                BanManager.Pardon(playersonlineline.PlatformID);
                                                NotificationNow(FusionProtectorInfo.ClientName, "UnBanned Player", NotificationType.SUCCESS);

                                            }
                                        });


                                    }

                                    NotificationNow(
                                        FusionProtectorInfo.ClientName,
                                        $"Players Online : {PlayersOnlines.Count}",
                                        NotificationType.SUCCESS,
                                        1.5f
                                    );
                                }
                            });
                        }
                        }
                }
                if (pageaction == ProtectionLogs)
                {
                    ProtectionLogs.RemoveAll();

                    ProtectionLogs.CreateFunction("Clear Protection Logs", Color.yellow, () =>
                    {
                        ClientExploitLogs.Clear();
                    });
                    ProtectionLogs.CreateFunction("Clear Spawn Logs", Color.yellow, () =>
                    {
                        SpawnLogs.Clear();
                    });
                    ProtectionLogs.CreateFunction("Clear Avi Switch Logs", Color.yellow, () =>
                    {
                        PlayeravatarStuff.Clear();
                    });
                    ProtectionLogs.Logsettings("Melon Console Spawn Logs", Color.cyan, ref spawnlogsmelonlog, enabled =>
                    {
                        spawnlogsmelonlog = enabled;
                    });

                    foreach (var (exploiterNickname, exploiterUsername, exploiterPlatformId, Exploitmessage) in ClientExploitLogs)
                    {

                        var clientexppage = ProtectionLogs.CreatePage($"+ {Exploitmessage}", Color.green);
                        var exploiter = clientexppage.CreatePage($"+ Exploiters", Color.green);

                        exploiter.CreateFunction("Exploiter :" + exploiterNickname + $" [{exploiterUsername}] ({exploiterPlatformId})", Color.yellow, () =>
                        {
                            var stringfornoti = $"{exploiterNickname} [{exploiterUsername}] [{exploiterPlatformId}] Exploit : [{Exploitmessage}]";
                            GUIUtility.systemCopyBuffer = Exploitmessage;
                            NotificationNow(FusionProtectorInfo.ClientName, "Copied Exploit Details To Clipboard", NotificationType.SUCCESS, 2.5f);
                        });


                    }
                }
                if (pageaction == statskicknow)
                {
                    statskicknow.RemoveAll();
                    void PageOptions(Page PageyNow, StatsKickerPresets presetnow)
                    {

                        var stats = new (string name, System.Func<string> getter, System.Action<string> setter)[]
                        {
                            ("Threshold Height", () => presetnow.Height, val => presetnow.Height = val),
                            ("Threshold Mass Arm", () => presetnow.MassArm, val => presetnow.MassArm = val),
                            ("Threshold Mass Chest", () => presetnow.MassChest, val => presetnow.MassChest = val),
                            ("Threshold Mass Head", () => presetnow.MassHead, val => presetnow.MassHead = val),
                            ("Threshold Mass Leg", () => presetnow.MassLeg, val => presetnow.MassLeg = val),
                            ("Threshold Mass Pelvis", () => presetnow.MassPelvis, val => presetnow.MassPelvis = val),
                            ("Threshold Mass Total", () => presetnow.MassTotal, val => presetnow.MassTotal = val),
                            ("Threshold Speed", () => presetnow.Speed, val => presetnow.Speed = val),
                            ("Threshold Strength Lower", () => presetnow.StrengthLower, val => presetnow.StrengthLower = val),
                            ("Threshold Strength Upper", () => presetnow.StrengthUpper, val => presetnow.StrengthUpper = val),
                            ("Threshold Vitality", () => presetnow.Vitality, val => presetnow.Vitality = val)
                        };

                        foreach (var (name, getter, setter) in stats)
                        {
                            PageyNow.CreateString(name, Color.yellow, getter(), (stringy) =>
                            {

                                if (float.TryParse(stringy, out var floatVal))
                                {
                                    setter(stringy);
                                    NotificationNow(FusionProtectorInfo.ClientName, $"Set {name} value to {floatVal}", NotificationType.SUCCESS, 3.5f);

                                }
                                else
                                {
                                    NotificationNow(FusionProtectorInfo.ClientName, "Failed Needs To Be A Int!!!!!!!!!!!", NotificationType.ERROR, 2.0f);
                                }

                                StatsKickerPresets.SavePresets();
                            });
                        }
                    }

                    foreach (var presetnow in StatsKickerPresets.StatsKickerPresetz)
                    {
                        var pagetemp = statskicknow.CreatePage("+ [SK] "+presetnow.TitleOfPreset, Color.green);
                        pagetemp.CreateString("Edit Preset Name", Color.white, presetnow.TitleOfPreset, (stringy) => {

                            presetnow.EditPresetName(stringy);
                            Menu.OpenPage(statskicknow);
                        });

                        PageOptions(pagetemp, presetnow);

                        pagetemp.CreateFunction("Apply Preset", Color.yellow, () =>
                        {
                            var ApplyIntoPreset = new string[]
                            {
                                presetnow.Height,
                                presetnow.MassArm,
                                presetnow.MassChest,
                                presetnow.MassHead,
                                presetnow.MassLeg,
                                presetnow.MassPelvis,
                                presetnow.MassTotal,
                                presetnow.Speed,
                                presetnow.StrengthLower,
                                presetnow.StrengthUpper,
                                presetnow.Vitality
                            };
                            StatsKickerPresets.CurrentPreset = ApplyIntoPreset;

                            NotificationNow(FusionProtectorInfo.ClientName, $"Applied Stats Kicker Preset [{presetnow.TitleOfPreset}]!", NotificationType.SUCCESS, 3.5f);
                            StatsKickerPresets.SavePresets();
                        });
                        pagetemp.CreateFunction("Remove Preset", Color.yellow, () =>
                        {
                            presetnow.RemovePreset();
                            Menu.OpenPage(statskicknow);
                        });


                    }
                }
                if (pageaction == teleportersnow)
                {
                    teleportersnow.RemoveAll();
                    foreach (var teleporter in TeleporterManager.Teleportersnowx)
                    {
                        var pagetemp = teleportersnow.CreatePage("+ "+teleporter.Map, Color.green);
                        var pagetemp2 = pagetemp.CreatePage("+ [TP] "+teleporter.TitleOfTeleporter, Color.green);

                        pagetemp2.CreateString("Edit Preset Name", Color.white, teleporter.TitleOfTeleporter, (stringy) => {

                            teleporter.EditPresetName(stringy);
                            Menu.OpenPage(teleportersnow);
                        });


                        pagetemp2.CreateFunction("Teleport To", Color.yellow, () =>
                        {
                            if (!GamemodeManager.IsGamemodeStarted)
                            {
                                teleporter.TeleportToIt();
                            }
                        });
                        pagetemp2.CreateFunction("Set As Spawn Point", Color.yellow, () =>
                        {
                            if (!GamemodeManager.IsGamemodeStarted)
                            {
                                Player.RigManager.checkpointPosition = teleporter.Position;
                                NotificationNow(FusionProtectorInfo.ClientName, "Set Spawn Point!", NotificationType.SUCCESS, 3.5f);
                            }
                        });

                    }
                }
                if (pageaction == loadoutpagesnow)
                {
                    loadoutpagesnow.RemoveAll();


                    void PageOptions(Page pageynow, string slotname, InventoryPage Now)
                    {
                        pageynow.CreateFunction("Remove", Color.yellow, () =>
                        {
                            NotificationNow(FusionProtectorInfo.ClientName, $"Removed {Now.GetSlotBarcode(slotname)} From Slot", NotificationType.SUCCESS, 3.5f);
                            Now.EditSlotBarcode(slotname, "Empty");
                            Menu.OpenPage(loadoutpagesnow);
                        });

                        pageynow.CreateFunction("Add Left Hand Item To Slot", Color.yellow, () =>
                        {
                            var leftHandEntity = JR_YourGetHand(WhichHand.Left)?.JR_GetMarrowEntity();
                            var leftBarcode = leftHandEntity?.JR_GetBarcodeID();

                            if (!string.IsNullOrEmpty(leftBarcode) && IsBarcodeInGame(leftBarcode))
                            {
                                Now.EditSlotBarcode(slotname, leftBarcode);
                                Menu.OpenPage(loadoutpagesnow);
                            }
                        });

                        pageynow.CreateFunction("Add Right Hand Item To Slot", Color.yellow, () =>
                        {
                            var rightHandEntity = JR_YourGetHand(WhichHand.Right)?.JR_GetMarrowEntity();
                            var rightBarcode = rightHandEntity?.JR_GetBarcodeID();

                            if (!string.IsNullOrEmpty(rightBarcode) && IsBarcodeInGame(rightBarcode))
                            {
                                Now.EditSlotBarcode(slotname, rightBarcode);
                                Menu.OpenPage(loadoutpagesnow);
                            }
                        });

                        pageynow.CreateFunction("Add Clipboard Barcode Item To Slot", Color.yellow, () =>
                        {
                            var clipBarcode = GUIUtility.systemCopyBuffer?.Trim();
                            if (!string.IsNullOrEmpty(clipBarcode) && IsBarcodeInGame(clipBarcode))
                            {
                                Now.EditSlotBarcode(slotname, clipBarcode);
                                Menu.OpenPage(loadoutpagesnow);
                            }
                        });
                    }

                    foreach (var pagenow in InventoryPage.InventoryPresets)
                    {
                        if (pagenow == null) continue;

                        var pagetemp = loadoutpagesnow.CreatePage("+ [L] " + (pagenow.TitleOfPreset ?? "No Title"), Color.green);

                        pagetemp.CreateString("Edit Preset Name", Color.white, pagenow.TitleOfPreset, (stringy) => {

                            pagenow.EditPresetName(stringy);
                            Menu.OpenPage(loadoutpagesnow);
                        });




                        foreach (var items in pagenow.Slots)
                        {
                            string slotName = items.Key;
                            string barcode = items.Value;

                            var tempy1 = pagetemp.CreatePage($"+ [{slotName}] " + (new SpawnableCrateReference(barcode)?.Crate?.name ?? "Empty"), Color.green);
                            PageOptions(tempy1, slotName, pagenow);

                        }



                        pagetemp.CreateFunction("Load Loadout", Color.yellow, () =>
                        {
                            pagenow.LoadIntoPlayer();
                        });

                        pagetemp.CreateFunction("Remove Loadout", Color.yellow, () =>
                        {
                            pagenow.RemovePreset();
                            Menu.OpenPage(loadoutpages);
                        });
                    }
                }
                if (pageaction == bodylognowpagexx)
                {
                    bodylognowpagexx.RemoveAll();


                    string SafeCrateName(string barcode)
                    {
                        if (string.IsNullOrEmpty(barcode))
                            return "EMPTY";

                        var crateRef = new AvatarCrateReference(barcode);
                        var name = crateRef?.Crate?.name;

                        return StripColorTags(name ?? "EMPTY");
                    }

                    void PageOptionsAvatar(Page pageynow, BodyLogPage PageNow, int slotindex)
                    {
                        if (pageynow == null || PageNow == null)
                            return;

                        pageynow.CreateFunction("Remove", Color.yellow, () =>
                        {
                            PageNow.ClearSlot(slotindex);
                            Menu.OpenPage(bodylognowpage);
                        });

                        pageynow.CreateFunction("Add Current Avatar To Slot", Color.yellow, () =>
                        {
                            string id = JR_YourAvatarBarcodeID();

                            if (!string.IsNullOrEmpty(id) && IsAvatarCrateExist(id))
                            {
                                PageNow.EditSlot(slotindex, id);
                                Menu.OpenPage(bodylognowpage);
                            }
                        });

                        pageynow.CreateFunction("Add Clipboard Barcode Avatar To Slot", Color.yellow, () =>
                        {
                            var clip = GUIUtility.systemCopyBuffer?.Trim();

                            if (!string.IsNullOrEmpty(clip) && IsAvatarCrateExist(clip))
                            {
                                PageNow.EditSlot(slotindex, clip);
                                Menu.OpenPage(bodylognowpage);
                            }
                        });
                    }


                    if (BodyLogPage.BodyLogPages == null)
                        return;

                    foreach (var pagenow in BodyLogPage.BodyLogPages)
                    {
                        if (pagenow == null)
                            continue;

                        var pagetemp = bodylognowpagexx.CreatePage("+ [B] " + (pagenow.TitleOfPreset ?? "Unnamed"), Color.green);


                        var sharepresetbodylog = pagetemp.CreatePage("+ [B] Share Preset", Color.green);

                        Menu.OnPageOpened += pageaction =>
                        {
                            if (pageaction == sharepresetbodylog)
                            {
                                sharepresetbodylog.RemoveAll();


                                foreach(var xxxxm in NetworkPlayers())
                                {
                                    sharepresetbodylog.CreateFunction(CleanedNAME(xxxxm), Color.yellow, () =>
                                    {
                                        if (TimeReferences.TimeSinceStartup - ShareBodyLogPageMessage._timeOfRequest <= ShareBodyLogPageMessage._requestCooldown)
                                        {
                                            return;
                                        }

                                        var data = ShareBodyLogPageData.Create(PlayerIDManager.LocalSmallID, pagenow.TitleOfPreset, pagenow.Slot1, pagenow.Slot2, pagenow.Slot3, pagenow.Slot4,pagenow.Slot5,pagenow.Slot6, pagenow.ModIoID1, pagenow.ModIoID2, pagenow.ModIoID3, pagenow.ModIoID4, pagenow.ModIoID5, pagenow.ModIoID6);

                                        MessageRelay.RelayModule<ShareBodyLogPageMessage, ShareBodyLogPageData>(data, new MessageRoute(xxxxm.JR_SmallID(), NetworkChannel.Reliable));

                                        NotificationNow(FusionProtectorInfo.ClientName,$"Sent Bodylog Page [{data.TitleOfPreset}] To {CleanedNAME(xxxxm)}",NotificationType.WARNING,4.0f);
                                    });

                                }


                            }
                        };


                        pagetemp.CreateString("Edit Preset Name", Color.white, pagenow.TitleOfPreset, (stringy) => {

                            pagenow.EditPresetName(stringy);
                            Menu.OpenPage(bodylognowpage);
                        });




                        string[] slots =
                        {
            pagenow.Slot1,
            pagenow.Slot2,
            pagenow.Slot3,
            pagenow.Slot4,
            pagenow.Slot5,
            pagenow.Slot6
        };

                        for (int i = 0; i < slots.Length; i++)
                        {
                            var slotPage = pagetemp.CreatePage($"+ B[{i + 1}] " + SafeCrateName(slots[i]), Color.green);
                            PageOptionsAvatar(slotPage, pagenow, i + 1);
                        }

                        pagetemp.CreateFunction("Apply Entire Current Bodylog To Preset", Color.yellow, () =>
                        {
                            var ApplyIntoPreset = new string[6]
                            {
                "EMPTY","EMPTY","EMPTY","EMPTY","EMPTY","EMPTY"
                            };

                            int index = 0;

                            var (bodylogreturn, Outerring) = JR_BodyLog(Player.PhysicsRig);
                            var crates = bodylogreturn?.avatarCrateRefs;

                            if (crates != null)
                            {
                                foreach (var crate in crates)
                                {
                                    if (index >= 6)
                                        break;

                                    var name = crate?.Crate?.Barcode?.ID ?? "EMPTY";
                                    ApplyIntoPreset[index++] = name;
                                }
                            }

                            pagenow.Slot1 = ApplyIntoPreset[0];
                            pagenow.Slot2 = ApplyIntoPreset[1];
                            pagenow.Slot3 = ApplyIntoPreset[2];
                            pagenow.Slot4 = ApplyIntoPreset[3];
                            pagenow.Slot5 = ApplyIntoPreset[4];
                            pagenow.Slot6 = ApplyIntoPreset[5];

                            BodyLogPage.SavePresets();
                            Menu.OpenPage(bodylognowpage);

                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                $"Applied Entire Current Bodylog To Preset [{pagenow.TitleOfPreset}]!",
                                NotificationType.SUCCESS,
                                3.5f);
                        });

                        pagetemp.CreateFunction("Apply/Refresh Preset", Color.yellow, () =>
                        {
                            ChangeBodyLogAvatarSlot(1, pagenow.Slot1 ?? "EMPTY", false);
                            ChangeBodyLogAvatarSlot(2, pagenow.Slot2 ?? "EMPTY", false);
                            ChangeBodyLogAvatarSlot(3, pagenow.Slot3 ?? "EMPTY", false);
                            ChangeBodyLogAvatarSlot(4, pagenow.Slot4 ?? "EMPTY", false);
                            ChangeBodyLogAvatarSlot(5, pagenow.Slot5 ?? "EMPTY", false);
                            ChangeBodyLogAvatarSlot(6, pagenow.Slot6 ?? "EMPTY", false);

                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                $"Applied Bodylog Page [{pagenow.TitleOfPreset}]!",
                                NotificationType.SUCCESS,
                                3.5f);
                            pagenow.ApplyPreset();
                            BodyLogPage.SavePresets();

                            Menu.OpenPage(bodylognowpage);
                        });

                        pagetemp.CreateFunction("Remove Preset", Color.yellow, () =>
                        {
                            pagenow.RemovePreset();
                        });
                    }
                }
                if (pageaction == cheatspresetsnow)
                {
                    cheatspresetsnow.RemoveAll();

                    void PageOptions(Page pageynow, CreateCheatToolsPreset Now, int slotindex)
                    {
                        CreateCheatToolsPreset.Item itemnow = new();
                        switch (slotindex)
                        {
                            case 1: itemnow = Now.Item1; break;
                            case 2: itemnow = Now.Item2; break;
                            case 3: itemnow = Now.Item3; break;
                            case 4: itemnow = Now.Item4; break;
                            case 5: itemnow = Now.Item5; break;
                        }

                        pageynow.CreateBool("Local Spawning", Color.yellow, itemnow.LocalSpawn, (enabled) =>
                        {
                            Now.EditSlotLocalSpawn(slotindex, enabled);
                            Menu.OpenPage(cheatspresetsnow);
                        }).Value = itemnow.LocalSpawn;

                        pageynow.CreateFunction("Remove", Color.yellow, () =>
                        {
                            Now.ClearDevSlot(slotindex);
                            Menu.OpenPage(cheatspresetsnow);
                        });
                        pageynow.CreateFunction("Add Left Hand Item To Slot", Color.yellow, () =>
                        {
                            if (IsBarcodeInGame(JR_YourGetHand(WhichHand.Left).JR_GetMarrowEntity().JR_GetBarcodeID()))
                            {
                                var leftnow = JR_YourGetHand(WhichHand.Left).JR_GetMarrowEntity().JR_GetBarcodeID();
                                Now.EditDevSlot(slotindex, leftnow);
                                Menu.OpenPage(cheatspresetsnow);
                            }
                        });

                        pageynow.CreateFunction("Add Right Hand Item To Slot", Color.yellow, () =>
                        {
                            if (IsBarcodeInGame(JR_YourGetHand(WhichHand.Right).JR_GetMarrowEntity().JR_GetBarcodeID()))
                            {
                                var rightnow = JR_YourGetHand(WhichHand.Right).JR_GetMarrowEntity().JR_GetBarcodeID();
                                Now.EditDevSlot(slotindex, rightnow);
                                Menu.OpenPage(cheatspresetsnow);
                            }
                        });

                        pageynow.CreateFunction("Add Clipboard Barcode Item To Slot", Color.yellow, () =>
                        {
                            if (IsBarcodeInGame(GUIUtility.systemCopyBuffer.Trim()))
                            {
                                var clippynow = GUIUtility.systemCopyBuffer.Trim();
                                Now.EditDevSlot(slotindex, clippynow);
                                Menu.OpenPage(cheatspresetsnow);
                            }
                        });
                    }

                    foreach (var cheatpresetty in CreateCheatToolsPreset.CheatPresets)
                    {
                        var pagetemp = cheatspresetsnow.CreatePage("+ [C] "+cheatpresetty.TitleOfPreset, Color.green);


                        var SHARECHEATTY = pagetemp.CreatePage("+ [C] Share Preset", Color.green);

                        Menu.OnPageOpened += pageaction =>
                        {
                            if (pageaction == SHARECHEATTY)
                            {
                                SHARECHEATTY.RemoveAll();


                                foreach (var xxxxm in NetworkPlayers())
                                {
                                    SHARECHEATTY.CreateFunction(CleanedNAME(xxxxm), Color.yellow, () =>
                                    {
                                        if (TimeReferences.TimeSinceStartup - ShareDevToolPresetMessage._timeOfRequest <= ShareDevToolPresetMessage._requestCooldown)
                                        {
                                            return;
                                        }

                                        var data = ShareDevToolPresetData.Create(PlayerIDManager.LocalSmallID,
                                            cheatpresetty.TitleOfPreset,
                                            cheatpresetty.Item1.BarcodeId,
                                            cheatpresetty.Item1.ModIoID,
                                            cheatpresetty.Item1.SpawnableName,
                                            cheatpresetty.Item1.LocalSpawn,
                                            cheatpresetty.Item2.BarcodeId,
                                            cheatpresetty.Item2.ModIoID,
                                            cheatpresetty.Item2.SpawnableName,
                                            cheatpresetty.Item2.LocalSpawn,
                                            cheatpresetty.Item3.BarcodeId,
                                            cheatpresetty.Item3.ModIoID,
                                            cheatpresetty.Item3.SpawnableName,
                                            cheatpresetty.Item3.LocalSpawn,
                                            cheatpresetty.Item4.BarcodeId,
                                            cheatpresetty.Item4.ModIoID,
                                            cheatpresetty.Item4.SpawnableName,
                                            cheatpresetty.Item4.LocalSpawn,
                                            cheatpresetty.Item5.BarcodeId,
                                            cheatpresetty.Item5.ModIoID,
                                            cheatpresetty.Item5.SpawnableName,
                                            cheatpresetty.Item5.LocalSpawn);

                                        MessageRelay.RelayModule<ShareDevToolPresetMessage, ShareDevToolPresetData>(data, new MessageRoute(xxxxm.JR_SmallID(), NetworkChannel.Reliable));

                                        NotificationNow(FusionProtectorInfo.ClientName, $"Sent DevToolPreset [{data.TitleOfPreset}] To {CleanedNAME(xxxxm)}", NotificationType.WARNING, 4.0f);
                                    });

                                }


                            }
                        };



                        pagetemp.CreateString("Edit Preset Name", Color.white, cheatpresetty.TitleOfPreset,(stringy) => {

                            cheatpresetty.EditPresetName(stringy);
                            Menu.OpenPage(cheatspresetsnow);
                        });



                        var pagetemp1 = pagetemp.CreatePage("+ C[1] "+cheatpresetty.Item1.SpawnableName, Color.green);
                        PageOptions(pagetemp1, cheatpresetty, 1);

                        var pagetemp2 = pagetemp.CreatePage("+ C[2] "+cheatpresetty.Item2.SpawnableName, Color.green);
                        PageOptions(pagetemp2, cheatpresetty, 2);

                        var pagetemp3 = pagetemp.CreatePage("+ C[3] "+cheatpresetty.Item3.SpawnableName, Color.green);
                        PageOptions(pagetemp3, cheatpresetty, 3);

                        var pagetemp4 = pagetemp.CreatePage("+ C[4] "+cheatpresetty.Item4.SpawnableName, Color.green);
                        PageOptions(pagetemp4, cheatpresetty, 4);

                        var pagetemp5 = pagetemp.CreatePage("+ C[5] "+cheatpresetty.Item5.SpawnableName, Color.green);
                        PageOptions(pagetemp5, cheatpresetty, 5);

                        pagetemp.CreateFunction("Apply/Save/Edit Preset", Color.yellow, () =>
                        {
                            CreateCheatToolsPreset.CurrentPresetNow = cheatpresetty;

                            SpawnableCrateReference[] PresetNow = new SpawnableCrateReference[]
{
                                new (CreateCheatToolsPreset.CurrentPresetNow.Item1.BarcodeId),
                                new (CreateCheatToolsPreset.CurrentPresetNow.Item2.BarcodeId),
                                new (CreateCheatToolsPreset.CurrentPresetNow.Item3.BarcodeId),
                                new (CreateCheatToolsPreset.CurrentPresetNow.Item4.BarcodeId),
                                new (CreateCheatToolsPreset.CurrentPresetNow.Item5.BarcodeId)
};

                            InstanceOfIt.crates = PresetNow;
                            NotificationNow(FusionProtectorInfo.ClientName, $"Applied Cheat Tool Preset Edit [{cheatpresetty.TitleOfPreset}]! Now Re-Apply Preset To Take Effect!", NotificationType.SUCCESS, 3.5f);
                            CreateCheatToolsPreset.SavePresets();
                        });
                        pagetemp.CreateFunction("Remove Preset", Color.yellow, () =>
                        {
                            cheatpresetty.RemovePreset();
                            Menu.OpenPage(cheatspresetsnow);
                        });


                    }
                }
                if (pageaction == colorpresetsnow)
                {
                    colorpresetsnow.RemoveAll();

                    bool TryParseRGBA(string text, out float r, out float g, out float b, out float a)
                    {
                        r = g = b = 0f;
                        a = 255f;

                        if (string.IsNullOrWhiteSpace(text))
                            return false;

                        text = text.Trim().ToLower();

                        if (text.StartsWith("rgba"))
                            text = text.Substring(4);
                        else if (text.StartsWith("rgb"))
                            text = text.Substring(3);

                        text = text.TrimStart('(', '[', '{');
                        text = text.TrimEnd(')', ']', '}');

                        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length != 3 && parts.Length != 4)
                            return false;

                        if (!float.TryParse(parts[0].Trim(), out r)) return false;
                        if (!float.TryParse(parts[1].Trim(), out g)) return false;
                        if (!float.TryParse(parts[2].Trim(), out b)) return false;

                        if (parts.Length == 4)
                        {
                            if (!float.TryParse(parts[3].Trim(), out a))
                                return false;
                        }

                        bool Valid(float v) => v >= 0f && v <= 255f;

                        return Valid(r) && Valid(g) && Valid(b) && Valid(a);
                    }


                    void PageOptionNow(Page PageyNow, BodyLogRadialMenuColorPreset NowPreset, string propertystring)
                    {
                        PageyNow.CreateString("Color R", Color.yellow, COLORR, (stringy) =>
                        {
                            if (float.TryParse(stringy, out var culR) && culR >= 0 && culR <= 255)
                            {
                                COLORR = stringy;

                                NowPreset.SetValue(propertystring + "_R", culR);

                                NotificationNow(FusionProtectorInfo.ClientName,
                                    "Set Value Into Color R",
                                    NotificationType.SUCCESS, 3.5F);
                            }
                            else
                            {
                                NotificationNow(FusionProtectorInfo.ClientName,
                                    "Value must be between 0 and 255!",
                                    NotificationType.ERROR, 2.0f);
                            }
                        });

                        PageyNow.CreateString("Color G", Color.yellow, COLORG, (stringy) =>
                        {
                            if (float.TryParse(stringy, out var culG) && culG >= 0 && culG <= 255)
                            {
                                COLORG = stringy;

                                NowPreset.SetValue(propertystring + "_G", culG);

                                NotificationNow(FusionProtectorInfo.ClientName,
                                    "Set Value Into Color G",
                                    NotificationType.SUCCESS, 3.5F);
                            }
                            else
                            {
                                NotificationNow(FusionProtectorInfo.ClientName,
                                    "Value must be between 0 and 255!",
                                    NotificationType.ERROR, 2.0f);
                            }
                        });

                        PageyNow.CreateString("Color B", Color.yellow, COLORB, (stringy) =>
                        {
                            if (float.TryParse(stringy, out var culB) && culB >= 0 && culB <= 255)
                            {
                                COLORB = stringy;

                                NowPreset.SetValue(propertystring + "_B", culB);

                                NotificationNow(FusionProtectorInfo.ClientName,
                                    "Set Value Into Color B",
                                    NotificationType.SUCCESS, 3.5F);
                            }
                            else
                            {
                                NotificationNow(FusionProtectorInfo.ClientName,
                                    "Value must be between 0 and 255!",
                                    NotificationType.ERROR, 2.0f);
                            }
                        });

                        PageyNow.CreateString("Transparency", Color.yellow, COLORA, (stringy) =>
                        {
                            if (float.TryParse(stringy, out var culA) && culA >= 0 && culA <= 255)
                            {
                                COLORA = stringy;

                                NowPreset.SetValue(propertystring + "_A", culA);

                                NotificationNow(FusionProtectorInfo.ClientName,
                                    "Set Value Into Transparency",
                                    NotificationType.SUCCESS, 3.5F);
                            }
                            else
                            {
                                NotificationNow(FusionProtectorInfo.ClientName,
                                    "Value must be between 0 and 255!",
                                    NotificationType.ERROR, 2.0f);
                            }
                        });

                        PageyNow.CreateFunction("Set Values From Clipboard", Color.yellow, () =>
                        {
                            if (TryParseRGBA(GUIUtility.systemCopyBuffer, out float r, out float g, out float b, out float a))
                            {

                                NowPreset.SetValue(propertystring+"_R", r);
                                COLORR = r.ToString();
                                NowPreset.SetValue(propertystring+"_G", g);
                                COLORG = g.ToString();
                                NowPreset.SetValue(propertystring+"_B", b);
                                COLORB = b.ToString();
                                NowPreset.SetValue(propertystring+"_A", a);
                                COLORA = a.ToString();

                                NotificationNow(FusionProtectorInfo.ClientName, $"Set Color Values From Clipboard R:{r} G:{g} B:{b} A:{a}", NotificationType.SUCCESS, 3.5f);
                            }
                            ApplyIt(NowPreset);
                        });

                        PageyNow.CreateFunction("Save/Edit Color", Color.yellow, () =>
                        {

                            float.TryParse(COLORR, out var cr);
                            float.TryParse(COLORG, out var cg);
                            float.TryParse(COLORB, out var cb);
                            float.TryParse(COLORA, out var ca);


                            NowPreset.SetValue(propertystring+"_R", cr);
                            NowPreset.SetValue(propertystring+"_G", cg);
                            NowPreset.SetValue(propertystring+"_B", cb);
                            NowPreset.SetValue(propertystring+"_A", ca);

                            BodyLogRadialMenuColorPreset.SavePresets();
                            NotificationNow(FusionProtectorInfo.ClientName, $"Applied Color Edit [{NowPreset.TitleOfPreset}]! Now Reopen The Radial Menu To Take Effect!", NotificationType.SUCCESS, 3.5f);

                            Menu.OpenPage(colorpresetsnow);
                            ApplyIt(NowPreset);
                        });

                    }
                    void ApplyIt(BodyLogRadialMenuColorPreset colorpreset)
                    {
                        var ApplyIntoPreset = new string[]
                     {
                                colorpreset.BodyLogColor_R.ToString(),
                                colorpreset.BodyLogColor_G.ToString(),
                                colorpreset.BodyLogColor_B.ToString(),
                                colorpreset.BodyLogColor_A.ToString(),
                                colorpreset.BodyLogBallColor_R.ToString(),
                                colorpreset.BodyLogBallColor_G.ToString(),
                                colorpreset.BodyLogBallColor_B.ToString(),
                                colorpreset.BodyLogBallColor_A.ToString(),
                                colorpreset.BodyLogLineColor_R.ToString(),
                                colorpreset.BodyLogLineColor_G.ToString(),
                                colorpreset.BodyLogLineColor_B.ToString(),
                                colorpreset.BodyLogLineColor_A.ToString(),
                                colorpreset.RadialMenuColor_R.ToString(),
                                colorpreset.RadialMenuColor_G.ToString(),
                                colorpreset.RadialMenuColor_B.ToString(),
                                colorpreset.RadialMenuColor_A.ToString(),
                     };
                        BodyLogRadialMenuColorPreset.CurrentPreset = ApplyIntoPreset;
                        NotificationNow(FusionProtectorInfo.ClientName, $"Applied Color Preset [{colorpreset.TitleOfPreset}]!", NotificationType.SUCCESS, 3.5f);
                        BodyLogRadialMenuColorPreset.SavePresets();
                    }



                    foreach (var colorpreset in BodyLogRadialMenuColorPreset.ColorPresets)
                    {
                        var pagetemp = colorpresetsnow.CreatePage("+ [BLRMC] "+colorpreset.TitleOfPreset, Color.green);


                        pagetemp.CreateString("Edit Preset Name", Color.white, colorpreset.TitleOfPreset, (stringy) => {

                            colorpreset.EditPresetName(stringy);
                            Menu.OpenPage(colorpresetsnow);
                        });


                        var pagetemp1 = pagetemp.CreatePage("+ [BLRMC 1] Bodylog Hologram Color", Color.green);
                        PageOptionNow(pagetemp1, colorpreset, "BodyLogColor");

                        var pagetemp2 = pagetemp.CreatePage("+ [BLRMC 2] Bodylog Ball Color", Color.green);
                        PageOptionNow(pagetemp2, colorpreset, "BodyLogBallColor");

                        var pagetemp3 = pagetemp.CreatePage("+ [BLRMC 3] Bodylog Line Color", Color.green);
                        PageOptionNow(pagetemp3, colorpreset, "BodyLogLineColor");

                        var pagetemp4 = pagetemp.CreatePage("+ [BLRMC 4] Radial Menu Color", Color.green);
                        PageOptionNow(pagetemp4, colorpreset, "RadialMenuColor");

                        pagetemp.CreateFunction("Apply/Refresh Color Preset", Color.yellow, () =>
                        {
                            ApplyIt(colorpreset);
                        });
                        pagetemp.CreateFunction("Remove Preset", Color.yellow, () =>
                        {
                            colorpreset.RemovePreset();
                            Menu.OpenPage(colorpresetsnow);
                        });


                    }
                }
                if (pageaction == FusionProfilesnow)
                {
                    FusionProfilesnow.RemoveAll();



                    foreach (var profilenowha in FusionProfilePresets.ProfilePresets)
                    {
                        var pagetemp = FusionProfilesnow.CreatePage("+ [FPP] "+profilenowha.TitleOfPreset, Color.green);

                        pagetemp.CreateString("Edit Preset Name", Color.white, profilenowha.TitleOfPreset, (stringy) => {

                            profilenowha.EditPresetName(stringy);
                            Menu.OpenPage(FusionProfilesnow);
                        });

                        pagetemp.CreateFunction("Apply Preset", Color.yellow, () =>
                        {
                            MelonCoroutines.Start(profilenowha.ApplyPreset());
                        });
                        pagetemp.CreateFunction("Replace Avatar With Current", Color.yellow, () =>
                        {
                            profilenowha.SetValue("AvatarAtTheTime", JR_YourAvatarBarcodeID());
                            NotificationNow(FusionProtectorInfo.ClientName, $"Set {profilenowha.TitleOfPreset} Avatar To Current!", NotificationType.SUCCESS, 3.5f);
                            FusionProfilePresets.SavePresets();
                        });
                        pagetemp.CreateFunction("Replace Description With Current", Color.yellow, () =>
                        {
                            profilenowha.SetValue("Description", JR_YourNetworkPlayer().JR_Description());
                            NotificationNow(FusionProtectorInfo.ClientName, $"Set {profilenowha.TitleOfPreset} Description To Current!", NotificationType.SUCCESS, 3.5f);
                            FusionProfilePresets.SavePresets();
                        });
                        pagetemp.CreateFunction("Replace Nickname With Current", Color.yellow, () =>
                        {
                            profilenowha.SetValue("Nickname", JR_YourNetworkPlayer().JR_Nickname());
                            NotificationNow(FusionProtectorInfo.ClientName, $"Set {profilenowha.TitleOfPreset} Nickname To Current!", NotificationType.SUCCESS, 3.5f);
                            FusionProfilePresets.SavePresets();
                        });

                        pagetemp.CreateFunction("Clear Bitmart Items", Color.yellow, () =>
                        {
                            profilenowha.BitMartItems = new System.Collections.Generic.List<string>();
                            FusionProfilePresets.SavePresets();
                        });

                        pagetemp.CreateFunction("Remove Preset", Color.yellow, () =>
                        {
                            profilenowha.RemovePreset();
                        });



                    }


                }
                if (pageaction == fppubs)
                {
                    AllFusionProtectorLobbies();
                }
                if (pageaction == pubs)
                {
                    AllLobbies();
                }

            };
            MultiplayerHooking.OnStartedServer+= delegate
            {
                playersavatarrefs.Clear();
                playersspawnrefs.Clear();
                PlayerSpawningStuff.Clear();
            };
            Hooking.OnMarrowGameStarted += delegate
            {
                MelonCoroutines.Start(OnStartOfGame());
            };


            Hooking.OnGripAttached += (gruppy, hund) => {
                if (removesounds)
                {
                    RemoveSoundGrip(gruppy);
                }
            };
            Hooking.OnGripDetached += (gruppy, hund) => {
                if (removesounds)
                {
                RemoveSoundGrip(gruppy);
            }
            };

        }
        internal static void CreateProtectorUI()
        {
            PageEx.boolslogged.Clear();
            PageEx.floatslogged.Clear();
            PageEx.intslogged.Clear();
            PageEx.enumvaluelogged.Clear();
            PageEx.stringslogged.Clear();
            
            string jsonText = File.ReadAllText(modiotokenfile);
            JObject settingsJson = JObject.Parse(jsonText);
            modiotoken = settingsJson["mod.io.access_token"]?.ToString();




            TryPatchit("BonelabSupport", "MarrowFusion.Bonelab.Patching.PopUpMenuViewPatches", "OnSpawnDelegate", typeof(CustomDevToolsFusion).GetMethod(nameof(CustomDevToolsFusion.Prefix)), null);
            TryPatchit("BonelabSupport", "MarrowFusion.Bonelab.Messages.BodyLogEffectMessage", "OnHandleMessage", typeof(BodyLogEffect).GetMethod(nameof(BodyLogEffect.Prefix)), null);

            IfDontHaveInstallThenDo("doge15567.BodyMallUI.Spawnable.BodyMallUI", 5469299, false);
            IfDontHaveInstallThenDo("Atlas.96.ModOMat.Spawnable.ModOMat", 4140942, false);

            FusionProtectorPage = Page.Root.CreatePage("+ "+FusionProtectorInfo.ClientName, Color.green);

            //RunScript
            Protectsettings = FusionProtectorPage.CreatePage("+ Settings", Color.cyan);
            playermessages = FusionProtectorPage.CreatePage("+ Player Senders", Color.white);

            FusionProtectorPage.CreateFunction("Revert Bodylog To Original", Color.yellow, () =>
            {
                ChangeBodyLogAvatarSlot(1, originalbodylog[0],false);
                ChangeBodyLogAvatarSlot(2, originalbodylog[1],false);
                ChangeBodyLogAvatarSlot(3, originalbodylog[2],false);
                ChangeBodyLogAvatarSlot(4, originalbodylog[3],false);
                ChangeBodyLogAvatarSlot(5, originalbodylog[4],false);
                ChangeBodyLogAvatarSlot(6, originalbodylog[5], true);
            });
            FusionProtectorPage.CreateFunction("Revert Profile To Original", Color.yellow, () =>
            {
                LocalPlayer.Metadata.Metadata.TrySetMetadata("Nickname", originalprofiledetails.Nickname);
                LocalPlayer.Metadata.Metadata.TrySetMetadata("Username", originalprofiledetails.Username);
                LocalPlayer.Metadata.Metadata.TrySetMetadata("Description", originalprofiledetails.Description);
                NotificationNowAlways(FusionProtectorInfo.ClientName,"Reverted Profile Back To Original",NotificationType.SUCCESS,3.5f);
            });

            FusionProtectorPage.LogsettingsEnum("Local Despawn All Filter", Color.yellow, ref DespawnerAllReal2, enabled =>
            {
                DespawnerAllReal2 = (DespawnerAll)enabled;
            });
            FusionProtectorPage.CreateFunction("Locally Despawn All", Color.yellow, () =>
            {
                DespawnAll(DespawnerAllReal2, true);

            });


            permissioneditornow = FusionProtectorPage.CreatePage("+ Permission Editor", Color.white);
           
            playeroptions = FusionProtectorPage.CreatePage("+ Online Player Options", Color.white);
            
            
            PlayersOnlinePage = playeroptions.CreatePage("+ Ban/UnBan Players Online", Color.white, 30);
            playeroptions.LogsettingsString("Find Player By Steam ID", Color.yellow, ref findem, (stringy) =>
            {
                findem = stringy;
                if (ulong.TryParse(findem, out var longfind))
                {
                    FindPlayersLobbyFromPlayerSteamID(longfind, (boolx, playerinfo) =>
                    {
                        Menu.DisplayDialog(
                            "Fusion Info By Steam ID!",
                            $"Username: {playerinfo.Username}\n" +
                            $"Nickname: {(string.IsNullOrEmpty(playerinfo.Nickname) ? "N/A" : playerinfo.Nickname)}\n" +
                            $"Avatar Mod ID: {playerinfo.AvatarModID}\n" +
                            $"Avatar Title: {playerinfo.AvatarTitle}"
                        );
                    });
                }
            }).SetTooltip("Finds Players Active Fusion Profile If They Are Online ATM Then Displays It To You...");
            playeroptions.LogsettingsString("Steam ID To Ban", Color.yellow, ref SteamIDSearch, (stringy) =>
            {
                if (ulong.TryParse(stringy, out var temp))
                {
                    SteamIDSearch = stringy;
                    NotificationNow(FusionProtectorInfo.ClientName, $"Set Value {SteamIDSearch}", NotificationType.SUCCESS, 2.0f);

                }
                else
                {
                    NotificationNow(FusionProtectorInfo.ClientName, "Failed Needs To Be A ULONG AKA STEAM ID!!!!!!!!!!!", NotificationType.ERROR, 2.0f);
                }
            });
            playeroptions.CreateFunction("UnBan/Ban Steam ID From Your Lobby", Color.yellow, () =>
            {
                var realid = SteamIDSearch.Trim();


                if (ulong.TryParse(realid, out var longynow))
                {
                    FindPlayersLobbyFromPlayerSteamID(longynow, (found, playernowinfo) =>
                    {

                        if (found)
                        {
                            if (!NetworkHelper.IsBanned(longynow))
                            {
                                BanInfo item = new()
                                {
                                    Player = playernowinfo,
                                    Reason = $"Manually Banned [{FusionProtectorInfo.ClientName}]"
                                };
                                BanManager.BanList.Bans.RemoveAll((BanInfo info2) => info2.Player.PlatformID == longynow);
                                BanManager.BanList.Bans.Add(item);
                                DataSaver.WriteJsonToFile("bans.json", BanManager.BanList);

                                NotificationNow(FusionProtectorInfo.ClientName, "Banned Player", NotificationType.SUCCESS);
                            }
                            else
                            {
                                BanManager.Pardon(longynow);
                                NotificationNow(FusionProtectorInfo.ClientName, "UnBanned Player", NotificationType.SUCCESS);
                            }
                        }
                        else
                        {
                            if (!NetworkHelper.IsBanned(longynow))
                            {
                                BanInfo item = new()
                                {
                                    Player = new PlayerInfo { Username = "Fusion Protector Banned", Nickname = "Fusion Protector Banned", PlatformID = longynow, Description = "Fusion Protector Banned", AvatarModID = -1, AvatarTitle = "Fusion Protector Banned" },
                                    Reason = $"Manually Banned [{FusionProtectorInfo.ClientName}]"
                                };
                                BanManager.BanList.Bans.RemoveAll((BanInfo info2) => info2.Player.PlatformID == longynow);
                                BanManager.BanList.Bans.Add(item);
                                DataSaver.WriteJsonToFile("bans.json", BanManager.BanList);

                                NotificationNow(FusionProtectorInfo.ClientName, "Banned Player", NotificationType.SUCCESS);
                            }
                            else
                            {
                                BanManager.Pardon(longynow);
                                NotificationNow(FusionProtectorInfo.ClientName, "UnBanned Player", NotificationType.SUCCESS);
                            }
                        }


                    });

                }
            });

            OnlineFriends = playeroptions.CreatePage("+ Online Friends", Color.white);

            fppubs = playeroptions.CreatePage("+ Fusion Protected Lobbies", Color.white);
            pubs = playeroptions.CreatePage("+ All Lobbies", Color.cyan);

            HOSTONLYPGE = FusionProtectorPage.CreatePage("+ [HOST ONLY]", Color.white);
            OwnerOnlyPg = FusionProtectorPage.CreatePage("+ [OWNER ONLY]", Color.white);
            OPERATORPG = FusionProtectorPage.CreatePage("+ [OPERATOR ONLY]", Color.white);

            unblockingnow = FusionProtectorPage.CreatePage("+ Unblock Stuff", Color.white);
            WarnedSpawnablesnow = unblockingnow.CreatePage("+ Warned Spawnables", Color.white);
            modidblockednow = unblockingnow.CreatePage("+ Block This Mod.IO Mod Completely", Color.white);
            BlockedSpawnablesnow = unblockingnow.CreatePage("+ Block This Spawnable", Color.white);
            SpawnablesKicknow = unblockingnow.CreatePage("+ Kick If Spawned", Color.white);
            blockentirepalletnow = unblockingnow.CreatePage("+ Block Pallet Completely", Color.white);
            blockentireauthornow = unblockingnow.CreatePage("+ Block Author Of Spawnable", Color.white);

            selfrestrictions = FusionProtectorPage.CreatePage("+ Self Restrictions", Color.white);
            selfrestrictions.Logsettings("Disable Wind SFX", Color.cyan, ref disablewindsfx, enabled =>
            {
              
                if (!disablewindsfx && enabled)
                {
                    var icons = GameObject.FindObjectsOfType<WindBuffetSFX>();
                    foreach (var icon in icons)
                    {
                        icon.windBuffetClip = null;
                        icon._buffetSrc = null;
                    }
                }
                disablewindsfx = enabled;
            });
            selfrestrictions.Logsettings("Disable Force Pull", Color.cyan, ref forcegrabdisablernow, enabled =>
            {
                forcegrabdisablernow = enabled;
            });
            selfrestrictions.Logsettings("Visually Hide Holsters Self", Color.cyan, ref hideholsters, enabled =>
            {
                if (!hideholsters && enabled)
                {
                    HolsterHiderAll(null, true);
                }
                hideholsters = enabled;
            });
            selfrestrictions.Logsettings("Visually Hide Holsters Players", Color.cyan, ref hideholstersplayers, enabled =>
            {
                if (!hideholstersplayers && enabled)
                {
                    foreach (var playernow in NetworkPlayers(true))
                    {
                        HolsterHiderAll(playernow, true);
                    }
                }
                hideholstersplayers = enabled;
            });
            selfrestrictions.Logsettings("Visually Disable Your Bodylog", Color.cyan, ref bodylog, enabled =>
            {

                if (!bodylog && enabled)
                {
                    JR_BodyLog(Player.PhysicsRig).bodylogreturn?.ballArt.gameObject.SetActive(true);
                    JR_BodyLog(Player.PhysicsRig).Outerring.gameObject.SetActive(true);
                }

                bodylog = enabled;
            });
            selfrestrictions.Logsettings("Visually Disable Players Bodylog", Color.cyan, ref bodylogplayers, enabled =>
            {
                if (!bodylogplayers && enabled)
                {
                    foreach (var playernow in NetworkPlayers(true))
                    {
                        var bodylog = JR_BodyLog(playernow.RigRefs.RigManager.physicsRig);

                        bodylog.bodylogreturn?.ballLine.gameObject.SetActive(true);
                        bodylog.bodylogreturn?.ballArt.gameObject.SetActive(true);
                        bodylog.Outerring.gameObject.SetActive(true);
                    }
                }
                bodylogplayers = enabled;
            });
            selfrestrictions.Logsettings("Disable Media Players", Color.cyan, ref disablemediaplayers, enabled =>
            {
                disablemediaplayers = enabled;
            });
            selfrestrictions.Logsettings("Remove Grip Icons", Color.cyan, ref grippy, enabled =>
            {

                if (!grippy && enabled)
                {
                    var icons = GameObject.FindObjectsOfType<InteractableIcon>();
                    foreach (var icon in icons)
                    {
                        icon.IconSize = 0f;
                        icon.scaledIconSize = 0f;
                    }
                }

                grippy = enabled;


            });

            FusionProtectorPage.CreateFunction("Join Discord [For Updates]", Color.yellow, () =>
            {
                OpenPageNow("https://tinyurl.com/jamesreborn");
                NotificationNow(FusionProtectorInfo.ClientName, "Opened James Reborns Discord In Browser.", NotificationType.SUCCESS);
            });

            spawngunesst = FusionProtectorPage.CreatePage("+ Spawngun Essentials", Color.green);
            spawngunesst.CreateFunction("Set Left Hand Item To Spawngun", Color.yellow, () =>
            {
                SetBarCodeToSpawnGun(JR_YourGetHand(WhichHand.Left).JR_GetMarrowEntity().JR_GetBarcodeID());
            });
            spawngunesst.CreateFunction("Set Right Hand Item To Spawngun", Color.yellow, () =>
            {
                SetBarCodeToSpawnGun(JR_YourGetHand(WhichHand.Right).JR_GetMarrowEntity().JR_GetBarcodeID());
            });
            spawngunesst.CreateFunction("Set Random Item To Spawngun", Color.yellow, () =>
            {
                SetBarCodeToSpawnGun(GetRandomByType(RandomizerType.AllSpawnables));
            });

            holdinginhands = FusionProtectorPage.CreatePage("+ Holding In Hand Essentials", Color.green);


            holdinginhands.Logsettings("Remove Gun/Melee Sounds From Held", Color.cyan, ref removesounds, enabled =>
            {
                removesounds = enabled;
            });


            holdinginhands.LogsettingsEnum("In Hand", Color.yellow, ref handnowreal, enabled =>
            {
                handnowreal = (handnow)enabled;
            });


            holdinginhands.CreateFunction("Delete Sound From Held Item", Color.yellow, () =>
            {
                var hand = handnowreal == handnow.Left ? WhichHand.Left : WhichHand.Right;
                var grip = JR_YourGetHand(hand)?.JR_GetAttachedObject()?.GetComponentInChildren<Grip>(true);

                if (grip != null)
                {
                    RemoveSoundGrip(grip);

                    NotificationNow(FusionProtectorInfo.ClientName, $"Tried To Delete Sound Sources From {BarcodeInHand().JR_BarcodeCrateName()}", NotificationType.SUCCESS, 3.5f);
                }
            });




            holdinginhands.CreateFunction("Copy Mod ID To Clipboard", Color.yellow, () =>
            {
                GUIUtility.systemCopyBuffer = CrateFilterer.GetModID(new SpawnableCrateReference(BarcodeInHand()).Crate.Pallet).ToString();

            });



            holdinginhands.CreateFunction("Despawn All Of This Item", Color.yellow, () =>
            {
                foreach (var netentity in NetworkEntities())
                {
                    var marrow = netentity?.JR_GetMarrowEntity();
                    if (marrow != null && marrow.JR_GetBarcodeID() == BarcodeInHand())
                    {
                        netentity.JR_Despawn();
                    }
                }
                NotificationNow(FusionProtectorInfo.ClientName, $"Despawned Everything Matching {BarcodeInHand().JR_BarcodeCrateName()}", NotificationType.SUCCESS, 3.5f);
            });
            holdinginhands.CreateFunction("Force Delete All Of This Locally", Color.yellow, () =>
            {
                foreach (var netentity in NetworkEntities())
                {
                    var marrow = netentity?.JR_GetMarrowEntity();
                    if (marrow != null && marrow.JR_GetBarcodeID() == BarcodeInHand())
                    {
                        marrow.gameObject.DestroyNow();
                    }
                }
                NotificationNow(FusionProtectorInfo.ClientName, $"Force Deleted Locally Everything Matching {BarcodeInHand().JR_BarcodeCrateName()}", NotificationType.SUCCESS, 3.5f);
            });
            holdinginhands.CreateFunction("Add/Remove Block Author Of Spawnable", Color.yellow, () =>
            {
                ToggleAddRemoveFromFile(new SpawnableCrateReference(BarcodeInHand()).Crate.Pallet.Author, blockentireauthor, blockauthornowlist, FusionProtectorInfo.ClientName, $"Added {BarcodeInHand().JR_BarcodeAuthor()} To Blocked Authors!.", $"Removed {BarcodeInHand().JR_BarcodeAuthor()} From Blocked Authors!.", true);
            });
            holdinginhands.CreateFunction("Add/Remove Block Pallet Completely", Color.yellow, () =>
            {
                ToggleAddRemoveFromFile(StripColorTags(new SpawnableCrateReference( BarcodeInHand()).Crate.Pallet.name), blockentirepallet, blockpalletnowlist, FusionProtectorInfo.ClientName, $"Added {BarcodeInHand().JR_BarcodePalletName()} To Blocked Pallets!.", $"Removed {BarcodeInHand().JR_BarcodePalletName()} From Blocked Pallets!.", true);
            });
            holdinginhands.CreateFunction("Copy Details To Clipboard", Color.yellow, () =>
            {
                var palleynow = new SpawnableCrateReference(BarcodeInHand()).Crate.Pallet;
                GUIUtility.systemCopyBuffer =
                    $"Spawnable Searcher Information : {palleynow?.name}\n" +
                    $"Mod IO : {CrateFilterer.GetModID(palleynow)}\n" +
                    $"Barcode ID : {BarcodeInHand()}\n" +
                    $"Pallet Author : {palleynow?.Author}\n" +
                    $"Pallet Name : {palleynow?.name}";

                NotificationNow(FusionProtectorInfo.ClientName, "Copied To Clipboard!", NotificationType.SUCCESS, 2f);
            });
            holdinginhands.CreateFunction("Add/Remove Kick If Spawned", Color.yellow, () =>
            {
                ToggleAddRemoveFromFile(BarcodeInHand(), SpawnablesKick, SpawnablesKickPath, FusionProtectorInfo.ClientName, $"Added {BarcodeInHand().JR_BarcodeCrateName()} To Kick If Spawned!.", $"Removed {BarcodeInHand().JR_BarcodeCrateName()} From Kick If Spawned!.");
            });
            holdinginhands.CreateFunction("Block/UnBlock This Spawnable", Color.yellow, () =>
            {
                ToggleAddRemoveFromFile(BarcodeInHand(), BlockedSpawnables, BlockedSpawnablesPath, FusionProtectorInfo.ClientName, $"Added {BarcodeInHand().JR_BarcodeCrateName()} To Blocked Spawnables!.", $"Removed {BarcodeInHand().JR_BarcodeCrateName()} From Blocked Spawnables!.");
            });
            holdinginhands.CreateFunction("Warn/UnWarn This Spawnable", Color.yellow, () =>
            {
                ToggleAddRemoveFromFile(BarcodeInHand(), WarnedSpawnables, WarnedSpawnablesPath, FusionProtectorInfo.ClientName, $"Added {BarcodeInHand().JR_BarcodeCrateName()} To Warn Spawnables!.", $"Removed {BarcodeInHand().JR_BarcodeCrateName()} From Warn Spawnables!.");
            });
            holdinginhands.CreateFunction("Add/Remove Block This Mod.IO Mod Completely", Color.yellow, () =>
            {
                ToggleAddRemoveFromFile(CrateFilterer.GetModID(new SpawnableCrateReference(BarcodeInHand()).Crate.Pallet).ToString(), modidblocked, ModIDBLOCKSPATH, FusionProtectorInfo.ClientName, $"Added {BarcodeInHand().JR_BarcodePalletName()} To Blocked Mod.IO Mod!.", $"Removed {BarcodeInHand().JR_BarcodePalletName()} From Blocked Mod.IO Mod!.", true);
            });
            holdinginhands.CreateFunction("Un/Favorite Spawnable", Color.yellow, () =>
            {
                var favorites = DataManager.ActiveSave?.PlayerSettings?.FavoriteSpawnables;
                if (favorites == null) return;

                if (!favorites.Contains(BarcodeInHand()))
                {
                    favorites.Add(BarcodeInHand());
                    DataManager.TrySaveActiveSave(SaveFlags.Complete);
                    NotificationNow(FusionProtectorInfo.ClientName, $"Added {BarcodeInHand().JR_BarcodeCrateName()} To SaveGame Favorites!", NotificationType.SUCCESS);
                }
                else
                {
                    favorites.Remove(BarcodeInHand());
                    DataManager.TrySaveActiveSave(SaveFlags.Complete);
                    NotificationNow(FusionProtectorInfo.ClientName, $"Removed {BarcodeInHand().JR_BarcodeCrateName()} From SaveGame Favorites!", NotificationType.SUCCESS);
                }
            });
            holdinginhands.CreateFunction("Add/Remove Custom Favorites [Spawnable]", Color.yellow, () =>
            {
                ToggleAddRemoveFromFile(BarcodeInHand(), CustomSpawnFav, SpawnableCustomFav, FusionProtectorInfo.ClientName, $"Added {BarcodeInHand().JR_BarcodeCrateName()} To Custom Spawnable Favorites!.", $"Removed {BarcodeInHand().JR_BarcodeCrateName()} From Custom Spawnable Favorites!.");
            });

            searchersnow = FusionProtectorPage.CreatePage("+ Searchers", Color.green);

            avatarsearcher = searchersnow.CreatePage("+ Avatar Searcher", Color.green);
            spawnablesearch = searchersnow.CreatePage("+ Spawnable Searcher", Color.green);
            levelsearcher = searchersnow.CreatePage("+ Level Searcher", Color.green);

            Timersz = FusionProtectorPage.CreatePage("+ Timers", Color.green);

            protectionstuff = FusionProtectorPage.CreatePage("+ Protections", Color.cyan);

            Notifications = FusionProtectorPage.CreatePage("+ Notifications", Color.cyan);
            Notifications.CreateFunction("Cancel Notifications", Color.yellow, () =>
            {
                LabFusion.UI.Popups.Notifier.CancelAll();
            });
            Notifications.Logsettings("Anti Spam Spawning Detection", Color.red, ref notificationspamspawn, enabled =>
            {
                notificationspamspawn = enabled;
            });
            Notifications.Logsettings("Invalid/Crash Stat Detection", Color.red, ref Invalidstatsnow, enabled =>
            {
                Invalidstatsnow = enabled;
            });
            Notifications.Logsettings("Spoof Username Detections", Color.red, ref spoofedusernameusername, enabled =>
            {
                spoofedusernameusername = enabled;
            });
            Notifications.Logsettings("Do Not Disturb", Color.red, ref donotdisturb, enabled =>
            {
                donotdisturb = enabled;
            });
            Notifications.Logsettings("FP Banlist Popups", Color.cyan, ref globalbannotification, enabled =>
            {
                globalbannotification = enabled;
            });
            Notifications.Logsettings("FP Blacklist Popups", Color.cyan, ref globalblocklistnotification, enabled =>
            {
                globalblocklistnotification = enabled;
            });
            Notifications.Logsettings("Exploit Notifications", Color.cyan, ref notificationsofexploits, enabled =>
            {
                notificationsofexploits = enabled;
            });
            Notifications.Logsettings("Strength Threshold", Color.green, ref strengthnotif, enabled =>
            {
                strengthnotif = enabled;
            });
            Notifications.Logsettings("Alt Notifications", Color.cyan, ref AltNotifications, enabled =>
            {
                AltNotifications = enabled;
            });
            Notifications.Logsettings("Alt Errors", Color.cyan, ref alterrornotis, enabled =>
            {
                alterrornotis = enabled;
            });
            Notifications.Logsettings("Block Spawnables", Color.cyan, ref blockspwnnotis, enabled =>
            {
                blockspwnnotis = enabled;
            });

            playerjoinlogsnow = FusionProtectorPage.CreatePage("+ Recently Met Players", Color.cyan);

            SaveTotxtpg = protectionstuff.CreatePage("+ Save To Text File", Color.cyan);
            SaveTotxtpg.Logsettings("Save Recently Met Players Log", Color.white, ref logrecentlymet, enabled =>
            {
                logrecentlymet = enabled;
            });
            SaveTotxtpg.Logsettings("Save Media Player Logs", Color.white, ref logmediaplayer, enabled =>
            {
                logmediaplayer = enabled;
            });
            SaveTotxtpg.Logsettings("Save Lobbies Since Login", Color.white, ref loglobbiessince, enabled =>
            {
                loglobbiessince = enabled;
            });
            SaveTotxtpg.Logsettings("Save Players Since Login", Color.white, ref logplayersince, enabled =>
            {
                logplayersince = enabled;
            });


            Mostusedprotections = protectionstuff.CreatePage("+ Most Used Protections", Color.cyan);

            Mostusedprotections.Logsettings("Anti Freeze Player", Color.cyan, ref AntiModTP, enabled =>
            {
                AntiModTP = enabled;
            }).SetTooltip("This makes it so owners can't teleport you to prevent menus who spam the teleport packet to freeze you!");
            Mostusedprotections.Logsettings("Delete Last Lobby Mods", Color.white, ref DeleteLastLobbyMods, enabled =>
            {
                DeleteLastLobbyMods = enabled;
            });
            Mostusedprotections.Logsettings("Prevent Notification Lag", Color.cyan, ref preventnotificationlag, enabled =>
            {
                preventnotificationlag = enabled;
            });
            Mostusedprotections.Logsettings("Respawn Protection", Color.cyan, ref fullspawnprotection, enabled =>
            {
                fullspawnprotection = enabled;
            });
            Mostusedprotections.Logsettingsint("Respawn Protection Timer", Color.green, ref spawnprotectiontimer, 1, 1, 60, intnow =>
            {

                spawnprotectiontimer = intnow;

            });
            Mostusedprotections.Logsettings("Despawn On Disconnect", Color.cyan, ref cleandisconnect, enabled =>
            {
                cleandisconnect = enabled;
            });
            Mostusedprotections.Logsettings("Some Protections [No Host]", Color.cyan, ref spawnprotectionsnot_host, enabled =>
            {
                spawnprotectionsnot_host = enabled;
            });

            Mostusedprotections.Logsettings("Avatar Switch Protection", Color.cyan, ref aviswitchprotection, enabled =>
            {
                aviswitchprotection = enabled;
            });
            Mostusedprotections.Logsettings("Anti Grief Bodylog", Color.cyan, ref AntiBodyLogGrief, enabled =>
            {
                AntiBodyLogGrief = enabled;
            });
            Mostusedprotections.Logsettings("Anti Gravity Change", Color.cyan, ref AntiGravityChange, enabled =>
            {
                AntiGravityChange = enabled;
            });
            Mostusedprotections.Logsettings("Anti Grab", Color.cyan, ref AntiGrab, enabled =>
            {
                AntiGrab = enabled;
            });
            Mostusedprotections.Logsettings("Anti Despawn EffectS", Color.cyan, ref antidespawneffect, enabled =>
            {
                antidespawneffect = enabled;
            });
            Mostusedprotections.Logsettings("Anti Bodylog Effects", Color.cyan, ref antibodylogeffect, enabled =>
            {
                antibodylogeffect = enabled;
            });
            Mostusedprotections.Logsettings("TP To Spawn Threshold", Color.cyan, ref TeleportThresHold, enabled =>
            {
                TeleportThresHold = enabled;
            });
            Mostusedprotections.Logsettingsfloat("TP To Spawn Value", Color.cyan, ref speedthreshold, 1.0f, 30.0f, 1000.0f, (value) =>
            {
                speedthreshold = value;
            });
            Mostusedprotections.Logsettings("Block Net Spawns Locally", Color.cyan, ref blockallspawnslocally, enabled =>
            {
                blockallspawnslocally = enabled;
            });


            featuresprotection = protectionstuff.CreatePage("+ Protection Features", Color.cyan);
            
            featuresprotection.Logsettings("Warn Avatar Change", Color.cyan, ref warnavinow, enabled =>
            {
                warnavinow = enabled;
            });
            featuresprotection.Logsettings("Block Avatar Author", Color.cyan, ref blockaviauthornow, enabled =>
            {
                blockaviauthornow = enabled;
            });
            featuresprotection.Logsettings("Block Avatar Pallet", Color.cyan, ref blockavipalletnow, enabled =>
            {
                blockavipalletnow = enabled;
            });
            featuresprotection.Logsettings("Block Avatars As Spawnables", Color.cyan, ref BLOCKAVATARSASSPAWNABLES, enabled =>
            {
                BLOCKAVATARSASSPAWNABLES = enabled;
            });
            featuresprotection.Logsettings("Block Pallet Completely", Color.cyan, ref BlockPalletCompletely, enabled =>
            {
                BlockPalletCompletely = enabled;
            });
            featuresprotection.Logsettings("Block Author Of Spawnable", Color.cyan, ref BlockAuthorOfSpawnable, enabled =>
            {
                BlockAuthorOfSpawnable = enabled;
            });
            featuresprotection.Logsettings("Block This Mod.IO Mod Completely", Color.cyan, ref ModIDBlocker, enabled =>
            {
                ModIDBlocker = enabled;
            });
            featuresprotection.Logsettings("Block/UnBlock This Spawnable", Color.cyan, ref blockedspawnables, enabled =>
            {
                blockedspawnables = enabled;
            });
            featuresprotection.Logsettings("Warn/UnWarn This Spawnable", Color.cyan, ref warnedspawnables, enabled =>
            {
                warnedspawnables = enabled;
            });
            
            ProtectionLogs = protectionstuff.CreatePage("+ Logs", Color.green);

            protectionstuff.Logsettings("Force Nametags On", Color.cyan, ref forcenametagson, enabled =>
            {
                forcenametagson = enabled;
            });
            protectionstuff.Logsettings("Remove Proximity Chat", Color.cyan, ref removeproxchat, enabled =>
            {
                removeproxchat = enabled;
            });
            protectionstuff.Logsettings("Anti Decals", Color.cyan, ref antidecal, enabled =>
            {
                antidecal = enabled;
            });
            protectionstuff.Logsettings("Block Exploits", Color.cyan, ref blockexploitscompletely, enabled =>
            {
                blockexploitscompletely = enabled;
            });

            teleporters = FusionProtectorPage.CreatePage("+ Teleporters", Color.green);
            teleporters.LogsettingsString("Teleporter Name", Color.yellow, ref teleportername, (stringy) =>
            {
                teleportername = stringy;
            });
            teleporters.CreateFunction("Add Teleporter", Color.yellow, () =>
            {
                bool exists = false;

                foreach (var t in TeleporterManager.Teleportersnowx)
                {
                    if (string.Equals(t.TitleOfTeleporter, teleportername, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    TeleporterManager.Teleportersnowx.Add(
                     new TeleporterManager(
                         StripColorTags(new LevelCrateReference(SceneStreamer.Session?.Level?.Barcode?.ID).Scannable.Title),
                         new Vector3(Player.RigManager.physicsRig.m_head.transform.position.x, Player.RigManager.physicsRig.m_head.transform.position.y, Player.RigManager.physicsRig.m_head.transform.position.z),
                         new Quaternion(Player.RigManager.physicsRig.m_head.transform.rotation.x, Player.RigManager.physicsRig.m_head.transform.rotation.y, Player.RigManager.physicsRig.m_head.transform.rotation.z, Player.RigManager.physicsRig.m_head.transform.rotation.w),
                         teleportername,
                         SceneStreamer.Session.Level.Barcode.ID
                     ));
                    TeleporterManager.SaveTeleporters();
                }
                else
                {
                    NotificationNow(FusionProtectorInfo.ClientName,
                        "This Teleporter Exists Already!",
                        NotificationType.WARNING,
                        2.5f);
                }
            });
            teleportersnow = teleporters.CreatePage("+ Active Teleporters", Color.yellow);

            cheatspreset = FusionProtectorPage.CreatePage("+ Dev Tool Presets", Color.green);
            cheatspreset.Logsettings("Everything Is Fusion Local Only", Color.cyan, ref localonlydevtools, enabled =>
            {
                localonlydevtools = enabled;
            });
            cheatspreset.LogsettingsString("Preset Name", Color.yellow, ref CHEATPRS, (stringy) =>
            {
                CHEATPRS = stringy;
            });
            cheatspreset.CreateFunction("Add Preset", Color.yellow, () =>
            {
                CreateDevToolPreset(CHEATPRS, new Item
                {
                    SpawnableName = StripColorTags(new SpawnableCrateReference("c1534c5a-5747-42a2-bd08-ab3b47616467").Crate.name),
                    BarcodeId = "c1534c5a-5747-42a2-bd08-ab3b47616467",
                    LocalSpawn = false,
                    ModIoID = CrateFilterer.GetModID(new SpawnableCrateReference("c1534c5a-5747-42a2-bd08-ab3b47616467").Crate.Pallet)
                }, new Item
                {
                    SpawnableName = StripColorTags(new SpawnableCrateReference("c1534c5a-6b38-438a-a324-d7e147616467").Crate.name),
                    BarcodeId = "c1534c5a-6b38-438a-a324-d7e147616467",
                    LocalSpawn = false,
                    ModIoID = CrateFilterer.GetModID(new SpawnableCrateReference("c1534c5a-6b38-438a-a324-d7e147616467").Crate.Pallet)
                }, new Item
                {
                    SpawnableName = "Empty",
                    BarcodeId = "Empty",
                    LocalSpawn = false,
                    ModIoID = -1
                }, new Item
                {
                    SpawnableName = "Empty",
                    BarcodeId = "Empty",
                    LocalSpawn = false,
                    ModIoID = -1
                }, new Item
                {
                    SpawnableName = "Empty",
                    BarcodeId = "Empty",
                    LocalSpawn = false,
                    ModIoID = -1
                });

            });
            cheatspresetsnow = cheatspreset.CreatePage("+ Active Presets", Color.green);

            bodylognowpage = FusionProtectorPage.CreatePage("+ Bodylog Pages", Color.green);
            bodylognowpage.LogsettingsString("Preset Name", Color.yellow, ref bodylogpagename, (stringy) =>
            {
                bodylogpagename = stringy;
            });
            bodylognowpage.CreateFunction("Add Preset", Color.yellow, () =>
            { 
                CreateBodyLogPage(bodylogpagename);


            });
            bodylognowpagexx = bodylognowpage.CreatePage("+ Active Presets", Color.green);

            loadoutpages = FusionProtectorPage.CreatePage("+ Loadouts", Color.green);
            loadoutpages.LogsettingsString("Loadout Name", Color.yellow, ref loadoutname, (stringy) =>
            {
                loadoutname = stringy;
            });
            loadoutpages.CreateFunction("Add Loadout", Color.yellow, () =>
            {
                InventoryPage.CaptureFromCurrentInventory(loadoutname);

            });
            loadoutpagesnow = loadoutpages.CreatePage("+ Active Loadouts", Color.green);
            loadoutpages.Logsettings("Keep Loadout", Color.cyan, ref KEEPLOADOUTINVENTORY, enabled =>
            {
                KEEPLOADOUTINVENTORY = enabled;
            });

            colorpresets = FusionProtectorPage.CreatePage("+ Bodylog & RadialMenu Color Presets", Color.green);
            colorpresets.Logsettings("Bodylog & Radial Menu Colors", Color.cyan, ref Bodylogradialcolors, enabled =>
            {
                Bodylogradialcolors = enabled;
            });

            colorpresets.LogsettingsString("Preset Name", Color.yellow, ref colornamenowx, (stringy) =>
            {
                colornamenowx = stringy;
            });
            colorpresets.CreateFunction("Add Preset", Color.yellow, () =>
            {
                bool exists = false;

                foreach (var t in BodyLogRadialMenuColorPreset.ColorPresets)
                {
                    if (string.Equals(t.TitleOfPreset, colornamenowx, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    BodyLogRadialMenuColorPreset.ColorPresets.Add(
    new BodyLogRadialMenuColorPreset(
        colornamenowx,

        0f,   // BodyLogColor_R
        255f, // BodyLogColor_G
        0f,   // BodyLogColor_B
        255f, // BodyLogColor_A

        0f,   // BodyLogBallColor_R
        255f, // BodyLogBallColor_G
        0f,   // BodyLogBallColor_B
        255f, // BodyLogBallColor_A

        0f,   // BodyLogLineColor_R
        255f, // BodyLogLineColor_G
        0f,   // BodyLogLineColor_B
        255f, // BodyLogLineColor_A

        0f,   // RadialMenuColor_R
        255f, // RadialMenuColor_G
        0f,   // RadialMenuColor_B
        255f  // RadialMenuColor_A
    )
);
                    BodyLogRadialMenuColorPreset.SavePresets();
                    NotificationNow(FusionProtectorInfo.ClientName,
    $"Added Preset {colornamenowx}!",
    NotificationType.SUCCESS,
    2.5f);
                }
                else
                {
                    NotificationNow(FusionProtectorInfo.ClientName,
                        "This Preset Name Exists Already!",
                        NotificationType.WARNING,
                        2.5f);
                }
            });
            colorpresetsnow = colorpresets.CreatePage("+ Active Presets", Color.green);

            FusionProfiles = FusionProtectorPage.CreatePage("+ Fusion Profile Presets", Color.green);
            FusionProfiles.LogsettingsString("Preset Name", Color.yellow, ref profilepresz, (stringy) =>
            {
                profilepresz = stringy;
            });
            FusionProfiles.CreateFunction("Add Preset", Color.yellow, () =>
            {
                if (string.IsNullOrWhiteSpace(profilepresz))
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        "Preset name cannot be empty!",
                        NotificationType.WARNING,
                        2.5f
                    );
                    return;
                }

                if (!NetworkInfo.HasServer)
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        "Start Or Join A Lobby To Store Your Preset!",
                        NotificationType.ERROR,
                        2.5f
                    );
                    return;
                }

                var player = JR_YourNetworkPlayer();
                if (player == null)
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        "Player not found.",
                        NotificationType.ERROR,
                        2.5f
                    );
                    return;
                }

                string presetName = profilepresz.Trim();

                bool exists = FusionProfilePresets.ProfilePresets.Any(t =>
                    string.Equals(
                        t.TitleOfPreset,
                        presetName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

                if (exists)
                {
                    NotificationNow(
                        FusionProtectorInfo.ClientName,
                        "This Preset Name Exists Already!",
                        NotificationType.WARNING,
                        2.5f
                    );
                    return;
                }

                string nickname = player.JR_Nickname();
                string description = player.JR_Description();
                string avatar = JR_YourAvatarBarcodeID();

                var newPreset = new FusionProfilePresets(
                    presetName,
                    nickname,
                    description,
                    avatar,
                    InternalServerHelpers.GetInitialEquippedItems()
                );

                FusionProfilePresets.ProfilePresets.Add(newPreset);
                FusionProfilePresets.SavePresets();

                NotificationNow(
                    FusionProtectorInfo.ClientName,
                    $"Added Preset {presetName}!",
                    NotificationType.SUCCESS,
                    2.5f
                );
            });
            FusionProfilesnow = FusionProfiles.CreatePage("+ Active Presets", Color.green);

            FusionProtectorPage.CreateFunction("Spawn BodyMall UI", Color.yellow, () =>
            {
                foreach (var p in GameObject.FindObjectsOfType<Poolee>())
                    if (p.gameObject.name.Contains("BodyMallUI"))
                        p.gameObject.DestroyNow();

                SpawnIt("doge15567.BodyMallUI.Spawnable.BodyMallUI",
                    Player.RigManager.physicsRig.m_head.position +
                    Player.RigManager.physicsRig.m_head.forward+
                    Player.RigManager.physicsRig.m_head.up,
                    Quaternion.identity, true);
            });
            FusionProtectorPage.CreateFunction("Teleport To Spawn", Color.yellow, () =>
            {
                if (!GamemodeManager.IsGamemodeStarted)
                {
                    LocalPlayer.TeleportToCheckpoint();
                }
                else
                {
                    NotificationNow(FusionProtectorInfo.ClientName, "Can't Do That Now!", NotificationType.ERROR, 2.0f);
                }
            });
            FusionProtectorPage.CreateFunction("Give Ammo", Color.yellow, () =>
            {
                if (!GamemodeManager.IsGamemodeStarted)
                {
                    LocalInventory.AddAmmo(99999999);
                }
                else
                {
                    NotificationNow(FusionProtectorInfo.ClientName, "Can't Do That Now!", NotificationType.ERROR, 2.0f);
                }
            });
            FusionProtectorPage.Logsettings("Auto Run", Color.cyan, ref autorunnow, enabled => { autorunnow = enabled; });

            levelsearcher.LogsettingsEnum("Search Method", Color.yellow, ref searchmethodlevelreal, enabled =>
            {
                searchmethodlevelreal = (SearchMethod)enabled;
            });
            levelsearcher.LogsettingsString("Match", Color.yellow, ref levelsrch, (stringy) =>
            {
                levelsrch = stringy;
            });
            levelsearcher.CreateFunction("Find Results", Color.yellow, () =>
            {

                MelonCoroutines.Start(Search(
                    levelsrch,
                    levelresults,
                    searchmethodlevelreal,
                    SearchMethodType.Level,
                    (barcode) =>
                    {
                        var nowy = new LevelCrateReference(barcode);
                        SceneStreamer.Load(nowy.Barcode);
                    }));

            });
            levelresults = levelsearcher.CreatePage("+ Level Searcher Results", Color.green);
            levelhistory = levelsearcher.CreatePage("+ Search History", Color.green);
            
            Protectsettings.CreateFunction("Manually Save Settings", Color.green, () =>
            {
                ManuallySave(true);
            });
            Protectsettings.Logsettings("Auto Save Settings", Color.cyan, ref autosavenow, enabled =>
            {
                autosavenow = enabled;
            });
            Protectsettings.Logsettingsint("Kick For Mins", Color.green, ref kicktime, 1, 1, 1000, intnow =>
            {
                kicktime = intnow;
            });

            Protectsettings.Logsettings("Hide FP From Lobbies", Color.green, ref HideFusionProtector, enabled =>
            {
                if (HideFusionProtector && !enabled)
                {
                    ModuleMessageManager.RegisterHandler<SendNotificationMessage>();
                    ModuleMessageManager.RegisterHandler<ProtectorPingMessage>();
                }
                HideFusionProtector = enabled;

            }).SetTooltip("Disables \"Using Fusion Protector Message Ping\"\r\nDisables \"Player Messaging\"\r\nDisables \"Messaging To Players With Reasons Why They Was Kicked [If Using FP]\"\r\nDisables \"FP-000 Code From Your Lobby & Also Doesnt Show Your Lobby In Fusion Protector Lobbies\"\r\nThis Hides from mods that try to snipe you because your using a actual good mod.");
            Protectsettings.CreateFunction("Set Current Level As Home World", Color.yellow, () =>
            {
                File.WriteAllText(homeworldnow, SceneStreamer.Session?.Level?.Barcode?.ID);
                NotificationNow(FusionProtectorInfo.ClientName, $"Made {StripColorTags(new LevelCrateReference(SceneStreamer.Session?.Level?.Barcode?.ID).Crate.name)} Your Home World!", NotificationType.SUCCESS, 3.5f);
            });
            Protectsettings.CreateFunction("Reload Website Data", Color.green, () =>
            {
                MelonCoroutines.Start(SiteStuff.UpdateSites());
            });
            Protectsettings.Logsettings("Keep Hidden Mods", Color.cyan, ref keephiddenmods, enabled =>
            {
                keephiddenmods = enabled;
            });
            Protectsettings.Logsettings("Global FP Blacklist", Color.cyan, ref globalblocklistnow, enabled =>
            {
                globalblocklistnow = enabled;
            });
            Protectsettings.Logsettings("Spawn Gun UI Alway", Color.cyan, ref spawngunuialways, enabled =>
            {
                spawngunuialways = enabled;
            });
            Protectsettings.Logsettingsfloat("Emergency Escape Seconds", Color.green, ref timerfoeesa, 1, 1, 170, intnow =>
            {

                timerfoeesa = intnow;

                EmergencyEscapetimer.Refresh(
                    newMins: null
                );

                EmergencyEscapetimer.Start(true, timerfoeesa);
            });
            Protectsettings.Logsettings("Emergency Escape Seconds Ago", Color.cyan, ref tpback10seconds, enabled =>
            {
                tpback10seconds = enabled;
            });
            Protectsettings.Logsettings("Send Bodylog To Players", Color.cyan, ref bodylogsending, enabled =>
            {
                bodylogsending = enabled;
            });
            Protectsettings.Logsettings("Send Mod.IO ID# To Players", Color.cyan, ref modidsending, enabled =>
            {
                modidsending = enabled;
            });
            Protectsettings.Logsettings("Share Bodylog Pages To Players", Color.cyan, ref sharebodylogpagenow, enabled =>
            {
                sharebodylogpagenow = enabled;
            });
            Protectsettings.Logsettings("Share DevToolPresets To Players", Color.cyan, ref sharedevtoolpresets, enabled =>
            {
                sharedevtoolpresets = enabled;
            });


            Protectsettings.Logsettings("Send Code Mod Base64", Color.cyan, ref base64files, enabled =>
            {
                base64files = enabled;
            });
            Protectsettings.Logsettings("Send Bits To Players", Color.cyan, ref bitsending, enabled =>
            {
                bitsending = enabled;
            });
            Protectsettings.Logsettings("Mod O Mat On Level Load", Color.cyan, ref modomatonload, enabled =>
            {
                modomatonload = enabled;
            });
            Protectsettings.Logsettings("Auto Kick Outdated FP Players", Color.cyan, ref autokickoldfusionprotectorusers, enabled =>
            {
                autokickoldfusionprotectorusers = enabled;
            });
            Protectsettings.Logsettings("Remove Fusion Global Banlist", Color.white, ref REMOVEDGLOBALBANLIST, enabled =>
            {
                REMOVEDGLOBALBANLIST = enabled;
            });
            Protectsettings.Logsettings("Player Messaging", Color.cyan, ref playermessaging, enabled =>
            {
                playermessaging = enabled;
            });
            Protectsettings.Logsettings("Media Player Protection", Color.cyan, ref mediaplayerprotection, enabled =>
            {
                mediaplayerprotection = enabled;
            });
            Protectsettings.Logsettings("Fusion Protector Lobby", Color.cyan, ref fusionprotectedlobby, enabled =>
            {
                fusionprotectedlobby = enabled;
                NetworkHelper.RefreshServerCode();
            });
            Protectsettings.CreateFunction("Log Installed Mods MOD.IO ID'S", Color.yellow, () =>
            {
                var palletList = AssetWarehouse.Instance.GetPallets();

                System.Collections.Generic.List<string> palletMessages = new System.Collections.Generic.List<string>();

                foreach (var palletNow in palletList)
                {
                    string message = $"[Name : {palletNow.name} Author : {palletNow.Author}]  => # ID {CrateFilterer.GetModID(palletNow)}";
                    palletMessages.Add(message);
                }

                string[] palletMessagesArray = palletMessages.ToArray();
                string allMessages = string.Join(Environment.NewLine, palletMessagesArray);

                GUIUtility.systemCopyBuffer = allMessages;
                NotificationNow(FusionProtectorInfo.ClientName, "All Installed Mods Mod.io ID'S Copied To Clipboard.", NotificationType.SUCCESS);
            });

            Protectsettings.Logsettings("Drop Stuff Before Despawn All", Color.cyan, ref dropallbefore, enabled =>
            {
                dropallbefore = enabled;
            });
            Protectsettings.Logsettings("Randomizers Use SLZ Items Only", Color.cyan, ref randomizerslzonly, enabled =>
            {
                if (!randomizerslzonly && enabled)
                {
                    MelonCoroutines.Start(LoadAssetsEnum(randomizerslzonly));

                    MelonLogger.Warning("Randomizer Use SLZ Items Only : " + randomizerslzonly);
                }

                randomizerslzonly = enabled;
            });
            Protectsettings.Logsettings("Save Bool Setting On Toggle", Color.cyan, ref togglesavesbool, enabled =>
            {
                togglesavesbool = enabled;
            });
            Protectsettings.Logsettings("Clear Protection Logs On Join Server", Color.cyan, ref clientexploitclearonnewserver, enabled =>
            {
                clientexploitclearonnewserver = enabled;
            });
            Protectsettings.Logsettings("Clear Spawn Logs On Join Server", Color.cyan, ref spawnlogexploitclearonnewserver, enabled =>
            {
                spawnlogexploitclearonnewserver = enabled;
            });
            Protectsettings.Logsettings("Clear Avi Switch Logs On Join Server", Color.cyan, ref switchlogexploitclearonnewserver, enabled =>
            {
                switchlogexploitclearonnewserver = enabled;
            });

            FusionProtectorPage.CreateFunction("Rejoin Out Of Bounds Server", Color.yellow, () =>
            {
                if (!string.IsNullOrEmpty(outofboundslobbycode))
                {
                    NetworkHelper.Disconnect();
                    NetworkHelper.JoinServerByCode(outofboundslobbycode);
                }
            });
            FusionProtectorPage.CreateFunction("Rejoin Last Server", Color.yellow, () =>
            {
                if (!string.IsNullOrEmpty(rejoinlastserver))
                {
                    NetworkHelper.Disconnect();
                    NetworkHelper.JoinServerByCode(rejoinlastserver);
                }
            });
            FusionProtectorPage.CreateFunction("Show Item Information ID's In Hands",Color.yellow,() =>{
                    var leftHand = JR_YourGetHand(WhichHand.Left);
                    var rightHand = JR_YourGetHand(WhichHand.Right);

                    var leftBarcode = leftHand?.JR_GetMarrowEntity()?.JR_GetBarcodeID();
                    var rightBarcode = rightHand?.JR_GetMarrowEntity()?.JR_GetBarcodeID();

                    string leftHandText =
                        leftBarcode != null
                            ? $"Left Hand : [{leftBarcode.JR_BarcodeCrateName()}] {leftBarcode}"
                            : "Left Hand : [Empty] Empty";

                    string rightHandText =
                        rightBarcode != null
                            ? $"Right Hand : [{rightBarcode.JR_BarcodeCrateName()}] {rightBarcode}"
                            : "Right Hand : [Empty] Empty";

                    string dialogText = leftHandText + "\n" + rightHandText;

                    BoneLib.BoneMenu.Menu.DisplayDialog(
                        "Information In Hands",
                        dialogText
                    );
                });
            FusionProtectorPage.Logsettingsint("Bodylog Slot", Color.green, ref currentbodylogindex, 1, 1, 6, intnow =>
            {
                currentbodylogindex = intnow;
            });
            FusionProtectorPage.CreateFunction("Set Current Avatar", Color.yellow, () =>
            {
                ChangeBodyLogAvatarSlot(currentbodylogindex, JR_YourAvatarBarcodeID(), true);
            });

            Timersz.LogsettingsEnum("Despawn Timer Filter", Color.yellow, ref DespawnerTimerAllReal, enabled =>
            {
                DespawnerTimerAllReal = (DespawnerAll)enabled;
            });
            Timersz.Logsettings("Despawn All", Color.cyan, ref DespawnAllTimer, enabled =>
            {
                DespawnAllTimer = enabled;
            });
            Timersz.Logsettingsint("Despawn All Timer", Color.green, ref DespawnAllTimerMins, 1, 1, 1000, intnow =>
            {
                DespawnAllTimerMins = intnow;
                DespawnAllTimera.Refresh(newMins: DespawnAllTimerMins);
                DespawnAllTimera.Start(true, DespawnAllTimerMins);
            });

            avatarsearcher.LogsettingsEnum("Avatar Search Type", Color.yellow, ref AvatarSearchTypeReal, enabled =>
            {
                AvatarSearchTypeReal = (AvatarSearchType)enabled;
            });
            avatarsearcher.LogsettingsEnum("Search Method", Color.yellow, ref searchmethodavatarreal, enabled =>
            {
                searchmethodavatarreal = (SearchMethod)enabled;
            });
            avatarsearcher.Logsettingsint("SetBodylog Slot", Color.green, ref bodylogindex, 1, 1, 6, intnow =>
            {
                bodylogindex = intnow;
            });
            avatarsearcher.LogsettingsString("Match", Color.yellow, ref searchavi, (stringy) =>
            {
                searchavi = stringy;
            });
            avatarsearcher.CreateFunction("Find Results", Color.yellow, () =>
            {
                void AvatarsearcherLessCode(string barcode)
                {
                    switch (AvatarSearchTypeReal)
                    {
                        case AvatarSearchType.ChangeInto:
                            ChangeIntoAvi(barcode);
                            break;

                        case AvatarSearchType.CopyDetailsToClipboard:
                            var referecny = new AvatarCrateReference(barcode);
                            GUIUtility.systemCopyBuffer =
                                $"Barcode ID : {referecny.Barcode.ID}\n" +
                                $"Pallet Name : {StripColorTags(referecny.Crate.Pallet.name)}\n" +
                                $"Pallet Author : {referecny.Crate.Pallet.Author}";
                            break;

                        case AvatarSearchType.SetBodyLog:
                            ChangeBodyLogAvatarSlot(bodylogindex, barcode, true);
                            break;
                    }
                }

                MelonCoroutines.Start(Search(
                    searchavi,
                    aviresults,
                    searchmethodavatarreal,
                    SearchMethodType.Avatar,
                    (barcode) =>
                    {
                        AvatarsearcherLessCode(barcode);
                    }));



            });
            aviresults = avatarsearcher.CreatePage("+ Avatar Searcher Results", Color.green);
            avisearchhistory = avatarsearcher.CreatePage("+ Search History", Color.green);

            spawnablesearch.LogsettingsEnum("Spawnable Search Type", Color.yellow, ref spawnablesrchtype, enabled =>
            {
                spawnablesrchtype = (SpawnableSearchType)enabled;
            });
            spawnablesearch.LogsettingsEnum("Search Method", Color.yellow, ref searchspawnabletypereal, enabled =>
            {
                searchspawnabletypereal = (SearchMethod)enabled;
            });
            spawnablesearch.LogsettingsString("Match", Color.yellow, ref spwnblesearch, (stringy) =>
            {
                spwnblesearch = stringy;
            });
            spawnablesearch.CreateFunction("Find Results", Color.yellow, () =>
            {
                void SpawnerFuncLessCode(string barcode)
                {
                    switch (spawnablesrchtype)
                    {
                        case SpawnableSearchType.Spawn:

                            SpawnIt(
                                barcode,
                                JR_YourGetHand(WhichHand.Left).transform.position +
                                JR_YourGetHand(WhichHand.Left).transform.forward +
                                JR_YourGetHand(WhichHand.Left).transform.up,
                                Quaternion.identity);

                            break;

                        case SpawnableSearchType.CopyDetailsToClipboard:

                            GUIUtility.systemCopyBuffer = barcode;
                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                $"Copied Barcode To Clipboard! {barcode}",
                                NotificationType.WARNING,
                                3.0f);

                            break;

                        case SpawnableSearchType.UnFavoriteAndFavorite:

                            if (!DataManager.ActiveSave.PlayerSettings.FavoriteSpawnables.Contains(barcode))
                            {
                                DataManager.ActiveSave.PlayerSettings.FavoriteSpawnables.Add(barcode);
                                DataManager.TrySaveActiveSave(SaveFlags.Complete);

                                NotificationNow(
                                    FusionProtectorInfo.ClientName,
                                    $"Added {barcode} To SaveGame Favorites!",
                                    NotificationType.SUCCESS);
                            }
                            else
                            {
                                DataManager.ActiveSave.PlayerSettings.FavoriteSpawnables.Remove(barcode);
                                DataManager.TrySaveActiveSave(SaveFlags.Complete);

                                NotificationNow(
                                    FusionProtectorInfo.ClientName,
                                    $"Removed {barcode} From SaveGame Favorites!",
                                    NotificationType.SUCCESS);
                            }

                            break;

                        case SpawnableSearchType.DespawnAllOfThis:

                            foreach (var netentity in NetworkEntities())
                            {
                                if (netentity.JR_GetMarrowEntity().JR_GetBarcodeID() == barcode)
                                {
                                    netentity.JR_Despawn();
                                }
                            }

                            NotificationNow(
                                FusionProtectorInfo.ClientName,
                                $"Despawned Everything Matching {StripColorTags(new SpawnableCrateReference(barcode).Crate.name)}",
                                NotificationType.SUCCESS,
                                3.5f);

                            break;

                        case SpawnableSearchType.SetInSpawnGun:

                            SetBarCodeToSpawnGun(barcode);

                            break;
                    }
                }

                MelonCoroutines.Start(Search(
                    spwnblesearch,
                    spawnableresults,
                    searchspawnabletypereal,
                    SearchMethodType.Spawnable,
                    (barcode) =>
                    {
                        SpawnerFuncLessCode(barcode);
                    }));

            });
            spawnableresults = spawnablesearch.CreatePage("+ Spawnable Searcher Results", Color.green);
            spawnablehistory = spawnablesearch.CreatePage("+ Search History", Color.green);

            FusionProtectorPage.Logsettings("Show Ammo Always", Color.cyan, ref showammoalways, enabled =>
            {
                showammoalways = enabled;
            });
            FusionProtectorPage.Logsettings("Personal Space", Color.cyan, ref personalspace, enabled =>
            {
                personalspace = enabled;
            });
            FusionProtectorPage.Logsettingsfloat("Personal Space Distance", Color.green, ref personalspacevalue, 0.1f, 0.5f, 30, floatnow =>
            {
                personalspacevalue = floatnow;
            });
            FusionProtectorPage.Logsettings("Unlimited Ammo", Color.cyan, ref unlammo, enabled => { unlammo = enabled; });
            FusionProtectorPage.CreateFunction("Dump Game Pallets", Color.yellow, () =>
            {
                MelonCoroutines.Start(DumpPalletsCoroutine());
            });
            
            Hostonlyoptions();
            OPERATORoptions();
            Owneroptionsonly();

            NotificationNow("Fusion Protector Built!", "Version "+FusionProtectorInfo.Version, NotificationType.SUCCESS, 6.0f);
            MelonCoroutines.Start(RunAfterBuild());
        }
    }
}