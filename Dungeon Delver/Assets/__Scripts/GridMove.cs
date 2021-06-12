using UnityEngine;

namespace __Scripts
{
    public class GridMove : MonoBehaviour
    {
        private IFacingMover _mover;

        private void Awake()
        {
            _mover = GetComponent<IFacingMover>();
        }

        private void FixedUpdate()
        {
            if (!_mover.Moving) return; // Если объект не перемещается, выйти
            var facing = _mover.GetFacing();

            // Если объект перемещается, применить выравнивание по сетке
            // Cначала получить координаты ближайшего узла сетки
            Vector2 rPos = _mover.RoomPos;
            Vector2 rPosGrid = _mover.GetRoomPosOnGrid();
            // Этот код полагается на интерфейс который использует InRoom для определения шага сетки

            // Затем подвинуть объект в сторону линии сетки
            float delta = 0;
            if(facing == 0 || facing == 2)
            {
                // Движени по горизонатали, выравнивание по оси y
                delta = rPosGrid.y - rPos.y;
            } else
            {
                // Движение по вертикали, выравнивание по оси х
                delta = rPosGrid.x - rPos.x;
            }
            if (delta == 0) return; // Объект уже выровнен по сетке

            var move = _mover.GetSpeed() * Time.fixedDeltaTime;
            move = Mathf.Min(move, Mathf.Abs(delta));
            if (delta < 0) move = -move;

            if(facing == 0 || facing ==2)
            {
                // Движени по горизонатали, выравнивание по оси y
                rPos.y += move;
            } else
            {
                // Движение по вертикали, выравнивание по оси х
                rPos.x += move;
            }

            _mover.RoomPos = rPos;
        }
    }
}
