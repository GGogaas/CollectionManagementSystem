using CollectionManagementSystem.Models;
using System.Globalization;

namespace CollectionManagementSystem.Services;

public class FileStorageService
{
    private readonly string _dataDirectory;
    private readonly string _imagesDirectory;

    public FileStorageService()
    {
        _dataDirectory = Path.Combine(FileSystem.AppDataDirectory, "Collections");
        _imagesDirectory = Path.Combine(FileSystem.AppDataDirectory, "Images");
        Directory.CreateDirectory(_dataDirectory);
        Directory.CreateDirectory(_imagesDirectory);
        System.Diagnostics.Debug.WriteLine($"[CollectionManagementSystem] Data path: {_dataDirectory}");
    }

    public string DataDirectory => _dataDirectory;
    public string ImagesDirectory => _imagesDirectory;

    public List<Collection> LoadAllCollections()
    {
        var collections = new List<Collection>();
        foreach (var file in Directory.GetFiles(_dataDirectory, "*.txt"))
        {
            try
            {
                var collection = ParseCollectionFile(file);
                if (collection != null)
                    collections.Add(collection);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CollectionManagementSystem] Error loading {file}: {ex.Message}");
            }
        }
        return collections;
    }

    public void SaveCollection(Collection collection)
    {
        var filePath = Path.Combine(_dataDirectory, $"{collection.Id}.txt");
        WriteCollectionToFile(collection, filePath);
    }

    public void DeleteCollection(Guid collectionId)
    {
        var filePath = Path.Combine(_dataDirectory, $"{collectionId}.txt");
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public void ExportCollection(Collection collection, string exportPath)
        => WriteCollectionToFile(collection, exportPath);

    public Collection? ImportCollection(string importPath)
        => ParseCollectionFile(importPath);

    public string? CopyImageToStorage(string sourcePath)
    {
        try
        {
            var ext = Path.GetExtension(sourcePath);
            var fileName = $"{Guid.NewGuid()}{ext}";
            File.Copy(sourcePath, Path.Combine(_imagesDirectory, fileName), overwrite: true);
            return fileName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CollectionManagementSystem] Image copy error: {ex.Message}");
            return null;
        }
    }

    public string GetFullImagePath(string imageFileName)
        => Path.Combine(_imagesDirectory, imageFileName);

    private void WriteCollectionToFile(Collection collection, string filePath)
    {
        var lines = new List<string>
        {
            "COLLECTION_START",
            $"Id={collection.Id}",
            $"Name={Escape(collection.Name)}",
            $"Type={Escape(collection.CollectionType)}",
            $"Description={Escape(collection.Description)}",
            "CUSTOM_FIELDS_START"
        };

        foreach (var field in collection.CustomFields)
        {
            lines.Add("FIELD_START");
            lines.Add($"Name={Escape(field.Name)}");
            lines.Add($"Type={field.FieldType}");
            lines.Add($"Options={string.Join("~", field.DropdownOptions.Select(Escape))}");
            lines.Add("FIELD_END");
        }

        lines.Add("CUSTOM_FIELDS_END");
        lines.Add("ITEMS_START");

        foreach (var item in collection.Items)
        {
            lines.Add("ITEM_START");
            lines.Add($"Id={item.Id}");
            lines.Add($"Name={Escape(item.Name)}");
            lines.Add($"Price={item.Price.ToString(CultureInfo.InvariantCulture)}");
            lines.Add($"Status={item.Status}");
            lines.Add($"Rating={item.Rating}");
            lines.Add($"Comment={Escape(item.Comment)}");
            lines.Add($"ImagePath={Escape(item.ImagePath)}");
            lines.Add("CUSTOM_VALUES_START");
            foreach (var kv in item.CustomFieldValues)
                lines.Add($"{Escape(kv.Key)}={Escape(kv.Value)}");
            lines.Add("CUSTOM_VALUES_END");
            lines.Add("ITEM_END");
        }

        lines.Add("ITEMS_END");
        lines.Add("COLLECTION_END");

        File.WriteAllLines(filePath, lines);
    }

    private Collection? ParseCollectionFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var lines = File.ReadAllLines(filePath);
        if (lines.Length == 0 || lines[0] != "COLLECTION_START") return null;

        var collection = new Collection();
        int i = 1;

        while (i < lines.Length && lines[i] != "COLLECTION_END")
        {
            var line = lines[i];

            if (TryGetValue(line, "Id=", out var idVal))
            { if (Guid.TryParse(idVal, out var g)) collection.Id = g; }
            else if (TryGetValue(line, "Name=", out var nameVal))
                collection.Name = Unescape(nameVal);
            else if (TryGetValue(line, "Type=", out var typeVal))
                collection.CollectionType = Unescape(typeVal);
            else if (TryGetValue(line, "Description=", out var descVal))
                collection.Description = Unescape(descVal);
            else if (line == "CUSTOM_FIELDS_START")
            {
                i++;
                while (i < lines.Length && lines[i] != "CUSTOM_FIELDS_END")
                {
                    if (lines[i] == "FIELD_START")
                    {
                        i++;
                        var field = new CustomField();
                        while (i < lines.Length && lines[i] != "FIELD_END")
                        {
                            if (TryGetValue(lines[i], "Name=", out var fn)) field.Name = Unescape(fn);
                            else if (TryGetValue(lines[i], "Type=", out var ft))
                            { if (Enum.TryParse<CustomFieldType>(ft, out var cft)) field.FieldType = cft; }
                            else if (TryGetValue(lines[i], "Options=", out var fo) && !string.IsNullOrEmpty(fo))
                                field.DropdownOptions = fo.Split('~').Select(Unescape).Where(o => !string.IsNullOrEmpty(o)).ToList();
                            i++;
                        }
                        if (!string.IsNullOrEmpty(field.Name))
                            collection.CustomFields.Add(field);
                    }
                    i++;
                }
            }
            else if (line == "ITEMS_START")
            {
                i++;
                while (i < lines.Length && lines[i] != "ITEMS_END")
                {
                    if (lines[i] == "ITEM_START")
                    {
                        i++;
                        var item = new CollectionItem();
                        while (i < lines.Length && lines[i] != "ITEM_END")
                        {
                            if (TryGetValue(lines[i], "Id=", out var itemId))
                            { if (Guid.TryParse(itemId, out var g)) item.Id = g; }
                            else if (TryGetValue(lines[i], "Name=", out var n)) item.Name = Unescape(n);
                            else if (TryGetValue(lines[i], "Price=", out var p))
                            { if (decimal.TryParse(p, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec)) item.Price = dec; }
                            else if (TryGetValue(lines[i], "Status=", out var s))
                            { if (Enum.TryParse<ItemStatus>(s, out var st)) item.Status = st; }
                            else if (TryGetValue(lines[i], "Rating=", out var r))
                            { if (int.TryParse(r, out var rat)) item.Rating = rat; }
                            else if (TryGetValue(lines[i], "Comment=", out var c)) item.Comment = Unescape(c);
                            else if (TryGetValue(lines[i], "ImagePath=", out var ip)) item.ImagePath = Unescape(ip);
                            else if (lines[i] == "CUSTOM_VALUES_START")
                            {
                                i++;
                                while (i < lines.Length && lines[i] != "CUSTOM_VALUES_END")
                                {
                                    var eqIdx = lines[i].IndexOf('=');
                                    if (eqIdx > 0)
                                        item.CustomFieldValues[Unescape(lines[i][..eqIdx])] = Unescape(lines[i][(eqIdx + 1)..]);
                                    i++;
                                }
                            }
                            i++;
                        }
                        if (!string.IsNullOrEmpty(item.Name))
                            collection.Items.Add(item);
                    }
                    i++;
                }
            }
            i++;
        }

        return collection;
    }

    private static bool TryGetValue(string line, string prefix, out string value)
    {
        if (line.StartsWith(prefix)) { value = line[prefix.Length..]; return true; }
        value = string.Empty;
        return false;
    }

    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r").Replace("~", "\\~");

    private static string Unescape(string value)
        => value.Replace("\\~", "~").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\\\", "\\");
}
