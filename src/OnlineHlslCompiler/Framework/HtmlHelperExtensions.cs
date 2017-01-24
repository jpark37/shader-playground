using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OnlineHlslCompiler.ViewModels;

namespace OnlineHlslCompiler.Framework
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString TargetProfilesAsJsonArray(this HtmlHelper html, Compiler compiler)
        {
            return new MvcHtmlString(Newtonsoft.Json.JsonConvert.SerializeObject(TargetProfiles(compiler)));
        }

        private static IEnumerable<object[]> TargetProfiles(Compiler compiler)
        {
            return Enum.GetValues(typeof(TargetProfile))
                .Cast<TargetProfile>()
                .Where(x => SupportsTargetProfile(compiler, x))
                .Select(x => new object[] { (int) x, x.ToString() });
        }

        private static bool SupportsTargetProfile(Compiler compiler, TargetProfile targetProfile)
        {
            switch (targetProfile)
            {
                case TargetProfile.cs_6_0:
                case TargetProfile.ds_6_0:
                case TargetProfile.gs_6_0:
                case TargetProfile.hs_6_0:
                case TargetProfile.ps_6_0:
                case TargetProfile.vs_6_0:
                    return compiler == Compiler.NewCompiler;

                default:
                    return compiler == Compiler.OldCompiler;
            }
        }
    }
}