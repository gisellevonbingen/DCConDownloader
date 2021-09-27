using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.Commons;
using Giselle.DCConDownloader.Data;
using Giselle.Forms;

namespace Giselle.DCConDownloader
{
    public class ConCell : OptimizedControl
    {
        private readonly OptimizedControl ImageBox;
        private readonly Label NameLabel;
        private readonly Button RemoveButton;
        private readonly TextBox KeywordsTextBox;
        private readonly TextBox TagsTextBox;

        private string Ext;
        private Texture Texture;

        public event EventHandler RemoveRequested;

        public ConCell()
        {
            this.SuspendLayout();
            var controls = this.Controls;

            var imageBox = this.ImageBox = new OptimizedControl();
            imageBox.Paint += this.OnImageBoxPaint;
            controls.Add(ImageBox);

            var nameLabel = this.NameLabel = new Label();
            nameLabel.TextAlign = ContentAlignment.MiddleLeft;
            controls.Add(nameLabel);

            var removeButton = this.RemoveButton = new Button();
            removeButton.Text = "X";
            removeButton.Click += this.OnRemoveButtonClick;
            removeButton.Font = this.FontManager[9.0F, FontStyle.Regular];
            controls.Add(removeButton);

            var keywordsTextBox = this.KeywordsTextBox = new TextBox();
            controls.Add(keywordsTextBox);

            var tagsTextBox = this.TagsTextBox = new TextBox();
            controls.Add(tagsTextBox);

            this.Ext = null;
            this.Texture = null;

            this.ResumeLayout(false);

            this.Padding = new Padding(1);
            this.ClientSize = new Size(102, 186);

            DCConDownloader.Instance.UiTick += this.OnUiTick;
        }

        private void OnRemoveButtonClick(object sender, EventArgs e)
        {
            this.OnRemoveRequested(new EventArgs());
        }

        protected virtual void OnRemoveRequested(EventArgs e)
        {
            this.RemoveRequested?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var pen = new Pen(Brushes.Black))
            {
                pen.Alignment = PenAlignment.Inset;
                pen.Width = this.Padding.All;
                var g = e.Graphics;

                g.DrawRectangle(pen, this.DisplayRectangle);
            }

        }

        private void OnUiTick(object sender, EventArgs e)
        {
            this.Refresh();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.Texture.DisposeQuietly();

            DCConDownloader.Instance.UiTick -= this.OnUiTick;
        }

        public CellData Parse(JSAssistEventArgs args)
        {
            var keywordsToString = this.KeywordsTextBox.Text;
            var keywords = keywordsToString.Replace(" ", "").Split(',');
            var prefixedKeywords = keywords.Select(t => args.KeywordPrefix + t).ToArray();

            var tagsToString = this.TagsTextBox.Text;
            var tags = tagsToString.Replace(" ", "").Split(',');
            var integratedTags = new List<string>();
            integratedTags.AddRange(args.CommonTags);

            if (string.IsNullOrWhiteSpace(tagsToString) == false)
            {
                integratedTags.AddRange(tags);
            }

            var data = new CellData();
            data.Keywords.AddRange(prefixedKeywords);
            data.Tags.AddRange(integratedTags);
            data.Ext = this.Ext;
            data.Texture = this.Texture;

            return data;
        }

        public void Bind(DCConPackage package, DCConImage image)
        {
            this.NameLabel.Text = image.Title;
            this.KeywordsTextBox.Text = image.Title;
            this.TagsTextBox.Text = string.Empty;

            this.Ext = image.Ext;
            this.Texture.DisposeQuietly();
            this.Texture = null;
            this.Invalidate();

            var path = image.Path;
            var crawler = DCConDownloader.Instance.DCConCrawler;

            new Thread(() =>
            {
                try
                {
                    using (var stream = crawler.DownloadAsStream(package.Referer, path))
                    {
                        var texture = new Texture();
                        texture.Load(stream);
                        this.Texture = texture;

                        ControlUtils.InvokeFNeeded(this, this.Refresh);
                    }

                }
                catch (Exception e)
                {
                    DCConDownloader.Instance.ShowCrashMessageBox(e);
                }

            }).Start();

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var imageBox = this.ImageBox;
            var imageBoxBounds = map[imageBox] = new Rectangle(layoutBounds.Left, layoutBounds.Top, layoutBounds.Width, layoutBounds.Width);

            var removeButton = this.RemoveButton;
            var removeButtonSize = new Size(20, 20);
            var removeButtonBounds = map[removeButton] = new Rectangle(new Point(layoutBounds.Right - removeButtonSize.Width, imageBoxBounds.Bottom), removeButtonSize);

            var lameLabel = this.NameLabel;
            map[lameLabel] = Rectangle.FromLTRB(layoutBounds.Left, removeButtonBounds.Top, removeButtonBounds.Left, removeButtonBounds.Bottom);

            int textBoxHeight = 27;

            var keywordsTextBox = this.KeywordsTextBox;
            var keywordsTextBounds = map[keywordsTextBox] = new Rectangle(layoutBounds.Left, removeButtonBounds.Bottom + 5, layoutBounds.Width, textBoxHeight);

            var tagsTextBox = this.TagsTextBox;
            map[tagsTextBox] = new Rectangle(layoutBounds.Left, keywordsTextBounds.Bottom + 5, layoutBounds.Width, textBoxHeight);

            return map;
        }

        private void OnImageBoxPaint(object sender, PaintEventArgs e)
        {
            var texture = this.Texture;

            if (texture != null)
            {
                var g = e.Graphics;
                var bounds = this.ImageBox.DisplayRectangle;
                texture.Update();
                texture.Render(g, bounds);
            }

        }

    }

}
