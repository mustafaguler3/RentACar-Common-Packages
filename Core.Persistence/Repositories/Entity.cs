using System;
namespace Core.Persistence.Repositories
{
	public class Entity<TId>
	{
		public TId Id { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public DateTime? DeletedDate { get; set; }

		public Entity()
		{
			Id = default;
		}

		public Entity(TId id)
		{
			Id = id;
		}
	}
}

