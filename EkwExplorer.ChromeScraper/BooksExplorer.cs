using System;
using System.Threading;
using System.Threading.Tasks;
using EkwExplorer.Core;
using EkwExplorer.Core.Models;
using Serilog;

namespace EkwExplorer.ChromeScraper
{
    public class BooksExplorer : IBooksExplorer
    {
        private static readonly ChromeOptionsProvider ChromeOptions = new ChromeOptionsProvider();

        private readonly ILogger _logger;
        private readonly IBooksRepository _booksRepository;

        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        private BookInfoSeeker _seeker;

        public BooksExplorer(ILogger logger, IBooksRepository booksRepository)
        {
            _logger = logger;
            _booksRepository = booksRepository;
        }

        public async Task Explore(CancellationToken cancellationToken)
        {
            _seeker = await OpenSeeker();

            var captchaErrors = 0;
            var downloadedBooks = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await ExploringStep(cancellationToken);

                    downloadedBooks++;
                    captchaErrors = 0;
                }
                catch (Exception exception) when (exception.Message.Contains("captcha"))
                {
                    _logger.Warning("Detected captcha.");

                    captchaErrors++;
                    if (captchaErrors > 5)
                    {
                        captchaErrors = 0;

                        _logger.Information("Reopening chrome driver...");
                        _seeker = await ReopenSeeker(_seeker);
                        _logger.Information("Opened new istance of chrome driver");
                    }
                }
                catch (Exception ex) when (ex.GetType().Namespace == "OpenQA.Selenium")
                {
                    _logger.Error(ex, "Error in Selenium driver");
                    _seeker = await ReopenSeeker(_seeker);
                }
                catch (InvalidOperationException ioe) when (ioe.Message.Contains("was not read"))
                {
                    _logger.Error(ioe, "Error while exploring ekw");
                    _seeker = await ReopenSeeker(_seeker);
                }

                _logger.Debug("Downloaded books: {DownloadedBooks}", downloadedBooks);
            }
        }

        private async Task ExploringStep(CancellationToken cancellationToken)
        {
            await RandomDelay(cancellationToken);

            var randomBook = await _booksRepository.GetRandomNotFilledBookAsync();

            _logger.Information($"Book to download: {randomBook.Number}");

            await RandomDelay(cancellationToken);

            var bookExists = _seeker.ReadBookInfo(randomBook);

            if (!bookExists)
            {
                BookInfo.MarkAsNotFound(randomBook);
            }
            else
            {
                if (randomBook?.BookType == null)
                {
                    throw new InvalidOperationException(
                        "something went wrong, book properties was not read");
                }

                if (randomBook.ClosureDate.Length <= 4)
                {
                    var properties = _seeker.ReadProperties();
                    randomBook.AddNewProperties(properties);
                }
            }

            await _booksRepository.UpdateBookAsync(randomBook);
            await _booksRepository.AddPropertyFromBookAsync(randomBook);

            await RandomDelay(cancellationToken);

            _seeker.BackToCriteria();
        }

        private async Task RandomDelay(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(GetRandomDelay(), cancellationToken);
            }
            catch (TaskCanceledException tce)
            {
                throw new OperationCanceledException(tce.Message, tce);
            }
        }

        private int GetRandomDelay()
            => _random.Next(800, 2000);

        private Task<BookInfoSeeker> OpenSeeker()
            => ReopenSeeker(null);

        private async Task<BookInfoSeeker> ReopenSeeker(BookInfoSeeker seeker)
        {
            seeker?.Clicker.Dispose();

            var clicker = await CreateClicker(ChromeOptions);

            seeker = new BookInfoSeeker(clicker);
            return seeker;
        }

        private async Task<IClicker> CreateClicker(ChromeOptionsProvider optionsProvider)
        {
            var clicker = new SeleniumClicker(optionsProvider.Get());

            clicker.GotoHome();
            await Task.Delay(2000);
            var tries = 0;
            while (tries<10)
            {
                try
                {
                    clicker.CloseCookiesInfo();
                    break;
                }
                catch
                {
                    tries++;
                    await Task.Delay(250);
                }
            }

            return clicker;
        }
    }
}