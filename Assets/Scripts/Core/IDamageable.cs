namespace Medallas.Core
{
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(int amount);
    }
}
