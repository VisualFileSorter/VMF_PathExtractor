using System.Collections.Generic;
using System.IO;
using System.Linq;

class VMF_PathExtractor
{
    class XYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
    class Entity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Next { get; set; }

        public XYZ Coords = new XYZ();

        public Entity? NextEntity;
    }

    // Function to check if one HashSet is a subset of another
    static bool IsSubset<T>(HashSet<T> subset, HashSet<T> superset)
    {
        return subset.All(item => superset.Contains(item));
    }

    // Function to sanitize list of HashSets
    static List<HashSet<T>> SanitizePaths<T>(List<HashSet<T>> paths)
    {
        List<HashSet<T>> sanitizedPaths = new List<HashSet<T>>();

        foreach (var path in paths)
        {
            bool isSubsetOfAny = false;

            foreach (var existingPath in sanitizedPaths)
            {
                if (IsSubset(path, existingPath))
                {
                    isSubsetOfAny = true;
                    break;
                }

                if (IsSubset(existingPath, path))
                {
                    // Remove the existing path if it is a subset of the current path
                    sanitizedPaths.Remove(existingPath);
                    break;
                }
            }

            if (!isSubsetOfAny)
            {
                sanitizedPaths.Add(path);
            }
        }

        return sanitizedPaths;
    }

    static void Main(string[] args)
    {
        if (args.Length == 2)
        {
            string vmf_path = args[0];
            string out_path = args[1];
            if (File.Exists(args[0]))
            {
                string[] vmf_file = File.ReadAllLines(vmf_path);
                List<Entity> entities = new List<Entity>();
                int brace_count = 0;

                Entity temp_entity = new Entity();
                bool found_path = false;
                foreach (string vmf_line in vmf_file)
                {
                    if (vmf_line.Trim() == "entity")
                    {
                        brace_count = 0;
                        found_path = false;
                        temp_entity = new Entity();
                        continue;
                    }

                    // Check for start of braced section
                    if (vmf_line.Trim() == "{")
                    {
                        brace_count++;
                    }

                    // Get Id
                    if (vmf_line.Trim().StartsWith("\"id\""))
                    {
                        string[] split_id = vmf_line.Trim().Split(" ");
                        if (split_id.Length == 2)
                        {
                            temp_entity.Id = int.Parse(split_id[1].Replace("\"", ""));
                        }
                    }

                    if (String.Equals(vmf_line.Trim(), "\"classname\" \"path_track\"",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        found_path = true;
                        continue;
                    }

                    // Note: This does not handle path_tracks that branch
                    if (found_path)
                    {
                        // Get name
                        if (vmf_line.Trim().StartsWith("\"targetname\""))
                        {
                            string[] split_targetname = vmf_line.Trim().Split(" ");
                            if (split_targetname.Length == 2)
                            {
                                temp_entity.Name = split_targetname[1].Replace("\"", "");
                            }
                        }

                        // Get next target
                        if (vmf_line.Trim().StartsWith("\"target\""))
                        {
                            string[] split_target = vmf_line.Trim().Split(" ");
                            if (split_target.Length == 2)
                            {
                                temp_entity.Next = split_target[1].Replace("\"", "");
                            }
                        }

                        // Get coordinates
                        if (vmf_line.Trim().StartsWith("\"origin\""))
                        {
                            string vmf_line_nq = vmf_line.Trim().Replace("\"", "");
                            string[] split_origin = vmf_line_nq.Split(" ");
                            if (split_origin.Length == 4)
                            {
                                temp_entity.Coords.X = double.Parse(split_origin[1]);
                                temp_entity.Coords.Y = double.Parse(split_origin[2]);
                                temp_entity.Coords.Z = double.Parse(split_origin[3]);
                            }
                        }
                    }

                    // Check for end of braced section
                    if (vmf_line.Trim() == "}")
                    {
                        brace_count--;
                    }
                    
                    // End of entity, add to entity list
                    if (brace_count == 0 && found_path)
                    {
                        entities.Add(temp_entity);
                    }
                }

                // Build connections
                foreach (var entity in entities)
                {
                    entity.NextEntity = entities.FirstOrDefault<Entity>(x => x.Name == entity.Next);
                }

                // Sanitize paths and create file contents with path coordinates
                List<HashSet<Entity>> paths = new List<HashSet<Entity>>();
                for (int i = 0; i < entities.Count; i++)
                {
                    Entity current_entity = entities.ElementAt(i);
                    HashSet<Entity> temp_path = new HashSet<Entity>();
                    temp_path.Add(current_entity);
                    while (current_entity.NextEntity != null && !temp_path.Contains(current_entity.NextEntity))
                    {
                        current_entity = current_entity.NextEntity;
                        temp_path.Add(current_entity);
                    }

                    paths.Add(temp_path);
                }
                List<HashSet<Entity>> paths_no_dupe = paths.Distinct(HashSet<Entity>.CreateSetComparer()).ToList();
                List<HashSet<Entity>> sanitized_paths = SanitizePaths(paths_no_dupe);

                // Build output string
                List<string> output_paths = new List<string>();
                foreach (var sanitized_path in sanitized_paths)
                {
                    foreach (Entity path_node in sanitized_path)
                    {

                        output_paths.Add($"{path_node.Name,-8} | {path_node.Coords.X,-8} | {path_node.Coords.Y,-8} | {path_node.Coords.Z,-8}");
                    }
                    
                    output_paths.Add("END PATH");
                    output_paths.Add("");
                    output_paths.Add("");
                }

                // Check output filepath is valid and write to file
                FileInfo file_info = null;
                try
                {
                    file_info = new System.IO.FileInfo(out_path);
                }
                catch (ArgumentException) { }
                catch (System.IO.PathTooLongException) { }
                catch (NotSupportedException) { }
                if (ReferenceEquals(file_info, null))
                {
                    Console.WriteLine("Error: Output filepath invalid!");
                }
                else
                {
                    File.WriteAllLines(out_path, output_paths);
                    Console.WriteLine($"Output file written to: {out_path}");
                }
            }
            else
            {
                Console.WriteLine("Error: VMF file does not exist!");
            }
        }
        else
        {
            Console.WriteLine("Error: Expected 2 arguments (1: filepath to .vmf; 2: filepath to output .txt)");
        }
    }
}