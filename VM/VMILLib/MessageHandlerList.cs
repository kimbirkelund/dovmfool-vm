using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class MessageHandlerList : IEnumerable<MessageHandlerBase> {
		public readonly int Count;
		Dictionary<CString, MessageHandlerBase> handlers;

		public MessageHandlerBase this[CString name] {
			get {
				if (handlers.ContainsKey( name ))
					return handlers[name];
				return null;
			}
		}

		public MessageHandlerList( IEnumerable<MessageHandlerBase> handlers ) {
			this.handlers = handlers.ToDictionary( c => c.Name );
			this.Count = this.handlers.Count;
		}

		public IEnumerator<MessageHandlerBase> GetEnumerator() {
			return handlers.Values.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public override string ToString() {
			return "Message handler count: " + Count;
		}
	}
}
