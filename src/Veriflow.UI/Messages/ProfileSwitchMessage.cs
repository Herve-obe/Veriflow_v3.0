namespace Veriflow.UI.Messages;

/// <summary>
/// Message to request profile switch (Video/Audio)
/// </summary>
public class ProfileSwitchMessage
{
    public ProfileSwitchMessage(bool switchToVideo)
    {
        SwitchToVideo = switchToVideo;
    }
    
    public bool SwitchToVideo { get; }
}
