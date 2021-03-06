﻿using System;
using System.Activities;
using System.ComponentModel;
using System.Xml.Linq;

using Twilio.Activities.Design;

namespace Twilio.Activities
{

    /// <summary>
    /// Produces a dial body to dial a queue.
    /// </summary>
    [Designer(typeof(DialQueueDesigner))]
    public sealed class DialQueue : DialNoun
    {

        /// <summary>
        /// Number to dial.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> Name { get; set; }

        /// <summary>
        /// Activities to be executed for the calling party before the call is connected.
        /// </summary>
        [Browsable(false)]
        public Activity Pickup { get; set; }

        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        protected override void ExecuteNoun(NativeActivityContext context)
        {
            var twilio = context.GetExtension<ITwilioContext>();
            var name = Name.Get(context);

            // add new Number element
            var element = new XElement("Queue",
                name);
            GetElement(context).Add(element);

            // bookmark to execute Caller activity
            if (Pickup != null)
                element.Add(new XAttribute("url",
                    twilio.ResolveBookmarkUrl(context.CreateTwilioBookmark(OnPickup))));
        }

        /// <summary>
        /// Invoked when the called party uri is requested.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bookmark"></param>
        /// <param name="value"></param>
        void OnPickup(NativeActivityContext context, Bookmark bookmark, object value)
        {
            context.ScheduleActivity(Pickup, OnPickupCompleted);
        }

        void OnPickupCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (completedInstance.State != ActivityInstanceState.Executing)
                return;

            if (!GetElement(context).HasElements)
                GetElement(context).Add(new XElement("Pause",
                    new XAttribute("length", 0)));
        }

    }

}
