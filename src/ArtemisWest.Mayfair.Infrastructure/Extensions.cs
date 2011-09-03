using System;
using System.Collections;

namespace ArtemisWest.Mayfair.Infrastructure
{
    public static class Extensions
    {
        public static decimal ToPowerOf(this decimal value, decimal exponent)
        {
            var x = Convert.ToDouble(value);
            var y = Convert.ToDouble(exponent);
            var result = Math.Pow(x, y);
            return Convert.ToDecimal(result);
        }

        public static int Count(this IEnumerable source)
        {
            if (source == null)
            {
                return 0;
            }
            var is2 = source as ICollection;
            if (is2 != null)
            {
                return is2.Count;
            }
            int num = 0;
            IEnumerator enumerator = source.GetEnumerator();
            {
                while (enumerator.MoveNext())
                {
                    num++;
                }
            }
            return num;
        }
    }
}
