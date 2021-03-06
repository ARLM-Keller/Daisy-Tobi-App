/*
 * based on the Java Wutka DTD Parser by Mark Wutka (http://www.wutka.com/)
 */
namespace DtdSharp
{
    public class TokenType
    {
        public int Value { get; set; }
        public string Name { get; set; }

        public TokenType(int aValue, string aName)
        {
            Value = aValue;
            Name = aName;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (!(o is TokenType)) return false;

            TokenType other = (TokenType)o;
            if (other.Value == Value) return true;
            return false;
        }

        public int HashCode()
        {
            return Name.GetHashCode();
        }
    }
}