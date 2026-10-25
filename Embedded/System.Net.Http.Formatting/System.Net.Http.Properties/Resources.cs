using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace System.Net.Http.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				Assembly assembly = typeof(Resources).Assembly;
				resourceMan = new ResourceManager("System.Net.Http.Properties.Resources", assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string AsyncResult_CallbackThrewException => ResourceManager.GetString("AsyncResult_CallbackThrewException", resourceCulture);

	internal static string AsyncResult_MultipleCompletes => ResourceManager.GetString("AsyncResult_MultipleCompletes", resourceCulture);

	internal static string AsyncResult_MultipleEnds => ResourceManager.GetString("AsyncResult_MultipleEnds", resourceCulture);

	internal static string AsyncResult_ResultMismatch => ResourceManager.GetString("AsyncResult_ResultMismatch", resourceCulture);

	internal static string ByteRangeStreamContentNoRanges => ResourceManager.GetString("ByteRangeStreamContentNoRanges", resourceCulture);

	internal static string ByteRangeStreamContentNotBytesRange => ResourceManager.GetString("ByteRangeStreamContentNotBytesRange", resourceCulture);

	internal static string ByteRangeStreamEmpty => ResourceManager.GetString("ByteRangeStreamEmpty", resourceCulture);

	internal static string ByteRangeStreamInvalidFrom => ResourceManager.GetString("ByteRangeStreamInvalidFrom", resourceCulture);

	internal static string ByteRangeStreamInvalidOffset => ResourceManager.GetString("ByteRangeStreamInvalidOffset", resourceCulture);

	internal static string ByteRangeStreamNoneOverlap => ResourceManager.GetString("ByteRangeStreamNoneOverlap", resourceCulture);

	internal static string ByteRangeStreamNoOverlap => ResourceManager.GetString("ByteRangeStreamNoOverlap", resourceCulture);

	internal static string ByteRangeStreamNotSeekable => ResourceManager.GetString("ByteRangeStreamNotSeekable", resourceCulture);

	internal static string ByteRangeStreamReadOnly => ResourceManager.GetString("ByteRangeStreamReadOnly", resourceCulture);

	internal static string CannotHaveNullInList => ResourceManager.GetString("CannotHaveNullInList", resourceCulture);

	internal static string CannotUseMediaRangeForSupportedMediaType => ResourceManager.GetString("CannotUseMediaRangeForSupportedMediaType", resourceCulture);

	internal static string CannotUseNullValueType => ResourceManager.GetString("CannotUseNullValueType", resourceCulture);

	internal static string CookieInvalidName => ResourceManager.GetString("CookieInvalidName", resourceCulture);

	internal static string CookieNull => ResourceManager.GetString("CookieNull", resourceCulture);

	internal static string DelegatingHandlerArrayContainsNullItem => ResourceManager.GetString("DelegatingHandlerArrayContainsNullItem", resourceCulture);

	internal static string DelegatingHandlerArrayHasNonNullInnerHandler => ResourceManager.GetString("DelegatingHandlerArrayHasNonNullInnerHandler", resourceCulture);

	internal static string ErrorReadingFormUrlEncodedStream => ResourceManager.GetString("ErrorReadingFormUrlEncodedStream", resourceCulture);

	internal static string FormUrlEncodedMismatchingTypes => ResourceManager.GetString("FormUrlEncodedMismatchingTypes", resourceCulture);

	internal static string FormUrlEncodedParseError => ResourceManager.GetString("FormUrlEncodedParseError", resourceCulture);

	internal static string HttpInvalidStatusCode => ResourceManager.GetString("HttpInvalidStatusCode", resourceCulture);

	internal static string HttpInvalidVersion => ResourceManager.GetString("HttpInvalidVersion", resourceCulture);

	internal static string HttpMessageContentAlreadyRead => ResourceManager.GetString("HttpMessageContentAlreadyRead", resourceCulture);

	internal static string HttpMessageContentStreamMustBeSeekable => ResourceManager.GetString("HttpMessageContentStreamMustBeSeekable", resourceCulture);

	internal static string HttpMessageErrorReading => ResourceManager.GetString("HttpMessageErrorReading", resourceCulture);

	internal static string HttpMessageInvalidMediaType => ResourceManager.GetString("HttpMessageInvalidMediaType", resourceCulture);

	internal static string HttpMessageParserEmptyUri => ResourceManager.GetString("HttpMessageParserEmptyUri", resourceCulture);

	internal static string HttpMessageParserError => ResourceManager.GetString("HttpMessageParserError", resourceCulture);

	internal static string HttpMessageParserInvalidHostCount => ResourceManager.GetString("HttpMessageParserInvalidHostCount", resourceCulture);

	internal static string HttpMessageParserInvalidUriScheme => ResourceManager.GetString("HttpMessageParserInvalidUriScheme", resourceCulture);

	internal static string InvalidArrayInsert => ResourceManager.GetString("InvalidArrayInsert", resourceCulture);

	internal static string JQuery13CompatModeNotSupportNestedJson => ResourceManager.GetString("JQuery13CompatModeNotSupportNestedJson", resourceCulture);

	internal static string JsonSerializerFactoryReturnedNull => ResourceManager.GetString("JsonSerializerFactoryReturnedNull", resourceCulture);

	internal static string JsonSerializerFactoryThrew => ResourceManager.GetString("JsonSerializerFactoryThrew", resourceCulture);

	internal static string MaxDepthExceeded => ResourceManager.GetString("MaxDepthExceeded", resourceCulture);

	internal static string MaxHttpCollectionKeyLimitReached => ResourceManager.GetString("MaxHttpCollectionKeyLimitReached", resourceCulture);

	internal static string MediaTypeFormatter_BsonParseError_MissingData => ResourceManager.GetString("MediaTypeFormatter_BsonParseError_MissingData", resourceCulture);

	internal static string MediaTypeFormatter_BsonParseError_UnexpectedData => ResourceManager.GetString("MediaTypeFormatter_BsonParseError_UnexpectedData", resourceCulture);

	internal static string MediaTypeFormatter_JsonReaderFactoryReturnedNull => ResourceManager.GetString("MediaTypeFormatter_JsonReaderFactoryReturnedNull", resourceCulture);

	internal static string MediaTypeFormatter_JsonWriterFactoryReturnedNull => ResourceManager.GetString("MediaTypeFormatter_JsonWriterFactoryReturnedNull", resourceCulture);

	internal static string MediaTypeFormatterCannotRead => ResourceManager.GetString("MediaTypeFormatterCannotRead", resourceCulture);

	internal static string MediaTypeFormatterCannotReadSync => ResourceManager.GetString("MediaTypeFormatterCannotReadSync", resourceCulture);

	internal static string MediaTypeFormatterCannotWrite => ResourceManager.GetString("MediaTypeFormatterCannotWrite", resourceCulture);

	internal static string MediaTypeFormatterCannotWriteSync => ResourceManager.GetString("MediaTypeFormatterCannotWriteSync", resourceCulture);

	internal static string MediaTypeFormatterNoEncoding => ResourceManager.GetString("MediaTypeFormatterNoEncoding", resourceCulture);

	internal static string MimeMultipartParserBadBoundary => ResourceManager.GetString("MimeMultipartParserBadBoundary", resourceCulture);

	internal static string MultipartFormDataStreamProviderNoContentDisposition => ResourceManager.GetString("MultipartFormDataStreamProviderNoContentDisposition", resourceCulture);

	internal static string MultipartStreamProviderInvalidLocalFileName => ResourceManager.GetString("MultipartStreamProviderInvalidLocalFileName", resourceCulture);

	internal static string NestedBracketNotValid => ResourceManager.GetString("NestedBracketNotValid", resourceCulture);

	internal static string NonNullUriRequiredForMediaTypeMapping => ResourceManager.GetString("NonNullUriRequiredForMediaTypeMapping", resourceCulture);

	internal static string NoReadSerializerAvailable => ResourceManager.GetString("NoReadSerializerAvailable", resourceCulture);

	internal static string ObjectAndTypeDisagree => ResourceManager.GetString("ObjectAndTypeDisagree", resourceCulture);

	internal static string ObjectContent_FormatterCannotWriteType => ResourceManager.GetString("ObjectContent_FormatterCannotWriteType", resourceCulture);

	internal static string QueryStringNameShouldNotNull => ResourceManager.GetString("QueryStringNameShouldNotNull", resourceCulture);

	internal static string ReadAsHttpMessageUnexpectedTermination => ResourceManager.GetString("ReadAsHttpMessageUnexpectedTermination", resourceCulture);

	internal static string ReadAsMimeMultipartArgumentNoBoundary => ResourceManager.GetString("ReadAsMimeMultipartArgumentNoBoundary", resourceCulture);

	internal static string ReadAsMimeMultipartArgumentNoContentType => ResourceManager.GetString("ReadAsMimeMultipartArgumentNoContentType", resourceCulture);

	internal static string ReadAsMimeMultipartArgumentNoMultipart => ResourceManager.GetString("ReadAsMimeMultipartArgumentNoMultipart", resourceCulture);

	internal static string ReadAsMimeMultipartErrorReading => ResourceManager.GetString("ReadAsMimeMultipartErrorReading", resourceCulture);

	internal static string ReadAsMimeMultipartErrorWriting => ResourceManager.GetString("ReadAsMimeMultipartErrorWriting", resourceCulture);

	internal static string ReadAsMimeMultipartHeaderParseError => ResourceManager.GetString("ReadAsMimeMultipartHeaderParseError", resourceCulture);

	internal static string ReadAsMimeMultipartParseError => ResourceManager.GetString("ReadAsMimeMultipartParseError", resourceCulture);

	internal static string ReadAsMimeMultipartStreamProviderException => ResourceManager.GetString("ReadAsMimeMultipartStreamProviderException", resourceCulture);

	internal static string ReadAsMimeMultipartStreamProviderNull => ResourceManager.GetString("ReadAsMimeMultipartStreamProviderNull", resourceCulture);

	internal static string ReadAsMimeMultipartStreamProviderReadOnly => ResourceManager.GetString("ReadAsMimeMultipartStreamProviderReadOnly", resourceCulture);

	internal static string ReadAsMimeMultipartUnexpectedTermination => ResourceManager.GetString("ReadAsMimeMultipartUnexpectedTermination", resourceCulture);

	internal static string RemoteStreamInfoCannotBeNull => ResourceManager.GetString("RemoteStreamInfoCannotBeNull", resourceCulture);

	internal static string SerializerCannotSerializeType => ResourceManager.GetString("SerializerCannotSerializeType", resourceCulture);

	internal static string UnMatchedBracketNotValid => ResourceManager.GetString("UnMatchedBracketNotValid", resourceCulture);

	internal static string UnsupportedIndent => ResourceManager.GetString("UnsupportedIndent", resourceCulture);

	internal static string XmlMediaTypeFormatter_InvalidSerializerType => ResourceManager.GetString("XmlMediaTypeFormatter_InvalidSerializerType", resourceCulture);

	internal static string XmlMediaTypeFormatter_NullReturnedSerializer => ResourceManager.GetString("XmlMediaTypeFormatter_NullReturnedSerializer", resourceCulture);

	internal Resources()
	{
	}
}
