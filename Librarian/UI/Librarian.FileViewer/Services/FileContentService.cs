namespace Librarian.FileViewer.Services;

public class FileContentService
{
    private readonly string _basePath = @"C:\Projects\BlazorProjects\Librarian\Crawl";

    public async Task<string> GetFileContentAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, filePath);
            
            if (!File.Exists(fullPath))
            {
                return $"File not found: {filePath}";
            }

            var fileInfo = new FileInfo(fullPath);
            var extension = fileInfo.Extension.ToLowerInvariant();
            
            // Check if it's a text file
            if (IsTextFile(extension))
            {
                // Limit file size to prevent memory issues
                if (fileInfo.Length > 10 * 1024 * 1024) // 10MB limit
                {
                    return $"File too large to display: {filePath} ({fileInfo.Length / (1024 * 1024)} MB)";
                }
                
                return await File.ReadAllTextAsync(fullPath);
            }
            else if (IsImageFile(extension))
            {
                return $"Image file: {filePath}\nSize: {fileInfo.Length} bytes\nLast modified: {fileInfo.LastWriteTime}";
            }
            else
            {
                return $"Binary file: {filePath}\nSize: {fileInfo.Length} bytes\nLast modified: {fileInfo.LastWriteTime}\nExtension: {extension}";
            }
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }

    public string GetFileType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".txt" => "text",
            ".md" => "markdown",
            ".json" => "json",
            ".xml" => "xml",
            ".html" => "html",
            ".css" => "css",
            ".js" => "javascript",
            ".cs" => "csharp",
            ".cpp" => "cpp",
            ".h" => "c",
            ".py" => "python",
            ".java" => "java",
            ".sql" => "sql",
            ".yml" or ".yaml" => "yaml",
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" => "image",
            ".pdf" => "pdf",
            ".zip" or ".rar" or ".7z" => "archive",
            _ => "unknown"
        };
    }

    private bool IsTextFile(string extension)
    {
        var textExtensions = new[]
        {
            ".txt", ".md", ".json", ".xml", ".html", ".css", ".js", ".cs", ".cpp", ".h", ".py", ".java", ".sql", ".yml", ".yaml", ".log", ".config", ".ini", ".bat", ".sh", ".ps1", ".razor", ".cshtml", ".jsx", ".tsx", ".ts", ".php", ".rb", ".go", ".rs", ".swift", ".kt", ".dart", ".scala", ".clj", ".lua", ".pl", ".r", ".m", ".vb", ".fs", ".fsx", ".ml", ".mli", ".elm", ".haskell", ".hs", ".lhs", ".erl", ".ex", ".exs", ".jl", ".nim", ".cr", ".d", ".pas", ".pp", ".inc", ".asm", ".s", ".S", ".nasm", ".fasm", ".masm", ".yasm", ".dockerfile", ".dockerignore", ".gitignore", ".gitattributes", ".editorconfig", ".eslintrc", ".prettierrc", ".babelrc", ".browserslistrc", ".npmrc", ".yarnrc", ".nvmrc", ".node-version", ".ruby-version", ".python-version", ".makefile", ".cmake", ".sln", ".csproj", ".vbproj", ".fsproj", ".vcxproj", ".vcproj", ".props", ".targets", ".nuspec", ".packages", ".paket", ".lock", ".gemfile", ".podfile", ".cartfile", ".package", ".toml", ".cfg", ".conf", ".properties", ".env", ".local", ".example", ".sample", ".template", ".stub", ".mock", ".test", ".spec", ".feature", ".story", ".scenario", ".gherkin", ".cucumber", ".behave", ".rspec", ".jasmine", ".jest", ".mocha", ".chai", ".sinon", ".karma", ".protractor", ".webdriver", ".selenium", ".cypress", ".playwright", ".puppeteer", ".storybook", ".chromatic", ".percy", ".applitools", ".browserstack", ".saucelabs", ".testcafe", ".nightwatch", ".codecept", ".detox", ".maestro", ".fastlane", ".circleci", ".travis", ".appveyor", ".azure", ".github", ".gitlab", ".bitbucket", ".jenkins", ".bamboo", ".teamcity", ".octopus", ".ansible", ".chef", ".puppet", ".vagrant", ".terraform", ".pulumi", ".cloudformation", ".arm", ".bicep", ".helm", ".kustomize", ".skaffold", ".tilt", ".docker-compose", ".kubernetes", ".openshift", ".istio", ".linkerd", ".consul", ".nomad", ".vault", ".boundary", ".waypoint", ".packer", ".vagrant", ".virtualbox", ".vmware", ".hyper-v", ".parallels", ".xen", ".kvm", ".qemu", ".libvirt", ".lxc", ".lxd", ".rkt", ".containerd", ".cri-o", ".podman", ".buildah", ".skopeo", ".oras", ".helm", ".flux", ".argo", ".tekton", ".knative", ".crossplane", ".operator", ".crd", ".apiversion", ".kind", ".metadata", ".spec", ".status", ".conditions", ".events", ".logs", ".metrics", ".traces", ".profiles", ".dumps", ".crashes", ".coredumps", ".memory", ".heap", ".cpu", ".gpu", ".disk", ".network", ".io", ".filesystem", ".database", ".cache", ".queue", ".stream", ".batch", ".job", ".task", ".workflow", ".pipeline", ".build", ".deploy", ".release", ".rollback", ".scale", ".autoscale", ".load", ".performance", ".stress", ".chaos", ".security", ".vulnerability", ".compliance", ".audit", ".governance", ".policy", ".rule", ".regulation", ".standard", ".guideline", ".best-practice", ".pattern", ".anti-pattern", ".smell", ".debt", ".refactor", ".migrate", ".upgrade", ".downgrade", ".patch", ".hotfix", ".feature", ".bug", ".issue", ".ticket", ".story", ".epic", ".theme", ".initiative", ".objective", ".goal", ".kpi", ".metric", ".dashboard", ".report", ".analysis", ".insight", ".recommendation", ".action", ".plan", ".strategy", ".roadmap", ".backlog", ".sprint", ".iteration", ".milestone", ".release", ".version", ".changelog", ".history", ".migration", ".upgrade", ".downgrade", ".rollback", ".recovery", ".backup", ".restore", ".disaster", ".continuity", ".availability", ".reliability", ".scalability", ".performance", ".security", ".privacy", ".compliance", ".governance", ".risk", ".threat", ".vulnerability", ".attack", ".defense", ".protection", ".detection", ".response", ".recovery", ".forensics", ".investigation", ".incident", ".event", ".alert", ".notification", ".message", ".email", ".sms", ".push", ".webhook", ".callback", ".api", ".rest", ".graphql", ".grpc", ".soap", ".xml-rpc", ".json-rpc", ".websocket", ".sse", ".mqtt", ".amqp", ".kafka", ".rabbitmq", ".redis", ".memcached", ".elasticsearch", ".solr", ".lucene", ".sphinx", ".whoosh", ".xapian", ".tantivy", ".bleve", ".riot", ".badger", ".boltdb", ".leveldb", ".rocksdb", ".sqlite", ".mysql", ".postgresql", ".mariadb", ".oracle", ".sqlserver", ".db2", ".sybase", ".informix", ".teradata", ".greenplum", ".vertica", ".redshift", ".snowflake", ".bigquery", ".athena", ".presto", ".trino", ".drill", ".impala", ".hive", ".pig", ".spark", ".flink", ".storm", ".samza", ".kafka-streams", ".akka", ".lagom", ".play", ".spring", ".micronaut", ".quarkus", ".helidon", ".vert.x", ".netty", ".undertow", ".jetty", ".tomcat", ".wildfly", ".jboss", ".websphere", ".weblogic", ".glassfish", ".payara", ".liberty", ".open-liberty", ".thorntail", ".smallrye", ".microprofile", ".jakarta", ".javax", ".jsr", ".jep", ".jcp", ".openjdk", ".oracle-jdk", ".adoptopenjdk", ".amazon-corretto", ".azul-zulu", ".bellsoft-liberica", ".eclipse-openj9", ".graalvm", ".native-image", ".substrate", ".quarkus-native", ".spring-native", ".micronaut-native", ".helidon-native", ".vert.x-native", ".netty-native", ".undertow-native", ".jetty-native", ".tomcat-native", ".wildfly-native", ".jboss-native", ".websphere-native", ".weblogic-native", ".glassfish-native", ".payara-native", ".liberty-native", ".open-liberty-native", ".thorntail-native", ".smallrye-native", ".microprofile-native", ".jakarta-native", ".javax-native", ".jsr-native", ".jep-native", ".jcp-native", ".openjdk-native", ".oracle-jdk-native", ".adoptopenjdk-native", ".amazon-corretto-native", ".azul-zulu-native", ".bellsoft-liberica-native", ".eclipse-openj9-native", ".graalvm-native"
        };
        
        return textExtensions.Contains(extension);
    }

    private bool IsImageFile(string extension)
    {
        var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".svg", ".webp", ".ico", ".tiff", ".tif" };
        return imageExtensions.Contains(extension);
    }
}