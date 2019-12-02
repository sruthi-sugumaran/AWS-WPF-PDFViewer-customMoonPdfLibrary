using _300983145_sruthi__Lab2;
using MoonPdfLib.Virtualizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _300983145_Sruthi__Lab2
{
    class Subscriber
    {
        ViewBook instance;
        public Subscriber()
        {

        }
        public Subscriber(ViewBook instance)
        {
            this.instance = instance;
        }
        // This function listen to the event if it is raised by the Publisher
        public void Listener(SingletonPublisherClass P)
        {
            P.EventTicked += P_EventTicked;
        }
        // It get executed when the event fired by the Publisher
        void P_EventTicked(SingletonPublisherClass P, BroadCast e)
        {
            Console.WriteLine("Current Page: " + e.Page_Number + " at " + e.BroadCast_Date);
            if (instance != null) instance.bookmark(e.Page_Number);
        }
    }
}
