using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zoomir.models;

namespace Zoomir.file
{
    public class TicketCreater
    {

        public void CreateTicket(FullOrderInfo orderInfo)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = orderInfo.Id + ".pdf";
            saveFileDialog.Filter = "PDF файлы (*.pdf)|*.pdf";

            string filePath;
            if (saveFileDialog.ShowDialog() == true)
            {
                filePath = saveFileDialog.FileName;
            }
            else
            {
                return;
            }

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += (sender, e) =>
            {
                Graphics graphics = e.Graphics;
                Font font = new Font("Arial", 12);
                Font titleFont = new Font("Arial", 16);
                float fontHeight = font.GetHeight();

                float startX = 10;
                float startY = 10;
                float offset = 40;

                graphics.DrawString("Талон на получение", titleFont, Brushes.Black, (e.PageBounds.Width - graphics.MeasureString("Талон на получение", font).Width) / 2, startY);
                startY += offset;

                graphics.DrawString($"Дата заказа: {orderInfo.Date.ToShortDateString()}", font, Brushes.Black, startX, startY);
                startY += offset;

                graphics.DrawString($"Номер заказа: {orderInfo.Id}", font, Brushes.Black, startX, startY);
                startY += offset;

                graphics.DrawString("Состав заказа:", font, Brushes.Black, startX, startY);
                startY += offset;

                foreach (var product in orderInfo.ProductCount)
                {
                    graphics.DrawString($"- {product.Title} - {product.Count} шт.", font, Brushes.Black, startX, startY);
                    startY += offset;
                }

                graphics.DrawString($"Сумма заказа: {orderInfo.TotalPrice}", font, Brushes.Black, startX, startY);
                startY += offset;

                graphics.DrawString($"Сумма скидки: {orderInfo.TotalDiscount}", font, Brushes.Black, startX, startY);
                startY += offset;

                graphics.DrawString($"Пункт выдачи: {orderInfo.PickUpPoint}", font, Brushes.Black, startX, startY);
                startY += offset;

                graphics.DrawString($"Код для получения: {orderInfo.Code}", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, startX, startY);
            };

            printDocument.PrinterSettings.PrintToFile = true;
            printDocument.PrinterSettings.PrintFileName = filePath;
            printDocument.Print();
        }
    }
}
