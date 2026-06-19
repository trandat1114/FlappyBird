namespace FlappyBird.UI.Border;

public static class BorderStyles
{
    public static readonly BorderSet Double  = new('╔','╗','╚','╝','═','║','╠','╣');
    public static readonly BorderSet Single  = new('┌','┐','└','┘','─','│','├','┤');
    public static readonly BorderSet Rounded = new('╭','╮','╰','╯','─','│','├','┤');
    public static readonly BorderSet ASCII   = new('+','+','+','+','-','|','+','+');

    public static BorderSet Get(BorderStyle style) => style switch
    {
        BorderStyle.Single  => Single,
        BorderStyle.Rounded => Rounded,
        BorderStyle.ASCII   => ASCII,
        _                   => Double,
    };
}
