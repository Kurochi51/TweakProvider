using SimpleTweaksPlugin.TweakSystem;

namespace CutsceneCommandTweak;

[TweakName("Kurochi Tweaks")]
public class KurochiTweaks : SubTweakManager<KurochiTweaks.SubTweak>
{
    public abstract class SubTweak : BaseTweak
    {
        public override string Key => $"{nameof(KurochiTweaks)}@{base.Key}";
    }
}
