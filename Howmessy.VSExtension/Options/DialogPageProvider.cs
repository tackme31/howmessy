namespace Howmessy.VSExtension.Options
{
    public static class DialogPageProvider
    {
        public class General : BaseOptionPage<GeneralOptions> { }
        public class CognitiveComplexity : BaseOptionPage<CognitiveComplexityOptions> { }
        public class CyclomaticComplexity : BaseOptionPage<CyclomaticComplexityOptions> { }
        public class MaintainabilityIndex : BaseOptionPage<MaintainabilityIndexOptions> { }
        public class LinesOfCode : BaseOptionPage<LinesOfCodeOptions> { }
    }
}
