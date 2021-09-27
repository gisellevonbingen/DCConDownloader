using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Giselle.DCConDownloader.Utils;
using Giselle.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Giselle.DCConDownloader
{
    public class MainForm : OptimizedForm
    {
        private readonly TextBox URLTextBox;
        private readonly Button VerifyButton;
        private readonly Label VerifyMessageLabel;

        private readonly ConPanel ConPanel;

        private readonly Label GitHubURLLabel;
        private readonly TextBox GitHubURLTextBox;

        private readonly Label PrefixKeywordLabel;
        private readonly TextBox PrefixKeywordTextBox;

        private readonly Label CommonTagsLabel;
        private readonly TextBox CommonTagsTextBox;

        private readonly Button CopyButton;

        public MainForm()
        {
            this.SuspendLayout();
            this.Text = "DCConHelper";
            var controls = this.Controls;

            var urlTextBox = this.URLTextBox = new TextBox();
            urlTextBox.Font = this.FontManager[13.0F, FontStyle.Regular];
            controls.Add(urlTextBox);

            var verifyButton = this.VerifyButton = new Button();
            verifyButton.FlatStyle = FlatStyle.Flat;
            verifyButton.Text = "확인";
            verifyButton.Click += this.OnVerifyButtonClick;
            controls.Add(verifyButton);

            var verifyMessageLabel = this.VerifyMessageLabel = new Label();
            verifyMessageLabel.TextAlign = ContentAlignment.MiddleRight;
            controls.Add(verifyMessageLabel);

            var conPanel = this.ConPanel = new ConPanel();
            controls.Add(conPanel);

            var gitHubURLLabel = this.GitHubURLLabel = new Label();
            gitHubURLLabel.Text = "경로 접두";
            controls.Add(gitHubURLLabel);
            var gitHubURLTextBox = this.GitHubURLTextBox = new TextBox();
            controls.Add(gitHubURLTextBox);

            var prefixKeywordLabel = this.PrefixKeywordLabel = new Label();
            prefixKeywordLabel.Text = "키워드 접두";
            controls.Add(prefixKeywordLabel);
            var prefixKeywordTextBox = this.PrefixKeywordTextBox = new TextBox();
            controls.Add(prefixKeywordTextBox);

            var commonTagsLabel = this.CommonTagsLabel = new Label();
            commonTagsLabel.Text = "공통 태그";
            controls.Add(commonTagsLabel);
            var commonTagsTextBox = this.CommonTagsTextBox = new TextBox();
            controls.Add(commonTagsTextBox);

            var copyButton = this.CopyButton = new Button();
            copyButton.FlatStyle = FlatStyle.Flat;
            copyButton.Text = "생성";
            copyButton.Click += this.OnCopyButtonClick;
            controls.Add(copyButton);

            this.ResumeLayout(false);

            this.ClientSize = new Size(650, 600);
        }

        private void OnCopyButtonClick(object sender, EventArgs e)
        {
            var gitHubBaseURL = this.GitHubURLTextBox.Text;

            var prefixKeywordTextBox = this.PrefixKeywordTextBox;
            var keywordPrefix = prefixKeywordTextBox.Text.Replace(" ", "");

            var commonTagsToString = this.CommonTagsTextBox.Text;
            var commonTags = new List<string>();

            if (string.IsNullOrWhiteSpace(commonTagsToString) == false)
            {
                commonTags.AddRange(commonTagsToString.Replace(" ", "").Split(','));
            }

            var args = new JSAssistEventArgs(keywordPrefix, commonTags);
            var list = this.ConPanel.Parse(args);

            var builder = new StringBuilder();
            var packagesDirectory = PathUtils.GetPath("Packages");
            var imageDirectory = PathUtils.GetPath(packagesDirectory, keywordPrefix);
            Directory.CreateDirectory(imageDirectory);

            foreach (var item in list)
            {
                var fileName = $"{item.Keywords[0]}.{item.Ext}";

                var jobject = new JObject
                {
                    ["keywords"] = new JArray(item.Keywords),
                    ["tags"] = new JArray(item.Tags),
                    ["path"] = new JValue($"{gitHubBaseURL}/{fileName}")
                };

                var line = jobject.ToString(Formatting.None) + ",";
                builder.AppendLine(line);

                item.Texture.Save($"{imageDirectory}/{fileName}");
            }

            var build = builder.ToString();

            if (string.IsNullOrWhiteSpace(build) != true)
            {
                File.WriteAllText($"{packagesDirectory}/{keywordPrefix}.json", builder.ToString());
                MessageBox.Show(this, "생성됨", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }

        }

        private void OnVerifyButtonClick(object sender, EventArgs e)
        {
            var verifyMessageLabel = this.VerifyMessageLabel;
            verifyMessageLabel.Text = "확인 중";

            var urlTextBox = this.URLTextBox;
            var url = urlTextBox.Text;

            var crawler = DCConDownloader.Instance.DCConCrawler;

            try
            {
                var package = crawler.GetPackage(url);
                verifyMessageLabel.Text = $"확인 완료 : {package.Title}";

                var conPanel = this.ConPanel;
                conPanel.Bind(package);

                var prefixKeywordTextBox = this.PrefixKeywordTextBox;
                prefixKeywordTextBox.Text = package.Title;

                var commonTagsTextBox = this.CommonTagsTextBox;
                commonTagsTextBox.Text = string.Join(",", package.Tags);
            }
            catch (Exception ex)
            {
                verifyMessageLabel.Text = ex.ToString();
                Console.WriteLine(ex);
            }

        }

        protected override Dictionary<Control, Rectangle> GetPreferredBounds(Rectangle layoutBounds)
        {
            var map = base.GetPreferredBounds(layoutBounds);

            var verifyButton = this.VerifyButton;
            var verifyButtonSize = new Size(120, 31);
            var verifyButtonBounds = map[verifyButton] = new Rectangle(new Point(layoutBounds.Right - verifyButtonSize.Width - 10, layoutBounds.Top), verifyButtonSize);

            var urlTextBox = this.URLTextBox;
            var urlTextBoxBounds = map[urlTextBox] = Rectangle.FromLTRB(layoutBounds.Left, layoutBounds.Top, verifyButtonBounds.Left - 10, verifyButtonBounds.Bottom);

            var verifyMessageLabel = this.VerifyMessageLabel;
            var verifyMessageLabelTop = urlTextBoxBounds.Bottom;
            var verifyMessageLabelBottom = verifyMessageLabelTop + 25;
            var verifyMessageLabelBounds = map[verifyMessageLabel] = Rectangle.FromLTRB(urlTextBoxBounds.Left, verifyMessageLabelTop, verifyButtonBounds.Right, verifyMessageLabelBottom);

            var copyButton = this.CopyButton;
            var copyButtonSize = new Size(91, 91);
            var copyButtonBounds = map[copyButton] = new Rectangle(new Point(layoutBounds.Right - copyButtonSize.Width, layoutBounds.Bottom - copyButtonSize.Height), copyButtonSize);

            int bottomControlHeight = 27;
            var bottomLabelSize = new Size(160, bottomControlHeight);
            int bottomLabelLeft = layoutBounds.Left;
            int bottomTextBoxLeft = bottomLabelLeft + bottomLabelSize.Width + 10;
            var bottomTextBoxSize = new Size(copyButtonBounds.Left - 10 - bottomTextBoxLeft, bottomControlHeight);

            var commonTagsLabel = this.CommonTagsLabel;
            var commonTagsLabelBounds = map[commonTagsLabel] = new Rectangle(new Point(bottomLabelLeft, copyButtonBounds.Bottom - bottomLabelSize.Height), bottomLabelSize);
            var commonTagsTextBox = this.CommonTagsTextBox;
            var commonTagsTextBoxBounds = map[commonTagsTextBox] = new Rectangle(new Point(bottomTextBoxLeft, commonTagsLabelBounds.Top), bottomTextBoxSize);

            var prefixKeywordLabel = this.PrefixKeywordLabel;
            var prefixKeywordLabelBounds = map[prefixKeywordLabel] = new Rectangle(new Point(bottomLabelLeft, commonTagsTextBoxBounds.Top - bottomLabelSize.Height - 5), bottomLabelSize);
            var prefixKeywordTextBox = this.PrefixKeywordTextBox;
            var prefixKeywordTextBoxBounds = map[prefixKeywordTextBox] = new Rectangle(new Point(bottomTextBoxLeft, prefixKeywordLabelBounds.Top), bottomTextBoxSize);

            var gitHubURLLabel = this.GitHubURLLabel;
            var gitHubURLLabelBounds = map[gitHubURLLabel] = new Rectangle(new Point(bottomLabelLeft, prefixKeywordTextBoxBounds.Top - bottomLabelSize.Height - 5), bottomLabelSize);
            var gitHubURLTextBox = this.GitHubURLTextBox;
            var gitHubURLTextBoxBounds = map[gitHubURLTextBox] = new Rectangle(new Point(bottomTextBoxLeft, gitHubURLLabelBounds.Top), bottomTextBoxSize);

            var conPanel = this.ConPanel;
            var conPanelTop = verifyMessageLabelBounds.Bottom;
            var conPanelBottom = gitHubURLTextBoxBounds.Top - 10;
            map[conPanel] = Rectangle.FromLTRB(layoutBounds.Left, conPanelTop, layoutBounds.Right, conPanelBottom);

            return map;
        }

    }

}
