using Unity.Netcode;
using Ladder.Netcode.StatTypes;
using Ladder.Netcode.Primitives;

namespace Ladder.EntityStatistics
{
    public interface ICombatant : IDamageable, IDamager, IKillable
    {

    }

    public interface IDamageable
    {
        public abstract void DoDamage(DamageType damageType, int damage);
    }

    public interface IDamager
    {
        public abstract void DealDamage(DamageType damageType, int damage);
    }

    public interface IHealable
    {
        public abstract void Heal(DamageType damageType, int health);
    }

    public interface IKillable
    {
        public abstract void Kill();
    }
}
