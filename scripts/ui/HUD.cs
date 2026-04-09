using Godot;
using ProiectDeLegume.Scripts.Localization;
using ProiectDeLegume.Scripts.Player;

namespace ProiectDeLegume.Scripts.UI;

/// <summary>
/// HUD with stat bars and prompt label. Created programmatically.
/// Lives on a CanvasLayer so it stays fixed on screen.
/// </summary>
public partial class HUD : CanvasLayer
{
    private ProgressBar _hpBar;
    private ProgressBar _hungerBar;
    private ProgressBar _thirstBar;
    private ProgressBar _staminaBar;

    private Label _hpLabel;
    private Label _hungerLabel;
    private Label _thirstLabel;
    private Label _staminaLabel;

    private Label _promptLabel;

    private const float BarWidth = 160f;
    private const float BarHeight = 16f;
    private const float BarX = 16f;
    private const float BarStartY = 640f;
    private const float BarSpacing = 22f;
    private const float LabelWidth = 60f;

    public override void _Ready()
    {
        var control = new Control();
        control.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(control);

        CreateBar(control, 0, Lang.Get("hud.hp"), new Color(0.8f, 0.2f, 0.2f), out _hpBar, out _hpLabel);
        CreateBar(control, 1, Lang.Get("hud.hunger"), new Color(0.9f, 0.6f, 0.1f), out _hungerBar, out _hungerLabel);
        CreateBar(control, 2, Lang.Get("hud.thirst"), new Color(0.2f, 0.4f, 0.9f), out _thirstBar, out _thirstLabel);
        CreateBar(control, 3, Lang.Get("hud.stamina"), new Color(0.2f, 0.8f, 0.2f), out _staminaBar, out _staminaLabel);

        // Prompt label (bottom center)
        _promptLabel = new Label();
        _promptLabel.Position = new Vector2(500, 690);
        _promptLabel.AddThemeColorOverride("font_color", Colors.White);
        _promptLabel.AddThemeFontSizeOverride("font_size", 16);
        control.AddChild(_promptLabel);
    }

    private void CreateBar(Control parent, int index, string label, Color color,
        out ProgressBar bar, out Label valueLabel)
    {
        float y = BarStartY + index * BarSpacing;

        // Label
        var nameLabel = new Label();
        nameLabel.Text = label;
        nameLabel.Position = new Vector2(BarX, y - 2);
        nameLabel.AddThemeFontSizeOverride("font_size", 12);
        nameLabel.AddThemeColorOverride("font_color", Colors.White);
        parent.AddChild(nameLabel);

        // Progress bar
        bar = new ProgressBar();
        bar.Position = new Vector2(BarX + LabelWidth, y);
        bar.CustomMinimumSize = new Vector2(BarWidth, BarHeight);
        bar.Size = new Vector2(BarWidth, BarHeight);
        bar.MinValue = 0;
        bar.MaxValue = 100;
        bar.Value = 100;
        bar.ShowPercentage = false;

        // Style the bar
        var fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = color;
        bar.AddThemeStyleboxOverride("fill", fillStyle);

        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(0.1f, 0.1f, 0.12f, 0.85f);
        bar.AddThemeStyleboxOverride("background", bgStyle);

        parent.AddChild(bar);

        // Value label
        valueLabel = new Label();
        valueLabel.Position = new Vector2(BarX + LabelWidth + BarWidth + 6, y - 2);
        valueLabel.AddThemeFontSizeOverride("font_size", 12);
        valueLabel.AddThemeColorOverride("font_color", Colors.White);
        valueLabel.Text = "100";
        parent.AddChild(valueLabel);
    }

    public void UpdateStats(PlayerStats stats)
    {
        UpdateBar(_hpBar, _hpLabel, stats.Hp, new Color(0.8f, 0.2f, 0.2f));
        UpdateBar(_hungerBar, _hungerLabel, stats.Hunger, new Color(0.9f, 0.6f, 0.1f));
        UpdateBar(_thirstBar, _thirstLabel, stats.Thirst, new Color(0.2f, 0.4f, 0.9f));
        UpdateBar(_staminaBar, _staminaLabel, stats.Fatigue, new Color(0.2f, 0.8f, 0.2f));
    }

    private void UpdateBar(ProgressBar bar, Label label, double value, Color baseColor)
    {
        bar.Value = value;
        label.Text = ((int)value).ToString();

        // Color changes at critical levels
        Color color = value switch
        {
            < 15 => new Color(0.9f, 0.1f, 0.1f),
            < 30 => new Color(0.9f, 0.9f, 0.1f),
            _ => baseColor
        };

        var style = (StyleBoxFlat)bar.GetThemeStylebox("fill").Duplicate();
        style.BgColor = color;
        bar.AddThemeStyleboxOverride("fill", style);
    }

    public void ShowPrompt(string message)
    {
        _promptLabel.Text = message;
    }

    public void HidePrompt()
    {
        _promptLabel.Text = "";
    }
}
