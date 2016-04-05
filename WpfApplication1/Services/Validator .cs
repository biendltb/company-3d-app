
using TIS_3dAntiCollision.Core;
namespace TIS_3dAntiCollision.Services
{
    static class Validator
    {
        public static bool ValidXOutOfRange(double x_pos)
        {
            if (x_pos < ConfigParameters.MIN_X_RANGE || x_pos > ConfigParameters.MAX_X_RANGE)
                return false;

            return true;
        }
    }
}
