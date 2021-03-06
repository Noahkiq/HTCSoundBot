﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Audio;
using System.IO;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Argotic.Common;
using Argotic.Syndication;


class Program
{
    static void Main(string[] args) => new Program().Start();

    public static bool ohseriouslyEnabled = true;

    private static DiscordClient _client = new DiscordClient();

    public void Start()
    {
        System.Timers.Timer caryCheckTimer = new System.Timers.Timer();
        caryCheckTimer.Elapsed += new ElapsedEventHandler(caryVideoChecker);
        caryCheckTimer.Interval = 60000;
        caryCheckTimer.Enabled = true;

        System.Timers.Timer jnjCheckTimer = new System.Timers.Timer();
        jnjCheckTimer.Elapsed += new ElapsedEventHandler(jnjVideoChecker);
        jnjCheckTimer.Interval = 60000;
        jnjCheckTimer.Enabled = true;

        System.Timers.Timer abaCheckTimer = new System.Timers.Timer();
        abaCheckTimer.Elapsed += new ElapsedEventHandler(abaVideoChecker);
        abaCheckTimer.Interval = 60000;
        abaCheckTimer.Enabled = true;

        System.Timers.Timer cbrCheckTimer = new System.Timers.Timer();
        cbrCheckTimer.Elapsed += new ElapsedEventHandler(cbrVideoChecker);
        cbrCheckTimer.Interval = 60000;
        cbrCheckTimer.Enabled = true;

        System.Timers.Timer frnCheckTimer = new System.Timers.Timer();
        frnCheckTimer.Elapsed += new ElapsedEventHandler(frnVideoChecker);
        frnCheckTimer.Interval = 60000;
        frnCheckTimer.Enabled = true;

        _client.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
        {
            x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
        });

        _client.Log.Message += (s, e) => Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");

        _client.MessageReceived += async (s, e) =>
        {
            if ((e.Message.RawText.Contains(":awseriously:")) && (e.Channel.Id == 222494171506671616) && (ohseriouslyEnabled))
            {
                var voiceChannel = _client.FindServers("HTwins Central").FirstOrDefault().FindChannels("[Memes] Sea of Davids").FirstOrDefault();

                var _vClient = await _client.GetService<AudioService>() // We use GetService to find the AudioService that we installed earlier. In previous versions, this was equivelent to _client.Audio()
                        .Join(voiceChannel); // Join the Voice Channel, and return the IAudioClient.

                string filePath = $"{Directory.GetCurrentDirectory()}\\sounds\\ohseriously.mp3"; // Grab current directory

                var channelCount = _client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }
                        _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                    }
                }

                _vClient.Wait(); // Waits for the currently playing sound file to end.

                await _vClient.Disconnect(); // Disconnects from the voice channel.

                ohseriouslyEnabled = false;
                _client.SetGame("💤");
                System.Threading.Thread.Sleep(150000);
                ohseriouslyEnabled = true;
                _client.SetGame("AW SERIOUSLY?");
            }
        };

        string token = File.ReadAllText("token.txt");
        _client.ExecuteAndWait(async () => {
            await _client.Connect(token, TokenType.Bot);
            _client.SetGame("AW SERIOUSLY?");
        });

    }

    private static void caryVideoChecker(object source, ElapsedEventArgs e)
    {
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        settings.RetrievalLimit = 1;

        Uri feedUrl = new Uri("https://www.youtube.com/feeds/videos.xml?user=carykh");
        AtomFeed feed = AtomFeed.Create(feedUrl, settings);
        var videos = feed.Entries;

        bool alreadyPosted = false;

        if (videos.Count() == 0)
            Console.WriteLine("[Error] Feed contained no information.");

        foreach (var video in videos)
        {
            string videoUrlsFile = Directory.GetCurrentDirectory() + "\\videoUrls.txt"; // String for the video URLS to check if a post is new, uses "videoUrls.txt" in directory the .exe is run from by default
            var logFile = File.ReadAllLines(videoUrlsFile);
            List<string> videoUrls = new List<string>(logFile);

            string newVideoUrl = video.Links.FirstOrDefault().Uri.ToString();
            string videoTitle = video.Title.Content;
            Console.WriteLine($"newVideoUrl has been grabbed, it is: {newVideoUrl}");

            foreach (var videoUrl in videoUrls)
            {
                if (newVideoUrl == videoUrl)
                {
                    alreadyPosted = true;
                }
            }

            try
            {
                if (alreadyPosted == false)
                {
                    Console.WriteLine($"Found new video URL - {newVideoUrl} (\"{videoTitle}\") - Sending to discord");

                    using (StreamWriter text = File.AppendText(videoUrlsFile))
                        text.WriteLine(newVideoUrl);

                    _client.GetServer(184755239952318464).GetChannel(185111014671515649).SendMessage("@everyone `carykh` has uploaded a new YouTube video!\n" +
                                                                                                    $"\"{videoTitle}\" - {newVideoUrl}");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[Error] Bot ran into an issue while trying to post the video to discord. {error.ToString()}");
            }
        }

    }

    private static void jnjVideoChecker(object source, ElapsedEventArgs e)
    {
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        settings.RetrievalLimit = 1;

        Uri feedUrl = new Uri("https://www.youtube.com/feeds/videos.xml?user=jacknjellify");
        AtomFeed feed = AtomFeed.Create(feedUrl, settings);
        var videos = feed.Entries;

        bool alreadyPosted = false;

        if (videos.Count() == 0)
            Console.WriteLine("[Error] Feed contained no information.");

        foreach (var video in videos)
        {
            string videoUrlsFile = Directory.GetCurrentDirectory() + "\\videoUrls.txt"; // String for the video URLS to check if a post is new, uses "videoUrls.txt" in directory the .exe is run from by default
            var logFile = File.ReadAllLines(videoUrlsFile);
            List<string> videoUrls = new List<string>(logFile);

            string newVideoUrl = video.Links.FirstOrDefault().Uri.ToString();
            string videoTitle = video.Title.Content;
            Console.WriteLine($"newVideoUrl has been grabbed, it is: {newVideoUrl}");

            foreach (var videoUrl in videoUrls)
                if (newVideoUrl == videoUrl)
                    alreadyPosted = true;

            try
            {
                if (alreadyPosted == false)
                {
                    Console.WriteLine($"Found new video URL - {newVideoUrl} (\"{videoTitle}\") - Sending to discord");

                    using (StreamWriter text = File.AppendText(videoUrlsFile))
                        text.WriteLine(newVideoUrl);

                    _client.GetServer(184755239952318464).GetChannel(185111014671515649).SendMessage("@everyone `jacknjellify` has uploaded a new YouTube video!\n" +
                                                                                                    $"\"{videoTitle}\" - {newVideoUrl}");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[Error] Bot ran into an issue while trying to post the video to discord. {error.ToString()}");
            }
        }

    }

    private static void abaVideoChecker(object source, ElapsedEventArgs e)
    {
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        settings.RetrievalLimit = 1;

        Uri feedUrl = new Uri("https://www.youtube.com/feeds/videos.xml?user=1abacaba1");
        AtomFeed feed = AtomFeed.Create(feedUrl, settings);
        var videos = feed.Entries;

        bool alreadyPosted = false;

        if (videos.Count() == 0)
            Console.WriteLine("[Error] Feed contained no information.");

        foreach (var video in videos)
        {
            string videoUrlsFile = Directory.GetCurrentDirectory() + "\\videoUrls.txt"; // String for the video URLS to check if a post is new, uses "videoUrls.txt" in directory the .exe is run from by default
            var logFile = File.ReadAllLines(videoUrlsFile);
            List<string> videoUrls = new List<string>(logFile);

            string newVideoUrl = video.Links.FirstOrDefault().Uri.ToString();
            string videoTitle = video.Title.Content;
            Console.WriteLine($"newVideoUrl has been grabbed, it is: {newVideoUrl}");

            foreach (var videoUrl in videoUrls)
                if (newVideoUrl == videoUrl)
                    alreadyPosted = true;

            try
            {
                if (alreadyPosted == false)
                {
                    Console.WriteLine($"Found new video URL - {newVideoUrl} (\"{videoTitle}\") - Sending to discord");

                    using (StreamWriter text = File.AppendText(videoUrlsFile))
                        text.WriteLine(newVideoUrl);

                    _client.GetServer(184755239952318464).GetChannel(227551939112468480).SendMessage("`abacaba` has uploaded a new YouTube video!\n" +
                                                                                                    $"\"{videoTitle}\" - {newVideoUrl}");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[Error] Bot ran into an issue while trying to post the video to discord. {error.ToString()}");
            }
        }

    }

    private static void cbrVideoChecker(object source, ElapsedEventArgs e)
    {
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        settings.RetrievalLimit = 1;

        Uri feedUrl = new Uri("https://www.youtube.com/feeds/videos.xml?channel_id=UCzyUqm2SsmCHHlDeFQgYcEA");
        AtomFeed feed = AtomFeed.Create(feedUrl, settings);
        var videos = feed.Entries;

        bool alreadyPosted = false;

        if (videos.Count() == 0)
            Console.WriteLine("[Error] Feed contained no information.");

        foreach (var video in videos)
        {
            string videoUrlsFile = Directory.GetCurrentDirectory() + "\\videoUrls.txt"; // String for the video URLS to check if a post is new, uses "videoUrls.txt" in directory the .exe is run from by default
            var logFile = File.ReadAllLines(videoUrlsFile);
            List<string> videoUrls = new List<string>(logFile);

            string newVideoUrl = video.Links.FirstOrDefault().Uri.ToString();
            string videoTitle = video.Title.Content;
            Console.WriteLine($"newVideoUrl has been grabbed, it is: {newVideoUrl}");

            foreach (var videoUrl in videoUrls)
                if (newVideoUrl == videoUrl)
                    alreadyPosted = true;

            try
            {
                if (alreadyPosted == false)
                {
                    Console.WriteLine($"Found new video URL - {newVideoUrl} (\"{videoTitle}\") - Sending to discord");

                    using (StreamWriter text = File.AppendText(videoUrlsFile))
                        text.WriteLine(newVideoUrl);

                    _client.GetServer(184755239952318464).GetChannel(227551939112468480).SendMessage("`Cube Roll` has uploaded a new YouTube video!\n" +
                                                                                                    $"\"{videoTitle}\" - {newVideoUrl}");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[Error] Bot ran into an issue while trying to post the video to discord. {error.ToString()}");
            }
        }

    }

    private static void frnVideoChecker(object source, ElapsedEventArgs e)
    {
        SyndicationResourceLoadSettings settings = new SyndicationResourceLoadSettings();
        settings.RetrievalLimit = 1;

        Uri feedUrl = new Uri("https://www.youtube.com/feeds/videos.xml?user=fernozzle");
        AtomFeed feed = AtomFeed.Create(feedUrl, settings);
        var videos = feed.Entries;

        bool alreadyPosted = false;

        if (videos.Count() == 0)
            Console.WriteLine("[Error] Feed contained no information.");

        foreach (var video in videos)
        {
            string videoUrlsFile = Directory.GetCurrentDirectory() + "\\videoUrls.txt"; // String for the video URLS to check if a post is new, uses "videoUrls.txt" in directory the .exe is run from by default
            var logFile = File.ReadAllLines(videoUrlsFile);
            List<string> videoUrls = new List<string>(logFile);

            string newVideoUrl = video.Links.FirstOrDefault().Uri.ToString();
            string videoTitle = video.Title.Content;
            Console.WriteLine($"newVideoUrl has been grabbed, it is: {newVideoUrl}");

            foreach (var videoUrl in videoUrls)
                if (newVideoUrl == videoUrl)
                    alreadyPosted = true;

            try
            {
                if (alreadyPosted == false)
                {
                    Console.WriteLine($"Found new video URL - {newVideoUrl} (\"{videoTitle}\") - Sending to discord");

                    using (StreamWriter text = File.AppendText(videoUrlsFile))
                        text.WriteLine(newVideoUrl);

                    _client.GetServer(184755239952318464).GetChannel(227551939112468480).SendMessage("`fernozzle` has uploaded a new YouTube video!\n" +
                                                                                                    $"\"{videoTitle}\" - {newVideoUrl}");
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"[Error] Bot ran into an issue while trying to post the video to discord. {error.ToString()}");
            }
        }

    }
}
