using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace System.Net.Http.Formatting;

public sealed class DelegatingEnumerable<T> : IEnumerable<T>, IEnumerable
{
	private IEnumerable<T> _source;

	public DelegatingEnumerable()
	{
		_source = Enumerable.Empty<T>();
	}

	public DelegatingEnumerable(IEnumerable<T> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		_source = source;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _source.GetEnumerator();
	}

	public void Add(object item)
	{
		throw new NotImplementedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _source.GetEnumerator();
	}
}
