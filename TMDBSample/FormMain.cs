using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using TMDbLib.Utilities.Serializer;

namespace TMDBSample
{
  public partial class FormMain : Form
  {
    public FormMain()
    {
      InitializeComponent();
    }

    List<string> listOfPictures = new();
    int cursor = 0;

    private async void ButtonGetMovie_Click(object sender, EventArgs e)
    {
      textBoxMovieInfo.Text = string.Empty;
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

    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private async Task FetchConfig(TMDbClient client)
    {
      FileInfo configJson = new("config.json");

      //textBoxMovieInfo.Text += "Config file: " + configJson.FullName + ", Exists: " + configJson.Exists;

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

      List<ImageData> imagesList = images.ToList();
      List<string> sizeList = sizes.ToList();
      StringBuilder infoMovie = new();
      foreach (ImageData imageData in imagesList)
      {
        infoMovie.Append(imageData.FilePath);
        infoMovie.Append("\t " + imageData.Width + "x" + imageData.Height);

        // Calculate the images path
        // There are multiple resizing available for each image, directly from TMDb.
        // There's always the "original" size if you're in doubt which to choose.
        StringBuilder movieResult = new();
        foreach (string size in sizeList)
        {
          Uri imageUri = client.GetImageUrl(size, imageData.FilePath);
          movieResult.Append("\t -> " + imageUri);
        }

        textBoxMovieInfo.Text += movieResult.ToString();
      }

      textBoxMovieInfo.Text += infoMovie.ToString();

      // Download an image for testing, uses the internal HttpClient in the API.
      textBoxMovieInfo.Text += "Downloading image for the first url, as a test";

      Uri testUrl = client.GetImageUrl(sizeList.First(), imagesList.First().FilePath);

      listOfPictures.Add(imagesList.First().FilePath.TrimStart('/'));
      byte[] bts = await client.GetImageBytesAsync(sizeList.First(), imagesList.First().FilePath);
      try
      {
        File.WriteAllBytes(imagesList.First().FilePath.TrimStart('/'), bts);
      }
      catch (Exception exception)
      {
        throw new Exception("exception while trying to write the picture to local disk: " + exception.Message);
      }

      textBoxMovieInfo.Text += $"Downloaded {testUrl}: {bts.Length} bytes";
      cursor = listOfPictures.Count;
      if (listOfPictures.Count == 0)
      {
        buttonNextPicture.Enabled = false;
        buttonPreviousPicture.Enabled = false;
      }
      else
      {
        buttonNextPicture.Enabled = true;
        buttonPreviousPicture.Enabled = true;
        pictureBoxMoviePoster.ImageLocation = listOfPictures[0];
      }
    }

    private async Task FetchMovieExample(TMDbClient client)
    {
      string query = string.Empty;
      if (string.IsNullOrEmpty(textBoxSearchMovie.Text))
      {
        query = "Predator";
      }
      else
      {
        query = textBoxSearchMovie.Text;
      }


      // This example shows the fetching of a movie.
      // Say the user searches for "Predator" in order to find "Predator II" or "Predator"
      SearchContainer<SearchMovie> results = await client.SearchMovieAsync(query);

      // The results is a list, currently on page 1 because we didn't specify any page.
      textBoxMovieInfo.Text += "Searched for movies: '" + query + "', found " + results.TotalResults + " results in " + results.TotalPages + " pages" + Environment.NewLine;

      // Let's iterate the first few hits
      StringBuilder movieResult = new();
      foreach (SearchMovie result in results.Results.Take(1))
      {
        // Print out each hit
        movieResult.Append(result.Id + ": " + result.Title + Environment.NewLine);
        movieResult.Append("\t Original Title: " + result.OriginalTitle + Environment.NewLine);
        movieResult.Append("\t Release date  : " + result.ReleaseDate + Environment.NewLine);
        movieResult.Append("\t Popularity    : " + result.Popularity + Environment.NewLine);
        movieResult.Append("\t Vote Average  : " + result.VoteAverage + Environment.NewLine);
        movieResult.Append("\t Vote Count    : " + result.VoteCount + Environment.NewLine);
        movieResult.Append("\t Backdrop Path : " + result.BackdropPath + Environment.NewLine);
        movieResult.Append("\t Poster Path   : " + result.PosterPath + Environment.NewLine);
      }

      textBoxMovieInfo.Text += movieResult.ToString();
      Spacer();
    }

    private void Spacer()
    {
      textBoxMovieInfo.Text += Environment.NewLine;
      textBoxMovieInfo.Text += " ----- " + Environment.NewLine;
      textBoxMovieInfo.Text += Environment.NewLine;
    }

    private void ButtonPreviousPicture_Click(object sender, EventArgs e)
    {
      if (cursor > 0 && listOfPictures.Count > 0)
      {
        cursor--;
        pictureBoxMoviePoster.ImageLocation = listOfPictures[cursor];
      }

      if (listOfPictures.Count == 0)
      {
        buttonNextPicture.Enabled = false;
        buttonPreviousPicture.Enabled = false;
      }

      if (cursor >= 0 && cursor <= listOfPictures.Count)
      {
        buttonPreviousPicture.Enabled = true;
      }
      else
      {
        buttonPreviousPicture.Enabled = false;
      }
    }

    private void ButtonNextPicture_Click(object sender, EventArgs e)
    {
      if (cursor < listOfPictures.Count - 1)
      {
        cursor++;
        pictureBoxMoviePoster.ImageLocation = listOfPictures[cursor];
      }

      if (listOfPictures.Count == 0)
      {
        buttonNextPicture.Enabled = false;
        buttonPreviousPicture.Enabled = false;
      }

      if (cursor <= listOfPictures.Count)
      {
        buttonNextPicture.Enabled = true;
      }
      else
      {
        buttonNextPicture.Enabled = false;
      }
    }
  }
}
