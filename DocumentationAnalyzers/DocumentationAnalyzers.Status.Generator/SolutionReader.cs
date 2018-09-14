// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Status.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.MSBuild;

    /// <summary>
    /// A class that is used to parse the DocumentationAnalyzers solution to get an overview
    /// about the implemented diagnostics.
    /// </summary>
    public class SolutionReader
    {
        private static Regex _diagnosticPathRegex = new Regex(@"(?<type>[A-Za-z]+)Rules\\(?<name>[A-Za-z0-9]+)\.cs$");
        private INamedTypeSymbol _diagnosticAnalyzerTypeSymbol;
        private INamedTypeSymbol _noCodeFixAttributeTypeSymbol;

        private Solution _solution;
        private Project _analyzerProject;
        private Project _codeFixProject;
        private MSBuildWorkspace _workspace;
        private Assembly _analyzerAssembly;
        private Assembly _codeFixAssembly;
        private Compilation _analyzerCompilation;
        private Compilation _codeFixCompilation;
        private ITypeSymbol _booleanType;
        private Type _batchFixerType;

        private SolutionReader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                if (e.Name.Contains(AnalyzerProjectName))
                {
                    return _analyzerAssembly;
                }

                return null;
            };
        }

        private string SlnPath { get; set; }

        private string AnalyzerProjectName { get; set; }

        private string CodeFixProjectName { get; set; }

        private ImmutableArray<CodeFixProvider> CodeFixProviders { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SolutionReader"/> class.
        /// </summary>
        /// <param name="pathToSln">The path to the DocumentationAnalayzers solution.</param>
        /// <param name="analyzerProjectName">The project name of the analyzer project.</param>
        /// <param name="codeFixProjectName">The project name of the code fix project.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
        public static async Task<SolutionReader> CreateAsync(string pathToSln, string analyzerProjectName = "DocumentationAnalyzers", string codeFixProjectName = "DocumentationAnalyzers.CodeFixes")
        {
            SolutionReader reader = new SolutionReader();

            reader.SlnPath = pathToSln;
            reader.AnalyzerProjectName = analyzerProjectName;
            reader.CodeFixProjectName = codeFixProjectName;
            reader._workspace = MSBuildWorkspace.Create();

            await reader.InitializeAsync().ConfigureAwait(false);

            return reader;
        }

        /// <summary>
        /// Analyzes the project and returns information about the diagnostics in it.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<ImmutableList<DocumentationDiagnostic>> GetDiagnosticsAsync()
        {
            var diagnostics = ImmutableList.CreateBuilder<DocumentationDiagnostic>();

            var syntaxTrees = _analyzerCompilation.SyntaxTrees;

            foreach (var syntaxTree in syntaxTrees)
            {
                var match = _diagnosticPathRegex.Match(syntaxTree.FilePath);
                if (!match.Success)
                {
                    continue;
                }

                string shortName = match.Groups["name"].Value;
                string noCodeFixReason = null;

                // Check if this syntax tree represents a diagnostic
                SyntaxNode syntaxRoot = await syntaxTree.GetRootAsync().ConfigureAwait(false);
                SemanticModel semanticModel = _analyzerCompilation.GetSemanticModel(syntaxTree);
                SyntaxNode classSyntaxNode = syntaxRoot.DescendantNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.ClassDeclaration));

                if (classSyntaxNode == null)
                {
                    continue;
                }

                INamedTypeSymbol classSymbol = semanticModel.GetDeclaredSymbol(classSyntaxNode) as INamedTypeSymbol;

                if (!InheritsFrom(classSymbol, _diagnosticAnalyzerTypeSymbol))
                {
                    continue;
                }

                if (classSymbol.IsAbstract)
                {
                    continue;
                }

                bool hasImplementation = HasImplementation(syntaxRoot);

                IEnumerable<DiagnosticDescriptor> descriptorInfos = GetDescriptor(classSymbol);

                foreach (var descriptorInfo in descriptorInfos)
                {
                    var (codeFixStatus, fixAllStatus) = GetCodeFixAndFixAllStatus(descriptorInfo.Id, classSymbol, out noCodeFixReason);
                    string status = GetStatus(classSymbol, syntaxRoot, semanticModel, descriptorInfo);
                    if (descriptorInfo.CustomTags.Contains(WellKnownDiagnosticTags.NotConfigurable))
                    {
                        continue;
                    }

                    var diagnostic = new DocumentationDiagnostic
                    {
                        Id = descriptorInfo.Id,
                        Category = descriptorInfo.Category,
                        HasImplementation = hasImplementation,
                        Status = status,
                        Name = shortName,
                        Title = descriptorInfo.Title.ToString(),
                        HelpLink = descriptorInfo.HelpLinkUri,
                        CodeFixStatus = codeFixStatus,
                        FixAllStatus = fixAllStatus,
                        NoCodeFixReason = noCodeFixReason,
                    };
                    diagnostics.Add(diagnostic);
                }
            }

            return diagnostics.ToImmutable();
        }

        private static bool HasImplementation(SyntaxNode syntaxRoot)
        {
            bool hasImplementation = true;
            foreach (var trivia in syntaxRoot.DescendantTrivia())
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    if (trivia.ToFullString().Contains("TODO: Implement analysis"))
                    {
                        hasImplementation = false;
                    }
                }
            }

            return hasImplementation;
        }

        private async Task InitializeAsync()
        {
            _solution = await _workspace.OpenSolutionAsync(SlnPath).ConfigureAwait(false);

            _analyzerProject = _solution.Projects.First(x => x.Name == AnalyzerProjectName || x.AssemblyName == AnalyzerProjectName);
            _analyzerCompilation = await _analyzerProject.GetCompilationAsync().ConfigureAwait(false);
            _analyzerCompilation = _analyzerCompilation.WithOptions(_analyzerCompilation.Options.WithOutputKind(OutputKind.DynamicallyLinkedLibrary));

            _codeFixProject = _solution.Projects.First(x => x.Name == CodeFixProjectName || x.AssemblyName == CodeFixProjectName);
            _codeFixCompilation = await _codeFixProject.GetCompilationAsync().ConfigureAwait(false);
            _codeFixCompilation = _codeFixCompilation.WithOptions(_codeFixCompilation.Options.WithOutputKind(OutputKind.DynamicallyLinkedLibrary));

            _booleanType = _analyzerCompilation.GetSpecialType(SpecialType.System_Boolean);

            LoadAssemblies();

            _noCodeFixAttributeTypeSymbol = _analyzerCompilation.GetTypeByMetadataName("DocumentationAnalyzers.NoCodeFixAttribute");
            _diagnosticAnalyzerTypeSymbol = _analyzerCompilation.GetTypeByMetadataName(typeof(DiagnosticAnalyzer).FullName);

            _batchFixerType = _codeFixAssembly.GetType("DocumentationAnalyzers.Helpers.CustomBatchFixAllProvider");

            InitializeCodeFixTypes();
        }

        private void InitializeCodeFixTypes()
        {
            var codeFixTypes = _codeFixAssembly.GetTypes().Where(x => x.FullName.EndsWith("CodeFixProvider"));

            CodeFixProviders = ImmutableArray.Create(
                codeFixTypes
                .Select(t => Activator.CreateInstance(t, true))
                .OfType<CodeFixProvider>()
                .Where(x => x != null)
                .ToArray());
        }

        private void LoadAssemblies()
        {
            _analyzerAssembly = GetAssembly(_analyzerProject);
            _codeFixAssembly = GetAssembly(_codeFixProject);
        }

        private Assembly GetAssembly(Project project)
        {
            return Assembly.LoadFile(project.OutputFilePath);
        }

        private string GetStatus(INamedTypeSymbol classSymbol, SyntaxNode root, SemanticModel model, DiagnosticDescriptor descriptor)
        {
            // Some analyzers use multiple descriptors. We analyze the first one and hope that
            // thats enough.
            var members = classSymbol.GetMembers().Where(x => x.Name.Contains("Descriptor")).ToArray();

            foreach (var member in members)
            {
                ObjectCreationExpressionSyntax initializer;
                SyntaxNode node = root.FindNode(member.Locations.FirstOrDefault().SourceSpan);

                if (node != null)
                {
                    initializer = (node as PropertyDeclarationSyntax)?.Initializer?.Value as ObjectCreationExpressionSyntax;
                    if (initializer == null)
                    {
                        initializer = (node as VariableDeclaratorSyntax)?.Initializer?.Value as ObjectCreationExpressionSyntax;

                        if (initializer == null)
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    continue;
                }

                var firstArgument = initializer.ArgumentList.Arguments[0];

                string constantValue = (string)model.GetConstantValue(firstArgument.Expression).Value;

                if (constantValue != descriptor.Id)
                {
                    continue;
                }

                // We use the fact that the only parameter that returns a boolean is the one we are interested in
                var enabledByDefaultParameter = from argument in initializer.ArgumentList.Arguments
                                                where Equals(model.GetTypeInfo(argument.Expression).Type, _booleanType)
                                                select argument.Expression;
                var parameter = enabledByDefaultParameter.FirstOrDefault();
                string parameterString = parameter.ToString();
                var analyzerConstantLength = "AnalyzerConstants.".Length;

                if (parameterString.Length < analyzerConstantLength)
                {
                    return parameterString;
                }

                return parameter.ToString().Substring(analyzerConstantLength);
            }

            return "Unknown";
        }

        private IEnumerable<DiagnosticDescriptor> GetDescriptor(INamedTypeSymbol classSymbol)
        {
            var analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(_analyzerAssembly.GetType(classSymbol.ToString()));

            // This currently only supports one diagnostic for each analyzer.
            return analyzer.SupportedDiagnostics;
        }

        private (CodeFixStatus, FixAllStatus) GetCodeFixAndFixAllStatus(string diagnosticId, INamedTypeSymbol classSymbol, out string noCodeFixReason)
        {
            CodeFixStatus codeFixStatus;
            FixAllStatus fixAllStatus;

            noCodeFixReason = null;

            var noCodeFixAttribute = classSymbol
                .GetAttributes()
                .SingleOrDefault(x => Equals(x.AttributeClass, _noCodeFixAttributeTypeSymbol));

            bool hasCodeFix = noCodeFixAttribute == null;
            if (!hasCodeFix)
            {
                codeFixStatus = CodeFixStatus.NotImplemented;
                fixAllStatus = FixAllStatus.None;
                if (noCodeFixAttribute.ConstructorArguments.Length > 0)
                {
                    noCodeFixReason = noCodeFixAttribute.ConstructorArguments[0].Value as string;
                }
            }
            else
            {
                // Check if the code fix actually exists
                var codeFixes = CodeFixProviders
                    .Where(x => x.FixableDiagnosticIds.Contains(diagnosticId))
                    .Select(x => IsBatchFixer(x))
                    .Where(x => x != null)
                    .Select(x => (bool)x).ToArray();

                hasCodeFix = codeFixes.Length > 0;

                codeFixStatus = hasCodeFix ? CodeFixStatus.Implemented : CodeFixStatus.NotYetImplemented;

                if (codeFixes.Any(x => x))
                {
                    fixAllStatus = FixAllStatus.BatchFixer;
                }
                else if (codeFixes.Length > 0)
                {
                    fixAllStatus = FixAllStatus.CustomImplementation;
                }
                else
                {
                    fixAllStatus = FixAllStatus.None;
                }
            }

            return (codeFixStatus, fixAllStatus);
        }

        private bool? IsBatchFixer(CodeFixProvider provider)
        {
            var fixAllProvider = provider.GetFixAllProvider();

            if (fixAllProvider == null)
            {
                return null;
            }
            else
            {
                return fixAllProvider.GetType() == _batchFixerType;
            }
        }

        private bool InheritsFrom(INamedTypeSymbol declaration, INamedTypeSymbol possibleBaseType)
        {
            while (declaration != null)
            {
                if (declaration.Equals(possibleBaseType))
                {
                    return true;
                }

                declaration = declaration.BaseType;
            }

            return false;
        }
    }
}
