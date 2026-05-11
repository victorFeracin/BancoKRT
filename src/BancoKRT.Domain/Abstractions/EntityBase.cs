using BancoKRT.Domain.Common;

namespace BancoKRT.Domain.Abstractions
{
    public abstract class EntityBase
    {
        public DateTime CreatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; }

        public bool IsDeleted { get; private set; } = false;

        protected EntityBase()
        {
            CreatedAt = DateTime.UtcNow;
        }

        protected void RestoreState(DateTime createdAt, bool isDeleted, DateTime? deletedAt)
        {
            CreatedAt = createdAt;
            IsDeleted = isDeleted;
            DeletedAt = deletedAt;
        }

        public DomainResult Delete()
        {
            if (IsDeleted)
            {
                return DomainResult.Failure(
                    DomainErrorType.BusinessRule,
                    "Entidade já excluída.");
            }

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            return DomainResult.Success();
        }
    }
}
