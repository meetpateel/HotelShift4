// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

GetChoiceAdvantageData("userName", "password");
Dictionary<string, string> data = GetPdfData(@"downloadpath", "report.pdf");
Console.WriteLine("Type    Amount");
Console.WriteLine("--------------");
foreach (var dataPoint in data)
{
    Console.WriteLine($"{dataPoint.Key}    ${dataPoint.Value}");
}
GetShift4Data("0000", "gm.userName", "password", data);



void GetChoiceAdvantageData(string userName, string password)
{
    ChromeOptions options = new ChromeOptions();
    //Download pdf when you go click on them
    options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
    var driver = new ChromeDriver(options);
    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    //Navigate to google page
    driver.Navigate().GoToUrl("https://www.choiceadvantage.com/choicehotels/sign_in.jsp");
    //Maximize the window
    driver.Manage().Window.Maximize();
    //Login
    driver.FindElement(By.Name("j_username")).SendKeys(userName);
    driver.FindElement(By.Name("j_password")).SendKeys(password);
    driver.FindElement(By.Id("greenButton")).Click();
    //Menu
    driver.FindElement(By.Id("act3")).Click();
    //Menu-Night Audit Option
    driver.FindElement(By.Id("menu4_11")).Click();
    driver.FindElement(By.ClassName("CHI_Button")).Click();
    //Hotel Journal Summary
    driver.FindElement(By.LinkText("Hotel Journal Summary")).Click();

    Thread.Sleep(2000);
    driver.Quit();
}

Dictionary<string, string> GetPdfData(string downloadsPath, string downloadedFileName)
{
    string downloadedFile = Path.Combine(downloadsPath, downloadedFileName);
    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
    if (Directory.Exists(downloadsPath))
    {
        if (File.Exists(downloadedFile))
        {
            string currentText = string.Empty;
            StringBuilder text = new StringBuilder();
            FileStream fs = new FileStream(downloadedFile, FileMode.Open, FileAccess.Read);

            PdfReader pdfReader = new PdfReader(fs);
            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.UTF8.GetBytes(currentText)));
                text.Append(currentText);
                pdfReader.Close();
            }
            string[] data = currentText.Split('\n');

            if (data != null)
            {
                foreach (var line in data)
                {
                    /*if (line.Contains("CA"))
                    //{
                    //    keyValuePairs.Add("CA", GetAmount("CA", line));
                    }
                    else*/
                    if (line.Contains("DS"))
                    {
                        keyValuePairs.Add("NS", GetAmount("DS", line));
                    }
                    else if (line.Contains("MC"))
                    {
                        keyValuePairs.Add("MC", GetAmount("MC", line));
                    }
                    else if (line.Contains("VI"))
                    {
                        keyValuePairs.Add("VS", GetAmount("VI", line));
                    }
                    else if (line.Contains("AX"))
                    {
                        keyValuePairs.Add("AX", GetAmount("AX", line));
                    }
                }
            }
            fs.Close();
        }
    }
    File.Delete(downloadedFile);
    return keyValuePairs;
}

string GetAmount(string paymentType, string line)
{

    var data = line.Split(' ');
    if (paymentType == "MC" || paymentType == "VI" || paymentType == "AX")
    {
        return data[6];
    }
    else
    {
        return data[5];
    }
}

void GetShift4Data(string accountNumber, string userName, string password, Dictionary<string, string> data)
{
    ChromeOptions options = new ChromeOptions();
    //Download pdf when you go click on them
    options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
    var driver = new ChromeDriver(options);

    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    //Navigate to google page
    driver.Navigate().GoToUrl("https://dollarsonthenet.net/login.cfm");
    //Maximize the window
    driver.Manage().Window.Maximize();
    //Login
    driver.FindElement(By.Id("serialNumber")).SendKeys(accountNumber);
    driver.FindElement(By.Id("userName")).SendKeys(userName);
    driver.FindElement(By.Id("password")).SendKeys(password);
    driver.FindElement(By.Id("login-submit-btn")).Click();
    //Calendar
    driver.FindElement(By.ClassName("businessDate")).Click();
    driver.FindElement(By.Id("busDateModeT")).Click();
    driver.FindElement(By.Id("busDateApplyButton")).Click();
    //Getting data
    WebElement element = (WebElement)driver.FindElement(By.Id("cancelDoBusinessDate"));
    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
    js.ExecuteScript("arguments[0].scrollIntoView();", element);
    Thread.Sleep(1000);
    //driver.FindElement(By.Id("cardSummary165167"));
    var table = driver.FindElement(By.Id("cardSummary165167"));
    var listOfRows = new List<IWebElement>(table.FindElements(By.TagName("tr")));
    string reportData = string.Empty;
    string shift4Data = string.Empty;
    foreach (var item in listOfRows)
    {
        if (item.Text.Contains("MC"))
        {
            reportData = Regex.Match(data["MC"], @"\d+.+\d").Value;
            shift4Data = Regex.Match(GetAmountFromChoiceAdvantage(item.Text), @"\d+.+\d").Value;

            if (shift4Data == reportData)
            {
                Console.WriteLine("MASTERCARD MATCH SUCCESS");
            }
            else
            {
                Console.WriteLine("Error MasterCard does not match");
                break;
            }
        }
        if (item.Text.Contains("VS"))
        {
            reportData = Regex.Match(data["VS"], @"\d+.+\d").Value;
            shift4Data = Regex.Match(GetAmountFromChoiceAdvantage(item.Text), @"\d+.+\d").Value;

            if (shift4Data == reportData)
            {
                Console.WriteLine("VISA MATCH SUCCESS");
            }
            else
            {
                Console.WriteLine("Error Visa does not match");
                break;
            }
        }
        if (item.Text.Contains("NS"))
        {
            reportData = Regex.Match(data["NS"], @"\d+.+\d").Value;
            shift4Data = Regex.Match(GetAmountFromChoiceAdvantage(item.Text), @"\d+.+\d").Value;

            if (shift4Data == reportData)
            {
                Console.WriteLine("DISCOVER MATCH SUCCESS");
            }
            else
            {
                Console.WriteLine("Error Discover does not match");
                break;
            }
        }
        if (item.Text.Contains("AX"))
        {
            reportData = Regex.Match(data["AX"], @"\d+.+\d").Value;
            shift4Data = Regex.Match(GetAmountFromChoiceAdvantage(item.Text), @"\d+.+\d").Value;

            if (shift4Data == reportData)
            {
                Console.WriteLine("AMERICAN EXPRESS MATCH SUCCESS");
            }
            else
            {
                Console.WriteLine("Error American Express does not match");
                break;
            }
        }
    }
    driver.FindElement(By.Id("closeBatchButtonEnabled")).Click();
    Thread.Sleep(10000);
    driver.Quit();
}

string GetAmountFromChoiceAdvantage(string line)
{
    return line.Split(' ').Last();
}