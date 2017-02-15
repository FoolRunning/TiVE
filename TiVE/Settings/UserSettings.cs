using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ProdigalSoftware.TiVE.Core.Backend;
using ProdigalSoftware.TiVE.RenderSystem.Lighting;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;

namespace ProdigalSoftware.TiVE.Settings
{
    /// <summary>
    /// User setting for the voxel detail distance
    /// </summary>
    internal enum VoxelDetailLevelDistance
    {
        Closest = 0,
        Close = 1,
        Mid = 2,
        Far = 3,
        Furthest = 4
    }

    /// <summary>
    /// User setting for the shadow detail level
    /// </summary>
    internal enum ShadowDetailLevel
    {
        Off = 255,
        Low = LODLevel.V4,
        Med = LODLevel.V8,
        High = LODLevel.V16,
        Ultra = LODLevel.V32
    }
    
    internal delegate void SettingsChangedHandler(string settingName, Setting newValue);

    internal sealed class UserSettings
    {
        public event SettingsChangedHandler SettingChanged;

        public const string FullScreenModeKey = "fullScreenMode";
        public const string LightingTypeKey = "lightingType";
        public const string WorldShadowDetailKey = "worldShadowDetail";
        public const string ParticleShadowDetailKey = "particleShadowDetail";
        //public const string PreLoadLightingKey = "preLoadLighting";
        public const string LightCullingTypeKey = "lightCullType";
        public const string CubifyVoxelsKey = "cubifyVoxels";
        public const string EnableVSyncKey = "enbaleVSync";
        public const string LightsPerBlockKey = "lightsPerBlock";
        public const string ShadowsPerBlockKey = "shadowsPerBlock";
        public const string AntiAliasAmountKey = "antiAliasAmount";
        public const string DetailDistanceKey = "detailDistance";
        public const string ChunkCreationThreadsKey = "chunkCreationThreads";
        public const string UseThreadedParticlesKey = "useThreadedParticles";
        public const string DisplayResolutionKey = "resolution";

        private const string UserSettingsFileName = "UserSettings.xml";

        private static readonly List<UserSettingOptions> settingOptions = new List<UserSettingOptions>();
        private readonly Dictionary<string, Setting> settings = new Dictionary<string, Setting>();

        static UserSettings()
        {
            settingOptions.Add(new UserSettingOptions(FullScreenModeKey, "Full screen mode", UserOptionTab.Display,
                new EnumSetting<FullScreenMode>(FullScreenMode.WindowFullScreen),
                new UserSettingOption("Full screen window", new EnumSetting<FullScreenMode>(FullScreenMode.WindowFullScreen)),
                new UserSettingOption("Full screen", new EnumSetting<FullScreenMode>(FullScreenMode.FullScreen)),
                new UserSettingOption("Window", new EnumSetting<FullScreenMode>(FullScreenMode.Windowed))));

            UserSettingOption[] availableSettings = TiVEController.Backend.AvailableDisplaySettings.Distinct()
                .OrderBy(d => d.Width * d.Height).ThenBy(d => d.RefreshRate)
                .Select(d => new UserSettingOption($"{d.Width}x{d.Height} - {d.RefreshRate}Hz", new ResolutionSetting(d))).ToArray();
            settingOptions.Add(new UserSettingOptions(DisplayResolutionKey, "Resolution", UserOptionTab.Display, 
                availableSettings[availableSettings.Length - 1].Value, availableSettings));

            settingOptions.Add(new UserSettingOptions(EnableVSyncKey, "V-sync", UserOptionTab.Display, new BoolSetting(true),
                new UserSettingOption("True", new BoolSetting(true)),
                new UserSettingOption("False", new BoolSetting(false))));

            settingOptions.Add(new UserSettingOptions(AntiAliasAmountKey, "Anti-Aliasing", UserOptionTab.Display, new IntSetting(4),
                new UserSettingOption("None", new IntSetting(0)),
                new UserSettingOption("2x", new IntSetting(2)),
                new UserSettingOption("4x", new IntSetting(4)),
                new UserSettingOption("8x", new IntSetting(8)),
                new UserSettingOption("16x", new IntSetting(16))));

            settingOptions.Add(new UserSettingOptions(DetailDistanceKey, "Detail distance", UserOptionTab.Display, 
                new EnumSetting<VoxelDetailLevelDistance>(VoxelDetailLevelDistance.Mid),
                new UserSettingOption("Closest", new EnumSetting<VoxelDetailLevelDistance>(VoxelDetailLevelDistance.Closest)),
                new UserSettingOption("Close", new EnumSetting<VoxelDetailLevelDistance>(VoxelDetailLevelDistance.Close)),
                new UserSettingOption("Mid", new EnumSetting<VoxelDetailLevelDistance>(VoxelDetailLevelDistance.Mid)),
                new UserSettingOption("Far", new EnumSetting<VoxelDetailLevelDistance>(VoxelDetailLevelDistance.Far)),
                new UserSettingOption("Furthest", new EnumSetting<VoxelDetailLevelDistance>(VoxelDetailLevelDistance.Furthest))));

            settingOptions.Add(new UserSettingOptions(WorldShadowDetailKey, "World shadow detail", UserOptionTab.Display,
                new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.High),
                new UserSettingOption("Off", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.Off)),
                new UserSettingOption("Low", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.Low)),
                new UserSettingOption("Medium", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.Med)),
                new UserSettingOption("High", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.High)),
                new UserSettingOption("Ultra", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.Ultra))));

            settingOptions.Add(new UserSettingOptions(ParticleShadowDetailKey, "Particle shadow detail", UserOptionTab.Display,
                new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.Med),
                new UserSettingOption("Off", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.Off)),
                new UserSettingOption("Low", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.Low)),
                new UserSettingOption("Medium", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.Med)),
                new UserSettingOption("High", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.High)),
                new UserSettingOption("Ultra", new EnumSetting<ShadowDetailLevel>(ShadowDetailLevel.Ultra))));

            settingOptions.Add(new UserSettingOptions(LightsPerBlockKey, "Max lights per block", UserOptionTab.Display, new IntSetting(10),
                new UserSettingOption(new IntSetting(5)),
                new UserSettingOption(new IntSetting(10)),
                new UserSettingOption(new IntSetting(15)),
                new UserSettingOption(new IntSetting(20))));

            settingOptions.Add(new UserSettingOptions(ShadowsPerBlockKey, "Max shadows per block", UserOptionTab.Display, new IntSetting(5),
                new UserSettingOption(new IntSetting(2)),
                new UserSettingOption(new IntSetting(3)),
                new UserSettingOption(new IntSetting(5)),
                new UserSettingOption(new IntSetting(10)),
                new UserSettingOption(new IntSetting(20))));

            settingOptions.Add(new UserSettingOptions(CubifyVoxelsKey, "Cubify voxels", UserOptionTab.Display, new BoolSetting(false),
                new UserSettingOption("False", new BoolSetting(false)),
                new UserSettingOption("True", new BoolSetting(true))));

            //settingOptions.Add(new UserSettingOptions(PreLoadLightingKey, "Pre-load lighting", UserOptionTab.Advanced, new BoolSetting(true),
            //    new UserSettingOption("True", new BoolSetting(true)),
            //    new UserSettingOption("False", new BoolSetting(false))));

            int totalCores = Environment.ProcessorCount;
            int numThreadOptions = totalCores > 3 ? totalCores - 1 : 1;
            UserSettingOption[] threadOptions = new UserSettingOption[numThreadOptions];
            for (int i = 0; i < numThreadOptions; i++)
                threadOptions[i] = new UserSettingOption(new IntSetting(i + 1));
            settingOptions.Add(new UserSettingOptions(ChunkCreationThreadsKey, "Chunk creation threads", UserOptionTab.Advanced,
                new IntSetting(totalCores > 3 ? totalCores - 2 : 1), threadOptions));

            settingOptions.Add(new UserSettingOptions(UseThreadedParticlesKey, "Threaded particles", UserOptionTab.Advanced, new BoolSetting(totalCores > 3),
                new UserSettingOption("True", new BoolSetting(true)),
                new UserSettingOption("False", new BoolSetting(false))));

            settingOptions.Add(new UserSettingOptions(LightCullingTypeKey, "Light culling type", UserOptionTab.Advanced,
                new EnumSetting<LightCullType>(LightCullType.Fast),
                new UserSettingOption("Fast", new EnumSetting<LightCullType>(LightCullType.Fast)),
                new UserSettingOption("For perfect shadows", new EnumSetting<LightCullType>(LightCullType.Accurate))));

            settingOptions.Add(new UserSettingOptions(LightingTypeKey, "Lighting type", UserOptionTab.Advanced, new EnumSetting<LightType>(LightType.Normal),
                new UserSettingOption("Normal", new EnumSetting<LightType>(LightType.Normal)),
                new UserSettingOption("Light count debug view", new EnumSetting<LightType>(LightType.Debug1)),
                new UserSettingOption("Light debug view", new EnumSetting<LightType>(LightType.Debug2))));
        }

        private static string SettingsFileFolder => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Prodigal Software", "TiVE");

        public static IEnumerable<UserSettingOptions> AllUserSettingOptions => 
            settingOptions;

        public void Set(string name, Setting newValue)
        {
            settings[name] = newValue;
            SettingChanged?.Invoke(name, newValue);
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
                                continue; // Could not find an option with the correct key so just keep the default value

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

            string defaultStr = defaultValue.SaveAsString();
            foreach (UserSettingOption option in validOptions)
            {
                if (option.Value.SaveAsString() == defaultStr)
                    option.Default = true;
            }
        }

        public IEnumerable<UserSettingOption> ValidOptions => validOptions;
    }

    internal sealed class UserSettingOption
    {
        public readonly Setting Value;

        private readonly string description;

        public bool Default;

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
            return Default ? description + " *" : description;
        }
    }
}
