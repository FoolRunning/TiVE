﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Settings
{
    internal sealed class UserSettings
    {
        public const string FullScreenModeKey = "fullScreenMode";
        public const string SimpleLightingKey = "simpleLighting";
        public const string ShadedVoxelsKey = "shadedVoxels";
        public const string EnableVSyncKey = "enbaleVSync";
        public const string AntiAliasAmountKey = "antiAliasAmount";
        public const string ChunkCreationThreadsKey = "chunkCreationThreads";

        private const string UserSettingsFileName = "UserSettings.xml";

        private readonly List<UserSettingOptions> settingOptions = new List<UserSettingOptions>();
        private readonly Dictionary<string, Setting> settings = new Dictionary<string, Setting>();

        private static string SettingsFileFolder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Prodigal Software", "TiVE");
            }
        }

        public IEnumerable<UserSettingOptions> AllUserSettingOptions
        {
            get { return settingOptions; }
        }

        public void Set(string name, Setting newValue)
        {
            settings[name] = newValue;
        }

        public Setting Get(string name)
        {
            Setting value;
            settings.TryGetValue(name, out value);
            return value ?? Setting.Null;
        }

        public void Save()
        {
            string settingsDir = SettingsFileFolder;
            if (!Directory.Exists(settingsDir))
                Directory.CreateDirectory(settingsDir);

            using (XmlTextWriter writer = new XmlTextWriter(Path.Combine(settingsDir, UserSettingsFileName), Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("Settings");
                foreach (KeyValuePair<string, Setting> setting in settings)
                    writer.WriteElementString(setting.Key, setting.Value.SaveAsString());
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public void Load()
        {
            Messages.Print("Loading user settings...");

            settingOptions.Add(new UserSettingOptions(FullScreenModeKey, "Full screen mode", UserOptionTab.Display, 
                new EnumSetting<FullScreenMode>(FullScreenMode.WindowFullScreen),
                new UserSettingOption("Full screen window *", new EnumSetting<FullScreenMode>(FullScreenMode.WindowFullScreen)),
                new UserSettingOption("Full screen", new EnumSetting<FullScreenMode>(FullScreenMode.FullScreen)),
                new UserSettingOption("Window", new EnumSetting<FullScreenMode>(FullScreenMode.Windowed))));

            settingOptions.Add(new UserSettingOptions(EnableVSyncKey, "V-sync", UserOptionTab.Display, new BoolSetting(true), 
                new UserSettingOption("True *", new BoolSetting(true)), 
                new UserSettingOption("False", new BoolSetting(false))));

            settingOptions.Add(new UserSettingOptions(AntiAliasAmountKey, "Anti-Aliasing", UserOptionTab.Display, new IntSetting(0), 
                new UserSettingOption("None *", new IntSetting(0)), 
                new UserSettingOption("2x", new IntSetting(2)), 
                new UserSettingOption("4x", new IntSetting(4)), 
                new UserSettingOption("8x", new IntSetting(8)), 
                new UserSettingOption("16x", new IntSetting(16))));

            settingOptions.Add(new UserSettingOptions(ShadedVoxelsKey, "Shade voxels", UserOptionTab.Display, new BoolSetting(false), 
                new UserSettingOption("True", new BoolSetting(true)), 
                new UserSettingOption("False *", new BoolSetting(false))));

            settingOptions.Add(new UserSettingOptions(SimpleLightingKey, "Simple lighting", UserOptionTab.Display, new BoolSetting(false),
                new UserSettingOption("True", new BoolSetting(true)),
                new UserSettingOption("False *", new BoolSetting(false))));

            settingOptions.Add(new UserSettingOptions(ChunkCreationThreadsKey, "Chunk creation threads", UserOptionTab.Advanced, new IntSetting(3),
                new UserSettingOption(new IntSetting(1)),
                new UserSettingOption(new IntSetting(2)),
                new UserSettingOption("3 *", new IntSetting(3)),
                new UserSettingOption(new IntSetting(4)),
                new UserSettingOption(new IntSetting(5)),
                new UserSettingOption(new IntSetting(6))));
            
            // Initialize all settings to their default values
            foreach (UserSettingOptions setting in settingOptions)
                settings.Add(setting.SettingKey, setting.DefaultValue);

            string settingsFilePath = Path.Combine(SettingsFileFolder, UserSettingsFileName);
            if (File.Exists(settingsFilePath))
            {
                using (XmlReader reader = new XmlTextReader(settingsFilePath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);
                    if (doc.DocumentElement != null)
                    {
                        foreach (XmlElement setting in doc.DocumentElement.ChildNodes.OfType<XmlElement>())
                        {
                            UserSettingOptions options = settingOptions.Find(op => op.SettingKey == setting.Name);
                            if (options == null)
                                continue; // Could not find an option with the correct key

                            string settingValue = setting.InnerText;
                            UserSettingOption selectedOption = options.ValidOptions.FirstOrDefault(op => op.Value.SaveAsString() == settingValue);
                            if (selectedOption != null)
                                settings[setting.Name] = selectedOption.Value;
                        }
                    }
                }
            }


            Messages.AddDoneText();
        }
    }

    internal enum UserOptionTab
    {
        Display,
        Controls,
        Sound,
        Advanced
    }

    internal sealed class UserSettingOptions
    {
        public readonly string SettingKey;
        public readonly string Description;
        public readonly UserOptionTab OptionTab;
        public readonly Setting DefaultValue;

        private readonly UserSettingOption[] validOptions;

        public UserSettingOptions(string settingKey, string description, UserOptionTab optionTab, Setting defaultValue, params UserSettingOption[] validOptions)
        {
            SettingKey = settingKey;
            Description = description;
            OptionTab = optionTab;
            DefaultValue = defaultValue;
            this.validOptions = validOptions;
        }

        public IEnumerable<UserSettingOption> ValidOptions
        {
            get { return validOptions; }
        }
    }

    internal sealed class UserSettingOption
    {
        public readonly Setting Value;

        private readonly string description;

        public UserSettingOption(Setting value) : this(value.ToString(), value)
        {
        }

        public UserSettingOption(string description, Setting value)
        {
            this.description = description;
            Value = value;
        }
        public override string ToString()
        {
            return description;
        }
    }
}
