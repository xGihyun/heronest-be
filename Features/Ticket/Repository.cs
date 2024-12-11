using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using DapperQueryBuilder;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using NanoidDotNet;
using Npgsql;
using QRCoder;

namespace Heronest.Features.Ticket;

using Color = System.Drawing.Color;
using Path = System.IO.Path;
using PdfRectangle = iText.Kernel.Geom.Rectangle;

public interface ITicketRepository
{
    Task<Ticket[]> GetMany(GetTicketFilter filter);
    Task<Ticket?> GetByTicketNumber(string ticketNumber);
    Task<Ticket> Create(CreateTicketRequest data);
    Task Update(UpdateTicketRequest data);
    void GeneratePdf(Ticket ticket, string? templatePath = null, string? outputPath = null);
    string GeneratePdfBatch(List<Ticket> tickets);
}

public class TicketRepository : ITicketRepository
{
    private NpgsqlDataSource dataSource;

    public TicketRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<Ticket?> GetByTicketNumber(string ticketNumber)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            SELECT
                tickets.ticket_id,
                tickets.ticket_number,
                tickets.created_at AS reserved_at,
                tickets.status,
                jsonb_build_object(
                    'user', jsonb_build_object(
                        'user_id', users.user_id,
                        'first_name', users.first_name,
                        'middle_name', users.middle_name,
                        'last_name', users.last_name
                    ),
                    'event', jsonb_build_object(
                        'event_id', events.event_id,
                        'name', events.name,
                        'start_at', events.start_at,
                        'end_at', events.end_at
                    ),
                    'seat', jsonb_build_object(
                        'seat_id', seats.seat_id,
                        'seat_number', seats.seat_number
                    ),
                    'venue', jsonb_build_object(
                        'venue_id', venues.venue_id,
                        'name', venues.name
                    )
                ) AS reservation_json
            FROM tickets
            JOIN users ON users.user_id = tickets.user_id
            JOIN events ON events.event_id = tickets.event_id
            JOIN seats ON seats.seat_id = tickets.seat_id
            JOIN venues ON venues.venue_id = events.venue_id
            WHERE tickets.ticket_number = {ticketNumber}
            "
        );

        var ticket = await sql.QueryFirstOrDefaultAsync<Ticket>();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower));

        ticket.Reservation =
            JsonSerializer.Deserialize<TicketReservation>(ticket.ReservationJson, options)
            ?? throw new JsonException("Failed to deserialize ticket reservation details.");

        return ticket;
    }

    public async Task<Ticket[]> GetMany(GetTicketFilter filter)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            SELECT
                tickets.ticket_id,
                tickets.ticket_number,
                tickets.created_at AS reserved_at,
                tickets.status,
                jsonb_build_object(
                    'user', jsonb_build_object(
                        'user_id', users.user_id,
                        'first_name', users.first_name,
                        'middle_name', users.middle_name,
                        'last_name', users.last_name,
                        'avatar_url', users.avatar_url
                    ),
                    'event', jsonb_build_object(
                        'event_id', events.event_id,
                        'name', events.name,
                        'start_at', events.start_at,
                        'end_at', events.end_at
                    ),
                    'seat', jsonb_build_object(
                        'seat_id', seats.seat_id,
                        'seat_number', seats.seat_number
                    ),
                    'venue', jsonb_build_object(
                        'venue_id', venues.venue_id,
                        'name', venues.name
                    )
                ) AS reservation_json
            FROM tickets
            JOIN users ON users.user_id = tickets.user_id
            JOIN events ON events.event_id = tickets.event_id
            JOIN seats ON seats.seat_id = tickets.seat_id
            JOIN venues ON venues.venue_id = events.venue_id
            WHERE 1=1
            "
        );

        if (filter.EventId.HasValue)
        {
            sql += $"AND tickets.event_id = {filter.EventId.Value}";
        }

        if (filter.UserId.HasValue)
        {
            sql += $"AND tickets.user_id = {filter.UserId.Value}";
        }

        // NOTE: Should probably be a query param as well.
        sql += $"ORDER BY tickets.created_at DESC";

        if (filter.Offset.HasValue && filter.Limit.HasValue)
        {
            sql += $"OFFSET {filter.Offset} LIMIT {filter.Limit}";
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower));

        var tickets = await sql.QueryAsync<Ticket>();
        tickets = tickets.Select(ticket =>
        {
            ticket.Reservation =
                JsonSerializer.Deserialize<TicketReservation>(ticket.ReservationJson, options)
                ?? throw new JsonException("Failed to deserialize ticket reservation details.");

            return ticket;
        });

        return tickets.ToArray();
    }

    public async Task<Ticket> Create(CreateTicketRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var ticketNumber = Nanoid.Generate("0123456789ABCDEF", 6);

        var sql = conn.QueryBuilder(
            $@"
            INSERT INTO tickets (user_id, seat_id, event_id, ticket_number, status)
            VALUES (
                {data.UserId}, 
                {data.SeatId}, 
                {data.EventId}, 
                {ticketNumber}, 
                'reserved'
                )
            "
        );

        await sql.ExecuteAsync();

        var ticket = await this.GetByTicketNumber(ticketNumber);

        if (ticket is null)
        {
            throw new Exception("Ticket is null after creation.");
        }

        this.GeneratePdf(ticket);

        return ticket;
    }

    public async Task Update(UpdateTicketRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            UPDATE tickets 
            SET status = {data.Status}
            WHERE ticket_id = {data.TicketId}
            "
        );

        await sql.ExecuteAsync();
    }

    public void GeneratePdf(Ticket ticket, string? templatePath = null, string? outputPath = null)
    {
        // NOTE: These paths should be in a configuration
        templatePath ??= "/home/gihyun/Development/svelte/heronest/static/ticket-template.pdf";
        outputPath ??= Path.Combine(
            "/home/gihyun/Development/svelte/heronest/static/storage/tickets",
            $"Ticket-{ticket.TicketNumber}.pdf"
        );

        using (PdfReader reader = new PdfReader(templatePath))
        using (PdfWriter writer = new PdfWriter(outputPath))
        using (PdfDocument pdfDoc = new PdfDocument(reader, writer))
        {
            var page = pdfDoc.GetFirstPage();
            var pdfCanvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);
            var document = new Document(pdfDoc);

            var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(
                iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD
            );

            var darkBlue = new DeviceRgb(28, 33, 87);
            var yellow = new DeviceRgb(255, 247, 87);

            PdfRectangle pageSize = page.GetPageSize();

            // Marshal's copy details
            float xRight = pageSize.GetWidth() - 52;
            AddRotatedText(
                document,
                $"{ticket.Reservation.User.FirstName} {ticket.Reservation.User.LastName}",
                xRight,
                50,
                90,
                8,
                font,
                darkBlue
            );
            AddRotatedText(document, ticket.TicketNumber, xRight + 12, 62, 90, 8, font, darkBlue);
            AddRotatedText(
                document,
                ticket.Reservation.Seat.SeatNumber,
                xRight + 24,
                56,
                90,
                8,
                font,
                darkBlue
            );
            AddRotatedText(
                document,
                ticket.Reservation.Event.Name,
                xRight + 36,
                56,
                90,
                8,
                font,
                darkBlue
            );
            AddRotatedText(
                document,
                ticket.Reservation.Venue.Name,
                xRight + 48,
                56,
                90,
                8,
                font,
                darkBlue
            );

            // Main details
            float eventNameWidth = boldFont.GetWidth(ticket.Reservation.Event.Name, 20);

            document.Add(
                new Paragraph(ticket.Reservation.Event.Name)
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetFontColor(darkBlue)
                    .SetFixedPosition(
                        pageSize.GetWidth() / 2 - 20 - eventNameWidth,
                        pageSize.GetHeight() / 2 + 20,
                        200
                    )
            );

            float venueNameWidth = font.GetWidth(ticket.Reservation.Venue.Name, 12);

            document.Add(
                new Paragraph(ticket.Reservation.Venue.Name)
                    .SetFont(font)
                    .SetFontSize(12)
                    .SetFontColor(yellow)
                    .SetFixedPosition(pageSize.GetWidth() - 110 - venueNameWidth, 20, 200)
            );

            float ticketNumberWidth = boldFont.GetWidth(ticket.TicketNumber, 16);

            document.Add(
                new Paragraph(ticket.TicketNumber)
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetFontColor(yellow)
                    .SetFixedPosition(
                        pageSize.GetWidth() - 110 - ticketNumberWidth,
                        pageSize.GetHeight() - 44,
                        200
                    )
            );

            // User details
            float xName = 195;
            float yName = 84;
            document.Add(
                new Paragraph(
                    $"{ticket.Reservation.User.FirstName} {ticket.Reservation.User.LastName}"
                )
                    .SetFont(font)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetFixedPosition(xName, yName, 200)
            );

            document.Add(
                new Paragraph(ticket.Reservation.Seat.SeatNumber)
                    .SetFont(font)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetFixedPosition(xName, yName - 17, 200)
            );

            string eventDate =
                $"{ticket.Reservation.Event.StartAt:MMM dd, yyyy} - {ticket.Reservation.Event.EndAt:MMM dd, yyyy}";
            document.Add(
                new Paragraph(eventDate)
                    .SetFont(font)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetFixedPosition(xName, yName - 17 * 2, 200)
            );

            string eventTime =
                $"{ticket.Reservation.Event.StartAt:hh:mm tt} - {ticket.Reservation.Event.EndAt:hh:mm tt}";
            document.Add(
                new Paragraph(eventTime)
                    .SetFont(font)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetFixedPosition(xName, yName - 17 * 3, 200)
            );

            // QR Code
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(ticket.TicketNumber, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(
                20,
                darkColor: Color.FromArgb(255, 255, 255),
                lightColor: Color.FromArgb(0, 0, 0, 0)
            );

            var qrImage = ImageDataFactory.Create(qrBytes);
            document.Add(
                new iText.Layout.Element.Image(qrImage)
                    .SetFixedPosition(8, 16)
                    .ScaleAbsolute(100, 100)
            );
        }
    }

    public string GeneratePdfBatch(List<Ticket> tickets)
    {
        const float shortBondWidth = 612; // 8.5 inches in points
        const float shortBondHeight = 792; // 11 inches in points
        const int ticketsPerPage = 4;

        string tempFolder = Path.Combine(Path.GetTempPath(), "TempTickets");
        string batchedFolder = Path.Combine(Path.GetTempPath(), "BatchedTickets");
        string batchOutput =
            "/home/gihyun/Development/svelte/heronest/static/storage/tickets/batches";
        string zipName = $"Tickets - {tickets[0].Reservation.Event.Name}.zip";
        string zipOutputPath = Path.Combine(batchOutput, zipName);

        if (!Directory.Exists(batchOutput))
        {
            Directory.CreateDirectory(batchOutput);
        }
        Directory.CreateDirectory(tempFolder);
        Directory.CreateDirectory(batchedFolder);

        // Generate individual ticket PDFs
        foreach (var ticket in tickets)
        {
            string ticketPath = Path.Combine(tempFolder, $"Ticket-{ticket.TicketNumber}.pdf");
            this.GeneratePdf(ticket, outputPath: ticketPath);
        }

        // Combine tickets into batches
        var ticketFiles = Directory.GetFiles(tempFolder, "*.pdf");
        int batchIndex = 0;

        for (int i = 0; i < ticketFiles.Length; i += ticketsPerPage)
        {
            string batchPdfPath = Path.Combine(batchedFolder, $"Batch-{batchIndex + 1}.pdf");

            using (PdfWriter writer = new PdfWriter(batchPdfPath))
            using (PdfDocument batchDoc = new PdfDocument(writer))
            {
                batchDoc.AddNewPage(new PageSize(shortBondWidth, shortBondHeight));

                for (int j = 0; j < ticketsPerPage && i + j < ticketFiles.Length; j++)
                {
                    string ticketFile = ticketFiles[i + j];

                    using (PdfReader ticketReader = new PdfReader(ticketFile))
                    using (PdfDocument ticketDoc = new PdfDocument(ticketReader))
                    {
                        var ticketPage = ticketDoc.GetFirstPage();

                        float ticketHeight = shortBondHeight / ticketsPerPage;
                        float yOffset = shortBondHeight - ((j + 1) * ticketHeight);

                        var pdfCanvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(
                            batchDoc.GetFirstPage()
                        );
                        pdfCanvas.AddXObjectAt(ticketPage.CopyAsFormXObject(batchDoc), 0, yOffset);
                    }
                }
            }

            batchIndex++;
        }

        // Create zip file
        string zipFilePath = zipOutputPath;
        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }
        ZipFile.CreateFromDirectory(batchedFolder, zipFilePath);

        // Clean up temporary files
        Directory.Delete(tempFolder, true);
        Directory.Delete(batchedFolder, true);

        return zipName;
    }

    private static void AddRotatedText(
        Document document,
        string text,
        float x,
        float y,
        float angle,
        float fontSize,
        PdfFont font,
        DeviceRgb color
    )
    {
        Paragraph paragraph = new Paragraph(text)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetFontColor(color)
            .SetRotationAngle(Math.PI * angle / 180);
        document.Add(paragraph.SetFixedPosition(x, y, 200));
    }

    private static void AddTicketDetails(
        Document document,
        Ticket ticket,
        float xOffset,
        float yOffset,
        PdfFont font,
        PdfFont boldFont,
        DeviceRgb darkBlue,
        DeviceRgb yellow
    )
    {
        float nameY = yOffset + 350;
        document.Add(
            new Paragraph($"{ticket.Reservation.User.FirstName} {ticket.Reservation.User.LastName}")
                .SetFont(font)
                .SetFontSize(12)
                .SetFontColor(ColorConstants.BLACK)
                .SetFixedPosition(xOffset + 10, nameY, 200)
        );

        document.Add(
            new Paragraph(ticket.Reservation.Seat.SeatNumber)
                .SetFont(font)
                .SetFontSize(12)
                .SetFontColor(ColorConstants.BLACK)
                .SetFixedPosition(xOffset + 10, nameY - 15, 200)
        );

        // Add other details such as event name, dates, and times as needed, similar to above.

        // Generate and add QR Code
        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(ticket.TicketNumber, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        byte[] qrBytes = qrCode.GetGraphic(
            20,
            darkColor: Color.FromArgb(255, 255, 255),
            lightColor: Color.FromArgb(0, 0, 0, 0)
        );

        var qrImage = ImageDataFactory.Create(qrBytes);
        document.Add(
            new iText.Layout.Element.Image(qrImage)
                .SetFixedPosition(xOffset + 200, yOffset + 300)
                .ScaleAbsolute(80, 80)
        );
    }

    private static void AddTemplatePage(
        PdfDocument targetPdf,
        PdfDocument templateDoc,
        float xOffset,
        float yOffset
    )
    {
        // Import the page from the template PDF into the target PDF
        var templatePage = templateDoc.GetFirstPage();
        var importedPage = targetPdf.AddPage(templatePage.CopyTo(targetPdf));

        // Adjust the position using a canvas overlay
        var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(importedPage);
        canvas.SaveState();
        canvas.ConcatMatrix(1, 0, 0, 1, xOffset, yOffset);
        canvas.RestoreState();
    }
}
