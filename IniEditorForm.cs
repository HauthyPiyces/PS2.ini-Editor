namespace PS2.ini_Editor
{
    public partial class IniEditorForm : Form
    {
        public IniEditorForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private readonly string defaultIniPath;
        private string currentIniPath = string.Empty;
        private Dictionary<string, ComboBox> settingDropdowns = new();
        private TextBox fpsTextBox;

        private readonly Dictionary<string, string[]> optionMappings = new()
        {
            { "GraphicsQuality", new[] { "Deaktiviert", "Niedrig", "Mittel", "Hoch" } },
            { "FSRQuality",      new[] { "Deaktiviert", "Niedrig", "Mittel", "Hoch" } },
            { "MotionBlur",      new[] { "Deaktiviert", "Aktiviert" } },
            { "VSync",           new[] { "Deaktiviert", "Aktiviert" } },
            { "AAQuality",       new[] { "Deaktiviert", "Niedrig", "Mittel", "Hoch" } },
            { "EffectsQuality",  new[] { "Deaktiviert", "Niedrig", "Mittel", "Hoch" } },
            { "TextureQuality",  new[] { "Deaktiviert", "Niedrig", "Mittel", "Hoch" } },
            { "ShadowQuality",   new[] { "Deaktiviert", "Niedrig", "Mittel", "Hoch", "Sehr Hoch" } },
            { "LightingQuality", new[] { "Deaktiviert", "Niedrig", "Mittel", "Hoch" } },
        };

        public IniEditorForm(Func<string> GetSteamLibraryPath)
        {
            defaultIniPath = Path.Combine(GetSteamLibraryPath(), @"steamapps\common\PlanetSide 2\UserOptions.ini");
            InitializeUI();
            TryLoadDefaultIni();
        }

        private void InitializeUI()
        {
            this.Text = "PlanetSide 2 allgemeine einstellungen";
            this.Size = new System.Drawing.Size(450, 550);

            int y = 20;
            foreach (var kv in optionMappings)
            {
                Label lbl = new() { Text = kv.Key, Left = 20, Top = y + 5, Width = 150 };
                ComboBox cb = new() { Left = 180, Top = y, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
                cb.Items.AddRange(kv.Value);
                this.Controls.Add(lbl);
                this.Controls.Add(cb);
                settingDropdowns[kv.Key] = cb;
                y += 35;
            }

            Label lblFps = new() { Text = "MaximumFPS", Left = 20, Top = y + 5, Width = 150 };
            fpsTextBox = new() { Left = 180, Top = y, Width = 200, PlaceholderText = "z. B. 144" };
            this.Controls.Add(lblFps);
            this.Controls.Add(fpsTextBox);
            y += 40;

            Button btnLoad = new() { Text = "INI manuell laden", Left = 20, Top = y, Width = 150 };
            btnLoad.Click += BtnLoad_Click;
            Button btnSave = new() { Text = "💾 Einstellungen speichern", Left = 180, Top = y, Width = 200 };
            btnSave.Click += BtnSave_Click;

            this.Controls.Add(btnLoad);
            this.Controls.Add(btnSave);
        }

        private void TryLoadDefaultIni()
        {
            if (File.Exists(defaultIniPath))
            {
                currentIniPath = defaultIniPath;
                LoadIni(currentIniPath);
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new()
            {
                Filter = "INI-Dateien (*.ini)|*.ini|Alle Dateien (*.*)|*.*"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                currentIniPath = ofd.FileName;
                LoadIni(currentIniPath);
            }
        }

        private void LoadIni(string path)
        {
            var lines = File.ReadAllLines(path);
            var renderSettings = lines
                .SkipWhile(line => !line.Trim().Equals("[Rendering]", StringComparison.OrdinalIgnoreCase))
                .Skip(1)
                .TakeWhile(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("["))
                .ToDictionary(
                    line => line.Split('=')[0].Trim(),
                    line => line.Split('=')[1].Trim(),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var kv in settingDropdowns)
            {
                if (renderSettings.TryGetValue(kv.Key, out string valueStr) && int.TryParse(valueStr, out int value))
                {
                    kv.Value.SelectedIndex = Math.Clamp(value, 0, kv.Value.Items.Count - 1);
                }
            }

            if (renderSettings.TryGetValue("MaximumFPS", out string fps))
                fpsTextBox.Text = fps;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!File.Exists(currentIniPath))
            {
                MessageBox.Show("INI-Datei nicht geladen.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var lines = File.ReadAllLines(currentIniPath).ToList();
            int start = lines.FindIndex(l => l.Trim().Equals("[Rendering]", StringComparison.OrdinalIgnoreCase));
            if (start == -1) return;

            int end = lines.FindIndex(start + 1, l => l.StartsWith("["));
            if (end == -1) end = lines.Count;

            var editableKeys = settingDropdowns.Keys.Append("MaximumFPS").ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = start + 1; i < end; i++)
            {
                string line = lines[i];
                if (!line.Contains("=")) continue;
                string key = line.Split('=')[0].Trim();
                if (!editableKeys.Contains(key)) continue;

                string newValue = "";
                if (key == "MaximumFPS")
                    newValue = fpsTextBox.Text.Trim();
                else if (settingDropdowns.TryGetValue(key, out ComboBox box) && box.SelectedIndex >= 0)
                    newValue = box.SelectedIndex.ToString();

                lines[i] = key + "=" + newValue;
            }

            File.WriteAllLines(currentIniPath, lines);
            MessageBox.Show("INI wurde aktualisiert.", "Fertig", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
