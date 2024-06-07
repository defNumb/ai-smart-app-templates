using DBTransferProject.Services;
using System.Text.RegularExpressions;

public class FilterAgent
{
    private readonly string[] _excludedEmailAddresses = { "customerservice@discountschoolsupply.com" }; // Add your excluded email addresses here

    public List<ApiMessage> FilterAndCleanMessages(List<ApiMessage> messages)
    {
        // Filter out messages sent by excluded email addresses
        messages = messages.Where(m => !IsMessageFromExcludedAddress(m)).ToList();

        // Clean the body of each message
        foreach (var message in messages)
        {
            message.Attributes.Preview = CleanMessageBody(message.Attributes.Preview);
        }

        return messages;
    }

    public bool IsMessageFromExcludedAddress(ApiMessage message)
    {
        var senderEmail = message.Attributes.Meta?.From;
        return _excludedEmailAddresses.Any(address => senderEmail?.Equals(address, StringComparison.OrdinalIgnoreCase) == true);
    }

    private string CleanMessageBody(string body)
    {
        // Remove past ccd bodies
        body = RemovePastCcdBodies(body);

        // Remove disclaimers, personal quotes, and random HTML text
        body = RemoveDisclaimers(body);
        body = RemovePersonalQuotes(body);
        body = RemoveMissionStatements(body);
        body = RemoveHtmlTags(body);

        // Remove junk text like "[ Maricopa.Gov<https://...> ] Facebook<https://...> | Instagram<https://...> | ..."
        body = RemoveJunkText(body);

        // Trim leading and trailing whitespace
        body = body.Trim();

        return body;
    }

    private string RemovePastCcdBodies(string body)
    {
        // Implement logic to remove past ccd bodies from the message body
        // You can use regular expressions or other string manipulation techniques
        // Example: body = Regex.Replace(body, @"-----Original Message-----.*", "", RegexOptions.Singleline);

        return body;
    }

    private string RemoveDisclaimers(string body)
    {
        var disclaimerPatterns = new[]
        {
            @"Note: This e-mail may contain privileged and confidential information.*",
            @"This email message and any files transmitted are sent with confidentiality in mind.*",
            @"CONFIDENTIAL NOTICE AND DISCLAIMER.*",
            @"Disclaimer.*",
            @"CONFIDENTIALITY NOTICE and DISCLAIMER:.*",
            @"DISCLAIMER\s*-.*",
            @"The content of this message \(email and attachments\) is confidential.*",
            @"This email and any attachments are intended solely for the use of the individual.*",
            @"If you have received this communication in error, please notify us immediately.*",
            @"We do not guarantee that this material is free from viruses.*",
            @"This e-mail message together with any attachment or reply should not be considered private.*",
            @"The information contained in this communication from the sender is confidential.*",
            @"This email has been scanned for viruses and malware.*",
            @"You should have no expectation that the content of emails sent to or from school district email addresses will remain private.*",
            @"The document\(s\) accompanying this electronic transmission contain confidential information belonging to the sender which is legally privileged.*",
            @"The content of this email is confidential and intended for the recipient specified in message only.*",
            @"Hallsville ISD puts the security of the client at a high priority.*",
            @"Hallsville Independent School District.*",
            @"This email and any attachments to it may be confidential and are intended solely for the use of the individual to whom it is addressed.*",
            @"Please note Florida has a very broad public records law.*"
        };

        foreach (var pattern in disclaimerPatterns)
        {
            body = Regex.Replace(body, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        return body;
    }


    private string RemovePersonalQuotes(string body)
    {
        var personalQuotePatterns = new[]
        {
            @"(?s)Personal Quote:.*",
            @"(?s)--.*" // Common separator for quotes
        };

        foreach (var pattern in personalQuotePatterns)
        {
            body = Regex.Replace(body, pattern, "", RegexOptions.IgnoreCase);
        }

        return body;
    }

    private string RemoveMissionStatements(string body)
    {
        var missionStatementPatterns = new[]
        {
            @"Mission:.*", // Matches mission statements
            @"Vision:.*", // Matches vision statements
            @"Continue a tradition of excellence by providing a world-class education.*", // Example of a specific vision statement
            @"Join our team today! Positions are available in.*", // Matches job postings
            @"We value your opinion, take our survey.*" // Matches survey requests
        };

        foreach (var pattern in missionStatementPatterns)
        {
            body = Regex.Replace(body, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        return body;
    }

    private string RemoveHtmlTags(string body)
    {
        // Regular expression to remove HTML tags while preserving content inside tags that follow the pattern of email addresses
        return Regex.Replace(body, @"<[^@\s>]+(?: [^@\s>]+)*>", "");
    }


    private string RemoveJunkText(string body)
    {
        var junkPatterns = new[]
       {
            @"\[.*?\]",
            @"Get Outlook for iOS<https://.*?>",
            @"<https://.*?>",
            @"(Facebook|Instagram|Twitter|YouTube|LinkedIn)<https://.*?>",
            @"MSU Extension Head Start & Early Head Start<https://.*?>",
            @"Nurturing Homes Initiative<https://.*?>",
            @"MS Child Care Resource & Referral<https://.*?>",
            @"To stay in touch with our team- click HERE<https://.*?>",
            @"TAKING CARE OF WHAT MATTERS",
            @"Learn more about Extension’s work:.*",
            @"cid:image\d+\.png@.*?<https://.*?>",
            @"Join our team today! Positions are available in.*", // Matches job postings
            @"We value your opinion, take our survey.*", // Matches survey requests
            @"https:\/\/urldefense.com\/v3\/__https:\/\/.*?__",
            @"https:\/\/urldefense.com\/v3\/__http:\/\/.*?__",
            @"https:\/\/urldefense.com\/v3\/.*?", // Matches URLs with urldefense.com format
            @"https:\/\/.*?\/__http:\/\/.*?__",
            @"https:\/\/.*?\/__https:\/\/.*?__",
            @"https:\/\/.*?", // Matches any other https URLs
            @"Unsubscribe.*"
        };

        foreach (var pattern in junkPatterns)
        {
            body = Regex.Replace(body, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        return body;
    }

}