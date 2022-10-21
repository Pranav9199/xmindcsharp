﻿using System.IO;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using System.Text;
using XMindAPI;
using System.Collections.Generic;

namespace simple
{
    class Program
    {
        public static ILogger<Program> Logger { get; private set; }

        public static TextWriter Out { get; set; } = Console.Out;

        static async Task Main(string demo = "file-2")
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .BuildServiceProvider();

            Logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();

            using var eventSourceListener = new EventSourceListener("XMind-XMindCsharpEventSource");
            switch (demo)
            {
                case "file":
                    await SaveWorkBookToFileSystem();
                    break;
                case "file-2":
                    await SaveWorkBookToFileSystem_Example2();
                    break;
                case "memory":
                    await InMemoryWorkBook();
                    break;
            }
        }

        private async static Task InMemoryWorkBook()
        {
            var book = new XMindConfiguration()
                .WithInMemoryWriter()
                .CreateWorkBook("test.xmind");
            await book.Save();
        }

        private static async Task SaveWorkBookToFileSystem()
        {
            // string basePath = Path.Combine(Path.GetTempPath(), "xmind-test");
            string basePath = Path.Combine("xmind-test");
            var bookName = "test.xmind";
            Logger.LogInformation(default(EventId), $"Base path: ${Path.Combine(basePath, bookName)}");
            var book = new XMindConfiguration()
                .WithFileWriter(basePath, zip: true)
                .CreateWorkBook(bookName);
            var sheet = book.GetPrimarySheet();
            var rootTopic = sheet.GetRootTopic();
            rootTopic.SetTitle("RootTopic");
            await book.Save();
        }
        private static async Task SaveWorkBookToFileSystem_Example2()
        {
            // string basePath = Path.Combine(Path.GetTempPath(), "xmind-test");
            string basePath = Path.Combine("xmind-test\\testtr");
            var bookName = "test.xmind";
            Logger.LogInformation(default(EventId), $"Base path: ${Path.Combine(basePath, bookName)}");
            var book = new XMindConfiguration()
                .WithFileWriter(basePath, zip: true)
                .CreateWorkBook(bookName);
            
            var sheet = book.GetPrimarySheet();
            var rootTopic = sheet.GetRootTopic();
            rootTopic.SetTitle("RootTopic");
            var asd = File.ReadAllBytes("C:\\Users\\PranavB\\Desktop\\attachments\\testt.jpg");
            var asd2 = File.ReadAllBytes("C:\\Users\\PranavB\\Desktop\\attachments\\test2.jpg");
            rootTopic.AddImage(asd, "mm1.jpg");
            var newTopic = book.CreateTopic("ChildTopic");
            rootTopic.Add(newTopic);
            newTopic.IsFolded = true;
            newTopic.HyperLink ="http://google.com";
            newTopic.AddLabel("notesss");
            newTopic.AddImage(asd2, "mm2.jpg");
            var myList = new List<KeyValuePair<string, string>>();

            // adding elements
            myList.Add(new KeyValuePair<string, string>("Laptop", "asdasdasd"));
            myList.Add(new KeyValuePair<string, string>("Desktop", "adczxvbtryn"));
            myList.Add(new KeyValuePair<string, string>("Tablet", "gggggggggggggggggg"));
            newTopic.AddNotes(myList);
            newTopic.AddMarker("priority-1");
            newTopic.AddMarker("task-half");
            var foldedTopic = book.CreateTopic("Folded");
            newTopic.Add(foldedTopic);
            newTopic.Add(foldedTopic);
            await book.Save();
        }
    }
    sealed class EventSourceListener : EventListener
    {
        private readonly string _eventSourceName;
        private readonly StringBuilder _messageBuilder = new StringBuilder();

        public EventSourceListener(string name)
        {
            _eventSourceName = name;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);

            if (eventSource.Name == _eventSourceName)
            {
                EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All);
            }
        }
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            base.OnEventWritten(eventData);

            string message;
            lock (_messageBuilder)
            {
                // _messageBuilder.Append("Event ");
                // _messageBuilder.Append(eventData.EventSource.Name);
                _messageBuilder.Append("\t");
                _messageBuilder.Append(eventData.EventName);
                _messageBuilder.Append(" : ");
                _messageBuilder.AppendJoin(',', eventData.Payload);
                message = _messageBuilder.ToString();
                _messageBuilder.Clear();
            }
            Console.WriteLine(message);
        }
    }
}
