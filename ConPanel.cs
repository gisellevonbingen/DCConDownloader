using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DCConDownloader.Data;
using Giselle.Forms;

namespace Giselle.DCConDownloader
{
    public class ConPanel : OptimizedControl
    {
        private readonly Panel Panel;
        private readonly List<ConCell> Cells;

        private bool CanAlignment;

        public ConPanel()
        {
            this.SuspendLayout();
            var controls = this.Controls;

            var panel = this.Panel = new Panel();
            panel.AutoScroll = true;
            controls.Add(panel);

            this.Cells = new List<ConCell>();

            this.CanAlignment = true;

            this.ResumeLayout(false);
        }

        public List<CellData> Parse(JSAssistEventArgs args)
        {
            var list = new List<CellData>();

            foreach (var cell in this.Cells)
            {
                var token = cell.Parse(args);
                list.Add(token);
            }

            return list;
        }

        public void Remove(ConCell cell)
        {
            var panel = this.Panel;
            var controls = panel.Controls;
            var cells = this.Cells;

            if (cells.Contains(cell) == true)
            {
                controls.Remove(cell);
                cells.Remove(cell);
                cell.RemoveRequested -= this.OnCellRemoveRequested;
                cell.DisposeQuietly();

                this.Realinment();
            }

        }

        public ConCell New()
        {
            var panel = this.Panel;
            var controls = panel.Controls;
            var cells = this.Cells;

            var cell = new ConCell();
            cell.RemoveRequested += this.OnCellRemoveRequested;
            controls.Add(cell);
            cells.Add(cell);

            this.Realinment();

            return cell;
        }

        public void Clear()
        {
            try
            {
                this.CanAlignment = false;

                foreach (var cell in this.Cells.ToArray())
                {
                    this.Remove(cell);
                }

            }
            finally
            {
                this.CanAlignment = true;
                this.Realinment();
            }

        }

        public void Bind(DCConPackage package)
        {
            this.Clear();

            try
            {
                var images = package.Images;

                this.CanAlignment = false;

                foreach (var image in images)
                {
                    var cell = this.New();
                    cell.Bind(package, image);
                }

            }
            finally
            {
                this.CanAlignment = true;
            }

            this.Realinment();
        }

        private void OnCellRemoveRequested(object sender, EventArgs e)
        {
            var cell = (ConCell)sender;
            var result = MessageBox.Show("해당 콘을 제거하시겠습니까?", "제거 확인", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

            if (result == DialogResult.OK)
            {
                this.Remove(cell);
            }

        }

        private void Realinment()
        {
            if (this.CanAlignment == false)
            {
                return;
            }

            var panel = this.Panel;
            var cells = this.Cells;

            if (cells.Count == 0)
            {
                return;
            }

            var first = cells[0];
            var size = panel.ClientSize;
            int cols = size.Width / first.Width;

            if (cols == 0)
            {
                return;
            }

            panel.AutoScroll = false;
            panel.SuspendLayout();

            for (int i = 0; i < cells.Count; i++)
            {
                int xi = i % cols;
                int yi = i / cols;

                var cell = cells[i];
                var left = xi * first.Width;
                var top = yi * first.Height;

                cell.Location = new Point(left, top);
            }

            panel.ResumeLayout(false);
            panel.AutoScroll = true;
        }

        protected override void UpdateControlsBoundsPreferred(Rectangle layoutBounds)
        {
            base.UpdateControlsBoundsPreferred(layoutBounds);

            this.Realinment();
        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);
            map[this.Panel] = layoutBounds;

            return map;
        }

    }

}
