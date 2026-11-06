// КОММЕНТАРИИ - МОИ, ОТНОСЯТСЯ К ЗАДАНИЮ "Подготовить краткое описание найденных ошибок и выполненных изменений"

namespace lab07;

public struct Branch
{
    public string name;
    public List<Branch> branches;
    public Branch(string name, List<Branch> branches)
    {
        this.name = name;
        this.branches = branches;
    }
}

public class TreeBuilder
{

    public static void Main()
    {
        if (!File.Exists("tree.txt")) return; // Добавил проверку на наличие файла

        var tree = parseTree(File.ReadAllLines("tree.txt"), 0);
        PrintTrees(tree);
    }

    // Сделал отдельно парс дерева
    public static List<Branch> parseTree(string[] lines, int lineIndex)
    {
        List<Branch> result = new List<Branch>();
        int beginsCount = 0;
        for (int i = lineIndex; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("BEGIN "))
            {
                if (beginsCount == 0)
                {
                    Branch branch = new Branch(line.Substring(6), parseTree(lines, i + 1));
                    result.Add(branch);
                    beginsCount++;
                }
                else beginsCount++;
            }
            else if (line == "END")
            {
                beginsCount--;
                if (beginsCount < 0) break;
                continue;
            }
            else if (line == "")
            {
                continue;
            }
            else
            {
                if (beginsCount == 0)
                {
                    Branch branch = new Branch(line, parseTree(lines, i + 1));
                    result.Add(branch);
                    beginsCount++;
                }
                else beginsCount++;
            }
        }
        return result;
    }

    // И отдельно отрисовку дерева
    public static void PrintTrees(List<Branch> tree)
    {
        foreach (var root in tree)
        {
            Console.WriteLine(root.name + $" ({root.branches.Count})");
            PrintBranches(root.branches, "");
        }
        Console.ReadLine();
    }

    // c ветвлениями
    public static void PrintBranches(List<Branch> tree, string prefix)
    {
        for (int i = 0; i < tree.Count; i++)
        {
            bool bIsLast = i == tree.Count - 1;
            Console.WriteLine(prefix + (bIsLast ? "└─" : "├─") + tree[i].name + $" ({tree[i].branches.Count})");
            if (tree[i].branches.Count > 0)
            {
                string childPrefix = prefix + (bIsLast ? "  " : "│ ");
                PrintBranches(tree[i].branches, childPrefix);
            }
        }  
    }

    // Снёс нахрен GetIndent и глобальные переменные за ненадобностью
    // Код прошлый имел много ошибок, и даже после починки рекурсии и неправильного говна в пределах данного файла
    // программа имела неправильную отрисовку дерева в разных ситуациях, да и рендер прямо при парсе идея такая себе.
    // По факту, поведение не поменял - она всё ещё строит дерево, просто теперь всё делается как надо
}