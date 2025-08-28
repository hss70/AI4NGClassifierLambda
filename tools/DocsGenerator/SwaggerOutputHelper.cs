using System.IO;

namespace DocsGenerator
{
    public static class SwaggerOutputHelper
    {
        public static string GetSolutionRoot()
        {
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directoryInfo != null && !File.Exists(Path.Combine(directoryInfo.FullName, "AI4NGClassifierLambda.sln")))
                directoryInfo = directoryInfo.Parent;

            if (directoryInfo == null)
                throw new DirectoryNotFoundException("Solution root not found.");

            return directoryInfo.FullName;
        }

        public static string GetOutputPath()
        {
            var solutionRoot = GetSolutionRoot();
            var docsFolder = Path.Combine(solutionRoot, "docs");

            if (!Directory.Exists(docsFolder))
                Directory.CreateDirectory(docsFolder);

            //This is the actual file path
            return Path.Combine(docsFolder, "swagger.yaml");
        }

        public static string GetContentRoot()
        {
            // Traverse upward from current directory to find the project root
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "AI4NGClassifierLambda.sln"))) dir = dir.Parent;

            if (dir == null)
                throw new DirectoryNotFoundException("Solution root not found.");

            return Path.Combine(dir.FullName, "src", "AI4NGClassifierLambda");
        }
    }
}