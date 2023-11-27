using System.Collections.Generic;

namespace CutsceneCommandTweak.Lists;

public static class CommandLists
{
    public static IReadOnlyList<string> masterVolumeCommands { get; } = new List<string>()
    {
        "/mastervolume",
        "/lautstärke",
        "/vgénéral",
    };

    public static IReadOnlyList<string> bgmVolumeCommands { get; } = new List<string>()
    {
        "/bgm",
        "/musik",
        "/vmusique",
    };

    public static IReadOnlyList<string> soundFxVolumeCommands { get; } = new List<string>()
    {
        "/soundeffects",
        "/soundeffekte",
        "/veffetssonores",
    };

    public static IReadOnlyList<string> voiceVolumeCommands { get; } = new List<string>()
    {
        "/voice",
        "/stimmen",
        "/vvoix",
    };

    public static IReadOnlyList<string> systemVolumeCommands { get; } = new List<string>()
    {
        "/systemsounds",
        "/systemtöne",
        "/vsonssystèmes",
    };

    public static IReadOnlyList<string> ambientVolumeCommands { get; } = new List<string>()
    {
        "/ambientsounds",
        "/umgebung",
        "/vambiancesonore",
    };

    public static IReadOnlyList<string> controllerSpeakerVolumeCommands { get; } = new List<string>()
    {
        "/systemsoundsspeaker",
        "/systemlautsprecher",
        "/vhautparleur",
        "/vhp",
    };

    public static IReadOnlyList<string> performanceVolumeCommands { get; } = new List<string>()
    {
        "/performsounds",
        "/komplaut",
        "/actionsinterprétation",
        "/ainterp",
    };

    public static IReadOnlyList<string> mountBgmVolumeCommands { get; } = new List<string>()
    {
        "/mountbgm",
        "/reitbgm",
        "/musiquemonture",
    };
}
