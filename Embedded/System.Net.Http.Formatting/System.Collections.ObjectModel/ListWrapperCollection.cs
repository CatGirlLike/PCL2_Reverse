using System.Collections.Generic;

namespace System.Collections.ObjectModel;

internal sealed class ListWrapperCollection<T> : Collection<T>
{
	private readonly List<T> _items;

	internal List<T> ItemsList => _items;

	internal ListWrapperCollection()
		: this(new List<T>())
	{
	}

	internal ListWrapperCollection(List<T> list)
		: base((IList<T>)list)
	{
		_items = list;
	}
}
