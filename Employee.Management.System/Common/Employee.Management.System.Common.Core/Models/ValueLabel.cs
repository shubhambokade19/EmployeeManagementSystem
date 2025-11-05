namespace Employee.Management.System.Common.Core.Models
{
    public class ValueLabel
    {
        public string? Value { get; set; }
        public string? Label { get; set; }
        public string? ParentValue { get; set; }

        public static implicit operator ValueLabel(List<ValueLabel> v)
        {
            throw new NotImplementedException();
        }

        public static implicit operator List<object>(ValueLabel v)
        {
            throw new NotImplementedException();
        }
    }
}
