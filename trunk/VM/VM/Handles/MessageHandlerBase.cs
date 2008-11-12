using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.Handles {
	public struct MessageHandlerBase {
		public const uint MESSAGEHANDLER_HEADER_OFFSET = 1;
		public const uint VISIBILITY_MASK = 0x00000003;
		public const uint IS_INTERNAL_MASK = 0x00000004;
		public const int IS_INTERNAL_RSHIFT = 3;
		public const int NAME_RSHIFT = 4;

		uint start;

		public static implicit operator uint( MessageHandlerBase v ) {
			return v.start;
		}

		public static implicit operator MessageHandlerBase( uint v ) {
			return new MessageHandlerBase { start = v };
		}

		public static explicit operator VMILMessageHandler( MessageHandlerBase v ) {
			return (uint) v;
		}

		public static explicit operator DelegateMessageHandler( MessageHandlerBase v ) {
			return (uint) v;
		}

		public static implicit operator MessageHandlerBase( VMILMessageHandler v ) {
			return (uint) v;
		}

		public static implicit operator MessageHandlerBase( DelegateMessageHandler v ) {
			return (uint) v;
		}
	}

	public static class ExtMessageHandlerBase {
		public static VisibilityModifier Visibility( this MessageHandlerBase messageHandler ) {
			return (VisibilityModifier) (messageHandler.Get( MessageHandlerBase.MESSAGEHANDLER_HEADER_OFFSET ) & MessageHandlerBase.VISIBILITY_MASK);
		}

		public static VisibilityModifier Visibility( this VMILMessageHandler messageHandler ) {
			return (VisibilityModifier) (messageHandler.Get( MessageHandlerBase.MESSAGEHANDLER_HEADER_OFFSET ) & MessageHandlerBase.VISIBILITY_MASK);
		}

		public static VisibilityModifier Visibility( this DelegateMessageHandler messageHandler ) {
			return (VisibilityModifier) (messageHandler.Get( MessageHandlerBase.MESSAGEHANDLER_HEADER_OFFSET ) & MessageHandlerBase.VISIBILITY_MASK);
		}

		public static bool IsInternal( this MessageHandlerBase messageHandler ) {
			return ((messageHandler.Get( MessageHandlerBase.MESSAGEHANDLER_HEADER_OFFSET ) & MessageHandlerBase.IS_INTERNAL_MASK) >> MessageHandlerBase.IS_INTERNAL_RSHIFT) != 0;
		}

		public static bool IsInternal( this VMILMessageHandler messageHandler ) {
			return false;
		}

		public static bool IsInternal( this DelegateMessageHandler messageHandler ) {
			return true;
		}

		/// <summary>
		/// Retrieves a pointer to the name of the message handler found at <c>messageHandlerBase</c>.
		/// </summary>
		/// <param name="vm">The virtual machine.</param>
		/// <param name="messageHandlerBase">A pointer to a message handler.</param>
		/// <returns>A pointer to the name of <c>messageHandler</c>.</returns>
		public static uint Name( this MessageHandlerBase messageHandler ) {
			return messageHandler.Get( MessageHandlerBase.MESSAGEHANDLER_HEADER_OFFSET ) >> MessageHandlerBase.NAME_RSHIFT;
		}

		public static uint Name( this VMILMessageHandler messageHandler ) {
			return messageHandler.Get( MessageHandlerBase.MESSAGEHANDLER_HEADER_OFFSET ) >> MessageHandlerBase.NAME_RSHIFT;
		}

		public static uint Name( this DelegateMessageHandler messageHandler ) {
			return messageHandler.Get( MessageHandlerBase.MESSAGEHANDLER_HEADER_OFFSET ) >> MessageHandlerBase.NAME_RSHIFT;
		}
	}
}
