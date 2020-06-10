using main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using main.Core;
using main.Exceptions;
using main.Utils;

namespace main.Services
{
    public class WikiService
    {
        private static readonly string[] WikiThreads = {
            "AddCharModel", "AddMenuItem", "AddPlayerClass", "AddPlayerClassEx", "AddSimpleModel", "AddSimpleModelTimed", "AddStaticPickup", "AddStaticVehicle", "AddStaticVehicleEx",
            "AddVehicleComponent", "AllowAdminTeleport", "AllowInteriorWeapons", "AllowPlayerTeleport", "ApplyActorAnimation", "ApplyAnimation", "Atan", "Attach3DTextLabelToPlayer",
            "Attach3DTextLabelToVehicle", "AttachCameraToObject", "AttachCameraToPlayerObject", "AttachObjectToObject", "AttachObjectToPlayer", "AttachObjectToVehicle", "AttachPlayerObjectToPlayer",
            "AttachPlayerObjectToVehicle", "AttachTrailerToVehicle", "Ban", "BanEx", "BlockIpAddress", "CallLocalFunction", "CallRemoteFunction", "CancelEdit", "CancelSelectTextDraw", "ChangeVehicleColor",
            "ChangeVehiclePaintjob", "Clamp", "ClearActorAnimations", "ClearAnimations", "ConnectNPC", "Create3DTextLabel", "CreateActor", "CreateExplosion", "CreateExplosionForPlayer", "CreateMenu",
            "CreateObject", "CreatePickup", "CreatePlayer3DTextLabel", "CreatePlayerObject", "CreatePlayerTextDraw", "CreateVehicle", "db_close", "db_debug_openfiles", "db_debug_openresults",
            "db_field_name", "db_free_result", "db_get_field", "db_get_field_assoc", "db_get_field_assoc_float", "db_get_field_assoc_int", "db_get_field_float", "db_get_field_int", "db_get_mem_handle",
            "db_get_result_mem_handle", "db_next_row", "db_num_fields", "db_num_rows", "db_open", "db_query", "Delete3DTextLabel", "DeletePVar", "DeletePlayer3DTextLabel", "DeleteSVar", "Deleteproperty",
            "DestroyActor", "DestroyMenu", "DestroyObject", "DestroyPickup", "DestroyPlayerObject", "DestroyVehicle", "DetachTrailerFromVehicle", "DisableInteriorEnterExits", "DisableMenu",
            "DisableMenuRow", "DisableNameTagLOS", "DisablePlayerCheckpoint", "DisablePlayerRaceCheckpoint", "DisableRemoteVehicleCollisions", "EditAttachedObject", "EditObject",
            "EditPlayerObject", "EnablePlayerCameraTarget", "EnableStuntBonusForAll", "EnableStuntBonusForPlayer", "EnableTirePopping", "EnableVehicleFriendlyFire", "EnableZoneNames", "Existproperty",
            "Fblockread", "Fblockwrite", "Fclose", "Fexist", "Fgetchar", "FindModelFileNameFromCRC", "FindTextureFileNameFromCRC", "Flength", "Float", "Floatabs", "Floatadd", "Floatcmp", "Floatcos",
            "Floatdiv", "Floatfract", "Floatlog", "Floatmul", "Floatpower", "Floatround", "Floatsin", "Floatsqroot", "Floatstr", "Floatsub", "Floattan", "Fmatch", "Fopen", "ForceClassSelection", "Format",
            "Fputchar", "Fread", "Fremove", "Fseek", "Ftemp", "Funcidx", "Fwrite", "GameModeExit", "GameTextForAll", "GameTextForPlayer", "GangZoneCreate", "GangZoneDestroy", "GangZoneFlashForAll",
            "GangZoneFlashForPlayer", "GangZoneHideForAll", "GangZoneHideForPlayer", "GangZoneShowForAll", "GangZoneShowForPlayer", "GangZoneStopFlashForAll", "GangZoneStopFlashForPlayer", "GetActorFacingAngle",
            "GetActorHealth", "GetActorPoolSize", "GetActorPos", "GetActorVirtualWorld", "GetAnimationName", "GetConsoleVarAsBool", "GetConsoleVarAsInt", "GetConsoleVarAsString", "GetGravity",
            "GetGravity", "GetMaxPlayers", "GetNetworkStats", "GetObjectModel", "GetObjectPos", "GetObjectRot", "GetPVarFloat", "GetPVarInt", "GetPVarNameAtIndex", "GetPVarString", "GetPVarType",
            "GetPVarsUpperIndex", "GetPlayerAmmo", "GetPlayerAmmo", "GetPlayerAnimationIndex", "GetPlayerArmour", "GetPlayerCameraAspectRatio", "GetPlayerCameraFrontVector", "GetPlayerCameraMode",
            "GetPlayerCameraPos", "GetPlayerCameraTargetActor", "GetPlayerCameraTargetObject", "GetPlayerCameraTargetPlayer", "GetPlayerCameraTargetVehicle", "GetPlayerCameraUpVector", "GetPlayerCameraZoom",
            "GetPlayerColor", "GetPlayerCustomSkin", "GetPlayerDistanceFromPoint", "GetPlayerDrunkLevel", "GetPlayerFacingAngle", "GetPlayerFightingStyle", "GetPlayerHealth", "GetPlayerInterior", "GetPlayerIp",
            "GetPlayerKeys", "GetPlayerKeys", "GetPlayerLastShotVectors", "GetPlayerMenu", "GetPlayerMoney", "GetPlayerName", "GetPlayerNetworkStats", "GetPlayerObjectModel", "GetPlayerObjectPos",
            "GetPlayerObjectRot", "GetPlayerPing", "GetPlayerPoolSize", "GetPlayerPos", "GetPlayerScore", "GetPlayerSkin", "GetPlayerSpecialAction", "GetPlayerState", "GetPlayerSurfingObjectID",
            "GetPlayerSurfingVehicleID", "GetPlayerTargetActor", "GetPlayerTargetPlayer", "GetPlayerTeam", "GetPlayerTime", "GetPlayerVehicleID", "GetPlayerVehicleSeat", "GetPlayerVelocity", "GetPlayerVersion",
            "GetPlayerVirtualWorld", "GetPlayerWantedLevel", "GetPlayerWeapon", "GetPlayerWeaponData", "GetPlayerWeaponState", "GetSVarFloat", "GetSVarInt", "GetSVarNameAtIndex", "GetSVarString", "GetSVarType",
            "GetSVarsUpperIndex", "GetServerTickRate", "GetServerVarAsBool", "GetServerVarAsInt", "GetServerVarAsString", "GetTickCount", "GetVehicleComponentInSlot", "GetVehicleComponentType", "GetVehicleDamageStatus",
            "GetVehicleDistanceFromPoint", "GetVehicleHealth", "GetVehicleModel", "GetVehicleModelInfo", "GetVehicleParamsCarDoors", "GetVehicleParamsCarWindows", "GetVehicleParamsSirenState", "GetVehiclePoolSize",
            "GetVehiclePos", "GetVehicleRotation", "GetVehicleRotationQuat", "GetVehicleTrailer", "GetVehicleVelocity", "GetVehicleVirtualWorld", "GetVehicleZAngle", "GetWeaponName", "Getarg", "Getdate",
            "Getproperty", "Gettime", "Gettime", "GivePlayerMoney", "GivePlayerWeapon", "Gpci", "HTTP", "Heapspace", "HideMenuForPlayer", "InterpolateCameraLookAt", "InterpolateCameraPos", "IsActorInvulnerable",
            "IsActorStreamedIn", "IsObjectMoving", "IsPlayerAdmin", "IsPlayerAttachedObjectSlotUsed", "IsPlayerConnected", "IsPlayerHoldingObject", "IsPlayerInAnyVehicle", "IsPlayerInCheckpoint",
            "IsPlayerInRaceCheckpoint", "IsPlayerInRangeOfPoint", "IsPlayerInVehicle", "IsPlayerNPC", "IsPlayerObjectMoving", "IsPlayerStreamedIn", "IsTrailerAttachedToVehicle", "IsValidActor",
            "IsValidObject", "IsValidPlayerObject", "IsValidVehicle", "IsVehicleStreamedIn", "Ispacked", "Kick", "KillTimer", "LimitGlobalChatRadius", "LimitPlayerMarkerRadius", "LinkVehicleToInterior",
            "ManualVehicleEngineAndLights", "Memcpy", "MoveObject", "MovePlayerObject", "NPC:GetPlayerArmedWeapon", "NPC:IsPlayerStreamedIn", "NPC:IsVehicleStreamedIn", "NPC:OnNPCConnect", "NPC:OnNPCDisconnect",
            "NPC:OnNPCEnterVehicle", "NPC:OnNPCExitVehicle", "NPC:OnNPCModeExit", "NPC:OnNPCModeInit", "NPC:OnNPCSpawn", "NPC:PauseRecordingPlayback", "NPC::ResumeRecordingPlayback", "NPC:SendChat", "NPC:SendCommand",
            "NPC:StartRecordingPlayback", "NPC:StopRecordingPlayback", "NetStats_BytesReceived", "NetStats_BytesSent", "NetStats_ConnectionStatus", "NetStats_GetConnectedTime", "NetStats_GetIpPort",
            "NetStats_MessagesReceived", "NetStats_MessagesRecvPerSecond", "NetStats_MessagesSent", "NetStats_PacketLossPercent", "Numargs", "PlayAudioStreamForPlayer", "PlayCrimeReportForPlayer",
            "PlayerPlaySound", "PlayerSpectatePlayer", "PlayerSpectateVehicle", "PlayerTextDrawAlignment", "PlayerTextDrawBackgroundColor", "PlayerTextDrawBoxColor", "PlayerTextDrawColor", "PlayerTextDrawDestroy",
            "PlayerTextDrawFont", "PlayerTextDrawHide", "PlayerTextDrawLetterSize", "PlayerTextDrawSetOutline", "PlayerTextDrawSetPreviewModel", "PlayerTextDrawSetPreviewRot", "PlayerTextDrawSetPreviewVehCol",
            "PlayerTextDrawSetProportional", "PlayerTextDrawSetSelectable", "PlayerTextDrawSetShadow", "PlayerTextDrawSetString", "PlayerTextDrawShow", "PlayerTextDrawTextSize", "PlayerTextDrawUseBox", "Print",
            "Printf", "PutPlayerInVehicle", "Random", "RedirectDownload", "RemoveBuildingForPlayer", "RemovePlayerAttachedObject", "RemovePlayerFromVehicle", "RemovePlayerMapIcon", "RemoveVehicleComponent",
            "RepairVehicle", "ResetPlayerMoney", "ResetPlayerWeapons", "SHA256", "SelectObject", "SelectTextDraw", "SendClientMessage", "SendClientMessageToAll", "SendDeathMessage", "SendDeathMessageToPlayer",
            "SendPlayerMessageToAll", "SendPlayerMessageToPlayer", "SendRconCommand", "SetActorFacingAngle", "SetActorHealth", "SetActorInvulnerable", "SetActorPos", "SetActorVirtualWorld", "SetCameraBehindPlayer",
            "SetDeathDropAmount", "SetDisabledWeapons", "SetGameModeText", "SetGravity", "SetMenuColumnHeader", "SetNameTagDrawDistance", "SetObjectMaterial", "SetObjectMaterialText", "SetObjectNoCameraCol", "SetObjectPos",
            "SetObjectRot", "SetObjectsDefaultCameraCol", "SetPVarFloat", "SetPVarInt", "SetPVarString", "SetPlayerAmmo", "SetPlayerAmmo", "SetPlayerArmedWeapon", "SetPlayerArmour", "SetPlayerAttachedObject",
            "SetPlayerCameraLookAt", "SetPlayerCameraPos", "SetPlayerChatBubble", "SetPlayerCheckpoint", "SetPlayerColor", "SetPlayerDrunkLevel", "SetPlayerFacingAngle", "SetPlayerFightingStyle", "SetPlayerHealth",
            "SetPlayerHoldingObject", "SetPlayerInterior", "SetPlayerMapIcon", "SetPlayerMarkerForPlayer", "SetPlayerName", "SetPlayerObjectMaterial", "SetPlayerObjectMaterialText", "SetPlayerObjectNoCameraCol",
            "SetPlayerObjectPos", "SetPlayerObjectRot", "SetPlayerPos", "SetPlayerPosFindZ", "SetPlayerRaceCheckpoint", "SetPlayerScore", "SetPlayerShopName", "SetPlayerSkillLevel", "SetPlayerSkin", "SetPlayerSpecialAction",
            "SetPlayerTeam", "SetPlayerTime", "SetPlayerVelocity", "SetPlayerVirtualWorld", "SetPlayerWantedLevel", "SetPlayerWeather", "SetPlayerWorldBounds", "SetSVarFloat", "SetSVarInt", "SetSVarString",
            "SetSpawnInfo", "SetTeamCount", "SetTimer", "SetTimerEx", "SetVehicleAngularVelocity", "SetVehicleHealth", "SetVehicleNumberPlate", "SetVehicleParamsCarDoors", "SetVehicleParamsCarWindows",
            "SetVehicleParamsEx", "SetVehicleParamsForPlayer", "SetVehiclePos", "SetVehicleToRespawn", "SetVehicleVelocity", "SetVehicleVirtualWorld", "SetVehicleZAngle", "SetWeather", "SetWorldTime", "Setarg",
            "Setproperty", "ShowMenuForPlayer", "ShowNameTags", "ShowPlayerDialog", "ShowPlayerMarkers", "ShowPlayerNameTagForPlayer", "SpawnPlayer", "StartRecordingPlayerData", "StopAudioStreamForPlayer", "StopObject",
            "StopPlayerHoldingObject", "StopPlayerObject", "StopRecordingPlayerData", "Strcat", "Strcmp", "Strdel", "Strfind", "Strfind", "Strins", "Strlen", "Strmid", "Strpack", "Strunpack", "Strval", "TextDrawAlignment",
            "TextDrawBackgroundColor", "TextDrawBoxColor", "TextDrawColor", "TextDrawCreate", "TextDrawDestroy", "TextDrawFont", "TextDrawHideForAll", "TextDrawHideForPlayer", "TextDrawLetterSize", "TextDrawSetOutline",
            "TextDrawSetPreviewModel", "TextDrawSetPreviewRot", "TextDrawSetPreviewVehCol", "TextDrawSetProportional", "TextDrawSetSelectable", "TextDrawSetShadow", "TextDrawSetString", "TextDrawShowForAll",
            "TextDrawShowForPlayer", "TextDrawTextSize", "TextDrawUseBox", "Tickcount", "TogglePlayerClock", "TogglePlayerControllable", "TogglePlayerSpectating", "Tolower", "Toupper", "UnBlockIpAddress",
            "Update3DTextLabelText", "UpdatePlayer3DTextLabelText", "UpdateVehicleDamageStatus", "UsePlayerPedAnims", "Uudecode", "Valstr", "VectorSize", "Category", "OnActorStreamIn", "OnActorStreamOut",
            "OnDialogResponse", "OnEnterExitModShop", "OnFilterScriptExit", "OnFilterScriptInit", "OnGameModeExit", "OnGameModeInit", "OnIncomingConnection", "OnObjectMoved", "OnPlayerClickMap", "OnPlayerClickPlayer",
            "OnPlayerClickPlayerTextDraw", "OnPlayerClickTextDraw", "OnPlayerCommandText", "OnPlayerConnect", "OnPlayerDeath", "OnPlayerDisconnect", "OnPlayerEditAttachedObject", "OnPlayerEditObject",
            "OnPlayerEnterCheckpoint", "OnPlayerEnterRaceCheckpoint", "OnPlayerEnterVehicle", "OnPlayerExitVehicle", "OnPlayerExitedMenu", "OnPlayerFinishedDownloading", "OnPlayerGiveDamage", "OnPlayerGiveDamageActor",
            "OnPlayerInteriorChange", "OnPlayerKeyStateChange", "OnPlayerLeaveCheckpoint", "OnPlayerLeaveRaceCheckpoint", "OnPlayerObjectMoved", "OnPlayerPickUpPickup", "OnPlayerPrivmsg", "OnPlayerRequestClass",
            "OnPlayerRequestDownload", "OnPlayerRequestSpawn", "OnPlayerSelectObject", "OnPlayerSelectedMenuRow", "OnPlayerSpawn", "OnPlayerStateChange", "OnPlayerStreamIn", "OnPlayerStreamOut", "OnPlayerTakeDamage",
            "OnPlayerTeamPrivmsg", "OnPlayerText", "OnPlayerUpdate", "OnPlayerWeaponShot", "OnRconCommand", "OnRconLoginAttempt", "OnTrailerUpdate", "OnUnoccupiedVehicleUpdate", "OnVehicleDamageStatusUpdate",
            "OnVehicleDeath", "OnVehicleMod", "OnVehiclePaintjob", "OnVehicleRespray", "OnVehicleSirenStateChange", "OnVehicleSpawn", "OnVehicleStreamIn", "OnVehicleStreamOut"
        };

        private readonly IHttpClient _httpClient;
        private readonly string _wikiUrl;
        
        public WikiService(IHttpClient httpClient)
        {
            _httpClient = httpClient;
            _wikiUrl = Configuration.GetVariable("Urls.Wiki.Docs");
        }
        
        public WikiPageData GetPageData(string article)
        {
            article = GetClosestArticleName(article);
            var url = $"{_wikiUrl}{article}";
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(
                _httpClient.GetContent(url)
                );

            if (!IsValidWikiPage(article, htmlDocument))
            {
                throw new InvalidWikiPageException("Failed to parse headers");
            }
            
            return new WikiPageData
            {
                Title = article,
                Url = url,
                Description = GetPageDescription(htmlDocument),
                Arguments = GetPageArguments(htmlDocument),
                ArgumentsDescriptions = GetPageArgumentsWithDescriptions(htmlDocument),
                CodeExample = GetPageCodeExample(htmlDocument)
            };
        }

        private Dictionary<string, string> GetPageArgumentsWithDescriptions(HtmlDocument htmlDocument)
        {
            var arguments = new Dictionary<string, string>();
            var argumentNodes = htmlDocument.DocumentNode.SelectNodes("//div")
                .Where(n => n.HasClass("param"));
            
            foreach (var argumentNode in argumentNodes)
            {
                try
                {
                    arguments.Add(
                        argumentNode.ChildNodes["table"].ChildNodes["tr"].ChildNodes[0].InnerText,
                        argumentNode.ChildNodes["table"].ChildNodes["tr"].ChildNodes[1].InnerText
                    );
                }
                catch (Exception)
                {
                }
            }

            return arguments;
        }

        private string GetPageDescription(HtmlDocument htmlDocument)
        {
            try
            {
                return htmlDocument.DocumentNode.SelectNodes("//div")
                    .FirstOrDefault(n => n.HasClass("description")).InnerText;
            }
            catch (Exception)
            {
                return "Unknown Description";
            }
        }

        private string GetPageCodeExample(HtmlDocument htmlDocument)
        {
            try
            {
                var codeExample = htmlDocument.DocumentNode.SelectNodes("//pre")
                    .FirstOrDefault(n => n.HasClass("pawn")).InnerText;
                
                return HttpUtility.HtmlDecode(codeExample);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        
        private string GetPageArguments(HtmlDocument htmlDocument)
        {
            try
            {
                return htmlDocument.DocumentNode.SelectNodes("//div")
                    .FirstOrDefault(n => n.HasClass("parameters")).InnerText;
            }
            catch (Exception)
            {
                return "()";
            }
        }
        
        private bool IsValidWikiPage(string article, HtmlDocument htmlDocument)
        {
            try
            {
                return (htmlDocument.DocumentNode.SelectSingleNode("//h1").InnerText.ToLower() == article.ToLower()) 
                       && (htmlDocument.DocumentNode.SelectNodes("//div")
                           .FirstOrDefault(n => n.HasClass("scripting")) != null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetClosestArticleName(string article)
        {
            try
            {
                article = WikiThreads.First(a => a.ToLower() == article.ToLower());
            }
            catch (Exception)
            {
                int minDistance = int.MaxValue;
                foreach (string thread in WikiThreads)
                {
                    int distance = StringHelper.ComputeLevenshteinDistance(article.ToLower(), thread.ToLower());
                    if ((distance <= 2) && (distance < minDistance))
                    {
                        article = thread;
                        minDistance = distance;
                    }
                }
            }
            return article;
        }
    }
}
