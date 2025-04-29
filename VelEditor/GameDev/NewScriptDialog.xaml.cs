using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VelEditor.GameProject;

using VelEditor.Utilities;

namespace VelEditor.GameDev
{
    /// <summary>
    /// Interaction logic for NewScriptDialog.xaml
    /// </summary>
    public partial class NewScriptDialog : Window
    {
        public static NewScriptDialog Instance = null;
        private static readonly string _cppCode =@"#include ""{0}.h""

namespace {1}
{{
	REGISTER_SCRIPT({0});

	void {0}::begin_play()
	{{

	}}

	void {0}::update(float dt)
	{{

	}}
}} // namespace {1}";

        private static readonly string _hCode = @"#pragma once
#include <string>

namespace {1}
{{
	class {0} : public vel::script::entity_script
	{{
	public:
		constexpr explicit {0}(vel::game_entity::entity entity)
			: vel::script::entity_script(entity) {{}}
        
        void begin_play() override;
		void update(float dt) override;
    private:
	}};
}}";

        private string _errorMsg = string.Empty;

        private static readonly string _namespace = GetNamespaceFromProjectName();
        public static string FinalName = string.Empty;


        /// <summary>
        /// remove white space
        /// </summary>
        private static string GetNamespaceFromProjectName()
        {
            var projectName = Project.Current.Name.Trim();
            if (string.IsNullOrEmpty(projectName)) return string.Empty;
            projectName = Regex.Replace(projectName, @"[^A-Za-z0-9_]", "");
            return projectName;
        }

        public void CloseDialog()
        {
            Close();
        }

        private bool Validate()
        {
            var name = scriptName.Text.Trim();
            var path = scriptPath.Text.Trim();
            var nameRegex = new Regex(@"[^A-Za-z0-9_]");

            if (string.IsNullOrEmpty(name))
                _errorMsg = "Empty name";

            else if (string.IsNullOrEmpty(path))
                _errorMsg = "Empty path";

            else if (nameRegex.IsMatch(name))
                _errorMsg = "Invalid character(s) used in script name";

            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                _errorMsg = "Invalid character(s) used in path";

            else if (!Path.GetFullPath(Path.Combine(Project.Current.Path, path)).Contains(Path.Combine(Project.Current.Path, @"GameCode\")))
                _errorMsg = "Script must be added to (a sub-folder of) GameCode";

            else if (File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.cpp"))) ||
                File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.h"))))
                _errorMsg = $"script {name} already exists in {path}";

            else
                _errorMsg = string.Empty;

            return string.IsNullOrEmpty(_errorMsg);
        }

        private async void OnCreateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                IsEnabled = false;
                busyAnimation.Visibility = Visibility.Visible;
                try
                {
                    string name = scriptName.Text;
                    string path = Path.GetFullPath(Path.Combine(Project.Current.Path, scriptPath.Text.Trim()));
                    string solution = Project.Current.Solution;
                    var projectName = Project.Current.Name;
                    FinalName = name;

                    //Logger.Log(MessageType.Info, path + "\n" + solution + "\n" + projectName);
                    await Task.Run(() => CreateScript(name, path, solution, projectName));
                }
                catch (Exception ex)
                {
                    FinalName = string.Empty;
                    Debug.Write(ex.Message);
                    Logger.Log(MessageType.Error, $"Failed to create {scriptName.Text} script: {ex.Message}");
                }
                finally
                {
                    NewScriptDialog.Instance.CloseDialog();
                }
            }
            else
            {
                messageTextBlock.Foreground = FindResource("Editor.RedBrush") as Brush;
                messageTextBlock.Text = _errorMsg;
            }
        }

        private void CreateScript(string name, string path, string solution, string projectName)
        {
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);

            var h = Path.GetFullPath(Path.Combine(path, $"{name}.h"));
            var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));

            using (var sw = File.CreateText(h))
            {
                sw.WriteLine(string.Format(_hCode, name, _namespace));
            }
            using (var sw = File.CreateText(cpp))
            {
                sw.WriteLine(string.Format(_cppCode, name, _namespace));
            }
            string[] files = new string[] { cpp, h };

            for(int i = 0; i < 3; ++i)
            {

                if (!VisualStudio.AddFilesToSolution(solution, projectName, files))
                    System.Threading.Thread.Sleep(1000);
                else break;
            }
        }

        private void OnScriptName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var name = scriptName.Text.Trim();
            messageTextBlock.Foreground = FindResource("Editor.FontBrush") as Brush;
            messageTextBlock.Text = $"{name}.h and {name}.cpp will be added to {Project.Current?.Name}.";
        }
        public NewScriptDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            scriptPath.Text = @"GameCode\";
            Instance = this;
        }
    }
}
