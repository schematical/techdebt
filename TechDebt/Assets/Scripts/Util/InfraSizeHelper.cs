using System;

namespace DefaultNamespace.Util
{
    public class InfraSizeHelper
    {
        public static int SizeToNumber(InfraSize  size)
        {
            switch (size)
            {
                case(InfraSize.Small):
                    return 1;
                    break;
                case(InfraSize.Medium):
                    return 2;
                    break;
                case(InfraSize.Large):
                    return 3;
                    break;
                default:
                    throw new NotImplementedException();

            }
        }
        public static InfraSize NumberToSize(int sizeNumber)
        {
            switch (sizeNumber)
            {
                case(1):
                    return InfraSize.Small;
                    break;
                case(2):
                    return InfraSize.Medium;
                    break;
                case(3):
                    return InfraSize.Large;
                    break;
                default:
                    throw new NotImplementedException();

            }
        }
    }
}