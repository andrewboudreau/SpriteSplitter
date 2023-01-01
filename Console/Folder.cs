using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.IO.Compression;

namespace SpriteSplitter
{
    public class Folder
    {
        private const char dash = '-';

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly List<string> history;

        private readonly int[] outputCounter = new int[64];

        private int outputId;

        private ConcurrentDictionary<string, int> Names { get; }

        private int OutputId => Interlocked.Increment(ref outputId);

        public Folder(string prefix, string resource, params object[] options)
        {
            Names = new ConcurrentDictionary<string, int>(Environment.ProcessorCount * 4, 64);
            ResourceValue = resource;
            ResourceName = "";
            history = new List<string>();
            InstanceId = ConvertToBase(long.Parse(DateTime.Now.Ticks.ToString()[4..^1]));

            var folderName = new StringBuilder();
            prefix = prefix.TrimEnd(dash);
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                folderName.Append(prefix);
                folderName.Append(dash);
            }
            Prefix = prefix;

            folderName.Append(InstanceId);

            if (!string.IsNullOrWhiteSpace(resource))
            {
                if (resource.Contains('\\') || resource.Contains('.'))
                {
                    var paths = resource.Split('\\');
                    var file = string.IsNullOrEmpty(paths[^1]) ? paths[^2] : paths[^1];
                    var part = file.Split('.');
                    resource = string.IsNullOrEmpty(part[^2]) ? part[^1] : part[^2];
                }

                ResourceName = resource.Trim().Trim(dash);
                folderName.Append(dash);
                folderName.Append(ResourceName);
            }

            foreach (var option in options)
            {
                folderName.Append(dash);
                folderName.Append(option);
            }

            FolderPath = $@".\{folderName}";
            Directory.CreateDirectory(FolderPath);
        }

        /// <summary>
        /// Gets a ticks-since-epoch encoded value. Unique for the current execution.
        /// </summary>
        public string InstanceId { get; }

        /// <summary>
        /// Gets the full path to this folder.
        /// </summary>
        public string FolderPath { get; }

        /// <summary>
        /// Gets the resource name used in the folder name.
        /// </summary>
        public string ResourceName { get; }

        /// <summary>
        /// Gets the original resource name provided.
        /// </summary>
        public string ResourceValue { get; }

        /// <summary>
        /// Gets the prefix value for the folder name
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Gets all of the files named during this instance.
        /// </summary>
        public IEnumerable<string> History => history;

        protected string NextFilePath(string name)
        {
            int hash = Names.GetOrAdd(name, name =>
            {
                return Names.Count < 64 ?
                    OutputId :
                    throw new InvalidOperationException("Only 64 different names are currently supported, names can be reused and uniqueness is guaranteed.");
            });

            int sequenceId = Interlocked.Increment(ref outputCounter[hash]);

            var fileName = $"{name.Split('.')[0]}-{sequenceId}.{name.Split('.')[1]}";
            var filePath = Path.Combine(FolderPath, fileName);

            history.Add(filePath);
            return filePath;
        }

        public string WriteJson<TData>(string fileName, IEnumerable<TData> bounds, Func<TData, object>? transformWith = default)
            => WriteTo(fileName, file =>
            {
                var jsonString = JsonSerializer.Serialize(bounds.Select(transformWith ?? (x => x!)), jsonOptions);
                File.WriteAllText(file, jsonString);
            });

        public string WriteTo(string fileName, Action<string> writeTo)
        {
            var file = NextFilePath(fileName);
            writeTo(file);
            return file;
        }

        public async Task<string> WriteTo(string fileName, Func<string, Task<string>> writeTo)
        {
            var file = NextFilePath(fileName);
            await writeTo(file);
            return file;
        }

        public string CompressFolder(Func<string, bool>? filter = default)
        {
            var zipFilePath = Path.Combine(FolderPath, ResourceName + ".zip");
            if (File.Exists(zipFilePath))
            {
                zipFilePath = Path.Combine(FolderPath, ResourceName + "-" + OutputId + ".zip");
            }

            using ZipArchive zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);

            foreach (string file in History.Where(filter ?? (x => true)))
                zip.CreateEntryFromFile(file, Path.GetFileName(file));
            
            return zipFilePath;
        }

        private static string ConvertToBase(long num, int nbase = 0)
        {
            const string chars = "0123456789ABCDEFGHJKMNOPQRSTUVWXYZabcedfghjkmnpqrstuwxyz";

            if (nbase < 2 || nbase > chars.Length)
                nbase = chars.Length;

            int rem;
            string newNumber = "";

            // in rem we have the offset of the char that was converted to the new base
            while (num >= nbase)
            {
                rem = (int)(num % nbase);
                newNumber = chars[rem] + newNumber;
                num /= nbase;
            }
            // the last number to convert
            newNumber = chars[(int)num] + newNumber;

            return newNumber;
        }
    }
}
