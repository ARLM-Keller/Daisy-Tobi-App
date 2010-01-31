using System.IO;

/*
 * based on the Java Wutka DTD Parser by Mark Wutka (http://www.wutka.com/)
 */
namespace DtdSharp
{
    public class DTDSequence : DTDContainer
    {   
        /** Writes out a declaration for this sequence */
        public override void Write(StreamWriter writer)
        {
            writer.Write("(");

            bool isFirst = true;

            foreach (DTDItem item in Items)
            {
                if (!isFirst) writer.Write(",");
                isFirst = false;

                item.Write(writer);
            }
            writer.Write(")");
            Cardinal.Write(writer);
        }

        public override bool Equals(object ob)
        {
            if (ob == this) return true;
            if (!(ob is DTDSequence)) return false;

            return base.Equals(ob);
        }
    }
}