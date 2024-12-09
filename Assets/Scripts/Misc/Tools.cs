
using UnityEngine;
namespace Game.Tools
{
        public static class VectorTools
        {
            /// <summary>
            /// Multiply a vector by another vector 
            /// </summary>
            /// <param name="value"></param>
            /// <param name="amount"></param>
            /// <returns></returns>
            public static Vector3 MultiplyBy(this Vector3 value, Vector3 amount)
            {
                value.x *= amount.x;
                value.y *= amount.y;
                value.z *= amount.z;
                return value;
            }
            
            public static Vector3 ToVector3Int(this Vector3 vector) => new Vector3(vector.x.ToInt(Round.Down), vector.y.ToInt(Round.Down), vector.z.ToInt(Round.Down));
        }

    public static class IntTools
    {
        /// <summary>
        /// Rounds the current value to the closet of the given two parameters.
        /// In case of equal difference, value will be rounded up.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min">Minimun the value should be</param>
        /// <param name="max">Maximum the value should be</param>
        public static void RoundToClosest(ref this int value, int min, int max)
        {
            //No point in comparisons, just assign and leave.
            if (min == max)
            {
                value = max;
                return;
            }

            //Swap the values over if they are in the wrong order
            if(min > max)
            {
                (max, min) = (min, max);
            }

            //Whichever one is smaller, is the closer one to the target value. 
            int minDiff = Mathf.Abs(min - value);
            int maxDiff = Mathf.Abs(max - value);
            value = minDiff < maxDiff ? min : max;
        }
    }

    public static class FloatTools
    {
        /// <summary>
        /// Rounds the value up or down, and casts it into an int
        /// </summary>
        /// <param name="value"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        public static int ToInt(this float value, Round round) => round == Round.Up ? Mathf.CeilToInt(value) : Mathf.FloorToInt(value);
        
        /// <summary>
        /// Returns the value, or the upper limit of the value, which ever is lower
        /// </summary>
        /// <param name="value"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static float LimitUpper(this float value, float limit) =>  value > limit ? limit : value;

        /// <summary>
        /// Returns the value, or the lower limit, whichever is higher
        /// </summary>
        /// <param name="value"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static float LimitLower(this float value, float limit) =>  value < limit ? limit : value;

        /// <summary>
        /// Limits the value between upper and lower bounds
        /// </summary>
        /// <param name="value"></param>
        /// <param name="upper">Highest the value should go</param>
        /// <param name="lower">Lowest the value should go</param>
        /// <returns></returns>
        public static float Limit(this float value, float upper, float lower) => value.LimitUpper(upper).LimitLower(lower);
    }

    /// <summary>
    /// Direction to round the value
    /// </summary>
    public enum  Round
    {
        Up,
        Down,
    }

}
