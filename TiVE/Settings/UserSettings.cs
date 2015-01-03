using System.Collections.Generic;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVE.Settings
{
    internal sealed class UserSettings
    {
        public const string SimpleLightingKey = "simpleLighting";
        public const string ShadedVoxelsKey = "shadedVoxels";
        public const string EnableVSyncKey = "enbaleVSync";
        public const string AntiAliasAmountKey = "antiAliasAmount";

        private readonly List<UserSettingOptions> settingOptions = new List<UserSettingOptions>();
        private readonly Dictionary<string, Setting> settings = new Dictionary<string, Setting>();

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

        public void Load()
        {
            Messages.Print("Loading user settings...");

            settingOptions.Add(new UserSettingOptions(EnableVSyncKey, "V-sync", "Options", new BoolSetting(true), 
                new UserSettingOption("True *", new BoolSetting(true)), 
                new UserSettingOption("False", new BoolSetting(false))));
            
            settingOptions.Add(new UserSettingOptions(AntiAliasAmountKey, "Anti-Aliasing", "Options", new IntSetting(0), 
                new UserSettingOption("None *", new IntSetting(0)), 
                new UserSettingOption("2x", new IntSetting(2)), 
                new UserSettingOption("4x", new IntSetting(4)), 
                new UserSettingOption("8x", new IntSetting(8)), 
                new UserSettingOption("16x", new IntSetting(16))));

            settingOptions.Add(new UserSettingOptions(ShadedVoxelsKey, "Shade voxels", "Options", new BoolSetting(false), 
                new UserSettingOption("True", new BoolSetting(true)), 
                new UserSettingOption("False *", new BoolSetting(false))));

            settingOptions.Add(new UserSettingOptions(SimpleLightingKey, "Simple Lighting", "Options", new BoolSetting(false),
                new UserSettingOption("True", new BoolSetting(true)),
                new UserSettingOption("False *", new BoolSetting(false))));
            
            // Initialize all settings to their default values
            foreach (UserSettingOptions setting in settingOptions)
                settings.Add(setting.SettingKey, setting.DefaultValue);

            // TODO: save and load from a file

            Messages.AddDoneText();
        }
    }

    internal sealed class UserSettingOptions
    {
        public readonly string SettingKey;
        public readonly string Description;
        public readonly string TabName;
        public readonly Setting DefaultValue;

        private readonly UserSettingOption[] validOptions;

        public UserSettingOptions(string settingKey, string description, string tabName, Setting defaultValue, params UserSettingOption[] validOptions)
        {
            SettingKey = settingKey;
            Description = description;
            TabName = tabName;
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
