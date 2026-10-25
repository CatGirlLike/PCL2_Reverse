using System.Reflection;

namespace System.Net.Http.Formatting;

public interface IRequiredMemberSelector
{
	bool IsRequiredMember(MemberInfo member);
}
