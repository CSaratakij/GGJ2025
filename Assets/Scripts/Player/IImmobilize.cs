using UnityEngine;

namespace Game
{
    public interface IImmobilize
    {
        void Immobilize(Color? color);
        bool CanImmobilize();
    }
}
