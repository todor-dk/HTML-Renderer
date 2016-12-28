// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Parse;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Layout engine for tables executing the complex layout of tables with rows/columns/headers/etc.
    /// </summary>
    internal sealed class CssLayoutEngineTable
    {
        #region Fields and Consts

        /// <summary>
        /// the main box of the table
        /// </summary>
        private readonly CssBox TableBox;

        /// <summary>
        ///
        /// </summary>
        private CssBox Caption;

        private CssBox HeaderBox;

        private CssBox FooterBox;

        /// <summary>
        /// collection of all rows boxes
        /// </summary>
        private readonly List<CssBox> Bodyrows = new List<CssBox>();

        /// <summary>
        /// collection of all columns boxes
        /// </summary>
        private readonly List<CssBox> Columns = new List<CssBox>();

        /// <summary>
        ///
        /// </summary>
        private readonly List<CssBox> AllRows = new List<CssBox>();

        private int ColumnCount;

        private bool WidthSpecified;

        private double[] ColumnWidths;

        private double[] ColumnMinWidths;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="tableBox"></param>
        private CssLayoutEngineTable(CssBox tableBox)
        {
            this.TableBox = tableBox;
        }

        /// <summary>
        /// Get the table cells spacing for all the cells in the table.<br/>
        /// Used to calculate the spacing the table has in addition to regular padding and borders.
        /// </summary>
        /// <param name="tableBox">the table box to calculate the spacing for</param>
        /// <returns>the calculated spacing</returns>
        public static double GetTableSpacing(CssBox tableBox)
        {
            int count = 0;
            int columns = 0;
            foreach (var box in tableBox.Boxes)
            {
                if (box.Display == CssConstants.TableColumn)
                {
                    columns += GetSpan(box);
                }
                else if (box.Display == CssConstants.TableRowGroup)
                {
                    foreach (CssBox cr in tableBox.Boxes)
                    {
                        count++;
                        if (cr.Display == CssConstants.TableRow)
                            columns = Math.Max(columns, cr.Boxes.Count);
                    }
                }
                else if (box.Display == CssConstants.TableRow)
                {
                    count++;
                    columns = Math.Max(columns, box.Boxes.Count);
                }

                // limit the amount of rows to process for performance
                if (count > 30)
                    break;
            }

            // +1 columns because padding is between the cell and table borders
            return (columns + 1) * GetHorizontalSpacing(tableBox);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        /// <param name="tableBox"> </param>
        public static void PerformLayout(RGraphics g, CssBox tableBox)
        {
            ArgChecker.AssertArgNotNull(g, "g");
            ArgChecker.AssertArgNotNull(tableBox, "tableBox");

            try
            {
                var table = new CssLayoutEngineTable(tableBox);
                table.Layout(g);
            }
            catch (Exception ex)
            {
                tableBox.HtmlContainer.ReportError(HtmlRenderErrorType.Layout, "Failed table layout", ex);
            }
        }

        #region Private Methods

        /// <summary>
        /// Analyzes the Table and assigns values to this CssTable object.
        /// To be called from the constructor
        /// </summary>
        private void Layout(RGraphics g)
        {
            MeasureWords(this.TableBox, g);

            // get the table boxes into the proper fields
            this.AssignBoxKinds();

            // Insert EmptyBoxes for vertical cell spanning.
            this.InsertEmptyBoxes();

            // Determine Row and Column Count, and ColumnWidths
            var availCellSpace = this.CalculateCountAndWidth();

            this.DetermineMissingColumnWidths(availCellSpace);

            // Check for minimum sizes (increment widths if necessary)
            this.EnforceMinimumSize();

            // While table width is larger than it should, and width is reducible
            this.EnforceMaximumSize();

            // Ensure there's no padding
            this.TableBox.PaddingLeft = this.TableBox.PaddingTop = this.TableBox.PaddingRight = this.TableBox.PaddingBottom = "0";

            // Actually layout cells!
            this.LayoutCells(g);
        }

        /// <summary>
        /// Get the table boxes into the proper fields.
        /// </summary>
        private void AssignBoxKinds()
        {
            foreach (var box in this.TableBox.Boxes)
            {
                switch (box.Display)
                {
                    case CssConstants.TableCaption:
                        this.Caption = box;
                        break;
                    case CssConstants.TableRow:
                        this.Bodyrows.Add(box);
                        break;
                    case CssConstants.TableRowGroup:
                        foreach (CssBox childBox in box.Boxes)
                        {
                            if (childBox.Display == CssConstants.TableRow)
                                this.Bodyrows.Add(childBox);
                        }

                        break;
                    case CssConstants.TableHeaderGroup:
                        if (this.HeaderBox != null)
                            this.Bodyrows.Add(box);
                        else
                            this.HeaderBox = box;
                        break;
                    case CssConstants.TableFooterGroup:
                        if (this.FooterBox != null)
                            this.Bodyrows.Add(box);
                        else
                            this.FooterBox = box;
                        break;
                    case CssConstants.TableColumn:
                        for (int i = 0; i < GetSpan(box); i++)
                            this.Columns.Add(box);
                        break;
                    case CssConstants.TableColumnGroup:
                        if (box.Boxes.Count == 0)
                        {
                            int gspan = GetSpan(box);
                            for (int i = 0; i < gspan; i++)
                            {
                                this.Columns.Add(box);
                            }
                        }
                        else
                        {
                            foreach (CssBox bb in box.Boxes)
                            {
                                int bbspan = GetSpan(bb);
                                for (int i = 0; i < bbspan; i++)
                                {
                                    this.Columns.Add(bb);
                                }
                            }
                        }

                        break;
                }
            }

            if (this.HeaderBox != null)
                this.AllRows.AddRange(this.HeaderBox.Boxes);

            this.AllRows.AddRange(this.Bodyrows);

            if (this.FooterBox != null)
                this.AllRows.AddRange(this.FooterBox.Boxes);
        }

        /// <summary>
        /// Insert EmptyBoxes for vertical cell spanning.
        /// </summary>
        private void InsertEmptyBoxes()
        {
            if (!this.TableBox.TableFixed)
            {
                int currow = 0;
                List<CssBox> rows = this.Bodyrows;

                foreach (CssBox row in rows)
                {
                    for (int k = 0; k < row.Boxes.Count; k++)
                    {
                        CssBox cell = row.Boxes[k];
                        int rowspan = GetRowSpan(cell);
                        int realcol = GetCellRealColumnIndex(row, cell); // Real column of the cell

                        for (int i = currow + 1; i < currow + rowspan; i++)
                        {
                            if (rows.Count > i)
                            {
                                int colcount = 0;
                                for (int j = 0; j < rows[i].Boxes.Count; j++)
                                {
                                    if (colcount == realcol)
                                    {
                                        rows[i].Boxes.Insert(colcount, new CssSpacingBox(this.TableBox, ref cell, currow));
                                        break;
                                    }

                                    colcount++;
                                    realcol -= GetColSpan(rows[i].Boxes[j]) - 1;
                                }
                            }
                        }
                    }

                    currow++;
                }

                this.TableBox.TableFixed = true;
            }
        }

        /// <summary>
        /// Determine Row and Column Count, and ColumnWidths
        /// </summary>
        /// <returns></returns>
        private double CalculateCountAndWidth()
        {
            // Columns
            if (this.Columns.Count > 0)
            {
                this.ColumnCount = this.Columns.Count;
            }
            else
            {
                foreach (CssBox b in this.AllRows)
                    this.ColumnCount = Math.Max(this.ColumnCount, b.Boxes.Count);
            }

            // Initialize column widths array with NaNs
            this.ColumnWidths = new double[this.ColumnCount];
            for (int i = 0; i < this.ColumnWidths.Length; i++)
                this.ColumnWidths[i] = double.NaN;

            double availCellSpace = this.GetAvailableCellWidth();

            if (this.Columns.Count > 0)
            {
                // Fill ColumnWidths array by scanning column widths
                for (int i = 0; i < this.Columns.Count; i++)
                {
                    CssLength len = new CssLength(this.Columns[i].Width); // Get specified width

                    // If some width specified
                    if (len.Number > 0)
                    {
                        // Get width as a percentage
                        if (len.IsPercentage)
                        {
                            this.ColumnWidths[i] = CssValueParser.ParseNumber(this.Columns[i].Width, availCellSpace);
                        }
                        else if (len.Unit == CssUnit.Pixels || len.Unit == CssUnit.None)
                        {
                            this.ColumnWidths[i] = len.Number; // Get width as an absolute-pixel value
                        }
                    }
                }
            }
            else
            {
                // Fill ColumnWidths array by scanning width in table-cell definitions
                foreach (CssBox row in this.AllRows)
                {
                    // Check for column width in table-cell definitions
                    for (int i = 0; i < this.ColumnCount; i++)
                    {
                        // limit column width check
                        if (i < 20 || double.IsNaN(this.ColumnWidths[i]))
                        {
                            if (i < row.Boxes.Count && row.Boxes[i].Display == CssConstants.TableCell)
                            {
                                double len = CssValueParser.ParseLength(row.Boxes[i].Width, availCellSpace, row.Boxes[i]);

                                // If some width specified
                                if (len > 0)
                                {
                                    int colspan = GetColSpan(row.Boxes[i]);
                                    len /= Convert.ToSingle(colspan);
                                    for (int j = i; j < i + colspan; j++)
                                    {
                                        this.ColumnWidths[j] = double.IsNaN(this.ColumnWidths[j]) ? len : Math.Max(this.ColumnWidths[j], len);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return availCellSpace;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="availCellSpace"></param>
        private void DetermineMissingColumnWidths(double availCellSpace)
        {
            double occupedSpace = 0f;

            // If a width was specified,
            if (this.WidthSpecified)
            {
                // Assign NaNs equally with space left after gathering not-NaNs
                int numOfNans = 0;

                // Calculate number of NaNs and occupied space
                foreach (double colWidth in this.ColumnWidths)
                {
                    if (double.IsNaN(colWidth))
                        numOfNans++;
                    else
                        occupedSpace += colWidth;
                }

                var orgNumOfNans = numOfNans;

                double[] orgColWidths = null;
                if (numOfNans < this.ColumnWidths.Length)
                {
                    orgColWidths = new double[this.ColumnWidths.Length];
                    for (int i = 0; i < this.ColumnWidths.Length; i++)
                        orgColWidths[i] = this.ColumnWidths[i];
                }

                if (numOfNans > 0)
                {
                    // Determine the max width for each column
                    double[] minFullWidths, maxFullWidths;
                    this.GetColumnsMinMaxWidthByContent(true, out minFullWidths, out maxFullWidths);

                    // set the columns that can fulfill by the max width in a loop because it changes the nanWidth
                    int oldNumOfNans;
                    do
                    {
                        oldNumOfNans = numOfNans;

                        for (int i = 0; i < this.ColumnWidths.Length; i++)
                        {
                            var nanWidth = (availCellSpace - occupedSpace) / numOfNans;
                            if (double.IsNaN(this.ColumnWidths[i]) && nanWidth > maxFullWidths[i])
                            {
                                this.ColumnWidths[i] = maxFullWidths[i];
                                numOfNans--;
                                occupedSpace += maxFullWidths[i];
                            }
                        }
                    }
                    while (oldNumOfNans != numOfNans);

                    if (numOfNans > 0)
                    {
                        // Determine width that will be assigned to un assigned widths
                        double nanWidth = (availCellSpace - occupedSpace) / numOfNans;

                        for (int i = 0; i < this.ColumnWidths.Length; i++)
                        {
                            if (double.IsNaN(this.ColumnWidths[i]))
                                this.ColumnWidths[i] = nanWidth;
                        }
                    }
                }

                if (numOfNans == 0 && occupedSpace < availCellSpace)
                {
                    if (orgNumOfNans > 0)
                    {
                        // spread extra width between all non width specified columns
                        double extWidth = (availCellSpace - occupedSpace) / orgNumOfNans;
                        for (int i = 0; i < this.ColumnWidths.Length; i++)
                        {
                            if (orgColWidths == null || double.IsNaN(orgColWidths[i]))
                                this.ColumnWidths[i] += extWidth;
                        }
                    }
                    else
                    {
                        // spread extra width between all columns with respect to relative sizes
                        for (int i = 0; i < this.ColumnWidths.Length; i++)
                            this.ColumnWidths[i] += (availCellSpace - occupedSpace) * (this.ColumnWidths[i] / occupedSpace);
                    }
                }
            }
            else
            {
                // Get the minimum and maximum full length of NaN boxes
                double[] minFullWidths, maxFullWidths;
                this.GetColumnsMinMaxWidthByContent(true, out minFullWidths, out maxFullWidths);

                for (int i = 0; i < this.ColumnWidths.Length; i++)
                {
                    if (double.IsNaN(this.ColumnWidths[i]))
                        this.ColumnWidths[i] = minFullWidths[i];
                    occupedSpace += this.ColumnWidths[i];
                }

                // spread extra width between all columns
                for (int i = 0; i < this.ColumnWidths.Length; i++)
                {
                    if (maxFullWidths[i] > this.ColumnWidths[i])
                    {
                        var temp = this.ColumnWidths[i];
                        this.ColumnWidths[i] = Math.Min(this.ColumnWidths[i] + ((availCellSpace - occupedSpace) / Convert.ToSingle(this.ColumnWidths.Length - i)), maxFullWidths[i]);
                        occupedSpace = occupedSpace + this.ColumnWidths[i] - temp;
                    }
                }
            }
        }

        /// <summary>
        /// While table width is larger than it should, and width is reductable.<br/>
        /// If table max width is limited by we need to lower the columns width even if it will result in clipping<br/>
        /// </summary>
        private void EnforceMaximumSize()
        {
            int curCol = 0;
            var widthSum = this.GetWidthSum();
            while (widthSum > this.GetAvailableTableWidth() && this.CanReduceWidth())
            {
                while (!this.CanReduceWidth(curCol))
                    curCol++;

                this.ColumnWidths[curCol] -= 1f;

                curCol++;

                if (curCol >= this.ColumnWidths.Length)
                    curCol = 0;
            }

            // if table max width is limited by we need to lower the columns width even if it will result in clipping
            var maxWidth = this.GetMaxTableWidth();
            if (maxWidth < 90999)
            {
                widthSum = this.GetWidthSum();
                if (maxWidth < widthSum)
                {
                    // Get the minimum and maximum full length of NaN boxes
                    double[] minFullWidths, maxFullWidths;
                    this.GetColumnsMinMaxWidthByContent(false, out minFullWidths, out maxFullWidths);

                    // lower all the columns to the minimum
                    for (int i = 0; i < this.ColumnWidths.Length; i++)
                        this.ColumnWidths[i] = minFullWidths[i];

                    // either min for all column is not enought and we need to lower it more resulting in clipping
                    // or we now have extra space so we can give it to columns than need it
                    widthSum = this.GetWidthSum();
                    if (maxWidth < widthSum)
                    {
                        // lower the width of columns starting from the largest one until the max width is satisfied
                        // limit iteration so bug won't create infinite loop
                        for (int a = 0; a < 15 && maxWidth < widthSum - 0.1; a++)
                        {
                            int nonMaxedColumns = 0;
                            double largeWidth = 0f, secLargeWidth = 0f;
                            for (int i = 0; i < this.ColumnWidths.Length; i++)
                            {
                                if (this.ColumnWidths[i] > largeWidth + 0.1)
                                {
                                    secLargeWidth = largeWidth;
                                    largeWidth = this.ColumnWidths[i];
                                    nonMaxedColumns = 1;
                                }
                                else if (this.ColumnWidths[i] > largeWidth - 0.1)
                                {
                                    nonMaxedColumns++;
                                }
                            }

                            double decrease = secLargeWidth > 0 ? largeWidth - secLargeWidth : (widthSum - maxWidth) / this.ColumnWidths.Length;
                            if (decrease * nonMaxedColumns > widthSum - maxWidth)
                                decrease = (widthSum - maxWidth) / nonMaxedColumns;
                            for (int i = 0; i < this.ColumnWidths.Length; i++)
                            {
                                if (this.ColumnWidths[i] > largeWidth - 0.1)
                                    this.ColumnWidths[i] -= decrease;
                            }

                            widthSum = this.GetWidthSum();
                        }
                    }
                    else
                    {
                        // spread extra width to columns that didn't reached max width where trying to spread it between all columns
                        // limit iteration so bug won't create infinite loop
                        for (int a = 0; a < 15 && maxWidth > widthSum + 0.1; a++)
                        {
                            int nonMaxedColumns = 0;
                            for (int i = 0; i < this.ColumnWidths.Length; i++)
                            {
                                if (this.ColumnWidths[i] + 1 < maxFullWidths[i])
                                    nonMaxedColumns++;
                            }

                            if (nonMaxedColumns == 0)
                                nonMaxedColumns = this.ColumnWidths.Length;

                            bool hit = false;
                            double minIncrement = (maxWidth - widthSum) / nonMaxedColumns;
                            for (int i = 0; i < this.ColumnWidths.Length; i++)
                            {
                                if (this.ColumnWidths[i] + 0.1 < maxFullWidths[i])
                                {
                                    minIncrement = Math.Min(minIncrement, maxFullWidths[i] - this.ColumnWidths[i]);
                                    hit = true;
                                }
                            }

                            for (int i = 0; i < this.ColumnWidths.Length; i++)
                            {
                                if (!hit || this.ColumnWidths[i] + 1 < maxFullWidths[i])
                                    this.ColumnWidths[i] += minIncrement;
                            }

                            widthSum = this.GetWidthSum();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check for minimum sizes (increment widths if necessary)
        /// </summary>
        private void EnforceMinimumSize()
        {
            foreach (CssBox row in this.AllRows)
            {
                foreach (CssBox cell in row.Boxes)
                {
                    int colspan = GetColSpan(cell);
                    int col = GetCellRealColumnIndex(row, cell);
                    int affectcol = col + colspan - 1;

                    if (this.ColumnWidths.Length > col && this.ColumnWidths[col] < this.GetColumnMinWidths()[col])
                    {
                        double diff = this.GetColumnMinWidths()[col] - this.ColumnWidths[col];
                        this.ColumnWidths[affectcol] = this.GetColumnMinWidths()[affectcol];

                        if (col < this.ColumnWidths.Length - 1)
                        {
                            this.ColumnWidths[col + 1] -= diff;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Layout the cells by the calculated table layout
        /// </summary>
        /// <param name="g"></param>
        private void LayoutCells(RGraphics g)
        {
            double startx = Math.Max(this.TableBox.ClientLeft + this.GetHorizontalSpacing(), 0);
            double starty = Math.Max(this.TableBox.ClientTop + this.GetVerticalSpacing(), 0);
            double cury = starty;
            double maxRight = startx;
            double maxBottom = 0f;
            int currentrow = 0;

            // change start X by if the table should align to center or right
            if (this.TableBox.TextAlign == CssConstants.Center || this.TableBox.TextAlign == CssConstants.Right)
            {
                double maxRightCalc = this.GetWidthSum();
                startx = this.TableBox.TextAlign == CssConstants.Right
                    ? this.GetAvailableTableWidth() - maxRightCalc
                    : startx + ((this.GetAvailableTableWidth() - maxRightCalc) / 2);

                this.TableBox.Location = new RPoint(startx - this.TableBox.ActualBorderLeftWidth - this.TableBox.ActualPaddingLeft - this.GetHorizontalSpacing(), this.TableBox.Location.Y);
            }

            for (int i = 0; i < this.AllRows.Count; i++)
            {
                var row = this.AllRows[i];
                double curx = startx;
                int curCol = 0;
                bool breakPage = false;

                for (int j = 0; j < row.Boxes.Count; j++)
                {
                    CssBox cell = row.Boxes[j];
                    if (curCol >= this.ColumnWidths.Length)
                        break;

                    int rowspan = GetRowSpan(cell);
                    var columnIndex = GetCellRealColumnIndex(row, cell);
                    double width = this.GetCellWidth(columnIndex, cell);
                    cell.Location = new RPoint(curx, cury);
                    cell.Size = new RSize(width, 0f);
                    cell.PerformLayout(g); // That will automatically set the bottom of the cell

                    // Alter max bottom only if row is cell's row + cell's rowspan - 1
                    CssSpacingBox sb = cell as CssSpacingBox;
                    if (sb != null)
                    {
                        if (sb.EndRow == currentrow)
                        {
                            maxBottom = Math.Max(maxBottom, sb.ExtendedBox.ActualBottom);
                        }
                    }
                    else if (rowspan == 1)
                    {
                        maxBottom = Math.Max(maxBottom, cell.ActualBottom);
                    }

                    maxRight = Math.Max(maxRight, cell.ActualRight);
                    curCol++;
                    curx = cell.ActualRight + this.GetHorizontalSpacing();
                }

                foreach (CssBox cell in row.Boxes)
                {
                    CssSpacingBox spacer = cell as CssSpacingBox;

                    if (spacer == null && GetRowSpan(cell) == 1)
                    {
                        cell.ActualBottom = maxBottom;
                        CssLayoutEngine.ApplyCellVerticalAlignment(g, cell);
                    }
                    else if (spacer != null && spacer.EndRow == currentrow)
                    {
                        spacer.ExtendedBox.ActualBottom = maxBottom;
                        CssLayoutEngine.ApplyCellVerticalAlignment(g, spacer.ExtendedBox);
                    }

                    // If one cell crosses page borders then don't need to check other cells in the row
                    if (this.TableBox.PageBreakInside == CssConstants.Avoid)
                    {
                        breakPage = cell.BreakPage();
                        if (breakPage)
                        {
                            cury = cell.Location.Y;
                            break;
                        }
                    }
                }

                // go back to move the whole row to the next page
                if (breakPage)
                {
                    if (i == 1) // do not leave single row in previous page
                        i = -1; // Start layout from the first row on new page
                    else
                        i--;

                    maxBottom = 0;
                    continue;
                }

                cury = maxBottom + this.GetVerticalSpacing();

                currentrow++;
            }

            maxRight = Math.Max(maxRight, this.TableBox.Location.X + this.TableBox.ActualWidth);
            this.TableBox.ActualRight = maxRight + this.GetHorizontalSpacing() + this.TableBox.ActualBorderRightWidth;
            this.TableBox.ActualBottom = Math.Max(maxBottom, starty) + this.GetVerticalSpacing() + this.TableBox.ActualBorderBottomWidth;
        }

        /// <summary>
        /// Gets the spanned width of a cell (With of all columns it spans minus one).
        /// </summary>
        private double GetSpannedMinWidth(CssBox row, CssBox cell, int realcolindex, int colspan)
        {
            double w = 0f;
            for (int i = realcolindex; i < row.Boxes.Count || i < realcolindex + colspan - 1; i++)
            {
                if (i < this.GetColumnMinWidths().Length)
                    w += this.GetColumnMinWidths()[i];
            }

            return w;
        }

        /// <summary>
        /// Gets the cell column index checking its position and other cells colspans
        /// </summary>
        /// <param name="row"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        private static int GetCellRealColumnIndex(CssBox row, CssBox cell)
        {
            int i = 0;

            foreach (CssBox b in row.Boxes)
            {
                if (b.Equals(cell))
                    break;
                i += GetColSpan(b);
            }

            return i;
        }

        /// <summary>
        /// Gets the cells width, taking colspan and being in the specified column
        /// </summary>
        /// <param name="column"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private double GetCellWidth(int column, CssBox b)
        {
            double colspan = Convert.ToSingle(GetColSpan(b));
            double sum = 0f;

            for (int i = column; i < column + colspan; i++)
            {
                if (column >= this.ColumnWidths.Length)
                    break;
                if (this.ColumnWidths.Length <= i)
                    break;
                sum += this.ColumnWidths[i];
            }

            sum += (colspan - 1) * this.GetHorizontalSpacing();

            return sum; // -b.ActualBorderLeftWidth - b.ActualBorderRightWidth - b.ActualPaddingRight - b.ActualPaddingLeft;
        }

        /// <summary>
        /// Gets the colspan of the specified box
        /// </summary>
        /// <param name="b"></param>
        private static int GetColSpan(CssBox b)
        {
            string att = b.GetAttribute("colspan", "1");
            int colspan;

            if (!int.TryParse(att, out colspan))
            {
                return 1;
            }

            return colspan;
        }

        /// <summary>
        /// Gets the rowspan of the specified box
        /// </summary>
        /// <param name="b"></param>
        private static int GetRowSpan(CssBox b)
        {
            string att = b.GetAttribute("rowspan", "1");
            int rowspan;

            if (!int.TryParse(att, out rowspan))
            {
                return 1;
            }

            return rowspan;
        }

        /// <summary>
        /// Recursively measures words inside the box
        /// </summary>
        /// <param name="box">the box to measure</param>
        /// <param name="g">Device to use</param>
        private static void MeasureWords(CssBox box, RGraphics g)
        {
            if (box != null)
            {
                foreach (var childBox in box.Boxes)
                {
                    childBox.MeasureWordsSize(g);
                    MeasureWords(childBox, g);
                }
            }
        }

        /// <summary>
        /// Tells if the columns widths can be reduced,
        /// by checking the minimum widths of all cells
        /// </summary>
        /// <returns></returns>
        private bool CanReduceWidth()
        {
            for (int i = 0; i < this.ColumnWidths.Length; i++)
            {
                if (this.CanReduceWidth(i))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tells if the specified column can be reduced,
        /// by checking its minimum width
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        private bool CanReduceWidth(int columnIndex)
        {
            if (this.ColumnWidths.Length >= columnIndex || this.GetColumnMinWidths().Length >= columnIndex)
                return false;
            return this.ColumnWidths[columnIndex] > this.GetColumnMinWidths()[columnIndex];
        }

        /// <summary>
        /// Gets the available width for the whole table.
        /// It also sets the value of WidthSpecified
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The table's width can be larger than the result of this method, because of the minimum
        /// size that individual boxes.
        /// </remarks>
        private double GetAvailableTableWidth()
        {
            CssLength tblen = new CssLength(this.TableBox.Width);

            if (tblen.Number > 0)
            {
                this.WidthSpecified = true;
                return CssValueParser.ParseLength(this.TableBox.Width, this.TableBox.ParentBox.AvailableWidth, this.TableBox);
            }
            else
            {
                return this.TableBox.ParentBox.AvailableWidth;
            }
        }

        /// <summary>
        /// Gets the available width for the whole table.
        /// It also sets the value of WidthSpecified
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The table's width can be larger than the result of this method, because of the minimum
        /// size that individual boxes.
        /// </remarks>
        private double GetMaxTableWidth()
        {
            var tblen = new CssLength(this.TableBox.MaxWidth);
            if (tblen.Number > 0)
            {
                this.WidthSpecified = true;
                return CssValueParser.ParseLength(this.TableBox.MaxWidth, this.TableBox.ParentBox.AvailableWidth, this.TableBox);
            }
            else
            {
                return 9999f;
            }
        }

        /// <summary>
        /// Calculate the min and max width for each column of the table by the content in all rows.<br/>
        /// the min width possible without clipping content<br/>
        /// the max width the cell content can take without wrapping<br/>
        /// </summary>
        /// <param name="onlyNans">if to measure only columns that have no calculated width</param>
        /// <param name="minFullWidths">return the min width for each column - the min width possible without clipping content</param>
        /// <param name="maxFullWidths">return the max width for each column - the max width the cell content can take without wrapping</param>
        private void GetColumnsMinMaxWidthByContent(bool onlyNans, out double[] minFullWidths, out double[] maxFullWidths)
        {
            maxFullWidths = new double[this.ColumnWidths.Length];
            minFullWidths = new double[this.ColumnWidths.Length];

            foreach (CssBox row in this.AllRows)
            {
                for (int i = 0; i < row.Boxes.Count; i++)
                {
                    int col = GetCellRealColumnIndex(row, row.Boxes[i]);
                    col = this.ColumnWidths.Length > col ? col : this.ColumnWidths.Length - 1;

                    if ((!onlyNans || double.IsNaN(this.ColumnWidths[col])) && i < row.Boxes.Count)
                    {
                        double minWidth, maxWidth;
                        row.Boxes[i].GetMinMaxWidth(out minWidth, out maxWidth);

                        var colSpan = GetColSpan(row.Boxes[i]);
                        minWidth = minWidth / colSpan;
                        maxWidth = maxWidth / colSpan;
                        for (int j = 0; j < colSpan; j++)
                        {
                            minFullWidths[col + j] = Math.Max(minFullWidths[col + j], minWidth);
                            maxFullWidths[col + j] = Math.Max(maxFullWidths[col + j], maxWidth);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the width available for cells
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It takes away the cell-spacing from <see cref="GetAvailableTableWidth"/>
        /// </remarks>
        private double GetAvailableCellWidth()
        {
            return this.GetAvailableTableWidth() - (this.GetHorizontalSpacing() * (this.ColumnCount + 1)) - this.TableBox.ActualBorderLeftWidth - this.TableBox.ActualBorderRightWidth;
        }

        /// <summary>
        /// Gets the current sum of column widths
        /// </summary>
        /// <returns></returns>
        private double GetWidthSum()
        {
            double f = 0f;

            foreach (double t in this.ColumnWidths)
            {
                if (double.IsNaN(t))
                    throw new Exception("CssTable Algorithm error: There's a NaN in column widths");
                else
                    f += t;
            }

            // Take cell-spacing
            f += this.GetHorizontalSpacing() * (this.ColumnWidths.Length + 1);

            // Take table borders
            f += this.TableBox.ActualBorderLeftWidth + this.TableBox.ActualBorderRightWidth;

            return f;
        }

        /// <summary>
        /// Gets the span attribute of the tag of the specified box
        /// </summary>
        /// <param name="b"></param>
        private static int GetSpan(CssBox b)
        {
            double f = CssValueParser.ParseNumber(b.GetAttribute("span"), 1);

            return Math.Max(1, Convert.ToInt32(f));
        }

        /// <summary>
        /// Gets the minimum width of each column
        /// </summary>
        private double[] GetColumnMinWidths()
        {
            if (this.ColumnMinWidths == null)
            {
                this.ColumnMinWidths = new double[this.ColumnWidths.Length];

                foreach (CssBox row in this.AllRows)
                {
                    foreach (CssBox cell in row.Boxes)
                    {
                        int colspan = GetColSpan(cell);
                        int col = GetCellRealColumnIndex(row, cell);
                        int affectcol = Math.Min(col + colspan, this.ColumnMinWidths.Length) - 1;
                        double spannedwidth = this.GetSpannedMinWidth(row, cell, col, colspan) + ((colspan - 1) * this.GetHorizontalSpacing());

                        this.ColumnMinWidths[affectcol] = Math.Max(this.ColumnMinWidths[affectcol], cell.GetMinimumWidth() - spannedwidth);
                    }
                }
            }

            return this.ColumnMinWidths;
        }

        /// <summary>
        /// Gets the actual horizontal spacing of the table
        /// </summary>
        private double GetHorizontalSpacing()
        {
            return this.TableBox.BorderCollapse == CssConstants.Collapse ? -1f : this.TableBox.ActualBorderSpacingHorizontal;
        }

        /// <summary>
        /// Gets the actual horizontal spacing of the table
        /// </summary>
        private static double GetHorizontalSpacing(CssBox box)
        {
            return box.BorderCollapse == CssConstants.Collapse ? -1f : box.ActualBorderSpacingHorizontal;
        }

        /// <summary>
        /// Gets the actual vertical spacing of the table
        /// </summary>
        private double GetVerticalSpacing()
        {
            return this.TableBox.BorderCollapse == CssConstants.Collapse ? -1f : this.TableBox.ActualBorderSpacingVertical;
        }

        #endregion
    }
}