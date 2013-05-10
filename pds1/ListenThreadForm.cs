using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Pipes;

namespace pds1
{
    static class ListenThreadForm
    {
        
        static public void InstanceMethod()
        {
            Console.WriteLine("ListenThreadForm.InstanceMethod is running on another thread.");


            // Pause for a moment to provide a delay to make 
            // threads more apparent.
            Thread.Sleep(1000);
            Console.WriteLine("The instance method (Form) called by the worker thread has ended.");
        }
    }
}
