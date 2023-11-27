namespace CutsceneCommandTweak.Lists;

public static class CommandLists
{
    public static List<string> masterVolumeCommands { get; } = new()
    {
        "/mastervolume",
        "/lautstärke",
        "/vgénéral",
    };

    public static List<string> bgmVolumeCommands { get; } = new()
    {
        "/bgm",
        "/musik",
        "/vmusique",
    };

    public static List<string> soundFxVolumeCommands { get; } = new()
    {
        "/soundeffects",
        "/soundeffekte",
        "/veffetssonores",
    };

    public static List<string> voiceVolumeCommands { get; } = new()
    {
        "/voice",
        "/stimmen",
        "/vvoix",
    };

    public static List<string> systemVolumeCommands { get; } = new()
    {
        "/systemsounds",
        "/systemtöne",
        "/vsonssystèmes",
    };

    public static List<string> ambientVolumeCommands { get; } = new()
    {
        "/ambientsounds",
        "/umgebung",
        "/vambiancesonore",
    };

    public static List<string> controllerSpeakerVolumeCommands { get; } = new()
    {
        "/systemsoundsspeaker",
        "/systemlautsprecher",
        "/vhautparleur",
        "/vhp",
    };

    public static List<string> performanceVolumeCommands { get; } = new()
    {
        "/performsounds",
        "/komplaut",
        "/actionsinterprétation",
        "/ainterp",
    };

    public static List<string> mountBgmVolumeCommands { get; } = new()
    {
        "/mountbgm",
        "/reitbgm",
        "/musiquemonture",
    };
}
