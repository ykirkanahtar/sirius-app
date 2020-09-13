namespace Sirius.Shared.Dtos
{
    public class LookUpDto
    {
        public LookUpDto(string value, string label)
        {
            Value = value;
            Label = label;
        }
        public string Value { get; private set; }
        public string Label { get; private set; }
    }
}