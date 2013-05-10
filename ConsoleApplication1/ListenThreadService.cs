using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleApplication1
{
    static class ListenThreadService
    {
        static public void InstanceMethod()
        {
            Console.WriteLine("ListenThreadService.InstanceMethod is running on another thread.");




            // Pause for a moment to provide a delay to make 
            // threads more apparent.
            Thread.Sleep(100);
            Console.WriteLine("The instance method (Service) called by the worker thread has ended.");
        }
    }
}
