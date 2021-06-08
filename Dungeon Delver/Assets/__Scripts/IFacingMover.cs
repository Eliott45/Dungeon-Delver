using UnityEngine;

namespace __Scripts
{
    public interface IFacingMover
    {
        int GetFacing();
        bool Moving { get; }
        float GetSpeed();
        float GridMult { get; }
        Vector2 RoomPos { get; set; }
        Vector2 RoomNum { get; set; }
        Vector2 GetRoomPosOnGrid(float mult = -1);
    }
}

