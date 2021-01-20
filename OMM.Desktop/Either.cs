using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMM.Desktop
{
    //Inspiration from https://mikhail.io/2016/01/validation-with-either-data-type-in-csharp/
    public struct Either<TLeft, TRight>
    {
        public readonly TLeft Left { get; }
        public readonly TRight Right { get; }
        public readonly bool IsLeft { get; }
        public readonly bool IsRight { get => !IsLeft; }

        public Either(TLeft left)
        {
            this.Left = left;
            this.Right = default;
            this.IsLeft = true;
        }

        public Either(TRight right)
        {
            this.Right = right;
            this.Left = default;
            this.IsLeft = false;
        }

        public bool TryGetLeftValue(out TLeft value)
        {
            value = Left;

            return IsLeft;
        }

        public bool TryGetRightValue(out TRight value)
        {
            value = Right;

            return IsRight;
        }

        public static implicit operator Either<TLeft, TRight>(TLeft left) => new Either<TLeft, TRight>(left);

        public static implicit operator Either<TLeft, TRight>(TRight right) => new Either<TLeft, TRight>(right);
    }
}
