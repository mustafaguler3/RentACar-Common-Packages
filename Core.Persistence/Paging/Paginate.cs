using System;
namespace Core.Persistence.Paging
{
	public class Paginate<T>
	{
		public Paginate()
		{
			Items = Array.Empty<T>();
		}

		public int Size { get; set; } // sayfada kaç data
		public int Index { get; set; } // hangi sayfadayız
		public int Count { get; set; } // toplam kayıp sayısı kaç
		public int Pages { get; set; } // toplam kaç sayfa var
		public IList<T> Items { get; set; }

		public bool HasPrevious => Index > 0; // önceki sayfa varmı
		public bool HasNext => Index + 1 < Pages; // 8 aslında 9. sayfadayız

	}
}

