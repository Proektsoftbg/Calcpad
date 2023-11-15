using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;


namespace Calcpad.OpenXml
{
    internal class TableBuilder
    {
        private readonly struct CellData(int rowSpan, bool isHead)
        {
            private readonly int _rowSpan = rowSpan;
            public bool IsMerged => _rowSpan > 0;
            public bool IsHead { get; } = isHead;

            public CellData CopyDown()
            {
                var newSpan = _rowSpan - 1;
                if (newSpan < 0)
                    newSpan = 0;
                return new CellData(newSpan, IsHead);
            }
        }

        public bool IsBorderedTable;
        private List<CellData> _currentRowSpan = [];
        private List<CellData> _prevRowSpan = [];
        private TableRow _currentRow;

        public Table AddTable()
        {
            _currentRowSpan.Clear();
            _prevRowSpan.Clear();
            _currentRow = null;
            var table = new Table();
            table.AppendChild(new TableProperties(new TableStyle() { Val = IsBorderedTable ? "bcpd" : "cpd" }));
            table.AppendChild(new TableGrid());
            return table;
        }

        public TableRow AddTableRow()
        {
            (_currentRowSpan, _prevRowSpan) = (_prevRowSpan, _currentRowSpan);
            _currentRowSpan.Clear();
            _currentRow = new TableRow();
            return _currentRow;
        }

        public TableCell AddTableCell(bool isHead, int colSpan, int rowSpan)
        {
            AddMergedCells();
            var tc = CreateTableCell(isHead);
            var tcp = tc.TableCellProperties;
            var n = colSpan == 0 ? 1 : colSpan;
            for (int i = 0; i < n; ++i)
            {
                _currentRowSpan.Add(new CellData(rowSpan, isHead));
            }
            if (colSpan > 1)
            {
                tcp.GridSpan = new GridSpan()
                {
                    Val = colSpan
                };
            }
            if (rowSpan > 1)
            {
                tcp.VerticalMerge = new VerticalMerge()
                {
                    Val = MergedCellValues.Restart
                };
            }
            return tc;
        }

        private TableCell CreateTableCell(bool isHead)
        {
            var tc = new TableCell();
            var tcp = new TableCellProperties();
            tc.TableCellProperties = tcp;
            const uint size = 8u;
            var color = isHead ? "AAAAAA" : "CCCCCC";
            if (!IsBorderedTable)
                return tc;

            tcp.TableCellBorders = new TableCellBorders()
            {
                LeftBorder = new LeftBorder()
                {
                    Val = BorderValues.Single,
                    Size = size,
                    Space = 0u,
                    Color = color
                },
                RightBorder = new RightBorder()
                {
                    Val = BorderValues.Single,
                    Size = size,
                    Space = 0u,
                    Color = color
                },
                TopBorder = new TopBorder()
                {
                    Val = BorderValues.Single,
                    Size = size,
                    Space = 0u,
                    Color = color
                },
                BottomBorder = new BottomBorder()
                {
                    Val = BorderValues.Single,
                    Size = size,
                    Space = 0u,
                    Color = color
                },
            };
            if (isHead)
            {
                tcp.Shading = new Shading()
                {
                    Fill = "F0F0F0",
                    Val = ShadingPatternValues.Clear
                };
            }
            return tc;
        }

        private void AddMergedCells()
        {
            var i1 = _currentRowSpan.Count;
            var n = _prevRowSpan.Count;
            for (int i = i1; i < n; ++i)
            {
                var cellData = _prevRowSpan[i].CopyDown();
                if (!cellData.IsMerged)
                    break;

                _currentRowSpan.Add(cellData);
                var tc = CreateTableCell(cellData.IsHead);
                tc.TableCellProperties.VerticalMerge = new VerticalMerge()
                {
                    Val = MergedCellValues.Continue
                };
                _currentRow.AppendChild(tc);
            }
        }
    }
}