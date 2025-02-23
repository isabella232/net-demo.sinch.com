using System;
using System.Reflection;

namespace demo.sinch.com.Areas.HelpPage.ModelDescriptions {
    public interface IModelDocumentationProvider {
        string GetDocumentation(MemberInfo member);
        string GetDocumentation(Type type);
    }
}