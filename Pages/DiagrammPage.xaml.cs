using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using _122_Tsygarin;

namespace _122_Tsygarin.Pages
{
    public partial class DiagrammPage : Page
    {
        private Tsygarin_DB_PaymentEntities _context = new Tsygarin_DB_PaymentEntities();

        public DiagrammPage()
        {
            InitializeComponent();

            ChartPayments.ChartAreas.Add(new ChartArea("Main"));

            var currentSeries = new Series("Платежи")
            {
                IsValueShownAsLabel = true
            };
            ChartPayments.Series.Add(currentSeries);

            CmbUser.ItemsSource = _context.User.ToList(); 
            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType)); 
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser &&
                CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var categoriesList = _context.Category.ToList();
                foreach (var category in categoriesList)
                {
                    currentSeries.Points.AddXY(category.Name,
                        _context.Payment.ToList()
                            .Where(u => u.User == currentUser && u.Category == category)
                            .Sum(u => u.Price * u.Num));
                }
            }
        }


        private void BtnWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем список пользователей и категорий из базы данных
                var allUsers = _context.User.ToList();
                var allCategories = _context.Category.ToList();

                // Создаем новый документ Word
                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                // Запускаем цикл по пользователям
                foreach (var user in allUsers)
                {
                    // Создаем абзац для хранения названия страницы
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userParagraph.set_Style("Заголовок");
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();
                    document.Paragraphs.Add(); // Пустая строка

                    // Добавляем новый абзац для вывода таблицы с платежами
                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count() + 1, 2);
                    paymentsTable.Borders.InsideLineStyle =
                    paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    // Добавляем названия колонок и их форматирование
                    Word.Range cellRange;

                    cellRange = paymentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = paymentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";

                    paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                    paymentsTable.Rows[1].Range.Font.Size = 14;
                    paymentsTable.Rows[1].Range.Bold = 1;
                    paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Заполняем колонки данными с последующим их форматированием
                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        var currentCategory = allCategories[i];
                        cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        cellRange = paymentsTable.Cell(i + 2, 2).Range;
                        cellRange.Text = user.Payment.ToList()
                            .Where(u => u.Category == currentCategory)
                            .Sum(u => u.Num * u.Price).ToString("N2") + " руб.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }

                    document.Paragraphs.Add(); // Пустая строка

                    // Добавляем расчет максимальной величины платежа
                    Payment maxPayment = user.Payment.OrderByDescending(u => u.Price * u.Num).FirstOrDefault();
                    if (maxPayment != null)
                    {
                        Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                        Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                        maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} " +
                            $"за {(maxPayment.Price * maxPayment.Num).ToString("N2")} руб. от {maxPayment.Date.ToString("dd.MM.yyyy")}";
                        maxPaymentParagraph.set_Style("Подзаголовок");
                        maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxPaymentRange.InsertParagraphAfter();
                    }

                    document.Paragraphs.Add(); // Пустая строка

                    // Добавляем расчет минимальной величины платежа
                    Payment minPayment = user.Payment.OrderBy(u => u.Price * u.Num).FirstOrDefault();
                    if (minPayment != null)
                    {
                        Word.Paragraph minPaymentParagraph = document.Paragraphs.Add();
                        Word.Range minPaymentRange = minPaymentParagraph.Range;
                        minPaymentRange.Text = $"Самый дешевый платеж - {minPayment.Name} " +
                            $"за {(minPayment.Price * minPayment.Num).ToString("N2")} руб. от {minPayment.Date.ToString("dd.MM.yyyy")}";
                        minPaymentParagraph.set_Style("Подзаголовок");
                        minPaymentRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minPaymentRange.InsertParagraphAfter();
                    }

                    // Добавляем разрыв страницы
                    if (user != allUsers.LastOrDefault())
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }

                // Добавляем колонтитул с номером страниц
                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                // Добавляем верхний колонтитул с текущей датой
                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Fields.Add(headerRange, Word.WdFieldType.wdFieldPage);
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                    headerRange.Font.Size = 10;
                    headerRange.Text = DateTime.Now.ToString("dd/MM/yyyy");
                }

                // Разрешаем отображение документа
                application.Visible = true;

                // Сохраняем документ (укажите путь)
                string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                document.SaveAs2(System.IO.Path.Combine(downloadsPath, "Payments.docx"));
                document.SaveAs2(System.IO.Path.Combine(downloadsPath, "Payments.pdf"), Word.WdExportFormat.wdExportFormatPDF);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в Word: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик кнопки "Excel"
        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем список пользователей с одновременной сортировкой по ФИО
                var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();

                // Создаем новую книгу Excel
                var application = new Excel.Application();
                application.SheetsInNewWorkbook = allUsers.Count();
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

                double grandTotal = 0;

                // Запускаем цикл по пользователям
                for (int i = 0; i < allUsers.Count(); i++)
                {
                    // Устанавливаем счетчик строк и называем листы
                    int startRowIndex = 1;
                    Excel.Worksheet worksheet = (Excel.Worksheet)application.Worksheets.Item[i + 1];

                    // Формируем уникальное имя листа (имена в Excel не могут повторяться и должны быть короче 31 символа)
                    string sheetName = allUsers[i].FIO;
                    if (sheetName.Length > 31)
                        sheetName = sheetName.Substring(0, 31);

                    // Проверяем уникальность имени
                    int counter = 1;
                    string originalName = sheetName;
                    bool nameExists = true;
                    while (nameExists)
                    {
                        nameExists = false;
                        foreach (Excel.Worksheet ws in workbook.Worksheets)
                        {
                            if (ws.Name == sheetName)
                            {
                                nameExists = true;
                                sheetName = $"{originalName}_{counter}";
                                if (sheetName.Length > 31)
                                    sheetName = $"{originalName.Substring(0, 28)}_{counter}";
                                counter++;
                                break;
                            }
                        }
                    }

                    worksheet.Name = sheetName;

                    // Добавляем названия колонок и форматируем их
                    worksheet.Cells[1][startRowIndex] = "Дата платежа";
                    worksheet.Cells[2][startRowIndex] = "Название";
                    worksheet.Cells[3][startRowIndex] = "Стоимость";
                    worksheet.Cells[4][startRowIndex] = "Количество";
                    worksheet.Cells[5][startRowIndex] = "Сумма";

                    Excel.Range columlHeaderRange = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[5][1]];
                    columlHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    columlHeaderRange.Font.Bold = true;
                    startRowIndex++;

                    // Группируем платежи текущего пользователя по категориям
                    var userCategories = allUsers[i].Payment.OrderBy(u => u.Date)
                        .GroupBy(u => u.Category).OrderBy(u => u.Key.Name);

                    // Создаем вложенный цикл по категориям платежей
                    foreach (var groupCategory in userCategories)
                    {
                        Excel.Range headerRange = worksheet.Range[worksheet.Cells[1][startRowIndex], worksheet.Cells[5][startRowIndex]];
                        headerRange.Merge();
                        headerRange.Value = groupCategory.Key.Name;
                        headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        headerRange.Font.Italic = true;
                        startRowIndex++;

                        // Создаем еще один вложенный цикл по платежам
                        foreach (var payment in groupCategory)
                        {
                            worksheet.Cells[1][startRowIndex] = payment.Date.ToString("dd.MM.yyyy");
                            worksheet.Cells[2][startRowIndex] = payment.Name;
                            worksheet.Cells[3][startRowIndex] = payment.Price;
                            (worksheet.Cells[3][startRowIndex] as Excel.Range).NumberFormat = "0.00";
                            worksheet.Cells[4][startRowIndex] = payment.Num;
                            worksheet.Cells[5][startRowIndex].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                            (worksheet.Cells[5][startRowIndex] as Excel.Range).NumberFormat = "0.00";
                            startRowIndex++;
                        }

                        // Добавляем названия к ячейкам для хранения общих затрат (ИТОГО)
                        Excel.Range sumRange = worksheet.Range[worksheet.Cells[1][startRowIndex], worksheet.Cells[4][startRowIndex]];
                        sumRange.Merge();
                        sumRange.Value = "ИТОГО:";
                        sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                        // Рассчитываем величину общих затрат
                        worksheet.Cells[5][startRowIndex].Formula = $"=SUM(E{startRowIndex - groupCategory.Count()}:E{startRowIndex - 1})";
                        sumRange.Font.Bold = worksheet.Cells[5][startRowIndex].Font.Bold = true;

                        grandTotal += (double)groupCategory.Sum(u => u.Price * u.Num);

                        startRowIndex++;
                    }

                    // Добавляем границы таблицы платежей
                    Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[5][startRowIndex - 1]];
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle =
                    Excel.XlLineStyle.xlContinuous;

                    // Устанавливаем автоширину всех столбцов листа
                    worksheet.Columns.AutoFit();
                }

                // Добавляем строку «Общий итог»
                Excel.Worksheet summarySheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
                summarySheet.Name = "Общий итог";

                summarySheet.Cells[1, 1] = "Общий итог:";
                summarySheet.Cells[1, 2] = grandTotal;

                Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
                summaryRange.Font.Bold = true;

                summarySheet.Columns.AutoFit();

                // Разрешаем отобразить таблицу по завершении экспорта
                application.Visible = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}