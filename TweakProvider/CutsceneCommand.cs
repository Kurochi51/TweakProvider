using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Dalamud;
using Dalamud.Utility;
using Dalamud.Game.Text;
using Dalamud.Game.Config;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using SimpleTweaksPlugin;
using SimpleTweaksPlugin.Utility;
using SimpleTweaksPlugin.TweakSystem;
using CutsceneCommandTweak.Lists;

namespace CutsceneCommandTweak;

[TweakName("Commands in cutscenes")]
[TweakDescription("Enable the use of certain commands in cutscenes")]
[TweakAuthor("Kurochi")]
[TweakAutoConfig]
public class CutsceneCommand : KurochiTweaks.SubTweak
{
#pragma warning disable S1104 // Fields should not have public accessibility
    public class Configs : TweakConfig
    {
        [TweakConfigOption("Toggle Master Volume", 1)]
        public bool MasterVolume;

        [TweakConfigOption("Toggle Background Music", 2)]
        public bool BackgroundMusic;

        [TweakConfigOption("Toggle Sound Effects", 3)]
        public bool SoundEffects;

        [TweakConfigOption("Toggle Voice", 4)]
        public bool Voice;

        [TweakConfigOption("Toggle System Sounds", 5)]
        public bool System;

        [TweakConfigOption("Toggle Ambient Sounds", 6)]
        public bool Ambient;

        [TweakConfigOption("Toggle Speaker Volume", 7)]
        public bool ControllerSpeaker;

        [TweakConfigOption("Toggle Performance", 8)]
        public bool Performance;

        [TweakConfigOption("Toggle Mount Music", 9)]
        public bool MountBackgroundMusic;
    }
#pragma warning restore S1104 // Fields should not have public accessibility

    public Configs Config { get; private set; } = null!;

    private const string EnMessageP1 = "The command “";
    private const string EnMessageP2 = "” is unavailable at this time.";

    private const string DeMessageP1 = "„";
    private const string DeMessageP2 = "“ wurde als Textkommando nicht richtig verwendet.";

    private const string FrMessageP1 = "La commande texte “";
    private const string FrMessageP2 = "” ne peut pas être utilisée de cette façon.";

    private const string JpMessage = "そのコマンドは現在使用できません。： ";

    private string loc1 = string.Empty, loc2 = string.Empty;

    private readonly List<string> allCommands = CommandLists.masterVolumeCommands
        .Concat(CommandLists.bgmVolumeCommands)
        .Concat(CommandLists.soundFxVolumeCommands)
        .Concat(CommandLists.voiceVolumeCommands)
        .Concat(CommandLists.systemVolumeCommands)
        .Concat(CommandLists.ambientVolumeCommands)
        .Concat(CommandLists.controllerSpeakerVolumeCommands)
        .Concat(CommandLists.performanceVolumeCommands)
        .Concat(CommandLists.mountBgmVolumeCommands)
        .ToList();

    protected override void Enable()
    {
        Config = LoadConfig<Configs>() ?? new Configs();
        Service.Chat.CheckMessageHandled += OnChatMessage;
        Service.Framework.Update += PopulateLoc;
    }

    private void PopulateLoc(IFramework _framework)
    {
        if (Service.ClientState is not { ClientLanguage: { } clientLanguage })
        {
            return;
        }

        loc1 = clientLanguage switch
        {
            ClientLanguage.French => FrMessageP1,
            ClientLanguage.German => DeMessageP1,
            ClientLanguage.Japanese => JpMessage,
            ClientLanguage.English => EnMessageP1,
            _ => throw new ArgumentException($"Client Language: {clientLanguage} is unsupported", clientLanguage.ToString()),
        };
        loc2 = clientLanguage switch
        {
            ClientLanguage.French => FrMessageP2,
            ClientLanguage.German => DeMessageP2,
            ClientLanguage.Japanese => string.Empty,
            ClientLanguage.English => EnMessageP2,
            _ => throw new ArgumentException($"Client Language: {clientLanguage} is unsupported", clientLanguage.ToString()),
        };
        Service.Framework.Update -= PopulateLoc;
    }

    private unsafe void OnChatMessage(XivChatType type,
        uint senderid,
        ref SeString sender,
        ref SeString message,
        ref bool isHandled)
    {
        if (type is not XivChatType.ErrorMessage || Service.ClientState is not { ClientLanguage: { } clientLanguage })
        {
            return;
        }

        if (Common.LastCommand is null ||
            !allCommands.Exists(command => Common.LastCommand->ToString().Contains(command, StringComparison.Ordinal)))
        {
            return;
        }

        var configCommandArray = new[]
        {
            (Config.MasterVolume, CommandLists.masterVolumeCommands, SystemConfigOption.IsSndMaster),
            (Config.BackgroundMusic, CommandLists.bgmVolumeCommands, SystemConfigOption.IsSndBgm),
            (Config.SoundEffects, CommandLists.soundFxVolumeCommands, SystemConfigOption.IsSndSe),
            (Config.Voice, CommandLists.voiceVolumeCommands, SystemConfigOption.IsSndVoice),
            (Config.System, CommandLists.systemVolumeCommands, SystemConfigOption.IsSndSystem),
            (Config.Ambient, CommandLists.ambientVolumeCommands, SystemConfigOption.IsSndEnv),
            (Config.ControllerSpeaker, CommandLists.controllerSpeakerVolumeCommands, SystemConfigOption.IsSoundPad),
            (Config.Performance, CommandLists.performanceVolumeCommands, SystemConfigOption.IsSndPerform),
            (Config.MountBackgroundMusic, CommandLists.mountBgmVolumeCommands, SystemConfigOption.SoundChocobo), // MountBGM
        };
        foreach (var (config, commands, option) in configCommandArray)
        {
            HandleCommands((config, commands, option), clientLanguage, message, ref isHandled, loc1, loc2);
        }
    }

    private static void HandleCommands((bool config, IReadOnlyList<string> commands, SystemConfigOption option) configCommandOption,
        ClientLanguage clientLanguage,
        SeString message,
        ref bool isHandled,
        string LocMessage,
        string? LocMessage2 = null)
    {
        foreach (var command in configCommandOption.commands)
        {
            var error = LocMessage + command;
            if (!LocMessage2.IsNullOrWhitespace())
            {
                error += LocMessage2;
            }

            if (!configCommandOption.config || !error.Equals(message.TextValue, StringComparison.Ordinal))
            {
                continue;
            }

            Service.GameConfig.TryGet(configCommandOption.option, out bool optionFlag);
            Service.GameConfig.Set(configCommandOption.option, !optionFlag);
            isHandled = true;
            var chatMessage = ConstructChatMessage(optionFlag, configCommandOption.option, clientLanguage);
            if (chatMessage.IsNullOrWhitespace())
            {
                chatMessage = "There was an issue creating the chat message for: " +
                              command +
                              " with client language: " +
                              clientLanguage;
            }

            Service.Chat.Print(new XivChatEntry
            {
                Message = SeString.Parse(Encoding.UTF8.GetBytes(chatMessage)),
                Type = XivChatType.Echo,
            });

            return;
        }
    }

#pragma warning disable MA0051 // Method is too long
    private static string ConstructChatMessage(bool optionFlag,
        SystemConfigOption option,
        ClientLanguage clientLanguage)
#pragma warning restore MA0051 // Method is too long
    {
        var message = string.Empty;
        switch (clientLanguage)
        {
            case ClientLanguage.French:
            {
                if (option is SystemConfigOption.SoundChocobo)
                {
                    message = "La musique à dos de monture a été";
                    message += optionFlag ? " désactivée." : " activée.";

                    break;
                }

                message = optionFlag ? "Vous avez activé " : "Vous avez désactivé ";
                message += option switch
                {
                    SystemConfigOption.IsSndMaster => "le volume général.",
                    SystemConfigOption.IsSndBgm => "la musique.",
                    SystemConfigOption.IsSndSe => "les effets sonores.",
                    SystemConfigOption.IsSndVoice => "les voix.",
                    SystemConfigOption.IsSndSystem => "les sons système.",
                    SystemConfigOption.IsSndEnv => "l'ambiance sonore.",
                    SystemConfigOption.IsSoundPad => "le haut-parleur pour les sons système.",
                    SystemConfigOption.IsSndPerform => "les Actions d'interprétation.",
                    _ => string.Empty,
                };
                if (message.Last() is not '.')
                {
                    return string.Empty;
                }

                break;
            }
            case ClientLanguage.German:
            {
                message = option switch
                {
                    SystemConfigOption.IsSndMaster => "Hauptlautstärke",
                    SystemConfigOption.IsSndBgm => "Hintergrundmusik",
                    SystemConfigOption.IsSndSe => "Soundeffekte",
                    SystemConfigOption.IsSndVoice => "Stimmen",
                    SystemConfigOption.IsSndSystem => "Systemtöne",
                    SystemConfigOption.IsSndEnv => "Umgebungsgeräusche",
                    SystemConfigOption.IsSoundPad => "Systemtöne über Lautsprecher",
                    SystemConfigOption.IsSndPerform => "Kompositionen",
                    SystemConfigOption.SoundChocobo => "Musik beim Reiten",
                    _ => message,
                };
                if (message.IsNullOrWhitespace())
                {
                    return string.Empty;
                }

                if (option is SystemConfigOption.SoundChocobo)
                {
                    message += optionFlag ? " stummgeschaltet" : " wieder eingeschaltet";

                    break;
                }

                message += optionFlag ? " wieder eingeschaltet" : " stummgeschaltet";

                break;
            }
            case ClientLanguage.Japanese:
            {
                message = option switch
                {
                    SystemConfigOption.IsSndMaster => "マスターボリューム",
                    SystemConfigOption.IsSndBgm => "BGM",
                    SystemConfigOption.IsSndSe => "効果音",
                    SystemConfigOption.IsSndVoice => "ボイス",
                    SystemConfigOption.IsSndSystem => "システム音",
                    SystemConfigOption.IsSndEnv => "環境音",
                    SystemConfigOption.IsSoundPad => "システム音のスピーカー出力",
                    SystemConfigOption.IsSndPerform => "楽器演奏",
                    SystemConfigOption.SoundChocobo => "マウント騎乗中のBGM再生を",
                    _ => message,
                };
                if (message.IsNullOrWhitespace())
                {
                    return string.Empty;
                }

                if (option is SystemConfigOption.SoundChocobo)
                {
                    message += optionFlag ? "無効にしました。" : "有効にしました。";

                    break;
                }

                message += optionFlag ? "のミュートを解除しました。" : "をミュートしました。";

                break;
            }
            case ClientLanguage.English:
            {
                message = option switch
                {
                    SystemConfigOption.IsSndMaster => "Master",
                    SystemConfigOption.IsSndBgm => "BGM",
                    SystemConfigOption.IsSndSe => "Sound effects",
                    SystemConfigOption.IsSndVoice => "Voice",
                    SystemConfigOption.IsSndSystem => "System sounds",
                    SystemConfigOption.IsSndEnv => "Ambient sounds",
                    SystemConfigOption.IsSoundPad => "System sounds speaker",
                    SystemConfigOption.IsSndPerform => "Performance",
                    SystemConfigOption.SoundChocobo => "Mount BGM",
                    _ => message,
                };
                if (message.IsNullOrWhitespace())
                {
                    return string.Empty;
                }

                if (option is SystemConfigOption.SoundChocobo)
                {
                    message += optionFlag ? " volume muted." : " volume unmuted";

                    break;
                }

                message += optionFlag ? " volume unmuted." : " volume muted.";

                break;
            }
        }

        return message;
    }

    protected override void Disable()
    {
        Service.Chat.CheckMessageHandled -= OnChatMessage;
        Service.Framework.Update -= PopulateLoc;
        SaveConfig(Config);
    }
}
