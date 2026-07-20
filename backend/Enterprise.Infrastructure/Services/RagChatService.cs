using Enterprise.Application.Common.Interfaces;
using Enterprise.Domain.Entities;
using Enterprise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Enterprise.Infrastructure.Services;

public class RagChatService : IChatService
{
    private readonly ApplicationDbContext _context;

    public RagChatService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> AskAsync(Guid workspaceId, string question)
    {
        var documents = await _context.Documents
            .Where(x => x.WorkspaceId == workspaceId)
            .Include(x => x.Chunks)
            .ToListAsync();

        if (documents.Count == 0)
        {
            return "No documents have been uploaded to this workspace yet.";
        }

        var allChunks = documents.SelectMany(x => x.Chunks).ToList();

        if (allChunks.Count == 0)
        {
            return "No readable text was extracted from uploaded documents. Please upload a supported PDF, DOCX, or TXT file.";
        }

        var queryTerms = Regex.Split(question ?? string.Empty, @"[^\p{L}\p{Nd}]+")
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Select(term => term.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        var relevantChunks = allChunks
            .Select(chunk => new
            {
                Chunk = chunk,
                Score = queryTerms.Count(term => chunk.Content.Contains(term, StringComparison.OrdinalIgnoreCase))
            })
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Chunk.Content.Length)
            .Take(5)
            .Select(result => result.Chunk)
            .ToList();

        if (relevantChunks.Count == 0)
        {
            relevantChunks = allChunks
                .OrderByDescending(x => x.Content.Length)
                .Take(3)
                .ToList();
        }

        var summary = ExtractBestAnswer(relevantChunks, question, queryTerms);

        if (!string.IsNullOrWhiteSpace(summary))
        {
            return summary;
        }

        var fallbackText = string.Join(Environment.NewLine + Environment.NewLine,
            relevantChunks.Select(x => TruncateText(x.Content, 500)));

        return $"Based on the uploaded documents, here is the most relevant information:\n\n{fallbackText}";
    }

    private static string ExtractBestAnswer(List<DocumentChunk> chunks, string question, List<string> queryTerms)
    {
        var topText = string.Join("\n", chunks.Select(x => x.Content));

        if (question.Contains("name", StringComparison.OrdinalIgnoreCase))
        {
            var name = ExtractLikelyName(topText);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return $"Based on the uploaded documents, the person's name appears to be: {name}.";
            }
        }

        if (question.Contains("education", StringComparison.OrdinalIgnoreCase))
        {
            var education = ExtractEducation(topText);
            if (!string.IsNullOrWhiteSpace(education))
            {
                return $"Based on the uploaded documents, the education information is:\n\n{education}";
            }
        }

        var sentences = SplitSentences(topText)
            .Select(sentence => sentence.Trim())
            .Where(sentence => !string.IsNullOrWhiteSpace(sentence))
            .ToList();

        var bestSentences = sentences
            .Select(sentence => new
            {
                Sentence = sentence,
                Score = queryTerms.Count(term => sentence.Contains(term, StringComparison.OrdinalIgnoreCase))
            })
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Sentence.Length)
            .Select(result => result.Sentence)
            .Distinct()
            .Take(5)
            .ToList();

        if (bestSentences.Count > 0)
        {
            return $"Based on the uploaded documents, here is a concise answer:\n\n{string.Join(" ", bestSentences)}";
        }

        return string.Empty;
    }

    private static IEnumerable<string> SplitSentences(string text)
    {
        var sentenceRegex = new Regex(@"(?<=[.!?])\s+", RegexOptions.Multiline);
        return sentenceRegex.Split(text);
    }

    private static string ExtractLikelyName(string text)
    {
        var explicitName = Regex.Match(text, @"(?im)^(?:name|candidate|applicant)[:\-\s]+(?<name>[A-Z][A-Za-z]+(?:\s+[A-Z][A-Za-z]+)+)$");
        if (explicitName.Success)
        {
            return explicitName.Groups["name"].Value.Trim();
        }

        var uppercaseLines = text.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 5 && line.Length <= 80)
            .Where(line => Regex.IsMatch(line, @"^[A-Z\s]+$") && line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 2)
            .ToList();

        if (uppercaseLines.Count > 0)
        {
            return uppercaseLines.First();
        }

        return string.Empty;
    }

    private static string ExtractEducation(string text)
    {
        var educationSection = Regex.Match(text, @"(?is)EDUCATION\b[\s\S]{0,500}(?=\n[A-Z ]{3,}\n|$)");
        if (educationSection.Success)
        {
            var sectionText = educationSection.Value.Trim();
            return TruncateText(sectionText, 400);
        }

        var fallback = Regex.Match(text, @"(?im)(?:Bachelors|Bachelor|Master|Masters|MBA|PhD|High School|Diploma)[^\n]*");
        return fallback.Success ? fallback.Value.Trim() : string.Empty;
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        return text.Substring(0, maxLength).TrimEnd() + "...";
    }
}
