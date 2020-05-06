using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.SelectEmbracingConstruct;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Files;
using JetBrains.ReSharper.Psi.Impl;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Text;
using JetBrains.UI.Icons;

namespace JetBrains.ReSharper.Plugins.Spring
{
    [ProjectFileType(typeof(SpringProjectFileType))]
    public class SpringProjectFileLanguageService : ProjectFileLanguageService
    {
        public SpringProjectFileLanguageService(ProjectFileType projectFileType) : base(projectFileType)
        {
        }

        public override IPsiSourceFileProperties GetPsiProperties(IProjectFile projectFile, IPsiSourceFile sourceFile,
            IsCompileService isCompileService)
        {
            this.AssertProjectFileType(projectFile.LanguageType);
            return new DefaultPsiProjectFileProperties(projectFile, sourceFile);
        }

        public override PsiLanguageType GetPsiLanguageType(IProjectFile projectFile)
        {
            return GetPsiLanguageType(projectFile.LanguageType);
        }

        public override PsiLanguageType GetPsiLanguageType(IPsiSourceFile sourceFile)
        {
            var projectFile = sourceFile.ToProjectFile();
            return projectFile != null ? GetPsiLanguageType(projectFile) : GetPsiLanguageType(sourceFile.LanguageType);
        }

        public override PsiLanguageType GetPsiLanguageType(ProjectFileType languageType)
        {
            return SpringLanguage.Instance;
        }

        public override ILexerFactory GetMixedLexerFactory(ISolution solution, IBuffer buffer,
            IPsiSourceFile sourceFile = null)
        {
            return new SpringLanguageService.SpringLexerFactory();
        }

        protected override PsiLanguageType PsiLanguageType => SpringLanguage.Instance;
        public override IconId Icon => null;
    }
}