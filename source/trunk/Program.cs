using System;
using System.Threading;
using System.Globalization;

namespace XNAPacMan {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        
        static void Main(string[] args) {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", true);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", true);
            using (XNAPacMan game = new XNAPacMan()) {
                game.Run();
            }
        }
    }
}

