namespace KDESessionManager.Objects.Configs
{
    public class DynamicOutputPath
    {
        public string FilterProperty { get; set;}
        public Dictionary<string, string> SingleOutputDetails { get; set;}
        public MultiOutput MultiOutputDetails { get; set; }

        public DynamicOutputPath(
                                string filterProperty,
                                Dictionary<string, string> singleOutputDetails,
                                MultiOutput multiOutput
                                )
        {
            FilterProperty = filterProperty;
            SingleOutputDetails = singleOutputDetails;
            MultiOutputDetails = multiOutput;
        }

        public class MultiOutput
        {
            public string[] PropertyValues { get; set; }
            public string Path { get; set; }

            public MultiOutput(string[] propertyValues, string path)
            {
                PropertyValues = propertyValues;
                Path = path;
            }
        }

        public static string GetPath(WindowFilter windowFilter, DynamicOutputPath dynamicOutputPath)
        {
            return "/home/nero/Workspace/IT_and_Dev/Apps/Current/Kde-Windows-Session/Kde-Session-Restore/data/sessions/hotseat/session.json"; // TODO: Replace hard coded solution.
        }
    }
}