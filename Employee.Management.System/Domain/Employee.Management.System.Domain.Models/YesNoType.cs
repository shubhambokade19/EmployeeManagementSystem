namespace Employee.Management.System.Domain.Models
{
    public class YesNoType
    {
        public const int ALL = 0;
        public const int NO = 1;
        public const int YES = 2;

        private static readonly string[] Names = new string[]
        {
            "(All)",
            "No",
            "Yes"
        };

        public YesNoType(string allOrSelect = GlobalConstants.All)
        {
            Names[0] = $"({allOrSelect})";
        }

        public string this[int index]
        {
            get
            {
                if (index >= Names.Count() || index < 0) return GlobalConstants.Unsupported;
                else return Names[index];
            }
        }
    }
}
