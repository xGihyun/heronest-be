using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using QRCoder;

namespace Heronest.Features.Ticket;

using Color = System.Drawing.Color;
using Path = System.IO.Path;
using PdfRectangle = iText.Kernel.Geom.Rectangle;

public class BatchTicketGenerator
{
    public void GenerateBatch(List<Ticket> tickets, string zipOutputPath)
    {
        const float shortBondWidth = 612; // 8.5 inches in points
        const float shortBondHeight = 792; // 11 inches in points
        const int ticketsPerPage = 4;

        string tempFolder = Path.Combine(Path.GetTempPath(), "TicketBatch");
        Directory.CreateDirectory(tempFolder);

        var darkBlue = new DeviceRgb(28, 33, 87);
        var yellow = new DeviceRgb(255, 247, 87);
        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        int batchIndex = 0;
        for (int i = 0; i < tickets.Count; i += ticketsPerPage)
        {
            string pdfPath = Path.Combine(tempFolder, $"Batch-{batchIndex + 1}.pdf");

            using (PdfWriter writer = new PdfWriter(pdfPath))
            using (PdfDocument pdfDoc = new PdfDocument(writer))
            {
                var pageSize = new PageSize(shortBondWidth, shortBondHeight);
                Document document = new Document(pdfDoc, pageSize);

                pdfDoc.AddNewPage();
                float ticketWidth = shortBondWidth / 2;
                float ticketHeight = shortBondHeight / 2;

                for (int j = 0; j < ticketsPerPage && i + j < tickets.Count; j++)
                {
                    Ticket ticket = tickets[i + j];

                    float xPosition = (j % 2) * ticketWidth;
                    float yPosition = shortBondHeight - ((j / 2 + 1) * ticketHeight);

                    // Draw Ticket Details
                    DrawTicket(
                        document,
                        ticket,
                        xPosition,
                        yPosition,
                        ticketWidth,
                        ticketHeight,
                        font,
                        boldFont,
                        darkBlue,
                        yellow
                    );
                }
            }

            batchIndex++;
        }

        // Create zip file
        string zipFilePath = zipOutputPath;
        if (File.Exists(zipFilePath))
            File.Delete(zipFilePath);
        ZipFile.CreateFromDirectory(tempFolder, zipFilePath);

        // Clean up temporary files
        Directory.Delete(tempFolder, true);
    }

    private void DrawTicket(
        Document document,
        Ticket ticket,
        float x,
        float y,
        float width,
        float height,
        PdfFont font,
        PdfFont boldFont,
        DeviceRgb darkBlue,
        DeviceRgb yellow
    )
    {
        var canvas = new Canvas(
            document.GetPdfDocument().GetFirstPage(),
            new Rectangle(x, y, width, height)
        );

        canvas.Add(
            new Paragraph(ticket.Reservation.Event.Name)
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetFontColor(darkBlue)
                .SetFixedPosition(x + 10, y + height - 40, 200)
        );

        canvas.Add(
            new Paragraph(ticket.Reservation.Venue.Name)
                .SetFont(font)
                .SetFontSize(12)
                .SetFontColor(yellow)
                .SetFixedPosition(x + 10, y + height - 60, 200)
        );

        canvas.Add(
            new Paragraph($"Seat: {ticket.Reservation.Seat.SeatNumber}")
                .SetFont(font)
                .SetFontSize(10)
                .SetFontColor(ColorConstants.BLACK)
                .SetFixedPosition(x + 10, y + height - 80, 200)
        );

        canvas.Add(
            new Paragraph($"Date: {ticket.Reservation.Event.StartAt:MMM dd, yyyy}")
                .SetFont(font)
                .SetFontSize(10)
                .SetFontColor(ColorConstants.BLACK)
                .SetFixedPosition(x + 10, y + height - 100, 200)
        );

        // QR Code
        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(ticket.TicketNumber, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        byte[] qrBytes = qrCode.GetGraphic(20);

        var qrImage = ImageDataFactory.Create(qrBytes);
        var qrElement = new iText.Layout.Element.Image(qrImage)
            .SetFixedPosition(x + width - 80, y + height - 80)
            .ScaleAbsolute(70, 70);

        canvas.Add(qrElement);

        canvas.Close();
    }
}
