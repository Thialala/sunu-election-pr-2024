
if (args.Length == 0)
{
    Console.WriteLine("Veuillez fournir le chemin du fichier PDF comme argument.");
    return;
}

var pdfFile = args[0];
var pdfFileFolder = Path.GetDirectoryName(pdfFile);
var outputDirectory = Path.Combine(pdfFileFolder ?? string.Empty, "données");

// Crée le répertoire de sortie s'il n'existe pas
Directory.CreateDirectory(outputDirectory);

ExportPdfPagesAsSeparateFiles(pdfFile, outputDirectory);
await ProcessPdfsForOcrAndSaveAsMarkdownAsync(outputDirectory);
await ExtractDataFromMarkdownAndSaveAsCsvAsync(outputDirectory);
ConsolidateCsvFiles(outputDirectory);


// Exporte chaque page d'un fichier PDF en fichiers PDF séparés
void ExportPdfPagesAsSeparateFiles(string pdfFile, string outputDirectory)
{

    for (int i = 1; i <= 14; i++)
    {
        var outputPdfPath = Path.Combine(outputDirectory, $"{i:D4}.pdf");
        if (File.Exists(outputPdfPath))
            continue;

        try
        {
            var fromDocument = new PdfDocument(new PdfReader(pdfFile));
            var toDocument = new PdfDocument(new PdfWriter(outputPdfPath));
            fromDocument.CopyPagesTo(i, i, toDocument, 1);

            toDocument.Close();
            fromDocument.Close();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'exportation de la page {i}: {ex.Message}");
        }
    }
}

// Traite les fichiers PDF pour OCR et les sauvegarde en Markdown
async Task ProcessPdfsForOcrAndSaveAsMarkdownAsync(string outputDirectory)
{
    var endpoint = Environment.GetEnvironmentVariable("AZURE_DOC_INTEL_ENDPOINT");
    var key = Environment.GetEnvironmentVariable("AZURE_DOC_INTEL_KEY");
    var credential = new AzureKeyCredential(key);
    var client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

    var pdfFiles = Directory.GetFiles(outputDirectory, "*.pdf");
    var options = new ParallelOptions { MaxDegreeOfParallelism = 2 * Environment.ProcessorCount };

    await Parallel.ForEachAsync(pdfFiles, options, async (pdfFile, token) =>
    {
        var markdownFilePath = $"{pdfFile}.md";
        if (File.Exists(markdownFilePath))
        {
            Console.WriteLine($"Skipping {pdfFile} because it already has a markdown file");
            return;
        }

        try
        {
            var markdownContent = await GetOcrResultAsMarkdownAsync(pdfFile, client);
            await File.WriteAllTextAsync(markdownFilePath, markdownContent);
            Console.WriteLine($"Created: {markdownFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du traitement OCR de {pdfFile}: {ex.Message}");
        }
    });
}

// Obtient le résultat de l'OCR sous forme de Markdown
async Task<string> GetOcrResultAsMarkdownAsync(string pdfFile, DocumentIntelligenceClient client)
{
    using var fileStream = File.OpenRead(pdfFile);
    var binaryData = BinaryData.FromStream(fileStream);
    var analyzeRequest = new AnalyzeDocumentContent
    {
        Base64Source = binaryData
    };
    var result = await client.AnalyzeDocumentAsync(waitUntil: WaitUntil.Completed, "prebuilt-layout", analyzeRequest: analyzeRequest, outputContentFormat: ContentFormat.Markdown);
    return result.Value.Content.ToString();
}

// Extrait les données des fichiers Markdown et les sauvegarde en CSV
async Task ExtractDataFromMarkdownAndSaveAsCsvAsync(string outputDirectory)
{
    var markdownFiles = Directory.GetFiles(outputDirectory, "*.md");
    var currentRegion = string.Empty;
    var currentDepartment = string.Empty;
    var currentCommune = string.Empty;
    foreach (var markdownFile in markdownFiles)
    {
        var markdownContent = File.ReadAllText(markdownFile);
        var markdownLines = File.ReadAllLines(markdownFile);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var document = Markdown.Parse(markdownContent, pipeline);

        var region = GetKeyValue(markdownLines, "Région");
        var departement = GetKeyValue(markdownLines, "Département");
        var commune = GetKeyValue(markdownLines, "Commune");

        if (!string.IsNullOrEmpty(region))
        {
            currentRegion = region;
            currentDepartment = departement;
            currentCommune = commune;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Region,Departement,Commune,Lieu de vote,Bureau,Electeurs,Implantation");

        foreach (var element in document.Descendants<Table>())
        {
            foreach (var row in element)
            {
                var cells = row.Descendants<TableCell>().Select(cell =>
                {
                    var inline = cell.Descendants<LiteralInline>().FirstOrDefault();
                    return inline?.Content.ToString();
                });

                if (cells.Any(cell => cell == null || cell.Contains("Lieu de vote", StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                var csvLine = $"{currentRegion},{currentDepartment},{currentCommune},{string.Join(",", cells.Where(cell => cell != null))}";
                sb.AppendLine(csvLine);
            }
        }

        await File.WriteAllTextAsync($"{markdownFile}.csv", sb.ToString());
    }
}

string GetKeyValue(string[] markdownLines, string key)
{
    var line = markdownLines.FirstOrDefault(l => l.Contains(key, StringComparison.InvariantCultureIgnoreCase));
    if (line == null)
    {
        return string.Empty;
    }

    return line.Split(":")[1].Trim();
}

// Consolide tous les fichiers CSV en un seul
void ConsolidateCsvFiles(string outputDirectory)
{
    string[] csvFilesFiles = Directory.GetFiles(outputDirectory, "*.csv");
    var filesToCheck = new List<string>();
    var allRecords = new List<BureauVote>();

    foreach (var csvFile in csvFilesFiles)
    {
        using (var reader = new StreamReader(csvFile))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<BureauVoteMap>();
            try
            {
                var records = csv.GetRecords<BureauVote>().ToList();
                if (!records.Any())
                {
                    filesToCheck.Add(csvFile);
                    Console.WriteLine(new FileInfo(csvFile).Name + ": " + "Fichier vide");
                }

                allRecords.AddRange(records);
            }
            catch (Exception ex)
            {
                filesToCheck.Add(csvFile);
                Console.WriteLine($"{(new FileInfo(csvFile)).Name}: Format ou données incorrects");
            }
        }
    }

    Console.WriteLine($"Nombres de fichiers potentiels à problème : {filesToCheck.Count}");
    Console.WriteLine($"Total bureaux de vote : {allRecords.Count}");
    Console.WriteLine($"Total électeurs : {allRecords.Sum(r => r.Electeurs)}");

    var consolidatedCsvOutputDirectory = Directory.GetParent(outputDirectory).FullName;
    var outputFile = Path.Combine(consolidatedCsvOutputDirectory, "CARTE_ELECTORALE_ELECTION_PR_DU_25FEV2024.csv");
    using (var writer = new StreamWriter(outputFile))
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        csv.WriteRecords(allRecords);
    }
}

