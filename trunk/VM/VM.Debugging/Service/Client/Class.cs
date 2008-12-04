using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using VMILLib;

namespace VM.Debugging.Service.Client {
	[DataContract]
	public sealed class Class {
		[DataMember]
		public string Filename { get; private set; }

		[DataMember]
		public string Name { get; private set; }

		[DataMember]
		public string FullName { get; private set; }

		[DataMember]
		public VisibilityModifier Visibility { get; private set; }

		[DataMember]
		public IEnumerable<string> SuperClassNames { get; private set; }

		[DataMember]
		public MessageHandler DefaultMessageHandler { get; private set; }

		[DataMember]
		public IEnumerable<MessageHandler> MessageHandlers { get; private set; }

		[DataMember]
		public IEnumerable<Class> InnerClasses { get; private set; }
	}
}
