using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Gustav.Domain
{
    /// <summary>Lucene.net</summary>
    public static class TextReviewer
    {
        private const Version _ver = Version.LUCENE_30;

        public static string AlphaNumericClean(string content)
        {
            var regex = new Regex("[^a-zA-Z0-9 -]");
            return regex.Replace(content, "");
        }

        public static int Find(string content, string pattern)
        {
            Analyzer analyzer = new SimpleAnalyzer();
            var directory = CreateDirectory(analyzer, content);
            var searcher = new IndexSearcher(directory);
            var parser = new QueryParser(_ver, content, analyzer);
            var query = parser.Parse(pattern);
            var booleanQuery = new BooleanQuery { { query, Occur.MUST } };

            var hits = searcher.Search(booleanQuery, 1);
            return hits.TotalHits;
        }

        public static int FindPatternInContent(string content, string pattern)
        {
            TopDocs result;

            using (var analyzer = new StandardAnalyzer(Version.LUCENE_29))
            using (var directory = CreateRamDirectory(analyzer, content))
            {
                var parser = new QueryParser(Version.LUCENE_29, "", analyzer);
                using (var indexSearcher = new IndexSearcher(directory, true))
                {
                    var query = parser.Parse(pattern);
                    result = indexSearcher.Search(query, 20);
                }
            }

            return result.TotalHits;
        }

        public static int Parse(string content, string pattern)
        {
            var properlyFormattedSearchString = $"content:{pattern}";
            return ParseRaw(content, properlyFormattedSearchString);
        }

        public static List<string> ParseFieldData(string[] fields, string content)
        {
            var fieldData = new List<string>();

            const string local = @"C:\\test_lucene";

            var dir = FSDirectory.Open(new DirectoryInfo(local)); // (1)
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);           // (2)
            var writer = new IndexWriter(dir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);

            var contentDoc = new Document();

            contentDoc.Add(new Lucene.Net.Documents.Field("content", content, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));

            writer.AddDocument(contentDoc);

            writer.Optimize();
            writer.Commit();
            writer.Dispose();

            var directory = FSDirectory.Open(new DirectoryInfo(local));
            analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var query = new MatchAllDocsQuery();
            var searcher = new IndexSearcher(directory, true);
            TopDocs topDocs = searcher.Search(query, 10);
            int results = topDocs.ScoreDocs.Length;

            for (int i = 0; i < results; i++)
            {
                ScoreDoc scoreDoc = topDocs.ScoreDocs[i];
                float score = scoreDoc.Score;
                int docId = scoreDoc.Doc;
                Document doc = searcher.Doc(docId);
                var docFields = doc.GetFields();
                var fieldCount = docFields.Count;
                foreach (var docField in docFields)
                {
                    fieldData.Add("<LOGENTRY> " + docField.Name + " : " + docField.StringValue + " </LOGENTRY>");
                }

                foreach (var field in fields)
                {
                    fieldData.Add(string.Concat(field, " -> ", doc.Get(field)));
                }

                Console.WriteLine("{0}. score {1}", i + 1, score);
                Console.WriteLine("ID: {0}", doc.Get("id"));
                Console.WriteLine("Text found: {0}\r\n", doc.Get("Name"));
            }

            return fieldData;
        }

        public static List<string> ParseFieldData(string content, string[] fields)
        {
            const string local = @"C:\\test_lucene";
            var results = new List<string>();
            WriteContent(content, local);
            foreach (var field in fields)
            {
                var values = GetAvailableFieldValues(local, field);
                foreach (var value in values)
                {
                    results.Add($"Field Name: {field} Field Value: {value}");
                }
            }

            return results;
        }

        public static string ParseValue(string content, string pattern)
        {
            var properlyFormattedSearchString = $"content:{pattern}";
            return ParseRawValue(content, properlyFormattedSearchString);
        }

        public static List<string> ParseValues(string content, string[] fields, string pattern)
        {
            var properlyFormattedSearchString = $"content:{pattern}";
            return ParseRawValues(content, fields, properlyFormattedSearchString);
        }

        public static Dictionary<string, string> ParseValues(string content, string[] fields)
        {
            var results = new Dictionary<string, string>();
            var valuesOnly = content;
            foreach (var field in fields)
            {
                valuesOnly = valuesOnly.Replace(field, "!");
            }

            var splitter = '!';
            var values = valuesOnly.Split(splitter);
            var cleanedValues = new List<string>();
            foreach (var value in values)
            {
                var trimmed = value.Trim();
                if (trimmed != string.Empty)
                {
                    var cleaned = trimmed.Replace("_", "");

                    cleanedValues.Add(trimmed);
                }
            }

            values = cleanedValues.ToArray();

            var index = 0;
            foreach (var field in fields)
            {
                if (index == values.Length) break;
                results.Add(field, AlphaNumericClean(values[index]));
                index++;
            }

            return results;
        }

        public static string[] RamDirectory(string content)
        {
            List<string> results;
            using (var analyzer = new StandardAnalyzer(Version.LUCENE_29))
            using (var directory = CreateRamDirectory(analyzer, content))
            {
                results = directory.ListAll().ToList();
            }

            return results.ToArray();
        }

        public static List<string> SearchForMovie(string content, string input)
        {
            var movieTitles = new List<string>();
            using (var analyzer = new StandardAnalyzer(Version.LUCENE_30))
            using (var directory = CreateRamDirectory(analyzer, content))
            {
                using (var reader = IndexReader.Open(directory, true))
                {
                    using (var searcher = new IndexSearcher(reader))
                    {
                        var fields = new[]
                        {
                            "Director",
                            "Title",
                            "Color",
                            "Country",
                            "Genre"
                        };
                        var parser = new MultiFieldQueryParser(Version.LUCENE_30,
                                                               fields,
                                                               analyzer)

                        {
                            AllowLeadingWildcard = true
                        };

                        var query = parser.Parse(string.Format("*{0}*", input));
                        var hits = searcher.Search(query, 20).ScoreDocs;

                        foreach (var doc in hits)
                        {
                            var document = searcher.Doc(doc.Doc);

                            var title = document.Get("Title");
                            var position = document.Get("Position");
                            var year = document.Get("Year");

                            var movie = string.Format("{0} - {1} ({2})", position, title, year);

                            if (!movieTitles.Contains(movie))
                            {
                                movieTitles.Add(movie);
                            }
                        }
                    }
                }
            }
            return movieTitles;
        }

        private static Document Create(string content)
        {
            var field = new Lucene.Net.Documents.Field("Content", content, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.NOT_ANALYZED);
            var doc = new Document();
            doc.Add(field);

            return doc;
        }

        private static Directory CreateDirectory(Analyzer analyzer, string content)
        {
            Directory directory = new RAMDirectory();

            var writer = new IndexWriter
                (directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            writer.AddDocument(Create(content));
            writer.Commit();

            return directory;
        }

        private static Directory CreateRamDirectory(Analyzer analyzer, string content)
        {
            var directory = new RAMDirectory();

            using (var indexWriter = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var luceneDocument = new Document();

                luceneDocument.Add(new Lucene.Net.Documents.Field("content", content, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));
                indexWriter.AddDocument(luceneDocument);

                indexWriter.Optimize();
                indexWriter.Commit();
            }

            return directory;
        }

        private static List<string> GetAvailableFieldValues(string dir, string fieldName)
        {
            List<string> fieldValues;

            using (Directory directory = FSDirectory.Open(dir))
            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            using (var reader = writer.GetReader())
            {
                fieldValues = ValuesFromField(reader, fieldName).ToList();
            }

            return fieldValues;
        }

        private static int ParseRaw(string content, string pattern)
        {
            const string local = @"C:\\test_lucene";

            var dir = FSDirectory.Open(new DirectoryInfo(local)); // (1)
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);           // (2)
            var writer = new IndexWriter(dir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);

            var contentDoc = new Document();
            contentDoc.Add(new Lucene.Net.Documents.Field("content", content, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));

            writer.AddDocument(contentDoc);

            writer.Optimize();
            writer.Commit();
            writer.Dispose();

            var directory = FSDirectory.Open(new DirectoryInfo(local));
            analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var parser = new MultiFieldQueryParser(Version.LUCENE_30, new[] { "Name", "Description" }, analyzer); // (1)
            Query query = parser.Parse(pattern);                           // (2)
            var searcher = new IndexSearcher(directory, true);          // (3)
            TopDocs topDocs = searcher.Search(query, 10);               // (4)

            int results = topDocs.ScoreDocs.Length;
            Console.WriteLine("Found {0} results", results);

            for (int i = 0; i < results; i++)
            {
                ScoreDoc scoreDoc = topDocs.ScoreDocs[i];
                float score = scoreDoc.Score;
                int docId = scoreDoc.Doc;
                Document doc = searcher.Doc(docId);

                Console.WriteLine("{0}. score {1}", i + 1, score);
                Console.WriteLine("ID: {0}", doc.Get("id"));
                Console.WriteLine("Text found: {0}\r\n", doc.Get("Name"));
            }

            return results;
        }

        private static string ParseRawValue(string content, string pattern)
        {
            const string local = @"C:\\test_lucene";

            var dir = FSDirectory.Open(new DirectoryInfo(local)); // (1)
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);           // (2)
            var writer = new IndexWriter(dir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);

            var contentDoc = new Document();
            contentDoc.Add(new Lucene.Net.Documents.Field("content", content, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));

            writer.AddDocument(contentDoc);

            writer.Optimize();
            writer.Commit();
            writer.Dispose();

            var directory = FSDirectory.Open(new DirectoryInfo(local));
            analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var parser = new MultiFieldQueryParser(Version.LUCENE_30, new[] { "content" }, analyzer); // (1)
            var query = parser.Parse(pattern);                           // (2)
            var searcher = new IndexSearcher(directory, true);          // (3)
            var topDocs = searcher.Search(query, 10);               // (4)

            var hits = topDocs.ScoreDocs.Length;
            var results = string.Empty;

            for (var i = 0; i < hits; i++)
            {
                var scoreDoc = topDocs.ScoreDocs[i];
                var docId = scoreDoc.Doc;
                var doc = searcher.Doc(docId);
                //doc.GetValues()
                results = doc.Get("content");
            }

            return results;
        }

        private static List<string> ParseRawValues(string content, string[] fields, string pattern)
        {
            const string local = @"C:\\test_lucene";

            var dir = FSDirectory.Open(new DirectoryInfo(local)); // (1)
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);           // (2)
            var writer = new IndexWriter(dir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);

            var contentDoc = new Document();
            contentDoc.Add(new Lucene.Net.Documents.Field("content", content, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));

            writer.AddDocument(contentDoc);

            writer.Optimize();
            writer.Commit();
            writer.Dispose();

            var directory = FSDirectory.Open(new DirectoryInfo(local));
            analyzer = new StandardAnalyzer(Version.LUCENE_30);

            var parser = new MultiFieldQueryParser(Version.LUCENE_30, fields, analyzer); // (1)
            Query query = parser.Parse(pattern);                           // (2)
            var searcher = new IndexSearcher(directory, true);          // (3)
            TopDocs topDocs = searcher.Search(query, 10);               // (4)

            int hits = topDocs.ScoreDocs.Length;
            var results = new List<string>();
            results.Add($"Found {hits} results");

            for (int i = 0; i < hits; i++)
            {
                var scoreDoc = topDocs.ScoreDocs[i];
                var score = scoreDoc.Score;
                var docId = scoreDoc.Doc;
                var doc = searcher.Doc(docId);

                foreach (var field in fields)
                {
                    results.Add($"ID: {doc.Get("id")}");
                    results.Add($"Text found: {doc.Get(field)}");
                }
            }

            return results;
        }

        private static IEnumerable<string> ValuesFromField(IndexReader reader, string field)
        {
            var termEnum = reader.Terms(new Term(field));

            do
            {
                var currentTerm = termEnum.Term;

                if (currentTerm.Field != field)
                    yield break;

                yield return currentTerm.Text;
            } while (termEnum.Next());
        }

        private static void WriteContent(string content, string dirPath)
        {
            using (var dir = FSDirectory.Open(new DirectoryInfo(dirPath)))
            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(dir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                var contentDoc = new Document();
                contentDoc.Add(new Lucene.Net.Documents.Field("content", content, Lucene.Net.Documents.Field.Store.YES, Lucene.Net.Documents.Field.Index.ANALYZED));

                writer.AddDocument(contentDoc);

                writer.Optimize();
                writer.Commit();
                writer.Dispose();
            }
        }
    }

    public class SampleData
    {
        public string Description { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format($"Id {Id} - Name {Name} - Description {Description}");
        }
    }
}