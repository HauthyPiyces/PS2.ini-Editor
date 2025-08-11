namespace PS2.ini_Editor
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Form mit automatischem Steam-Pfad starten
            Application.Run(new IniEditorForm(() =>
            {
                string steamPath = null;
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                        steamPath = key.GetValue("SteamPath") as string;
                }
                return steamPath ?? "";
            }));
        }
    }
}