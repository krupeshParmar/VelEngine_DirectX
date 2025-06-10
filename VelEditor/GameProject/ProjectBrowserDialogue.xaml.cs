using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace VelEditor.GameProject
{
    /// <summary>
    /// Interaction logic for ProjectBrowserDialogue.xaml
    /// </summary>
    public partial class ProjectBrowserDialogue : Window
    {
        private readonly CubicEase _easing = new() { EasingMode = EasingMode.EaseInOut };
        public static bool GotoNewProjectTab { get; set; }
        public ProjectBrowserDialogue()
        {
            InitializeComponent();
            Loaded += OnProjectBrowserLoaded;
        }

        private void OnProjectBrowserLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnProjectBrowserLoaded;
            if(!OpenProject.Projects.Any() || GotoNewProjectTab)
            {
                if (!GotoNewProjectTab)
                {
                    openProjectButton.IsEnabled = false;
                    openProjectView.Visibility = Visibility.Hidden;
                }
                OnToggleButton_Click(createProjectButton, new RoutedEventArgs());
            }
            GotoNewProjectTab = false;
        }
        private void AnimateToCreateProject()
        {
            var highlightAnimation = new DoubleAnimation(210, 410, new Duration(TimeSpan.FromSeconds(0.2)));
            highlightAnimation.EasingFunction = _easing;
            highlightAnimation.Completed += (s, e) =>
            {
                var animation = new ThicknessAnimation(new Thickness(0), new Thickness(-1600, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.4)));
                animation.EasingFunction = _easing;
                BrowserContent.BeginAnimation(MarginProperty, animation);
            };
            highlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
        }

        private void AnimateToOpenProject()
        {
            var highlightAnimation = new DoubleAnimation(410, 210, new Duration(TimeSpan.FromSeconds(0.2)));
            highlightAnimation.EasingFunction = _easing;
            highlightAnimation.Completed += (s, e) =>
            {
                var animation = new ThicknessAnimation(new Thickness(-1600, 0, 0, 0), new Thickness(0), new Duration(TimeSpan.FromSeconds(0.4)));
                animation.EasingFunction = _easing;
                BrowserContent.BeginAnimation(MarginProperty, animation);
            };
            highlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
        }

        private void OnToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == openProjectButton)
            {
                if (createProjectButton.IsChecked == true)
                {
                    createProjectButton.IsChecked = false;
                    AnimateToOpenProject();
                    openProjectView.IsEnabled = true;
                    createProjectView.IsEnabled = false;
                    //BrowserContent.Margin = new Thickness(0);
                }
                openProjectButton.IsChecked = true;
            }
            else
            {
                if (openProjectButton.IsChecked == true)
                {
                    openProjectButton.IsChecked = false;
                    AnimateToCreateProject();
                    createProjectView.IsEnabled = true;
                    openProjectView.IsEnabled = false;
                    //BrowserContent.Margin = new Thickness(-1600, 0, 0, 0);
                }
                createProjectButton.IsChecked = true;
            }
        }
    }
}
