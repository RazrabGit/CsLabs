// НАПИСАНО БЕЗ НЕЙРОСЕТЕЙ
using lab07;
using System.IO;

namespace ProgramTests
{
    public class Tests
    {
        private string _testDirectory;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "SaveManagerTests-" + Guid.NewGuid());

            Directory.CreateDirectory(_testDirectory);

            File.WriteAllText(Path.Combine(_testDirectory, "tree.txt"),
           "A\r\nBEGIN B\r\nBEGIN C\r\nEND\r\nBEGIN D\r\nEND\r\nEND\r\nBEGIN E\r\nEND");

            File.WriteAllText(Path.Combine(_testDirectory, "tree1.txt"),
            "A\r\n\r\nBEGIN B\r\nBEGIN C\r\nEND\r\n\r\nBEGIN D\r\nEND\r\nEND\r\n\r\nBEGIN E\r\nEND\r\nEND\r\n\r\nB\r\nBEGIN SUS\r\nEND");

            File.WriteAllText(Path.Combine(_testDirectory, "tree2.txt"),
            "A\r\n\r\nBEGIN B\r\nBEGIN C\r\n");

            File.WriteAllText(Path.Combine(_testDirectory, "tree3.txt"),
            "A\r\nBEGIN B\r\nBEGIN C\r\nBEGIN E\r\nEND\r\nBEGIN F\r\nEND\r\nEND\r\nEND\r\nBEGIN D");
        }

        [Test]
        public void ParseTree_PreservesNesting()
        {
            var tree = TreeBuilder.parseTree(File.ReadAllLines(Path.Combine(_testDirectory, "tree.txt")), 0);
            Assert.That(tree[0].branches[0].branches[0].name, Is.EqualTo("C"));
        } 

        [Test]
        public void ParseTree_IsNotFirstTreesWorks()
        {
            var tree = TreeBuilder.parseTree(File.ReadAllLines(Path.Combine(_testDirectory, "tree1.txt")), 0);
            Assert.That(tree[1].name, Is.EqualTo("B"));
            Assert.That(tree[1].branches[0].name, Is.EqualTo("SUS"));
        }

        [Test]
        public void ParseTree_CheckSingleBranches_WithoutEnd()
        {
            var tree = TreeBuilder.parseTree(File.ReadAllLines(Path.Combine(_testDirectory, "tree2.txt")), 0);
            Assert.That(tree[0].branches[0].name, Is.EqualTo("B"));
            Assert.That(tree[0].branches[0].branches[0].name, Is.EqualTo("C"));
        }

        [Test]
        public void ParseTree_CheckSubSubBranches_WithoutEnd()
        {
            var tree = TreeBuilder.parseTree(File.ReadAllLines(Path.Combine(_testDirectory, "tree3.txt")), 0);
            Assert.That(tree[0].branches[0].branches[0].branches[0].name, Is.EqualTo("E"));
            Assert.That(tree[0].branches[0].branches[0].branches[1].name, Is.EqualTo("F"));
            Assert.That(tree[0].branches[1].name, Is.EqualTo("D"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
    }
}
