using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Diagnostics;

namespace Needlework.Net.Controls;

[TemplatePart("PART_GithubButton", typeof(Button))]
public partial class UserCard : ContentControl
{
    private Button? _githubButton;

    public UserCard()
    {
        UserImageMargin = new(0, !double.IsNaN(Height) ? 100 - Height : 0, 0, 0);
    }

    public static readonly StyledProperty<IImage?> UserImageProperty =
        AvaloniaProperty.Register<UserCard, IImage?>(nameof(UserImage), defaultValue: null);

    public IImage? UserImage
    {
        get { return GetValue(UserImageProperty); }
        set { SetValue(UserImageProperty, value); }
    }

    public static readonly StyledProperty<string?> UserNameProperty =
        AvaloniaProperty.Register<UserCard, string?>(nameof(UserName), defaultValue: null);

    public string? UserName
    {
        get { return GetValue(UserNameProperty); }
        set { SetValue(UserNameProperty, value); }
    }

    public static readonly StyledProperty<string?> UserGithubProperty =
        AvaloniaProperty.Register<UserCard, string?>(nameof(UserGithub), defaultValue: null);

    public string? UserGithub
    {
        get { return GetValue(UserGithubProperty); }
        set { SetValue(UserGithubProperty, value); }
    }

    public static readonly DirectProperty<UserCard, Thickness> UserImageMarginProperty =
        AvaloniaProperty.RegisterDirect<UserCard, Thickness>(nameof(UserImageMargin), o => o.UserImageMargin);

    private Thickness _userImageMargin = new(0, 0, 0, 0);

    public Thickness UserImageMargin
    {
        get { return _userImageMargin; }
        private set { SetAndRaise(UserImageMarginProperty, ref _userImageMargin, value); }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        SizeChanged += UserCard_SizeChanged;

        if (_githubButton != null)
        {
            _githubButton.Click -= GithubButton_Click;
        }

        _githubButton = e.NameScope.Find("PART_GithubButton") as Button;

        if (_githubButton != null)
        {
            _githubButton.Click += GithubButton_Click;
        }
    }

    private void UserCard_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UserImageMargin = new(0, !double.IsNaN(e.NewSize.Height) ? 100 - e.NewSize.Height : 0, 0, 0);
    }

    private void GithubButton_Click(object? sender, RoutedEventArgs e)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo($"https://github.com/{UserGithub}") { UseShellExecute = true }
        };
        process.Start();
    }
}