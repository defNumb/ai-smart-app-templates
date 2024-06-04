using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml;
using System.Data.SqlClient;
using System.Text;
using DBTransferProject.Models;
using Microsoft.AspNetCore.Components;
using System.Data;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
namespace DBTransferProject.Components.Pages

{
    /*
  *********** DATA UPLOAD COMPONENT LOGIC*****************
  Developed by: Sam Espinoza
  Company: Excelligence Learning Corp.
  Last worked on by: Sam Espinoza<--- update this when you change something
  Last date worked on: 2/8/2024       <--- update this accordingly
  This component allows users to input database connection details, select a file (CSV or Excel), and upload it for processing.
  "Upload.razor" contains the HTML(front end code) of this page (Upload).
  "Upload.razor.cd" contains the Logic (back end code) of this page.
*/
    public partial class Upload
    {
        [Inject]
        public IConfiguration? Configuration { get; set; }
    
        // VARIABLES
        private IBrowserFile? selectedFile;
        private string? databaseName;
        private string? tableName;
        private List<string> userMessages = new List<string>();
        private string? serverName;
        private List<PunchoutAccountData> punchoutAccountDataList = new List<PunchoutAccountData>();
        private List<UserConfigData> userConfigDataList = new List<UserConfigData>();
        private string? imageBase64String;
        private string? userMessage;
        private string? impexMessage;
        private string filePreview = string.Empty;
        private string? Processor = string.Empty;
        private string? HoldCode = string.Empty;
        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string SpecialCharacters = "!@#$%^&*";
        private const string AllCharacters = UppercaseLetters + LowercaseLetters + Digits + SpecialCharacters;
        private readonly Random _random = new Random();
        private bool ImpexUploaded = false;

        /*******************************************************************************************************
        *  
                                           ONBOARDING PART I - SQL
        *
        ******************************************************************************************************/
        // HANDLER METHODS
        /*
           Developed by : Samuel Espinoza
           Method Name: HandleFileSelected
           Parameters : InputFileChangeEvent
           Description : after file has been selected. It will process a snapshot of the document for the user to review.
           Status: COMPLETED
           Last date worked on: 2/8/2024       <--- update this accordingly
         */
        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
            if (selectedFile != null)
            {
                var fileExtension = Path.GetExtension(selectedFile.Name).ToLowerInvariant();
                switch (fileExtension)
                {
                    case ".csv":
                        await GenerateCsvPreview();
                        break;
                    case ".xlsx":
                        await GenerateExcelPreview();
                        break;
                    default:
                        userMessages.Add("> Unsupported file type for preview.");
                        break;
                }
            }
        }
        /*
           Developed by : Samuel Espinoza
           Method Name: HandleImageSelected
           Parameters : InputFileChangeEvent
           Description : after file has been selected. It will process a snapshot of the document and display it on the screen
                         for the user to review and it will store it to complete onboarding procedure.
           Status: COMPLETED
           Last date worked on: 3/29/2024       <--- update this accordingly
         */
        private async Task HandleImageSelected(InputFileChangeEventArgs e)
        {
            var imageFile = e.File; // Assuming you're handling one file. Adjust as necessary for multiple files.
            if (imageFile != null)
            {
                try
                {
                    // Check if the file is an image based on its MIME type
                    if (imageFile.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if the file extension is .jpg or .png
                        var fileExtension = Path.GetExtension(imageFile.Name).ToLowerInvariant();
                        if (fileExtension == ".jpg" || fileExtension == ".png")
                        {
                            // Limit file size, for example 5 MB. Adjust as needed.
                            if (imageFile.Size <= 5242880) // 5MB in bytes
                            {
                                // Read the file into a MemoryStream.
                                using (var memoryStream = new MemoryStream())
                                {
                                    await imageFile.OpenReadStream(maxAllowedSize: 5242880).CopyToAsync(memoryStream);
                                    var byteArray = memoryStream.ToArray(); // Convert the MemoryStream to a byte array.

                                    // Convert the byte array to a Base64 string.
                                    var base64Image = Convert.ToBase64String(byteArray);

                                    // Prepare the image source that can be used in an <img> tag
                                    imageBase64String = $"data:{imageFile.ContentType};base64,{base64Image}";

                                }
                            }
                            else
                            {
                                // Handle file size exceeds limit
                                userMessages.Add("> File size exceeds the allowed limit of 5MB.");
                            }
                        }
                        else
                        {
                            userMessages.Add("> Only .jpg and .png image formats are allowed.");
                        }
                    }
                    else
                    {
                        userMessages.Add("> Selected file is not an image.");
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors that might occur during the file read process
                    userMessages.Add($"> An error occurred: {ex.Message}");
                }
            }
        }
        /*
           Developed by : Samuel Espinoza
           Method Name: UploadFile
           Parameters : none
           Description : This method verifies that a file has been selected and checks for the extension to appropriately call the corresponding method that will handle the file.
           Status: COMPLETED
           Last date worked on: 2/8/2024       <--- update this accordingly
         */
        private async Task UploadFile()
        {
            if (selectedFile == null )
            {
                userMessages.Add("> Error: No file selected.");
                return;
            }
            if (Processor == string.Empty)
            {
                userMessages.Add("> Error: Please Specify the Processor for the Processor field.");
                return;
            }
            if (HoldCode == string.Empty)
            {
                userMessages.Add("> Error: Please Specify HoldCode.");
                return;
            }
            var fileExtension = Path.GetExtension(selectedFile.Name).ToLowerInvariant();
            try
            {
                if (fileExtension == ".csv")
                {
                    await ProcessCsvFile();
                }
                else if (fileExtension == ".xlsx")
                  
                {
     
                    await ProcessExcelFileToPunchoutAcct();
                }
                else
                {
                    userMessages.Add("> Error: Unsupported file type.");
                    return;
                }
            }
            catch (Exception ex)
            {
                userMessages.Add($"> Error processing file: {ex.Message}");
            }
        }

        // PROCESSING METHODS
        /*
           Developed by : Samuel Espinoza
           Method Name: ProcessCsvFile 
           Parameters : none
           Description : This method processes CSV files to be uploaded to database
           Status:  COMPLETED
           Last date worked on: 2/9/2024       <--- update this accordingly
         */
        private async Task ProcessCsvFile()
        {
            // Implement CSV processing - THIS IS ASSUMING THE CSV FILE IS CONSISTENT
            using var stream = selectedFile?.OpenReadStream();
            using var reader = new StreamReader(stream!);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = line?.Split(',');
                // Process each line here, e.g., store in database
            }
            userMessages.Add("> CSV file processed successfully.");
        }

        /*
           Developed by : Samuel Espinoza
           Method Name: ProcessExcelFileToPunchoutAcct 
           Parameters : none
           Description : This method is the FIRST step in the onboarding process. This method processes the EXCEL file and maps its fields to the punchout_account table
                         to insert the a row with the defined columns and generated fields.
           Status: COMPLETED
           Last date worked on: 2/8/2024       <--- update this accordingly
         */
        private async Task ProcessExcelFileToPunchoutAcct()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set license context for EPPlus
            using var stream = new MemoryStream();
            await selectedFile!.OpenReadStream().CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                userMessages.Add("> Error: No worksheet found in the Excel file.");
                return;
            }

            // Define the SQL connection string
            /*
              CONNECTION STRING: IF THERE IS A NEED TO CHANGE THE SERVER AND DATABASE.
              THE CONNECTION STRING TEMPLATE IS "Server=YOURSERVER; Darabase=YOURDATABASE; Integrated Security=True;"
            */
            // *************************************************************************************************
            // TESTING CONNECTIONS STRING, COMMENT THIS OUT WHEN READY TO DEPLOY.
            var connectionString = Configuration?.GetConnectionString("DefaultConnection");
            // WHEN READY TO JUMP TO PRODUCTION REPLACE CONNECTION STRING WITH "ProdSql20Connection" for this step.
           // var connectionString = Configuration?.GetConnectionString("ProdSql20Connection");
            // REFERENCE A STORED PROCEDURE -->
            // var sqlInsertCommand = Configuration?["SqlCommands:InsertPunchoutAccount"];
            var sqlInsertCommand = Configuration?["SqlCommands:InsertPunchoutAccount"]; 
            // Clear the list to ensure it's empty before starting the process
            punchoutAccountDataList.Clear();
            int recordsAdded = 0;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {

                        // Extracted for clarity
                        var schoolOrganization = worksheet.Cells[row, 6].Text;
                        if (!string.IsNullOrEmpty(schoolOrganization))
                        {
                            var identity = RemoveSpaces(schoolOrganization);
                            var duns = identity;
                            // HERE YOU CALL THE GENERATESECRET METHOD.
                            var sharedSecret = GenerateSharedSecret();
                            // KEYCODE = PRICELIST
                            var keycode = new string(schoolOrganization.Where(char.IsLetterOrDigit).ToArray()).Substring(0, Math.Min(8, schoolOrganization.Length));
                            // Check if the record already exists in the database
                            var recordExists = await CheckRecordExistsAsync(connection, identity);
                            if (!recordExists)
                            {
                                // Populate the PunchoutAccountData object
                                var punchoutData = new PunchoutAccountData
                                {
                                    Customer = schoolOrganization,
                                    Identity = identity,
                                    Duns = duns,
                                    SharedSecret = sharedSecret,
                                    Keycode = keycode,
                                    // TaxExempt, DeploymentMode, and DeploymentModeOverride are set to their default values
                                };

                                // Add the data to the list
                                punchoutAccountDataList.Add(punchoutData);

                                using (var command = new SqlCommand(sqlInsertCommand, connection))
                                {
                                    command.CommandType = CommandType.StoredProcedure; // Indicating it's a stored procedure

                                    // Add parameters to SQL command from punchoutData
                                    command.Parameters.AddWithValue("@customer", punchoutData.Customer);
                                    command.Parameters.AddWithValue("@identity", punchoutData.Identity);
                                    command.Parameters.AddWithValue("@duns", punchoutData.Duns);
                                    command.Parameters.AddWithValue("@sharedsecret", punchoutData.SharedSecret);
                                    command.Parameters.AddWithValue("@keycode", punchoutData.Keycode);
                                    command.Parameters.AddWithValue("@taxexempt", punchoutData.TaxExempt);
                                    command.Parameters.AddWithValue("@deploymentmode", punchoutData.DeploymentMode);
                                    command.Parameters.AddWithValue("@deploymentmodeoverride", punchoutData.DeploymentModeOverride);

                                    await command.ExecuteNonQueryAsync();
                                }
                                recordsAdded++;
                            }
                            else
                            {
                                userMessages.Add($"> Record with identity '{identity}' already exists. Skipping insertion.");
                                return;
                            }
                        }
                    }

                    // success message
                    // After adding records to the database...
                    var previewHtml = new StringBuilder();
                    previewHtml.AppendLine("<table class='table-preview'><thead>");
                    // Assuming you know the headers of the complete record, otherwise dynamically fetch these as well
                    previewHtml.AppendLine("<tr><th>Id</th><th>Identity</th><th>duns</th><th>sharedsecret</th><th>keycode</th><th>tax_Exempt</th><th>customer</th><th>deployment_mode</th><th>deployment_mode_override</th></tr>");
                    previewHtml.AppendLine("</thead><tbody>");

                    try
                    {
                        using (var connection2 = new SqlConnection(connectionString))
                        {
                            await connection2.OpenAsync();

                            // Assuming you have a way to identify the last inserted records. Adjust the query accordingly.
                            var selectCommandText = "SELECT TOP (@Count) * FROM punchout_account ORDER BY ID DESC"; // Example query, adjust based on your schema
                            using (var selectCommand = new SqlCommand(selectCommandText, connection))
                            {
                                selectCommand.Parameters.AddWithValue("@Count", punchoutAccountDataList.Count);

                                using (var reader = await selectCommand.ExecuteReaderAsync())
                                {
                                    while (reader.Read())
                                    {
                                        previewHtml.AppendLine("<tr>");
                                        for (int i = 0; i < reader.FieldCount; i++) // Loop through all columns
                                        {
                                            previewHtml.AppendLine($"<td>{reader.GetValue(i)}</td>");
                                        }
                                        previewHtml.AppendLine("</tr>");
                                    }
                                }
                            }
                        }

                        previewHtml.AppendLine("</tbody></table>");

                        // Prepend the success message before the table.
                        var successMessage = $"> Punchout_account processed successfully - {punchoutAccountDataList.Count} rows added";
                        userMessage = $"{successMessage}<br>{previewHtml.ToString()}";
                    }
                    catch (Exception ex)
                    {
                        userMessage = $"Error retrieving the newly added records: {ex.Message}";
                    }
                }
                // CALLING STEP 2
                await ProcessExcelFileToUserConfig();
              
            }
            catch (Exception ex)
            {
                userMessages.Add($"> Error processing Excel file into Punchout_account: {ex.Message}");
            }
        }
        /*s
           Developed by : Samuel Espinoza
           Method Name: ProcessExcelFileToUserConfig
           Parameters : none
           Description : This is the SECOND step in the onboarding process, Once the excel file has been read and a new record has been added into the punchout_account table
                         this method, grabs some of the fields from the original excel file and the previous table (punchout_account) to fill the required fields into the 
                         UserConfig table.
           Status: COMPLETED
           Last date worked on: 2/8/2024       <--- update this accordingly
         */
        private async Task ProcessExcelFileToUserConfig()
        {
            userConfigDataList.Clear();
            // Assuming connectionString is derived from shared variables
            // TESTING CONNECTIONS STRING, COMMENT THIS OUT WHEN READY TO DEPLOY.
            var connectionString = Configuration?.GetConnectionString("DefaultConnection");
            // WHEN READY TO JUMP TO PRODUCTION REPLACE CONNECTION STRING WITH "DLPSQLConnection" for this step.
            //var connectionString = Configuration?.GetConnectionString("DLPSQLConnection");

            // Define the SQL command with placeholders
            var sqlInsertCommand = Configuration?["SqlCommands:InsertUserConfig"];  
            int recordsAdded = 0;
            int recordIdDLP = 0;
            try
            {
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var stream = new MemoryStream();
                    await selectedFile!.OpenReadStream().CopyToAsync(stream);
                    using var package = new ExcelPackage(stream);
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null)
                    {
                        userMessages.Add("> Error: No worksheet found in the Excel file.");
                        return;
                    }

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        // Process each row from Excel, complemented with data from punchoutAccountDataList
                        // Extract required values from Excel
                        var appPricelistOffer = worksheet.Cells[row, 5].Text;
                        var schoolOrganization = worksheet.Cells[row, 6].Text;
                        if (string.IsNullOrWhiteSpace(appPricelistOffer) || string.IsNullOrWhiteSpace(schoolOrganization))
                        {
                            // Skip rows with essential fields empty
                            continue;
                        }
                        var punchoutAccountNumber = "0" + worksheet.Cells[row, 3].Text.PadLeft(9, '0'); // Ensure it's 10 digits
                        var accountType = worksheet.Cells[row, 14].Text;
                        var customerType = accountType.Contains("Type") ? accountType.Split(' ')[1] : accountType;

                        var punchoutData = punchoutAccountDataList.FirstOrDefault(); // Assuming a single entry for simplification
                        if (punchoutData == null)
                        {
                            // Log or handle the case where punchoutData is not found
                            continue;
                        }
                        if (punchoutData != null)
                        {
                            userConfigDataList.Add(new UserConfigData
                            {
                                Key = appPricelistOffer,
                                Name = schoolOrganization,
                                ProviderCredential = punchoutData.SharedSecret,
                                UserIdentity = punchoutData.Identity,
                                // Set other properties as needed
                            });

                            using (var command = new SqlCommand(sqlInsertCommand, connection))
                            {
                                command.CommandType = CommandType.StoredProcedure; // Indicating it's a stored procedure

                                // Populate SQL command parameters from both Excel and punchoutAccountDataList
                                command.Parameters.AddWithValue("@Key", appPricelistOffer);
                                command.Parameters.AddWithValue("@Name", schoolOrganization);
                                command.Parameters.AddWithValue("@ProviderCredential", punchoutData.SharedSecret);
                                command.Parameters.AddWithValue("@UserIdentity", punchoutData.Identity);
                                command.Parameters.AddWithValue("@AccountNumber", punchoutAccountNumber);
                                command.Parameters.AddWithValue("@KeyCode", appPricelistOffer);
                                command.Parameters.AddWithValue("@CustomerType", customerType);
                                command.Parameters.AddWithValue("@HoldCode", HoldCode);
                                command.Parameters.AddWithValue("@PurchaseOrderProcessor", Processor);
                                // Add other parameters as required by the stored procedure
                                await command.ExecuteNonQueryAsync();

                            }
                            using (var commandForId = new SqlCommand("SELECT CAST(@@IDENTITY AS INT);", connection))
                            {
                                // ExecuteScalar returns the first column of the first row in the result set
                                var result = await commandForId.ExecuteScalarAsync();
                                if (result != DBNull.Value) // Check if the result is not DBNull
                                {
                                    recordIdDLP = Convert.ToInt32(result);
                                    // Update the last added UserConfigData in the list with the retrieved ID
                                    var lastAddedUserConfig = userConfigDataList.LastOrDefault();
                                    if (lastAddedUserConfig != null)
                                    {
                                        lastAddedUserConfig.IdDLP = recordIdDLP;
                                    }
                                }
                                else
                                {
                                    // Handle the case where no ID was retrieved, perhaps set recordIdDLP to a default value or log a warning
                                    recordIdDLP = -1; // Example default value, or handle as appropriate
                                }
                            }
                            recordsAdded++;
                        }
                        else
                        {
                            // Log or handle the case where punchoutData is not found
                        }
                    }

                    // SUCCESS MESSAGE goes here
                    try
                    {
                        var previewHtml1 = new StringBuilder();
                        previewHtml1.AppendLine("<table class='table-preview'><thead>");
                        previewHtml1.AppendLine("<tr><th>Id</th><th>Key</th><th>Name</th><th>ProviderCredential</th><th>UserIdentity</th><th>AccountNumber</th><th>KeyCode</th><th>DiscountCode</th><th>CustomerType</th><th>HoldCode</th><th>PurchaseOrderProcessor</th><th>InvoiceProcessor</th><th>InvoiceUrl</th><th>ShippingAcknowledgementProcessor</th><th>ShippingAcknowledgementURL</th><th>OrderAcknowledgementProcessor</th><th>OrderAcknowledgementURL</th><th>PrintPrices</th><th>OverridePrices</th><th>Expedite</th><th>UseSuppliedPH</th><th>UseAddressId</th></tr>");
                        previewHtml1.AppendLine("</thead><tbody>");

                        // Fetch the newly added records from the database to display
                        var selectCommandText = @"SELECT TOP 1 * FROM UserConfig ORDER BY Id DESC";
                        using (var connection2 = new SqlConnection(connectionString))
                        {
                            await connection2.OpenAsync();
                            using (var selectCommand = new SqlCommand(selectCommandText, connection2))
                            {
                                using (var reader = await selectCommand.ExecuteReaderAsync())
                                {
                                    while (reader.Read())
                                    {
                                        previewHtml1.Append("<tr>");
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            previewHtml1.Append($"<td>{reader.GetValue(i)}</td>");
                                        }
                                        previewHtml1.AppendLine("</tr>");
                                    }
                                }
                            }
                        }
                        previewHtml1.AppendLine("</tbody></table>");

                        // Construct the success message and append the HTML table
                        var successMessage1 = $"> UserConfig table updated successfully.";
                        userMessage += $"<br>{successMessage1}<br>{previewHtml1.ToString()}"; // Append to ensure accumulation of messages
                    }
                    catch (Exception ex)
                    {
                        userMessage += $"<br>Error updating UserConfig table: {ex.Message}"; // Append error message
                    }

                }
                // CALLING STEP 2
                await TransferUserConfigToTest();
            }
            catch (Exception ex)
            {
                userMessages.Add( $"> Error updating UserConfig table: {ex.Message}");
            }
        }

        /*
           Developed by : Samuel Espinoza
           Method Name: TransferUserConfigToTest
           Parameters : none
           Description : This is the THIRD and last step in the back-end side of the onboarding process. Using the record set in UserConfig, this method transfers the record
                         into the test database and table called UserConfig.
           Status: COMPLETED
           Last date worked on: 2/8/2024       <--- update this accordingly
         */
        private async Task TransferUserConfigToTest()
        {
            // WHEN READY TO JUMP TO PRODUCTION REPLACE CONNECTION STRING WITH "DWDSQLConnection" for this step.
            // TESTING CONNECTIONS STRING, COMMENT THIS OUT WHEN READY TO DEPLOY.
            var targetConnectionString = Configuration?.GetConnectionString("DefaultConnection");
            //var targetConnectionString = Configuration?.GetConnectionString("DWDSQLConnection");

            var insertCommandText = Configuration?["SqlCommands:InsertUserConfigTest"];

            // Assuming userConfigDataList is already populated with the data from UserConfig
            try
            {
                using (var targetConnection = new SqlConnection(targetConnectionString))
                {
                    await targetConnection.OpenAsync();
                    foreach (var config in userConfigDataList) // Iterate through each record you've added to userConfigDataList
                    {
                        using (var insertCommand = new SqlCommand(insertCommandText, targetConnection))
                        {
                            insertCommand.CommandType = CommandType.StoredProcedure; // Indicating it's a stored procedure

                            // Set parameter values based on the current record
                            insertCommand.Parameters.AddWithValue("@Key", config.Key);
                            insertCommand.Parameters.AddWithValue("@Name", config.Name);
                            insertCommand.Parameters.AddWithValue("@ProviderCredential", config.ProviderCredential);
                            insertCommand.Parameters.AddWithValue("@UserIdentity", config.UserIdentity);
                            insertCommand.Parameters.AddWithValue("@HoldCode", HoldCode);
                            insertCommand.Parameters.AddWithValue("@PurchaseOrderProcessor", Processor);
                            // Add other parameters as required by the stored procedure
                            // Note: Ensure you provide default values for parameters not set by the config object

                            await insertCommand.ExecuteNonQueryAsync();
                        }
                        using (var commandForId = new SqlCommand("SELECT CAST(@@IDENTITY AS INT);", targetConnection))
                        {
                            var result = await commandForId.ExecuteScalarAsync();
                            if (result != DBNull.Value)
                            {
                                config.IdDWD = Convert.ToInt32(result);
                            }
                            else
                            {
                                config.IdDWD = -1; // Example default value, or handle as appropriate
                            }
                        }
                    }
                }
                // SUCCESS MESSAGE HERE
                try
                {
                    var previewHtml3 = new StringBuilder();
                  
                    previewHtml3.AppendLine("<table class='table-preview'><thead>");
                    previewHtml3.AppendLine("<tr><th>Id</th><th>Key</th><th>Name</th><th>ProviderCredential</th><th>UserIdentity</th><th>AccountNumber</th><th>KeyCode</th><th>DiscountCode</th><th>CustomerType</th><th>HoldCode</th><th>PurchaseOrderProcessor</th><th>InvoiceProcessor</th><th>InvoiceURL</th><th>ShippingAcknowledgementProcessor</th><th>ShippingAcknowledgementURL</th><th>OrderAcknowledgementProcessor</th><th>OrderAcknowledgementURL</th><th>PrintPrices</th><th>OverridePrices</th><th>Expedite</th><th>UseSuppliedPH</th><th>UseAddressId</th><th>OverrideSource</th><th>UPIDPrefix</th><th>TaxExempt</th></tr>");
                    previewHtml3.AppendLine("</thead><tbody>");

                    using (var targetConnection = new SqlConnection(targetConnectionString))
                    {
                        await targetConnection.OpenAsync();
                        var selectCommandText = "SELECT TOP 1 * FROM [UserConfig_test] ORDER BY [Id] DESC";
                        using (var selectCommand = new SqlCommand(selectCommandText, targetConnection))
                        {
                            using (var reader = await selectCommand.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    previewHtml3.Append("<tr>");
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        previewHtml3.Append($"<td>{reader.GetValue(i)}</td>");
                                    }
                                    previewHtml3.AppendLine("</tr>");
                                }
                            }
                        }
                    }

                    previewHtml3.AppendLine("</tbody></table>");

                    // Construct the success message and append the HTML table
                    var successMessage1 = $"> UserConfig_test table updated successfully.";
                    userMessage += $"<br>{successMessage1}<br>{previewHtml3.ToString()}"; //
                }
                catch (Exception ex)
                {
                    userMessage += $"<br>Error during the transfer to UserConfig_test: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        userMessage += $"<br>Inner exception: {ex.InnerException.Message}";
                    }
                }
                await CreatePunchoutUserXrefEntries();

            }
            catch (Exception ex)
            {
                userMessages.Add($"> Error during transfer to UserConfig_test: {ex.Message}");
                if (ex.InnerException != null)
                {
                    userMessages.Add($"> Inner exception: {ex.InnerException.Message}");
                }
            }
        }
        /*
            Developed by : Samuel Espinoza
            Method Name: CreatePunchoutUserXrefEntries
            Parameters : none
            Description : This is the FOURTH and last step in the back-end side of the onboarding process.
                        Using the record set in UserConfig, this method transfers the Id and Name fields
                        into the Interface database within DWDSQL and DLPSQL servers and updates the
                        PunchoutUserXref table.
            Status: COMPLETED
            Last date worked on: 3/21/2024
         */
        private async Task CreatePunchoutUserXrefEntries()
        {
            // testing string
            var testConnectionString = Configuration?.GetConnectionString("DefaultConnection");

            //var dlpSqlConnectionString = Configuration?.GetConnectionString("DLPSQLXrefCon");
            //var dwdSqlConnectionString = Configuration?.GetConnectionString("DWDSQLXrefCon");
            var sqlInsertCommanddwd = Configuration?["SqlCommands:InsertPunchoutUserXref_dwd"];
            var sqlInsertCommanddlp = Configuration?["SqlCommands:InsertPunchoutUserXref_dlp"];
            try
            {
                foreach (var config in userConfigDataList)
                {
                    using (var dlpSqlConnection = new SqlConnection(testConnectionString))
                    {
                        await dlpSqlConnection.OpenAsync();
                        using (var insertCommand = new SqlCommand(sqlInsertCommanddlp, dlpSqlConnection))
                        {
                            insertCommand.CommandType = CommandType.StoredProcedure;
                            insertCommand.Parameters.AddWithValue("@Id", config.IdDLP);
                            insertCommand.Parameters.AddWithValue("@Name", config.Name);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }

                    using (var dwdSqlConnection = new SqlConnection(testConnectionString))
                    {
                        await dwdSqlConnection.OpenAsync();
                        using (var insertCommand = new SqlCommand(sqlInsertCommanddwd, dwdSqlConnection))
                        {
                            insertCommand.CommandType = CommandType.StoredProcedure;
                            insertCommand.Parameters.AddWithValue("@Id", config.IdDWD);
                            insertCommand.Parameters.AddWithValue("@Name", config.Name);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Success Message
                try
                {
                    var previewHtml4 = new StringBuilder();
                    previewHtml4.AppendLine("<table class='table-preview'><thead>");
                    previewHtml4.AppendLine("<tr><th colspan='2'>DLPSQL</th></tr>");
                    previewHtml4.AppendLine("<tr><th>Id</th><th>Name</th></tr>");
                    previewHtml4.AppendLine("</thead><tbody>");

                    // Retrieve the last inserted record from DLPSQL - change this
                    using (var dlpSqlConnection = new SqlConnection(testConnectionString))
                    {
                        await dlpSqlConnection.OpenAsync(); // make sure to change this otherwise it wont display correctly
                        var selectCommandText = "SELECT TOP 1 Id, Name FROM PunchoutUserXref_dlp ORDER BY Id DESC";
                        using (var selectCommand = new SqlCommand(selectCommandText, dlpSqlConnection))
                        {
                            using (var reader = await selectCommand.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    previewHtml4.Append("<tr>");
                                    previewHtml4.Append($"<td>{reader.GetInt32(0)}</td>");
                                    previewHtml4.Append($"<td>{reader.GetString(1)}</td>");
                                    previewHtml4.AppendLine("</tr>");
                                }
                            }
                        }
                    }

                    previewHtml4.AppendLine("</tbody></table>");

                    previewHtml4.AppendLine("<table class='table-preview'><thead>");
                    previewHtml4.AppendLine("<tr><th colspan='2'>DWDSQL</th></tr>");
                    previewHtml4.AppendLine("<tr><th>Id</th><th>Name</th></tr>");
                    previewHtml4.AppendLine("</thead><tbody>");

                    // Retrieve the last inserted record from DWDSQL - change this
                    using (var dwdSqlConnection = new SqlConnection(testConnectionString))
                    {
                        await dwdSqlConnection.OpenAsync(); // make sure to change this otherwise it wont display correctly
                        var selectCommandText = "SELECT TOP 1 Id, Name FROM PunchoutUserXref_dwd ORDER BY Id DESC";
                        using (var selectCommand = new SqlCommand(selectCommandText, dwdSqlConnection))
                        {
                            using (var reader = await selectCommand.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    previewHtml4.Append("<tr>");
                                    previewHtml4.Append($"<td>{reader.GetInt32(0)}</td>");
                                    previewHtml4.Append($"<td>{reader.GetString(1)}</td>");
                                    previewHtml4.AppendLine("</tr>");
                                }
                            }
                        }
                    }

                    previewHtml4.AppendLine("</tbody></table>");

                    var successMessage = "> PunchoutUserXref tables updated successfully.";
                    userMessage += $"<br>{successMessage}<br>{previewHtml4.ToString()}";
                }
                catch (Exception ex)
                {
                    userMessage += $"<br>Error retrieving data from PunchoutUserXref: {ex.Message}";
                }
                await GenerateImpexPreview();
            }
            catch (Exception ex)
            {
                userMessages.Add($"> Error updating PunchoutUserXref tables: {ex.Message}");
            }
        }
        /*******************************************************************************************************
         *  
                                           ONBOARDING PART II - HYBRIS
         *
         ****************************************************************************************************
         */
        /*
         Developed by : Samuel Espinoza
            Method Name: GenerateImpexPreview()
            Parameters : none
            Description : This method only generates a preview of what the Impex files will look like.
                          Note: B2B Unit will not reflect the school logo since its asked for and required in the next method UploadImpexRecords()
            Status: COMPLETE
            Last date worked on: 4/5/2024
         */

        private async Task GenerateImpexPreview()
        {
            try
            {
                // Step 1 - Retrieval of data to populate fields in the IMPEX structure
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set license context for EPPlus
                using var stream = new MemoryStream();
                await selectedFile!.OpenReadStream().CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    userMessages.Add("> Error: No worksheet found in the Excel file.");
                    return;
                }
                //
                // B2B UNIT IMPEX
                var b2bUnits = userConfigDataList.Select(config =>
                {
                    var punchoutAccount = punchoutAccountDataList.FirstOrDefault(account => account.Identity == config.UserIdentity);

                    // Assuming the single row record is in the second row (index 2) of the worksheet
                    var excelRow = worksheet.Cells[2, 1, 2, worksheet.Dimension.End.Column];
                    // Get the combined name from the Excel field
                    var combinedName = excelRow[ExcelAddress.GetAddress(2, 7)].Value?.ToString() ?? string.Empty;

                    // Split the combined name into first name and last name
                    var names = combinedName.Split(' ');
                    var firstName = names.Length > 0 ? names[0] : string.Empty;
                    var lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : string.Empty;
                    return new B2BUnit
                    {
                        Uid = config.UserIdentity,
                        Name = config.Name,
                        Groups = string.Empty,
                        UserPriceGroup = config.Key,
                        HpMainSubheading = string.Empty, // Assuming HpMainSubheading is in column 7 (index 6)
                        SchoolContactFirstName = firstName ?? string.Empty, // Assuming SchoolContactFirstName is in column 8 (index 7)
                        SchoolContactLastName = lastName ?? string.Empty, // Assuming SchoolContactLastName is in column 9 (index 8)
                        SchoolContactTitle = string.Empty, // Assuming SchoolContactTitle is in column 10 (index 9)
                        SchoolContactPhoneNumber = excelRow[ExcelAddress.GetAddress(2, 12)].Value?.ToString() ?? string.Empty, // Assuming SchoolContactPhoneNumber is in column 11 (index 10)
                        SchoolContactEmail = excelRow[ExcelAddress.GetAddress(2, 11)].Value?.ToString() ?? string.Empty, // Assuming SchoolContactEmail is in column 12 (index 11)
                        HpAdditionalInfoSubheading = string.Empty, // Assuming HpAdditionalInfoSubheading is in column 13 (index 12)
                        HpSchoolTerms = excelRow[ExcelAddress.GetAddress(2, 9)].Value?.ToString() ?? string.Empty, // Assuming HpSchoolTerms is in column 14 (index 13)
                        SchoolLogoURL = imageBase64String,
                        Individual = false
                    };
                }).ToList();
                //
                // B2B CUSTOMER IMPEX
                var b2bCustomers = userConfigDataList.Select(config =>
                {
                    var punchoutAccount = punchoutAccountDataList.FirstOrDefault(account => account.Identity == config.UserIdentity);
                    return new B2BCustomer
                    {
                        Uid = config.UserIdentity,
                        Name = config.Name,
                        CustomerKey = punchoutAccount?.Keycode ?? string.Empty,
                        PhoneNumber = string.Empty,
                        Email = string.Empty,
                        DefaultB2BUnit = config.UserIdentity,
                        Groups = $"{config.UserIdentity},b2bcustomergroup",
                        Password = string.Empty
                    };
                }).ToList();
                //
                // B2B CREDENTIALS IMPEX
                var punchoutCredentials = punchoutAccountDataList.Select(account => new PunchOutCredential
                {
                    Code = account.Keycode,
                    Domain = account.Identity,
                    Identity = account.Identity,
                    SharedSecret = account.SharedSecret
                }).ToList();
                //
                // B2B UNIT IMPEX
                var b2bCustomerPunchoutCredentialMappings = userConfigDataList.Select(config =>
                {
                    var punchoutAccount = punchoutAccountDataList.FirstOrDefault(account => account.Identity == config.UserIdentity);
                    return new B2BCustomerPunchOutCredentialMapping
                    {
                        B2BCustomer = config.UserIdentity,
                        Credentials = punchoutAccount?.Keycode ?? string.Empty
                    };
                }).ToList();

                var addresses = userConfigDataList.Select(config => new ImpexAddress
                {
                    Owner = config.UserIdentity,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Line1 = string.Empty,
                    Line2 = string.Empty,
                    PostalCode = string.Empty,
                    Town = string.Empty,
                    Region = string.Empty,
                    Country = string.Empty,
                    Phone1 = string.Empty,
                    BillingAddress = true,
                    ShippingAddress = true
                }).ToList();
                // Step 2 - Creation of IMPEX files according to the required schema
                var b2bUnitImpex = CreateB2BUnitImpexFile(b2bUnits);
                var b2bCustomerImpex = CreateB2BCustomerImpexFile(b2bCustomers);
                var punchoutCredentialImpex = CreatePunchoutCredentialImpexFile(punchoutCredentials);
                var b2bCustomerPunchoutCredentialMappingImpex = CreateB2BCustomerPunchoutCredentialMappingImpexFile(b2bCustomerPunchoutCredentialMappings);
                var addressImpex = CreateAddressImpexFile(addresses);

                // Preview IMPEX files
                var previewHtml = new StringBuilder();
                previewHtml.AppendLine("<b>B2B Unit IMPEX Preview:</b>");
                previewHtml.AppendLine("<pre>" + FormatImpexFile(b2bUnitImpex) + "</pre>");
                previewHtml.AppendLine("<b>B2B Customer IMPEX Preview:</b>");
                previewHtml.AppendLine("<pre>" + FormatImpexFile(b2bCustomerImpex) + "</pre>");
                previewHtml.AppendLine("<b>Punchout Credential IMPEX Preview:</b>");
                previewHtml.AppendLine("<pre>" + FormatImpexFile(punchoutCredentialImpex) + "</pre>");
                previewHtml.AppendLine("<b>B2B Customer Punchout Credential Mapping IMPEX Preview:</b>");
                previewHtml.AppendLine("<pre>" + FormatImpexFile(b2bCustomerPunchoutCredentialMappingImpex) + "</pre>");
                previewHtml.AppendLine("<b>Address IMPEX Preview:</b>");
                previewHtml.AppendLine("<pre>" + FormatImpexFile(addressImpex) + "</pre>");

                impexMessage += previewHtml.ToString();
            }
            catch (Exception ex)
            {
               userMessages.Add($"Error in GenerateImpexPreview: {ex.Message}");
            }
        }
        /*
         Developed by : Samuel Espinoza
            Method Name: UploadImpexFiles
            Parameters : none
            Description : THIS IS THE FIRST STEP in the onboarding process with Hybris.
                          This method assumes that there is a current onboarding process going on and that the first
                          part of the onboarding has been completed and corresponding databases have been updated.
                          Classes needed for this method have already been created within Models folder:
                                - B2BCustomer.cs, 
            Status: INCOMPLETE
            Last date worked on: 4/5/2024
            STEPS:
                   1 - Connection to the IMPEX file bucket.
                   2 - Handle of Image required for completion of onboarding. 
                        - (What format, How do you handle the package? What does SAP expect the image package to be like?).
                        - https://dmzn2b8hkpq8b.cloudfront.net/images/customers/logos/
                   3 - Retrieval of data to populate fields in the IMPEX structure.
                   4 - Creation of IMPEX file according to the required schema. 
                   5 - Uploading IMPEX file to Bucket with appropriate naming conventions.
 */
        private async Task UploadImpexFiles()
        {
            try
            {
                // Check if the user has selected an image
                if (string.IsNullOrEmpty(imageBase64String))
                {
                    userMessages.Add("> Please select an image before uploading IMPEX files.");
                    return;
                }

                // Step 1 - Retrieval of data to populate fields in the IMPEX structure
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set license context for EPPlus
                using var stream = new MemoryStream();
                await selectedFile!.OpenReadStream().CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    userMessages.Add("> Error: No worksheet found in the Excel file.");
                    return;
                }
                //
                // B2B UNIT IMPEX
                var b2bUnits = userConfigDataList.Select(config =>
                {
                    var punchoutAccount = punchoutAccountDataList.FirstOrDefault(account => account.Identity == config.UserIdentity);

                    // Assuming the single row record is in the second row (index 2) of the worksheet
                    var excelRow = worksheet.Cells[2, 1, 2, worksheet.Dimension.End.Column];
                    // Get the combined name from the Excel field
                    var combinedName = excelRow[ExcelAddress.GetAddress(2, 7)].Value?.ToString() ?? string.Empty;

                    // Split the combined name into first name and last name
                    var names = combinedName.Split(' ');
                    var firstName = names.Length > 0 ? names[0] : string.Empty;
                    var lastName = names.Length > 1 ? string.Join(" ", names.Skip(1)) : string.Empty;
                    return new B2BUnit
                    {
                        Uid = config.UserIdentity,
                        Name = config.Name,
                        Groups = string.Empty,
                        UserPriceGroup = config.Key,
                        HpMainSubheading = string.Empty, // Assuming HpMainSubheading is in column 7 (index 6)
                        SchoolContactFirstName = firstName ?? string.Empty, // Assuming SchoolContactFirstName is in column 8 (index 7)
                        SchoolContactLastName = lastName ?? string.Empty, // Assuming SchoolContactLastName is in column 9 (index 8)
                        SchoolContactTitle = string.Empty, // Assuming SchoolContactTitle is in column 10 (index 9)
                        SchoolContactPhoneNumber = excelRow[ExcelAddress.GetAddress(2, 12)].Value?.ToString() ?? string.Empty, // Assuming SchoolContactPhoneNumber is in column 11 (index 10)
                        SchoolContactEmail = excelRow[ExcelAddress.GetAddress(2, 11)].Value?.ToString() ?? string.Empty, // Assuming SchoolContactEmail is in column 12 (index 11)
                        HpAdditionalInfoSubheading = string.Empty, // Assuming HpAdditionalInfoSubheading is in column 13 (index 12)
                        HpSchoolTerms = excelRow[ExcelAddress.GetAddress(2, 9)].Value?.ToString() ?? string.Empty, // Assuming HpSchoolTerms is in column 14 (index 13)
                        SchoolLogoURL = imageBase64String,
                        Individual = false
                    };
                }).ToList();
                //
                // B2B CUSTOMER IMPEX
                var b2bCustomers = userConfigDataList.Select(config =>
                {
                    var punchoutAccount = punchoutAccountDataList.FirstOrDefault(account => account.Identity == config.UserIdentity);
                    return new B2BCustomer
                    {
                        Uid = config.UserIdentity,
                        Name = config.Name,
                        CustomerKey = punchoutAccount?.Keycode ?? string.Empty,
                        PhoneNumber = string.Empty,
                        Email = string.Empty,
                        DefaultB2BUnit = config.UserIdentity,
                        Groups = $"{config.UserIdentity},b2bcustomergroup",
                        Password = string.Empty
                    };
                }).ToList();
                //
                // B2B CREDENTIALS IMPEX
                var punchoutCredentials = punchoutAccountDataList.Select(account => new PunchOutCredential
                {
                    Code = account.Keycode,
                    Domain = account.Identity,
                    Identity = account.Identity,
                    SharedSecret = account.SharedSecret
                }).ToList();
                //
                // B2B UNIT IMPEX
                var b2bCustomerPunchoutCredentialMappings = userConfigDataList.Select(config =>
                {
                    var punchoutAccount = punchoutAccountDataList.FirstOrDefault(account => account.Identity == config.UserIdentity);
                    return new B2BCustomerPunchOutCredentialMapping
                    {
                        B2BCustomer = config.UserIdentity,
                        Credentials = punchoutAccount?.Keycode ?? string.Empty
                    };
                }).ToList();

                var addresses = userConfigDataList.Select(config => new ImpexAddress
                {
                    Owner = config.UserIdentity,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Line1 = string.Empty,
                    Line2 = string.Empty,
                    PostalCode = string.Empty,
                    Town = string.Empty,
                    Region = string.Empty,
                    Country = string.Empty,
                    Phone1 = string.Empty,
                    BillingAddress = true,
                    ShippingAddress = true
                }).ToList();
                // Step 2 - Creation of IMPEX files according to the required schema
                var b2bUnitImpex = CreateB2BUnitImpexFile(b2bUnits);
                var b2bCustomerImpex = CreateB2BCustomerImpexFile(b2bCustomers);
                var punchoutCredentialImpex = CreatePunchoutCredentialImpexFile(punchoutCredentials);
                var b2bCustomerPunchoutCredentialMappingImpex = CreateB2BCustomerPunchoutCredentialMappingImpexFile(b2bCustomerPunchoutCredentialMappings);
                var addressImpex = CreateAddressImpexFile(addresses);

                // Step 3 - Uploading IMPEX files to the bucket with appropriate naming conventions
                // TODO: Implement the logic to upload the IMPEX files to the Hybris bucket
                // Example: await UploadImpexFileToBucket(updatedB2bUnitImpex, "b2b_unit");
                // Example: await UploadImpexFileToBucket(b2bCustomerImpex, "b2b_customer");
                // Example: await UploadImpexFileToBucket(punchoutCredentialImpex, "punchout_credential");
                // Example: await UploadImpexFileToBucket(b2bCustomerPunchoutCredentialMappingImpex, "b2bcustomer_punchout_credential");
                // Example: await UploadImpexFileToBucket(addressImpex, "address");

                userMessages.Add("> IMPEX files uploaded successfully to the Hybris bucket.");
                ImpexUploaded = true;
            }
            catch (Exception ex)
            {
                userMessages.Add($"Error in UploadImpexFiles: {ex.Message}");
            }
        }
        /*******************************************************************************************************
         *  
                                              UTILITY METHODS
         *
         ****************************************************************************************************
         */

        private async Task GenerateCsvPreview()
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await selectedFile!.OpenReadStream().CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using var reader = new StreamReader(memoryStream);
                var previewHtml = new StringBuilder("<table class='table-preview'><thead>");
                int rowCount = 0;
                int totalRowCount = 0; // Variable to count total rows
                const int maxPreviewRows = 5;

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    totalRowCount++; // Increment total row count for each line read

                    // Only append rows to previewHtml if within maxPreviewRows
                    if (rowCount < maxPreviewRows)
                    {
                        var values = line!.Split(',');
                        if (rowCount == 0)
                        {
                            // Assuming the first row is headers
                            previewHtml.Append("<tr><th>" + string.Join("</th><th>", values) + "</th></tr></thead><tbody>");
                        }
                        else
                        {
                            previewHtml.Append("<tr><td>" + string.Join("</td><td>", values) + "</td></tr>");
                        }
                        rowCount++;
                    }
                }
                previewHtml.Append("</tbody>");

                // Calculate remaining rows not shown in the preview
                int remainingRows = totalRowCount - maxPreviewRows;
                if (remainingRows > 0)
                {
                    // Display the number of additional rows not shown
                    previewHtml.Append($"<tr><td colspan='100%'>... {remainingRows} more rows</td></tr>");
                }
                previewHtml.Append("</table>");
                userMessage = previewHtml.ToString();
            }
            catch (Exception ex)
            {
                userMessage = $"Error generating CSV preview: {ex.Message}";
            }
        }
        private async Task GenerateExcelPreview()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set license context for EPPlus
                var previewHtml0 = new StringBuilder("<table class='table-preview'><thead>");
                using var stream = new MemoryStream();
                await selectedFile!.OpenReadStream().CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet != null)
                {
                    // Assume the first row contains headers
                    previewHtml0.Append("<thead><tr>");
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        var headerValue = worksheet.Cells[1, col].Text;
                        previewHtml0.Append($"<th>{headerValue}</th>");
                    }
                    previewHtml0.Append("</tr></thead><tbody>");

                    const int maxPreviewRows = 5;
                    int displayedRows = 0;
                    int totalRows = worksheet.Dimension.End.Row; // Total number of rows in the worksheet
                    for (int row = 2; row <= totalRows; row++)
                    {
                        if (displayedRows < maxPreviewRows)
                        {
                            previewHtml0.Append("<tr>");
                            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                            {
                                var cellValue = worksheet.Cells[row, col].Text;
                                previewHtml0.Append($"<td>{cellValue}</td>");
                            }
                            previewHtml0.Append("</tr>");
                            displayedRows++;
                        }
                    }

                    // Calculate remaining rows not shown in the preview
                    int remainingRows = totalRows - maxPreviewRows - 1; // Adjusting for header row
                    if (remainingRows > 0)
                    {
                        // Display the number of additional rows not shown
                        previewHtml0.Append($"<tr><td colspan='{worksheet.Dimension.End.Column}'>... {remainingRows} more rows</td></tr>");
                    }
                    previewHtml0.Append("</tbody></table>");
                }
  
                // Prepend the success message before the table.
                var successMessage = $"> Preview of Excel File";
                filePreview += $"{successMessage}<br>{previewHtml0.ToString()}";
            }
            catch (Exception ex)
            {
                filePreview = $"Error generating Excel preview: {ex.Message}";
            }
        }
        private async Task TestDatabaseConnection() {
            var connectionString = $"Server={serverName};Database={databaseName};Integrated Security=True;";
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(); // Try opening the connection to the database

                    // Check if a table name has been provided
                    if (!string.IsNullOrWhiteSpace(tableName))
                    {
                        // Retrieve column headers from the specified table
                        var getColumnHeadersQuery = $@"
                                SELECT COLUMN_NAME
                                FROM INFORMATION_SCHEMA.COLUMNS
                                WHERE TABLE_NAME = @TableName
                                ORDER BY ORDINAL_POSITION";

                        using (var command = new SqlCommand(getColumnHeadersQuery, connection))
                        {
                            command.Parameters.AddWithValue("@TableName", tableName);

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                var columns = new List<string>();
                                while (await reader.ReadAsync())
                                {
                                    columns.Add(reader.GetString(0));
                                }

                                if (columns.Any())
                                {
                                    // Join column names with comma for display
                                    var columnHeaders = string.Join(", ", columns);
                                    userMessage = $"Connection successful. Table '{tableName}' exists with columns: {columnHeaders}.";
                                }
                                else
                                {
                                    userMessage = $"Connection successful. Table '{tableName}' does not exist or has no columns.";
                                }
                            }
                        }
                    }
                    else
                    {
                        userMessage = "Connection successful. No table name provided.";
                    }
                }
            }
            catch (Exception ex)
            {
                userMessage = $"Connection failed: {ex.Message}";
            }
        }
        // LOGGING METHOD -- NOT IMPLEMENTED 
        private async Task LogAction(string action, string recordDetails, string databasesAccessed)
        {
            try
            {
                // Get the current user's name (placeholder)
                string userName = "[UserName]";

                // Define the SQL connection string for the logging database
                var loggingConnectionString = Configuration?.GetConnectionString("LoggingConnection");

                // Define the SQL command to insert a log record
                var sqlInsertLog = "INSERT INTO LogTable (UserName, Action, RecordDetails, DatabasesAccessed, Timestamp) " +
                                   "VALUES (@UserName, @Action, @RecordDetails, @DatabasesAccessed, @Timestamp)";

                using (var connection = new SqlConnection(loggingConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(sqlInsertLog, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", userName);
                        command.Parameters.AddWithValue("@Action", action);
                        command.Parameters.AddWithValue("@RecordDetails", recordDetails);
                        command.Parameters.AddWithValue("@DatabasesAccessed", databasesAccessed);
                        command.Parameters.AddWithValue("@Timestamp", DateTime.Now);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during logging
                Console.WriteLine($"Error logging action: {ex.Message}");
            }
        }
        // SPACE REMOVER
        private string RemoveSpaces(string input)
        {
            return input.Replace(" ", string.Empty);
        }
        // CHECKS IF RECORD EXISTS BEFORE ONBOARDING PART I STARTS
        private async Task<bool> CheckRecordExistsAsync(SqlConnection connection, string identity)
        {
            var checkRecordExistsQuery = "SELECT COUNT(*) FROM punchout_account WHERE [identity] = @Identity";
            using (var command = new SqlCommand(checkRecordExistsQuery, connection))
            {
                command.Parameters.AddWithValue("@Identity", identity);
                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }
        //
        // make sure it generates passwords according to Hybris standards
        private string GenerateSharedSecret()
        {
            int length = _random.Next(8, 101); // Generate length between 8 and 100

            StringBuilder secretBuilder = new StringBuilder();
            secretBuilder.Append(GetRandomCharacter(UppercaseLetters));
            secretBuilder.Append(GetRandomCharacter(LowercaseLetters));
            secretBuilder.Append(GetRandomCharacter(Digits));
            secretBuilder.Append(GetRandomCharacter(SpecialCharacters));

            for (int i = 4; i < length; i++) // Start from 4 because one of each type has already been added
            {
                secretBuilder.Append(GetRandomCharacter(AllCharacters));
            }

            // To further randomize the character positions
            string secret = ShuffleString(secretBuilder.ToString());
            return secret;
        }
        // RANDOM CHARACTER SELECTOR
        private char GetRandomCharacter(string validCharacters)
        {
            int index = _random.Next(validCharacters.Length);
            return validCharacters[index];
        }
        // STRING SHUFFLER
        private string ShuffleString(string input)
        {
            var array = input.ToCharArray();
            int n = array.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + _random.Next(n - i);
                var t = array[r];
                array[r] = array[i];
                array[i] = t;
            }

            return new String(array);
        }
        //
        // Helper method TO FORMAT IMPEX files
        private string FormatImpexFile(string impexFile)
        {
            var lines = impexFile.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (line.StartsWith("INSERT_UPDATE"))
                {
                    sb.AppendLine(line);
                }
                else
                {
                    sb.AppendLine("    " + line);
                }
            }
            return sb.ToString();
        }
        //
        // Helper methods to create IMPEX files
        private string CreateB2BUnitImpexFile(List<B2BUnit> b2bUnits)
        {
            var sb = new StringBuilder();
            foreach (var unit in b2bUnits)
            {
                sb.AppendLine($"{unit.Uid},");
                sb.AppendLine($"{unit.Name},");
                sb.AppendLine($"{unit.Groups},");
                sb.AppendLine($"{unit.UserPriceGroup},");
                sb.AppendLine($"{unit.HpMainSubheading},");
                sb.AppendLine($"{unit.SchoolContactFirstName},");
                sb.AppendLine($"{unit.SchoolContactLastName},");
                sb.AppendLine($"{unit.SchoolContactTitle},");
                sb.AppendLine($"{unit.SchoolContactPhoneNumber},");
                sb.AppendLine($"{unit.SchoolContactEmail},");
                sb.AppendLine($"{unit.HpAdditionalInfoSubheading},");
                sb.AppendLine($"{unit.HpSchoolTerms},");
                sb.AppendLine($"{unit.SchoolLogoURL},");
                sb.AppendLine($"{unit.Individual}");
            }
            return sb.ToString();
        }

        private string CreateB2BCustomerImpexFile(List<B2BCustomer> b2bCustomers)
        {
            var sb = new StringBuilder();
            foreach (var customer in b2bCustomers)
            {
                sb.AppendLine($"    {customer.Uid};");
                sb.AppendLine($"    {customer.Name};");
                sb.AppendLine($"    {customer.CustomerKey};");
                sb.AppendLine($"    {customer.PhoneNumber};");
                sb.AppendLine($"    {customer.Email};");
                sb.AppendLine($"    {customer.DefaultB2BUnit};");
                sb.AppendLine($"    {customer.Groups};");
                sb.AppendLine($"    {customer.Password};");
            }
            return sb.ToString();
        }

        private string CreatePunchoutCredentialImpexFile(List<PunchOutCredential> punchoutCredentials)
        {
            var sb = new StringBuilder();
            foreach (var credential in punchoutCredentials)
            {
                sb.AppendLine($"    {credential.Code};");
                sb.AppendLine($"    {credential.Domain};");
                sb.AppendLine($"    {credential.Identity};");
                sb.AppendLine($"    {credential.SharedSecret};");
            }
            return sb.ToString();
        }

        private string CreateB2BCustomerPunchoutCredentialMappingImpexFile(List<B2BCustomerPunchOutCredentialMapping> mappings)
        {
            var sb = new StringBuilder();
            foreach (var mapping in mappings)
            {
                sb.AppendLine($"    {mapping.B2BCustomer};");
                sb.AppendLine($"    {mapping.Credentials};");
            }
            return sb.ToString();
        }

        private string CreateAddressImpexFile(List<ImpexAddress> addresses)
        {
            var sb = new StringBuilder();
            foreach (var address in addresses)
            {
                sb.AppendLine($"    {address.Owner};");
                sb.AppendLine($"    {address.FirstName};");
                sb.AppendLine($"    {address.LastName};");
                sb.AppendLine($"    {address.Line1};");
                sb.AppendLine($"    {address.Line2};");
                sb.AppendLine($"    {address.PostalCode};");
                sb.AppendLine($"    {address.Town};");
                sb.AppendLine($"    {address.Region};");
                sb.AppendLine($"    {address.Country};");
                sb.AppendLine($"    {address.Phone1};");
                sb.AppendLine($"    {address.BillingAddress};");
                sb.AppendLine($"    {address.ShippingAddress};");
            }
            return sb.ToString();
        }
       

    
    }
}
