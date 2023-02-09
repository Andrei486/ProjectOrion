using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System.Text.RegularExpressions;
using System.Diagnostics;
using ShipSheets;

namespace ShipSheets
{
    public class ShipSheetCreator
    {
        private Ship Ship;
        private const double VERTICAL_MARGINS = 20;
        private const double HORIZONTAL_MARGINS = 20;
        private const double VERTICAL_TABLE_PADDING = 4;
        private const double HORIZONTAL_TABLE_PADDING = 4;
        private const double SECTION_SPACING = 6;
        private const string DAMAGE_BUBBLE = "[_]";

        private struct FontPreset
        {
            public XFont Font;
            public XBrush Brush;

            public FontPreset(XFont font, XBrush brush)
            {
                Font = font;
                Brush = brush;
            }

            public FontPreset(XFont font) : this(font, XBrushes.Black) { }
        }

        private static Dictionary<string, FontPreset> fontPresets = new Dictionary<string, FontPreset>()
        {
            { "Heading 2", new FontPreset(new XFont("Arial", 14, XFontStyle.Bold), XBrushes.Navy) },
            { "Heading 3", new FontPreset(new XFont("Arial", 11, XFontStyle.Bold), XBrushes.Navy) },
            { "Mono Heading 3", new FontPreset(new XFont("Consolas", 11, XFontStyle.Bold)) },
            { "Paragraph", new FontPreset(new XFont("Arial", 9)) },
            { "Mono", new FontPreset(new XFont("Consolas", 8)) },
            { "Stat Box", new FontPreset(new XFont("Consolas", 11)) }
        };

        public ShipSheetCreator(Ship ship)
        {
            Ship = ship;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private FontPreset GetFontPreset(string name)
        {
            return fontPresets[name];
        }

        private XPoint AddStats(XGraphics gfx, double x, double y)
        {
            ShipStat[][] statColumns = new ShipStat[][]
            {
                new ShipStat[] {ShipStat.HP, ShipStat.Shields, ShipStat.Reactor, ShipStat.Ammo, ShipStat.Restores},
                new ShipStat[] {ShipStat.Evasion, ShipStat.Armour, ShipStat.Speed, ShipStat.Sensors, ShipStat.Signature}
            };
            var endX = gfx.PageSize.Width - HORIZONTAL_MARGINS;
            var columnWidth = (endX - x - SECTION_SPACING * 2 * (statColumns.Length - 1)) / statColumns.Length;
            var statBoxHeight = gfx.MeasureString("Test", GetFontPreset("Stat Box").Font).Height * 1.5 + 2 * VERTICAL_TABLE_PADDING;
            var currentX = x;
            var currentY = y;
            XPoint point;
            for (int i = 0; i < statColumns.Length; i++)
            {
                currentY = y;
                var stats = statColumns[i];
                for (int j = 0; j < stats.Length; j++)
                {
                    point = AddStatBox(gfx,
                        new XRect(currentX, currentY, columnWidth, statBoxHeight),
                        stats[j]);
                    currentY = point.Y;
                }
                currentX += columnWidth + 2 * SECTION_SPACING;
            }
            return new XPoint(x, currentY);
        }

        private XPoint AddStatBox(XGraphics gfx, XRect area, ShipStat stat)
        {
            var topLeftStatBox = area.TopRight;
            topLeftStatBox.Offset(-area.Height * 3, 0);
            var statBoxArea = new XRect(topLeftStatBox, area.BottomRight);
            var statNameArea = new XRect(area.TopLeft, statBoxArea.BottomLeft);
            var format = new XStringFormat()
            {
                LineAlignment = XLineAlignment.Center
            };
            string displayString;
            if (stat.IsGauge())
            {
                format.Alignment = XStringAlignment.Far;
                displayString = string.Format("/ {0}", Ship.GetStat(stat));
            }
            else
            {
                format.Alignment = XStringAlignment.Center;
                displayString = string.Format("{0} (+  )", Ship.GetStat(stat));
            }
            var fontPreset = GetFontPreset("Stat Box");
            gfx.DrawRectangle(XPens.Black, statBoxArea);
            statBoxArea.Inflate(-HORIZONTAL_TABLE_PADDING, 0);
            gfx.DrawString(displayString, fontPreset.Font, fontPreset.Brush, statBoxArea, format);
            fontPreset = GetFontPreset("Heading 3");
            format = new XStringFormat()
            {
                Alignment = XStringAlignment.Near,
                LineAlignment = XLineAlignment.Center
            };
            gfx.DrawString(stat.GetDisplayString(), fontPreset.Font, fontPreset.Brush, statNameArea, format);
            return area.BottomLeft;
        }

        private XPoint AddTraits(XGraphics gfx, double x, double y)
        {
            var fontPreset = GetFontPreset("Mono");
            var headerFontPreset = GetFontPreset("Mono Heading 3");
            var columns = new List<Func<KeyValuePair<string, string>, string>>()
            {
                pair => pair.Key,
                pair => pair.Value
            };

            return AddTable(gfx, x, y, Ship.Traits, columns, fontPreset, headerFontPreset, new string[] { "NAME", "DESCRIPTION" }, multilineLast: true);
        }

        private XPoint AddSystems(XGraphics gfx, double x, double y)
        {
            ShipSystem[][] systemColumns = new ShipSystem[][]
            {
                (from system in Ship.Systems where system.Slots == 0 select system).ToArray(),
                (from system in Ship.Systems where system.Slots > 0 select system).Concat(Enumerable.Repeat(ShipSystem.Filler(), Ship.GetFreeSystemSlots())).ToArray()
            };
            string[] headers = new string[]
            {
                "SYSTEMS - CORE",
                "SYSTEMS - SLOTS"
            };
            var columnMap = new List<Func<ShipSystem, string>>()
            {
                system => system.Name,
                system => system.BubbleText == null ? (system.HitPoints > 0 ? string.Join(" ", Enumerable.Repeat(DAMAGE_BUBBLE, system.HitPoints)) : "") 
                    : string.Join(" ", from text in system.BubbleText select string.Format("[{0}]", text)),
                system => system.Description
            };
            var endX = gfx.PageSize.Width - HORIZONTAL_MARGINS;
            var currentX = x;
            var currentY = y;
            var maxY = y;
            var columnWidth = (endX - x - SECTION_SPACING * 2 * (systemColumns.Length - 1)) / systemColumns.Length;
            XPoint point;
            double tableStartY = y;
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                point = AddHeading(gfx, currentX, currentY, header,
                    GetFontPreset("Heading 2"),
                    new XStringFormat
                    {
                        Alignment = XStringAlignment.Near,
                        LineAlignment = XLineAlignment.Center
                    },
                    endX: currentX + columnWidth
                );
                currentX += columnWidth + 2 * SECTION_SPACING;
                tableStartY = point.Y + SECTION_SPACING;
            }

            currentX = x;
            for (int i = 0; i < systemColumns.Length; i++)
            {
                currentY = tableStartY;
                point = AddTable(gfx, currentX, currentY, systemColumns[i], columnMap, GetFontPreset("Mono"), GetFontPreset("Mono Heading 3"),
                    headers: new string[] {"NAME", "DMG", "DESCRIPTION"}, drawLines: true, multilineLast: true, endX: currentX + columnWidth);
                currentY = point.Y;
                currentX += columnWidth + 2 * SECTION_SPACING;
                maxY = Math.Max(maxY, currentY);
            }
            return new XPoint(x, maxY);
        }

        private XPoint AddMounts(XGraphics gfx, double x, double y)
        {
            var columns = new List<Func<Mount, string>>
            {
                mount => mount.Weapon == null ? string.Format("{0} (S{1})", DAMAGE_BUBBLE, mount.Size) : string.Format("{0} {1}", DAMAGE_BUBBLE, mount.Weapon.Name),
                mount => mount.IsSpinal ? string.Format("{0}S", mount.MainArc.GetDisplayString())
                    : string.Format("{0}{1}", mount.MainArc.GetDisplayString(), mount.Type.GetDisplayString()),
                mount => mount.Weapon == null ? "" : mount.Weapon.Range.ToString(),
                mount => mount.Weapon == null ? "" : mount.Weapon.AmmoCost.ToString(),
                mount => mount.Weapon == null ? "" : mount.Weapon.PowerCost.ToString(),
                mount => mount.Weapon == null ? string.Format(" (x{0})", mount.Count) : MultiplyDice(mount.Weapon.GetShots(), mount.Count),
                mount => mount.Weapon == null ? "" : mount.Weapon.ArmorPenetration.ToString(),
                mount => mount.Weapon == null ? "" : mount.Weapon.Damage,
                mount => mount.Weapon == null ? " " : string.Join(", ", mount.Weapon.Tags)
            };

            return AddTable(gfx, x, y, Ship.Mounts, columns, GetFontPreset("Mono"), GetFontPreset("Mono Heading 3"), headers: new string[] {
                "WEAPON NAME", "POS", "RANGE", "AMMO", "PW", "SHOTS", "AP", "DMG", "TAGS"}, drawLines: true, multilineLast: true);
        }

        private XPoint AddBays(XGraphics gfx, double x, double y)
        {
            var columns = new List<Func<Bay, string>>
            {
                bay => bay.Craft == null ? string.Format("{0} (S{1})", DAMAGE_BUBBLE, bay.Size) : string.Format("{0} {1}", DAMAGE_BUBBLE, bay.Craft.Name),
                bay => string.Join("", from arc in bay.Arcs select arc.GetDisplayString()),
                bay => bay.Craft == null ? "" : bay.Craft.GetStat(ShipStat.Speed).ToString(),
                bay => bay.Craft == null ? "" : bay.Craft.AmmoCost.ToString(),
                bay => bay.Craft == null ? "" : bay.Craft.PowerCost.ToString(),
                bay => bay.Craft == null ? string.Format(" (x{0})", bay.Count) : MultiplyDice(bay.Craft.GetSwarm(), bay.Count),
                bay => bay.Craft == null ? "" : bay.Craft.GetArmorPenetration().ToString(),
                bay => bay.Craft == null ? "" : bay.Craft.GetDamage() == null ? "" : bay.Craft.GetDamage().ToString(),
                bay => bay.Craft == null ? " " : string.Join(", ", bay.Craft.Tags),
            };

            return AddTable(gfx, x, y, Ship.Bays, columns, GetFontPreset("Mono"), GetFontPreset("Mono Heading 3"), headers: new string[] {
                "PAYLOAD NAME", "POS", "SPEED", "AMMO", "PW", "SWARM", "AP", "DMG", "TAGS"}, drawLines: true, multilineLast: true);
        }

        private double GetColumnWidth(XGraphics gfx, IEnumerable<string> data, FontPreset fontPreset, FontPreset headerFontPreset)
        {
            var font = fontPreset.Font;
            var headerFont = headerFontPreset.Font;
            double maxWidth = (from str in data select gfx.MeasureString(str, font).Width).Max();
            maxWidth = Math.Max(maxWidth, gfx.MeasureString(data.First(), headerFont).Width);
            maxWidth += 2 * HORIZONTAL_TABLE_PADDING;
            return maxWidth;
        }

        private double[] GetColumnHeights(XGraphics gfx, IEnumerable<string> data, FontPreset fontPreset, double maxColumnWidth)
        {
            List<double> heights = new List<double>();
            var tf = new XTextFormatter(gfx);
            double totalLength;
            XSize textSize;
            foreach (string text in data)
            {
                textSize = gfx.MeasureString(text, fontPreset.Font);
                totalLength = textSize.Width * 1.2;
                int lineCount = (int)Math.Max(Math.Ceiling(totalLength / maxColumnWidth), 1);
                heights.Add(textSize.Height * lineCount + 2 * VERTICAL_TABLE_PADDING);
            }
            return heights.ToArray();
        }

        /// <summary>
        /// Draws a table column
        /// </summary>
        /// <param name="page">Document page to draw on.</param>
        /// <param name="x">x-value for the upper-left-hand corner of the column.</param>
        /// <param name="y">y-value for the upper-left-hand corner of the column.</param>
        /// <param name="data">Strings to display in the column. Includes the header, if any.</param>
        /// <param name="fontPreset">Font to use when displaying non-header strings.</param>
        /// <param name="headerFontPreset">Font to use when displaying header strings. If null, use same font as non-header rows.</param>
        /// <param name="drawLines">True if line separators should be drawn between rows, or false if not.</param>
        /// <returns>Maximum length of the table column.</returns>
        private XPoint AddTableColumn(XGraphics gfx, double x, double y, IEnumerable<string> data, FontPreset fontPreset, FontPreset headerFontPreset,
            bool drawLines = true, double[] heights = null, bool useRemainingWidth = false, double endX=-1)
        {
            var font = fontPreset.Font;
            var headerFont = headerFontPreset.Font;
            var tf = new XTextFormatter(gfx);
            if (endX == -1) endX = gfx.PageSize.Width - HORIZONTAL_MARGINS;
            double maxLength;
            if (useRemainingWidth)
            {
                maxLength = endX - x;
            }
            else
            {
                maxLength = (from str in data select gfx.MeasureString(str, font).Width).Max();
                maxLength = Math.Max(maxLength, gfx.MeasureString(data.First(), headerFont).Width) + 2 * HORIZONTAL_TABLE_PADDING;
            }
            var currentY = y;
            var rightX = x + maxLength;
            bool isHeader = true;

            int i = 0;
            foreach (string datum in data)
            {
                var currentFont = isHeader ? headerFont : font;
                var currentBrush = isHeader ? headerFontPreset.Brush : fontPreset.Brush;
                var height = heights == null ? gfx.MeasureString(datum, currentFont).Height + 2 * VERTICAL_TABLE_PADDING : heights[i++];
                var rect = new XRect(x, currentY, maxLength, height);
                XStringFormat format = new XStringFormat()
                {
                    Alignment = XStringAlignment.Center,
                    LineAlignment = XLineAlignment.Center
                };
                if (useRemainingWidth && heights != null && !isHeader)
                {
                    rect.Inflate(-HORIZONTAL_TABLE_PADDING, -VERTICAL_TABLE_PADDING);
                    tf.DrawString(datum, currentFont, currentBrush, rect);
                }
                else
                {
                    gfx.DrawString(datum, currentFont, currentBrush, rect, format);
                }

                if (drawLines) gfx.DrawLine(XPens.Black, x, currentY, rightX, currentY);

                isHeader = false;
                currentY += height;
                if (drawLines) gfx.DrawLine(XPens.Black, x, currentY, rightX, currentY);
            }

            //draw left and right column separators
            if (drawLines)
            {
                gfx.DrawLine(XPens.Black, x, y, x, currentY);
                gfx.DrawLine(XPens.Black, rightX, y, rightX, currentY);
            }

            return new XPoint(rightX, currentY);
        }

        private XPoint AddTable<T>(XGraphics gfx, double x, double y, IEnumerable<T> objects, IEnumerable<Func<T, string>> columns,
            FontPreset fontPreset, FontPreset headerFontPreset, string[] headers = null, bool drawLines = true, bool multilineLast = false, double endX=-1)
        {
            if (headers != null && headers.Length != columns.Count())
            {
                throw new ArgumentException("Length of headers must be the same as length of columns for a table!");
            }
            if (endX == -1) endX = gfx.PageSize.Width - HORIZONTAL_MARGINS;
            var currentX = x;
            XPoint bottomRight = new XPoint();
            int i = 0;
            var remainingWidth = endX - x;
            string header = null;
            double[] rowHeights = null;

            if (multilineLast)
            {
                foreach (Func<T, string> columnMap in columns)
                {
                    IEnumerable<string> data = from obj in objects select columnMap(obj);
                    if (headers != null)
                    {
                        header = headers[i++];
                        data = data.Prepend(header);
                    }
                    if (i < columns.Count())
                    {
                        remainingWidth -= GetColumnWidth(gfx, data, fontPreset, headerFontPreset);
                    }
                }
                var columnData = (from obj in objects select columns.Last()(obj));
                if (headers != null)
                {
                    columnData = columnData.Prepend(headers[headers.Length - 1]);
                }
                rowHeights = GetColumnHeights(gfx, columnData, fontPreset, remainingWidth);
            }

            i = 0;
            foreach (Func<T, string> columnMap in columns)
            {
                IEnumerable<string> data = from obj in objects select columnMap(obj);
                if (headers != null) data = data.Prepend(headers[i++]);

                if (multilineLast)
                {
                    bottomRight = AddTableColumn(gfx, currentX, y, data, fontPreset, headerFontPreset, drawLines, rowHeights, useRemainingWidth: i == columns.Count(), endX: endX);
                }
                else
                {
                    bottomRight = AddTableColumn(gfx, currentX, y, data, fontPreset, headerFontPreset, drawLines, useRemainingWidth: i == headers.Length);
                }

                currentX = bottomRight.X;
            }
            return new XPoint(x, bottomRight.Y);
        }

        private string MultiplyDice(string dice, int constant)
        {
            int result = 0;
            bool isDigit = int.TryParse(dice, out result);
            if (isDigit)
            {
                return (result * constant).ToString();
            }
            string pattern = "(\\d+)d(\\d+)(?:\\+(\\d+))?";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(dice);
            if (!m.Success)
            {
                throw new ArgumentException("String " + dice + " does not represent a die roll!");
            }
            int diceCount = int.Parse(m.Groups[1].Value) * constant;
            int dieSize = int.Parse(m.Groups[2].Value);
            string calculatedString = string.Format("{0}d{1}", diceCount, dieSize);
            if (m.Groups.Count > 3 && m.Groups[3].Value != "")
            {
                int addedValue = int.Parse(m.Groups[3].Value) * constant;
                calculatedString += string.Format("+{0}", addedValue);
            }
            return calculatedString;
        }

        private XPoint AddHeading(XGraphics gfx, double x, double y, string text, FontPreset preset, XStringFormat format, double endX = -1)
        {
            var rect = new XRect(x, y, endX > 0 ? endX : gfx.PageSize.Width - HORIZONTAL_MARGINS, gfx.MeasureString(text, preset.Font, format).Height);
            gfx.DrawString(text, preset.Font, preset.Brush, rect, format);
            return new XPoint(x + rect.Width, y + rect.Height);
        }

        public void CreateSheet(string outputPath, bool openFile = false)
        {
            var document = new PdfDocument();

            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double currentX = HORIZONTAL_MARGINS;
            double currentY = VERTICAL_MARGINS;

            //draw title
            var point = AddHeading(gfx, currentX, currentY,
                (Ship.Identifier != null ? string.Format("{0}: {1}", Ship.Identifier, Ship.Name) : Ship.Name).ToUpper(),
                GetFontPreset("Heading 2"),
                new XStringFormat
                {
                    Alignment = XStringAlignment.Center,
                    LineAlignment = XLineAlignment.Center
                }
            );
            currentY = point.Y + SECTION_SPACING;

            //draw stats
            point = AddStats(gfx, currentX, currentY);
            currentY = point.Y + SECTION_SPACING;

            //draw traits
            point = AddHeading(gfx, currentX, currentY,
                "TRAITS",
                GetFontPreset("Heading 2"),
                new XStringFormat
                {
                    Alignment = XStringAlignment.Near,
                    LineAlignment = XLineAlignment.Center
                }
            );
            currentY = point.Y + SECTION_SPACING;
            point = AddTraits(gfx, currentX, currentY);
            currentY = point.Y + SECTION_SPACING;

            point = AddSystems(gfx, currentX, currentY);
            currentY = point.Y + SECTION_SPACING;

            //draw mount table heading & table
            point = AddHeading(gfx, currentX, currentY,
                "MOUNTS",
                GetFontPreset("Heading 2"),
                new XStringFormat
                {
                    Alignment = XStringAlignment.Near,
                    LineAlignment = XLineAlignment.Center
                }
            );
            currentY = point.Y + SECTION_SPACING;
            point = AddMounts(gfx, currentX, currentY);
            currentY = point.Y + SECTION_SPACING;

            //draw bay table heading & table
            point = AddHeading(gfx, currentX, currentY,
                "BAYS",
                GetFontPreset("Heading 2"),
                new XStringFormat
                {
                    Alignment = XStringAlignment.Near,
                    LineAlignment = XLineAlignment.Center
                }
            );
            currentY = point.Y + SECTION_SPACING;
            point = AddBays(gfx, currentX, currentY);
            currentY = point.Y + SECTION_SPACING;
            document.Save(outputPath);
            if (openFile) Process.Start(new ProcessStartInfo()
            {
                FileName = outputPath,
                UseShellExecute = true
            });
        }
    }
}
