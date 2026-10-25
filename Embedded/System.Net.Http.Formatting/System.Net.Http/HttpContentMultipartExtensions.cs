using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http.Formatting.Parsers;
using System.Net.Http.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpContentMultipartExtensions
{
	private class MultipartAsyncContext
	{
		public Stream ContentStream { get; private set; }

		public ICollection<HttpContent> Result { get; private set; }

		public byte[] Data { get; private set; }

		public MimeMultipartBodyPartParser MimeParser { get; private set; }

		public MultipartAsyncContext(Stream contentStream, MimeMultipartBodyPartParser mimeParser, byte[] data, ICollection<HttpContent> result)
		{
			ContentStream = contentStream;
			Result = result;
			MimeParser = mimeParser;
			Data = data;
		}
	}

	private const int MinBufferSize = 256;

	private const int DefaultBufferSize = 32768;

	public static bool IsMimeMultipartContent(this HttpContent content)
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		return MimeMultipartBodyPartParser.IsMimeMultipartContent(content);
	}

	public static bool IsMimeMultipartContent(this HttpContent content, string subtype)
	{
		if (string.IsNullOrWhiteSpace(subtype))
		{
			throw Error.ArgumentNull("subtype");
		}
		if (content.IsMimeMultipartContent() && content.Headers.ContentType.MediaType.Equals("multipart/" + subtype, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	public static Task<MultipartMemoryStreamProvider> ReadAsMultipartAsync(this HttpContent content)
	{
		return content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider(), 32768);
	}

	public static Task<MultipartMemoryStreamProvider> ReadAsMultipartAsync(this HttpContent content, CancellationToken cancellationToken)
	{
		return content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider(), 32768, cancellationToken);
	}

	public static Task<T> ReadAsMultipartAsync<T>(this HttpContent content, T streamProvider) where T : MultipartStreamProvider
	{
		return content.ReadAsMultipartAsync(streamProvider, 32768);
	}

	public static Task<T> ReadAsMultipartAsync<T>(this HttpContent content, T streamProvider, CancellationToken cancellationToken) where T : MultipartStreamProvider
	{
		return content.ReadAsMultipartAsync(streamProvider, 32768, cancellationToken);
	}

	public static Task<T> ReadAsMultipartAsync<T>(this HttpContent content, T streamProvider, int bufferSize) where T : MultipartStreamProvider
	{
		return content.ReadAsMultipartAsync(streamProvider, bufferSize, CancellationToken.None);
	}

	public static async Task<T> ReadAsMultipartAsync<T>(this HttpContent content, T streamProvider, int bufferSize, CancellationToken cancellationToken) where T : MultipartStreamProvider
	{
		if (content == null)
		{
			throw Error.ArgumentNull("content");
		}
		if (streamProvider == null)
		{
			throw Error.ArgumentNull("streamProvider");
		}
		if (bufferSize < 256)
		{
			throw Error.ArgumentMustBeGreaterThanOrEqualTo("bufferSize", bufferSize, 256);
		}
		Stream contentStream;
		try
		{
			contentStream = await content.ReadAsStreamAsync();
		}
		catch (Exception innerException)
		{
			throw new IOException(System.Net.Http.Properties.Resources.ReadAsMimeMultipartErrorReading, innerException);
		}
		using MimeMultipartBodyPartParser parser = new MimeMultipartBodyPartParser(content, streamProvider);
		byte[] data = new byte[bufferSize];
		await MultipartReadAsync(new MultipartAsyncContext(contentStream, parser, data, streamProvider.Contents), cancellationToken);
		await streamProvider.ExecutePostProcessingAsync(cancellationToken);
		return streamProvider;
	}

	private static async Task MultipartReadAsync(MultipartAsyncContext context, CancellationToken cancellationToken)
	{
		while (true)
		{
			int bytesRead;
			try
			{
				bytesRead = await context.ContentStream.ReadAsync(context.Data, 0, context.Data.Length, cancellationToken);
			}
			catch (Exception innerException)
			{
				throw new IOException(System.Net.Http.Properties.Resources.ReadAsMimeMultipartErrorReading, innerException);
			}
			IEnumerable<MimeBodyPart> enumerable = context.MimeParser.ParseBuffer(context.Data, bytesRead);
			foreach (MimeBodyPart part in enumerable)
			{
				foreach (ArraySegment<byte> segment in part.Segments)
				{
					try
					{
						await part.WriteSegment(segment, cancellationToken);
					}
					catch (Exception innerException2)
					{
						part.Dispose();
						throw new IOException(System.Net.Http.Properties.Resources.ReadAsMimeMultipartErrorWriting, innerException2);
					}
				}
				if (CheckIsFinalPart(part, context.Result))
				{
					return;
				}
			}
		}
	}

	private static bool CheckIsFinalPart(MimeBodyPart part, ICollection<HttpContent> result)
	{
		if (part.IsComplete)
		{
			HttpContent completedHttpContent = part.GetCompletedHttpContent();
			if (completedHttpContent != null)
			{
				result.Add(completedHttpContent);
			}
			bool isFinal = part.IsFinal;
			part.Dispose();
			return isFinal;
		}
		return false;
	}
}
