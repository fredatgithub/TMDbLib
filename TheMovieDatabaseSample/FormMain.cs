#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TheMovieDatabaseSample.Properties;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using TMDbLib.Utilities.Serializer;

namespace TheMovieDatabaseSample
{
  public partial class FormMain : Form
  {
    public FormMain()
    {
      InitializeComponent();
    }

    public readonly Dictionary<string, string> LanguageDicoEn = new Dictionary<string, string>();
    public readonly Dictionary<string, string> LanguageDicoFr = new Dictionary<string, string>();
    private string _currentLanguage = "english";
    private ConfigurationOptions _configurationOptions = new ConfigurationOptions();

    private void QuitToolStripMenuItemClick(object sender, EventArgs e)
    {
      SaveWindowValue();
      Application.Exit();
    }

    private void AboutToolStripMenuItemClick(object sender, EventArgs e)
    {
      AboutBoxApplication aboutBoxApplication = new AboutBoxApplication();
      aboutBoxApplication.ShowDialog();
    }

    private void DisplayTitle()
    {
      Text += $" {GetVersion()}";
    }

    public static string GetVersion()
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
      return $"V{fvi.FileMajorPart}.{fvi.FileMinorPart}.{fvi.FileBuildPart}.{fvi.FilePrivatePart}";
    }

    private void FormMainLoad(object sender, EventArgs e)
    {
      LoadSettingsAtStartup();
    }

    private void LoadSettingsAtStartup()
    {
      DisplayTitle();
      GetWindowValue();
      LoadLanguages();
      SetLanguage(Settings.Default.LastLanguageUsed);
    }

    private void LoadConfigurationOptions()
    {
      _configurationOptions.Option1Name = Settings.Default.Option1Name;
      _configurationOptions.Option2Name = Settings.Default.Option2Name;
    }

    private void SaveConfigurationOptions()
    {
      _configurationOptions.Option1Name = Settings.Default.Option1Name;
      _configurationOptions.Option2Name = Settings.Default.Option2Name;
    }

    private void LoadLanguages()
    {
      if (!File.Exists(Settings.Default.LanguageFileName))
      {
        CreateLanguageFile();
      }

      // read the translation file and feed the language
      XDocument xDoc;
      try
      {
        xDoc = XDocument.Load(Settings.Default.LanguageFileName);
      }
      catch (Exception exception)
      {
        MessageBox.Show(Resources.Error_while_loading_the + Punctuation.OneSpace +
          Settings.Default.LanguageFileName + Punctuation.OneSpace + Resources.XML_file +
          Punctuation.OneSpace + exception.Message);
        CreateLanguageFile();
        return;
      }
      var result = from node in xDoc.Descendants("term")
                   where node.HasElements
                   let xElementName = node.Element("name")
                   where xElementName != null
                   let xElementEnglish = node.Element("englishValue")
                   where xElementEnglish != null
                   let xElementFrench = node.Element("frenchValue")
                   where xElementFrench != null
                   select new
                   {
                     name = xElementName.Value,
                     englishValue = xElementEnglish.Value,
                     frenchValue = xElementFrench.Value
                   };
      foreach (var i in result)
      {
        if (!LanguageDicoEn.ContainsKey(i.name))
        {
          LanguageDicoEn.Add(i.name, i.englishValue);
        }
#if DEBUG
        else
        {
          MessageBox.Show(Resources.Your_XML_file_has_duplicate_like + Punctuation.Colon +
            Punctuation.OneSpace + i.name);
        }
#endif
        if (!LanguageDicoFr.ContainsKey(i.name))
        {
          LanguageDicoFr.Add(i.name, i.frenchValue);
        }
#if DEBUG
        else
        {
          MessageBox.Show(Resources.Your_XML_file_has_duplicate_like + Punctuation.Colon +
            Punctuation.OneSpace + i.name);
        }
#endif
      }
    }

    private static void CreateLanguageFile()
    {
      var minimumVersion = new List<string>
      {
        "<?xml version=\"1.0\" encoding=\"utf-8\" ?>",
        "<terms>",
         "<term>",
        "<name>MenuFile</name>",
        "<englishValue>File</englishValue>",
        "<frenchValue>Fichier</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileNew</name>",
        "<englishValue>New</englishValue>",
        "<frenchValue>Nouveau</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileOpen</name>",
        "<englishValue>Open</englishValue>",
        "<frenchValue>Ouvrir</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileSave</name>",
        "<englishValue>Save</englishValue>",
        "<frenchValue>Enregistrer</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFileSaveAs</name>",
        "<englishValue>Save as ...</englishValue>",
        "<frenchValue>Enregistrer sous ...</frenchValue>",
        "</term>",
        "<term>",
        "<name>MenuFilePrint</name>",
        "<englishValue>Print ...</englishValue>",
        "<frenchValue>Imprimer ...</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenufilePageSetup</name>",
          "<englishValue>Page setup</englishValue>",
          "<frenchValue>Aperçu avant impression</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenufileQuit</name>",
          "<englishValue>Quit</englishValue>",
          "<frenchValue>Quitter</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEdit</name>",
          "<englishValue>Edit</englishValue>",
          "<frenchValue>Edition</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditCancel</name>",
          "<englishValue>Cancel</englishValue>",
          "<frenchValue>Annuler</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditRedo</name>",
          "<englishValue>Redo</englishValue>",
          "<frenchValue>Rétablir</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditCut</name>",
          "<englishValue>Cut</englishValue>",
          "<frenchValue>Couper</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditCopy</name>",
          "<englishValue>Copy</englishValue>",
          "<frenchValue>Copier</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditPaste</name>",
          "<englishValue>Paste</englishValue>",
          "<frenchValue>Coller</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuEditSelectAll</name>",
          "<englishValue>Select All</englishValue>",
          "<frenchValue>Sélectionner tout</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuTools</name>",
          "<englishValue>Tools</englishValue>",
          "<frenchValue>Outils</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuToolsCustomize</name>",
          "<englishValue>Customize ...</englishValue>",
          "<frenchValue>Personaliser ...</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuToolsOptions</name>",
          "<englishValue>Options</englishValue>",
          "<frenchValue>Options</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuLanguage</name>",
          "<englishValue>Language</englishValue>",
          "<frenchValue>Langage</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuLanguageEnglish</name>",
          "<englishValue>English</englishValue>",
          "<frenchValue>Anglais</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuLanguageFrench</name>",
          "<englishValue>French</englishValue>",
          "<frenchValue>Français</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelp</name>",
          "<englishValue>Help</englishValue>",
          "<frenchValue>Aide</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpSummary</name>",
          "<englishValue>Summary</englishValue>",
          "<frenchValue>Sommaire</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpIndex</name>",
          "<englishValue>Index</englishValue>",
          "<frenchValue>Index</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpSearch</name>",
          "<englishValue>Search</englishValue>",
          "<frenchValue>Rechercher</frenchValue>",
        "</term>",
        "<term>",
          "<name>MenuHelpAbout</name>",
          "<englishValue>About</englishValue>",
          "<frenchValue>A propos de ...</frenchValue>",
        "</term>",
        "</terms>"
      };

      StreamWriter sw = new StreamWriter(Settings.Default.LanguageFileName);
      foreach (string item in minimumVersion)
      {
        sw.WriteLine(item);
      }

      sw.Close();
    }

    private void GetWindowValue()
    {
      Width = Settings.Default.WindowWidth;
      Height = Settings.Default.WindowHeight;
      Top = Settings.Default.WindowTop < 0 ? 0 : Settings.Default.WindowTop;
      Left = Settings.Default.WindowLeft < 0 ? 0 : Settings.Default.WindowLeft;
      SetDisplayOption(Settings.Default.DisplayToolStripMenuItem);
      LoadConfigurationOptions();
    }

    private void SaveWindowValue()
    {
      Settings.Default.WindowHeight = Height;
      Settings.Default.WindowWidth = Width;
      Settings.Default.WindowLeft = Left;
      Settings.Default.WindowTop = Top;
      Settings.Default.LastLanguageUsed = frenchToolStripMenuItem.Checked ? "French" : "English";
      Settings.Default.DisplayToolStripMenuItem = GetDisplayOption();
      SaveConfigurationOptions();
      Settings.Default.Save();
    }

    private string GetDisplayOption()
    {
      if (SmallToolStripMenuItem.Checked)
      {
        return "Small";
      }

      if (MediumToolStripMenuItem.Checked)
      {
        return "Medium";
      }

      return LargeToolStripMenuItem.Checked ? "Large" : string.Empty;
    }

    private void SetDisplayOption(string option)
    {
      UncheckAllOptions();
      switch (option.ToLower())
      {
        case "small":
          SmallToolStripMenuItem.Checked = true;
          break;
        case "medium":
          MediumToolStripMenuItem.Checked = true;
          break;
        case "large":
          LargeToolStripMenuItem.Checked = true;
          break;
        default:
          SmallToolStripMenuItem.Checked = true;
          break;
      }
    }

    private void UncheckAllOptions()
    {
      SmallToolStripMenuItem.Checked = false;
      MediumToolStripMenuItem.Checked = false;
      LargeToolStripMenuItem.Checked = false;
    }

    private void FormMainFormClosing(object sender, FormClosingEventArgs e)
    {
      SaveWindowValue();
    }

    private void FrenchToolStripMenuItemClick(object sender, EventArgs e)
    {
      _currentLanguage = Language.French.ToString();
      SetLanguage(Language.French.ToString());
      AdjustAllControls();
    }

    private void EnglishToolStripMenuItemClick(object sender, EventArgs e)
    {
      _currentLanguage = Language.English.ToString();
      SetLanguage(Language.English.ToString());
      AdjustAllControls();
    }

    private void SetLanguage(string myLanguage)
    {
      switch (myLanguage)
      {
        case "English":
          frenchToolStripMenuItem.Checked = false;
          englishToolStripMenuItem.Checked = true;
          fileToolStripMenuItem.Text = LanguageDicoEn["MenuFile"];
          newToolStripMenuItem.Text = LanguageDicoEn["MenuFileNew"];
          openToolStripMenuItem.Text = LanguageDicoEn["MenuFileOpen"];
          saveToolStripMenuItem.Text = LanguageDicoEn["MenuFileSave"];
          saveasToolStripMenuItem.Text = LanguageDicoEn["MenuFileSaveAs"];
          printPreviewToolStripMenuItem.Text = LanguageDicoEn["MenuFilePrint"];
          printPreviewToolStripMenuItem.Text = LanguageDicoEn["MenufilePageSetup"];
          quitToolStripMenuItem.Text = LanguageDicoEn["MenufileQuit"];
          editToolStripMenuItem.Text = LanguageDicoEn["MenuEdit"];
          cancelToolStripMenuItem.Text = LanguageDicoEn["MenuEditCancel"];
          redoToolStripMenuItem.Text = LanguageDicoEn["MenuEditRedo"];
          cutToolStripMenuItem.Text = LanguageDicoEn["MenuEditCut"];
          copyToolStripMenuItem.Text = LanguageDicoEn["MenuEditCopy"];
          pasteToolStripMenuItem.Text = LanguageDicoEn["MenuEditPaste"];
          selectAllToolStripMenuItem.Text = LanguageDicoEn["MenuEditSelectAll"];
          toolsToolStripMenuItem.Text = LanguageDicoEn["MenuTools"];
          personalizeToolStripMenuItem.Text = LanguageDicoEn["MenuToolsCustomize"];
          optionsToolStripMenuItem.Text = LanguageDicoEn["MenuToolsOptions"];
          languagetoolStripMenuItem.Text = LanguageDicoEn["MenuLanguage"];
          englishToolStripMenuItem.Text = LanguageDicoEn["MenuLanguageEnglish"];
          frenchToolStripMenuItem.Text = LanguageDicoEn["MenuLanguageFrench"];
          helpToolStripMenuItem.Text = LanguageDicoEn["MenuHelp"];
          summaryToolStripMenuItem.Text = LanguageDicoEn["MenuHelpSummary"];
          indexToolStripMenuItem.Text = LanguageDicoEn["MenuHelpIndex"];
          searchToolStripMenuItem.Text = LanguageDicoEn["MenuHelpSearch"];
          aboutToolStripMenuItem.Text = LanguageDicoEn["MenuHelpAbout"];
          DisplayToolStripMenuItem.Text = LanguageDicoEn["Display"];
          SmallToolStripMenuItem.Text = LanguageDicoEn["Small"];
          MediumToolStripMenuItem.Text = LanguageDicoEn["Medium"];
          LargeToolStripMenuItem.Text = LanguageDicoEn["Large"];


          _currentLanguage = "English";
          break;
        case "French":
          frenchToolStripMenuItem.Checked = true;
          englishToolStripMenuItem.Checked = false;
          fileToolStripMenuItem.Text = LanguageDicoFr["MenuFile"];
          newToolStripMenuItem.Text = LanguageDicoFr["MenuFileNew"];
          openToolStripMenuItem.Text = LanguageDicoFr["MenuFileOpen"];
          saveToolStripMenuItem.Text = LanguageDicoFr["MenuFileSave"];
          saveasToolStripMenuItem.Text = LanguageDicoFr["MenuFileSaveAs"];
          printPreviewToolStripMenuItem.Text = LanguageDicoFr["MenuFilePrint"];
          printPreviewToolStripMenuItem.Text = LanguageDicoFr["MenufilePageSetup"];
          quitToolStripMenuItem.Text = LanguageDicoFr["MenufileQuit"];
          editToolStripMenuItem.Text = LanguageDicoFr["MenuEdit"];
          cancelToolStripMenuItem.Text = LanguageDicoFr["MenuEditCancel"];
          redoToolStripMenuItem.Text = LanguageDicoFr["MenuEditRedo"];
          cutToolStripMenuItem.Text = LanguageDicoFr["MenuEditCut"];
          copyToolStripMenuItem.Text = LanguageDicoFr["MenuEditCopy"];
          pasteToolStripMenuItem.Text = LanguageDicoFr["MenuEditPaste"];
          selectAllToolStripMenuItem.Text = LanguageDicoFr["MenuEditSelectAll"];
          toolsToolStripMenuItem.Text = LanguageDicoFr["MenuTools"];
          personalizeToolStripMenuItem.Text = LanguageDicoFr["MenuToolsCustomize"];
          optionsToolStripMenuItem.Text = LanguageDicoFr["MenuToolsOptions"];
          languagetoolStripMenuItem.Text = LanguageDicoFr["MenuLanguage"];
          englishToolStripMenuItem.Text = LanguageDicoFr["MenuLanguageEnglish"];
          frenchToolStripMenuItem.Text = LanguageDicoFr["MenuLanguageFrench"];
          helpToolStripMenuItem.Text = LanguageDicoFr["MenuHelp"];
          summaryToolStripMenuItem.Text = LanguageDicoFr["MenuHelpSummary"];
          indexToolStripMenuItem.Text = LanguageDicoFr["MenuHelpIndex"];
          searchToolStripMenuItem.Text = LanguageDicoFr["MenuHelpSearch"];
          aboutToolStripMenuItem.Text = LanguageDicoFr["MenuHelpAbout"];
          DisplayToolStripMenuItem.Text = LanguageDicoFr["Display"];
          SmallToolStripMenuItem.Text = LanguageDicoFr["Small"];
          MediumToolStripMenuItem.Text = LanguageDicoFr["Medium"];
          LargeToolStripMenuItem.Text = LanguageDicoFr["Large"];

          _currentLanguage = "French";
          break;
        default:
          SetLanguage("English");
          break;
      }
    }

    private void CutToolStripMenuItemClick(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { }); // add your controls in the List
      var tb = focusedControl as TextBox;
      if (tb != null)
      {
        CutToClipboard(tb);
      }
    }

    private void CopyToolStripMenuItemClick(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { }); // add your controls in the List
      var tb = focusedControl as TextBox;
      if (tb != null)
      {
        CopyToClipboard(tb);
      }
    }

    private void PasteToolStripMenuItemClick(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { }); // add your controls in the List
      var tb = focusedControl as TextBox;
      if (tb != null)
      {
        PasteFromClipboard(tb);
      }
    }

    private void SelectAllToolStripMenuItemClick(object sender, EventArgs e)
    {
      Control focusedControl = FindFocusedControl(new List<Control> { }); // add your controls in the List
      TextBox control = focusedControl as TextBox;
      if (control != null) control.SelectAll();
    }

    private void CutToClipboard(TextBoxBase tb, string errorMessage = "nothing")
    {
      if (tb != ActiveControl) return;
      if (tb.Text == string.Empty)
      {
        DisplayMessage(Translate("ThereIs") + Punctuation.OneSpace +
          Translate(errorMessage) + Punctuation.OneSpace +
          Translate("ToCut") + Punctuation.OneSpace, Translate(errorMessage),
          MessageBoxButtons.OK);
        return;
      }

      if (tb.SelectedText == string.Empty)
      {
        DisplayMessage(Translate("NoTextHasBeenSelected"),
          Translate(errorMessage), MessageBoxButtons.OK);
        return;
      }

      Clipboard.SetText(tb.SelectedText);
      tb.SelectedText = string.Empty;
    }

    private void CopyToClipboard(TextBoxBase tb, string message = "nothing")
    {
      if (tb != ActiveControl) return;
      if (tb.Text == string.Empty)
      {
        DisplayMessage(Translate("ThereIsNothingToCopy") + Punctuation.OneSpace,
          Translate(message), MessageBoxButtons.OK);
        return;
      }

      if (tb.SelectedText == string.Empty)
      {
        DisplayMessage(Translate("NoTextHasBeenSelected"),
          Translate(message), MessageBoxButtons.OK);
        return;
      }

      Clipboard.SetText(tb.SelectedText);
    }

    private void PasteFromClipboard(TextBoxBase tb)
    {
      if (tb != ActiveControl) return;
      var selectionIndex = tb.SelectionStart;
      tb.SelectedText = Clipboard.GetText();
      tb.SelectionStart = selectionIndex + Clipboard.GetText().Length;
    }

    private void DisplayMessage(string message, string title, MessageBoxButtons buttons)
    {
      MessageBox.Show(this, message, title, buttons);
    }

    private string Translate(string index)
    {
      string result = string.Empty;
      switch (_currentLanguage.ToLower())
      {
        case "english":
          result = LanguageDicoEn.ContainsKey(index) ? LanguageDicoEn[index] :
           "the term: \"" + index + "\" has not been translated yet.\nPlease tell the developer to translate this term";
          break;
        case "french":
          result = LanguageDicoFr.ContainsKey(index) ? LanguageDicoFr[index] :
            "the term: \"" + index + "\" has not been translated yet.\nPlease tell the developer to translate this term";
          break;
      }

      return result;
    }

    private static Control FindFocusedControl(Control container)
    {
      foreach (Control childControl in container.Controls.Cast<Control>().Where(childControl => childControl.Focused))
      {
        return childControl;
      }

      return (from Control childControl in container.Controls
              select FindFocusedControl(childControl)).FirstOrDefault(maybeFocusedControl => maybeFocusedControl != null);
    }

    private static Control FindFocusedControl(List<Control> container)
    {
      return container.FirstOrDefault(control => control.Focused);
    }

    private static Control FindFocusedControl(params Control[] container)
    {
      return container.FirstOrDefault(control => control.Focused);
    }

    private static Control FindFocusedControl(IEnumerable<Control> container)
    {
      return container.FirstOrDefault(control => control.Focused);
    }

    private static string PeekDirectory()
    {
      string result = string.Empty;
      FolderBrowserDialog fbd = new FolderBrowserDialog();
      if (fbd.ShowDialog() == DialogResult.OK)
      {
        result = fbd.SelectedPath;
      }

      return result;
    }

    private string PeekFile()
    {
      string result = string.Empty;
      OpenFileDialog fd = new OpenFileDialog();
      if (fd.ShowDialog() == DialogResult.OK)
      {
        result = fd.SafeFileName;
      }

      return result;
    }

    private void SmallToolStripMenuItemClick(object sender, EventArgs e)
    {
      UncheckAllOptions();
      SmallToolStripMenuItem.Checked = true;
      AdjustAllControls();
    }

    private void MediumToolStripMenuItemClick(object sender, EventArgs e)
    {
      UncheckAllOptions();
      MediumToolStripMenuItem.Checked = true;
      AdjustAllControls();
    }

    private void LargeToolStripMenuItemClick(object sender, EventArgs e)
    {
      UncheckAllOptions();
      LargeToolStripMenuItem.Checked = true;
      AdjustAllControls();
    }

    private static void AdjustControls(params Control[] listOfControls)
    {
      if (listOfControls.Length == 0)
      {
        return;
      }

      int position = listOfControls[0].Width + 33; // 33 is the initial padding
      bool isFirstControl = true;
      foreach (Control control in listOfControls)
      {
        if (isFirstControl)
        {
          isFirstControl = false;
        }
        else
        {
          control.Left = position + 10;
          position += control.Width;
        }
      }
    }

    private void AdjustAllControls()
    {
      AdjustControls(); // insert here all labels, textboxes and buttons, one method per line of controls
    }

    private void OptionsToolStripMenuItemClick(object sender, EventArgs e)
    {
      FormOptions frmOptions = new FormOptions(_configurationOptions);

      if (frmOptions.ShowDialog() == DialogResult.OK)
      {
        _configurationOptions = frmOptions.ConfigurationOptions2;
      }
    }

    private static void SetButtonEnabled(Button button, params Control[] controls)
    {
      bool result = true;
      foreach (Control ctrl in controls)
      {
        if (ctrl.GetType() == typeof(TextBox))
        {
          if (((TextBox)ctrl).Text == string.Empty)
          {
            result = false;
            break;
          }
        }

        if (ctrl.GetType() == typeof(ListView))
        {
          if (((ListView)ctrl).Items.Count == 0)
          {
            result = false;
            break;
          }
        }

        if (ctrl.GetType() == typeof(ComboBox))
        {
          if (((ComboBox)ctrl).SelectedIndex == -1)
          {
            result = false;
            break;
          }
        }
      }

      button.Enabled = result;
    }

    private void TextBoxNameKeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        // do something
      }
    }

    private async void ButtonGetMovie_Click(object sender, EventArgs e)
    {
      // Instantiate a new client, all that's needed is an API key, but it's possible to 
      // also specify if SSL should be used, and if another server address should be used.
      using TMDbClient client = new TMDbClient("c6b31d1cdad6a56a23f0c913e2482a31");

      // We need the config from TMDb in case we want to get stuff like images
      // The config needs to be fetched for each new client we create, but we can cache it to a file (as in this example).
      await FetchConfig(client);

      // Try fetching a movie
      await FetchMovieExample(client);

      // Once we've got a movie, or person, or so on, we can display images. 
      // TMDb follow the pattern shown in the following example
      // This example also shows an important feature of most of the Get-methods.
      await FetchImagesExample(client);
    }

    private async Task FetchConfig(TMDbClient client)
    {
      FileInfo configJson = new FileInfo("config.json");

      textBoxMovieInfo.Text += "Config file: " + configJson.FullName + ", Exists: " + configJson.Exists;

      if (configJson.Exists && configJson.LastWriteTimeUtc >= DateTime.UtcNow.AddHours(-1))
      {
        textBoxMovieInfo.Text += "Using stored config";
        string json = File.ReadAllText(configJson.FullName, Encoding.UTF8);

        client.SetConfig(TMDbJsonSerializer.Instance.DeserializeFromString<TMDbConfig>(json));
      }
      else
      {
        textBoxMovieInfo.Text += "Getting new config";
        TMDbConfig config = await client.GetConfigAsync();

        textBoxMovieInfo.Text += "Storing config";
        string json = TMDbJsonSerializer.Instance.SerializeToString(config);
        File.WriteAllText(configJson.FullName, json, Encoding.UTF8);
      }

      Spacer();
    }

    private async Task FetchImagesExample(TMDbClient client)
    {
      const int movieId = 76338; // Thor: The Dark World (2013)

      // In the call below, we're fetching the wanted movie from TMDb, but we're also doing something else.
      // We're requesting additional data, in this case: Images. This means that the Movie property "Images" will be populated (else it will be null).
      // We could combine these properties, requesting even more information in one go:
      //      client.GetMovieAsync(movieId, MovieMethods.Images);
      //      client.GetMovieAsync(movieId, MovieMethods.Images | MovieMethods.Releases);
      //      client.GetMovieAsync(movieId, MovieMethods.Images | MovieMethods.Trailers | MovieMethods.Translations);
      //
      // .. and so on..
      // 
      // Note: Each method normally corresponds to a property on the resulting object. If you haven't requested the information, the property will most likely be null.

      // Also note, that while we could have used 'client.GetMovieImagesAsync()' - it was better to do it like this because we also wanted the Title of the movie.
      Movie movie = await client.GetMovieAsync(movieId, MovieMethods.Images);

      textBoxMovieInfo.Text += "Fetching images for '" + movie.Title + "'";

      // Images come in three forms, each dispayed below
      textBoxMovieInfo.Text += "Displaying Backdrops";
      await ProcessImages(client, movie.Images.Backdrops.Take(3), client.Config.Images.BackdropSizes);

      textBoxMovieInfo.Text += "Displaying Posters";
      await ProcessImages(client, movie.Images.Posters.Take(3), client.Config.Images.PosterSizes);

      textBoxMovieInfo.Text += "Displaying Logos";
      await ProcessImages(client, movie.Images.Logos.Take(3), client.Config.Images.LogoSizes);

      Spacer();
    }

    private async Task ProcessImages(TMDbClient client, IEnumerable<ImageData> images, IEnumerable<string> sizes)
    {
      // Displays basic information about each image, as well as all the possible adresses for it.
      // All images should be available in all the sizes provided by the configuration.

      List<ImageData> imagesLst = images.ToList();
      List<string> sizesLst = sizes.ToList();

      foreach (ImageData imageData in imagesLst)
      {
        textBoxMovieInfo.Text += imageData.FilePath;
        textBoxMovieInfo.Text += "\t " + imageData.Width + "x" + imageData.Height;

        // Calculate the images path
        // There are multiple resizing available for each image, directly from TMDb.
        // There's always the "original" size if you're in doubt which to choose.
        foreach (string size in sizesLst)
        {
          Uri imageUri = client.GetImageUrl(size, imageData.FilePath);
          textBoxMovieInfo.Text += "\t -> " + imageUri;
        }
      }

      // Download an image for testing, uses the internal HttpClient in the API.
      textBoxMovieInfo.Text += "Downloading image for the first url, as a test";

      Uri testUrl = client.GetImageUrl(sizesLst.First(), imagesLst.First().FilePath);
      byte[] bts = await client.GetImageBytesAsync(sizesLst.First(), imagesLst.First().FilePath);

      textBoxMovieInfo.Text += $"Downloaded {testUrl}: {bts.Length} bytes";
    }

    private async Task FetchMovieExample(TMDbClient client)
    {
      string query = "Thor";

      // This example shows the fetching of a movie.
      // Say the user searches for "Thor" in order to find "Thor: The Dark World" or "Thor"
      SearchContainer<SearchMovie> results = await client.SearchMovieAsync(query);

      // The results is a list, currently on page 1 because we didn't specify any page.
      textBoxMovieInfo.Text += "Searched for movies: '" + query + "', found " + results.TotalResults + " results in " + results.TotalPages + " pages";

      // Let's iterate the first few hits
      StringBuilder movieResult = new();
      foreach (SearchMovie result in results.Results.Take(3))
      {
        // Print out each hit
        movieResult.Append(result.Id + ": " + result.Title);
        movieResult.Append("\t Original Title: " + result.OriginalTitle);
        movieResult.Append("\t Release date  : " + result.ReleaseDate);
        movieResult.Append("\t Popularity    : " + result.Popularity);
        movieResult.Append("\t Vote Average  : " + result.VoteAverage);
        movieResult.Append("\t Vote Count    : " + result.VoteCount);
        movieResult.Append("\t Backdrop Path : " + result.BackdropPath);
        movieResult.Append("\t Poster Path   : " + result.PosterPath);
      }

      textBoxMovieInfo.Text += movieResult.ToString();
      Spacer();
    }

    private void Spacer()
    {
      textBoxMovieInfo.Text += Environment.NewLine;
      textBoxMovieInfo.Text += " ----- ";
      textBoxMovieInfo.Text += Environment.NewLine;
    }
  }
}