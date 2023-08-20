namespace KDESessionManager.Objects.Configs
{
    public class Config
    {
        public string SessionFileIdPrefix { get; set; }
        public string SpacesReplacement { get; set; }

        public string DefaultWindowFilterPath { get; set; }
        public string[] WindowFiltersPathsSelection { get; set; }

        public string DefaultDynamicOutputPath { get; set; }
        public string[] DynamicOutputPathsSelection { get; set; }

        public Config (
                        string sessionFileIdPrefix,
                        string spacesReplacement,
                        string defaultWindowFilterPath,
                        string[] windowFiltersPathsSelection,
                        string defaultDynamicOutputPath,
                        string[] dynamicOutputPathsSelection
                        )
                        {
                            SessionFileIdPrefix = sessionFileIdPrefix;
                            SpacesReplacement = spacesReplacement;
                            DefaultWindowFilterPath = defaultWindowFilterPath;
                            WindowFiltersPathsSelection = windowFiltersPathsSelection;
                            DefaultDynamicOutputPath = defaultDynamicOutputPath;
                            DynamicOutputPathsSelection = dynamicOutputPathsSelection;
                        }
    }
}