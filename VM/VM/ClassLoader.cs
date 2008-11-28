using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sekhmet;
using vml = VMILLib;
using vmo = VM.VMObjects;
using VM.VMObjects;

namespace VM {
	class ClassLoader : IDisposable {
		bool disposed;
		vml.SourceReader reader;
		Handle<vmo.VMILMessageHandler> entrypoint;

		public ClassLoader( vml.SourceReader reader ) {
			this.reader = reader;
		}

		public ClassLoader( Stream input ) : this( new vml.SourceReader( input ) ) { }
		public ClassLoader( string fileName ) : this( new vml.SourceReader( fileName ) ) { }

		/// <summary>
		/// Reads the classes stored in input into the virtual machine.
		/// </summary>
		/// <returns>The entrypoint specified in the input if any.</returns>
		public Handle<vmo.VMILMessageHandler> Read() {
			reader.Read().Classes.ForEach( c => VirtualMachine.RegisterClass( ReadClass( null, c ) ) );

			return entrypoint;
		}

		Handle<vmo.Class> ReadClass( Handle<Class> parentClass, vml.Class lc ) {
			var oc = vmo.Class.CreateInstance( lc.SuperClasses.Count, lc.Handlers.Count, lc.InnerClasses.Count );

			var handlers = new List<Handle<vmo.MessageHandlerBase>>();
			lc.Handlers.ForEach( h => handlers.Add( ReadMessageHandler( oc, h ) ) );

			var innerClasses = new List<Handle<vmo.Class>>();
			lc.InnerClasses.ForEach( c => innerClasses.Add( ReadClass( oc, c ) ) );

			oc.InitInstance( lc.Visibility, lc.Name.ToVMString().Intern(), parentClass, lc.SuperClasses.Select( sc => sc.ToVMString().Intern() ).ToList(), 
				lc.Fields.Count, lc.DefaultHandler != null ? ReadMessageHandler( oc, lc.DefaultHandler ) : null, handlers, innerClasses );

			return oc;
		}

		Handle<vmo.MessageHandlerBase> ReadMessageHandler( Handle<Class> cls, vml.MessageHandlerBase lh ) {
			Handle<vmo.MessageHandlerBase> h;
			if (lh.IsExternal) {
				var leh = (vml.ExternalMessageHandler) lh;
				var oh = vmo.DelegateMessageHandler.CreateInstance();
				oh.InitInstance( lh.Name.ToVMString().Intern(), lh.Visibility, cls, lh.IsEntrypoint, lh.Arguments.Count, leh.ExternalName.ToVMString().Intern() );
				h = oh.To<MessageHandlerBase>();
			} else {
				var lvh = (vml.VMILMessageHandler) lh;
				var vh = vmo.VMILMessageHandler.CreateInstance( lvh.Instructions.Count );

				vh.InitInstance( lvh.Name.ToVMString().Intern(), lvh.Visibility, cls, lvh.IsEntrypoint, lvh.Arguments.Count, lvh.Locals.Count, ReadInstructions( lvh.Instructions ) );

				if (vh.IsEntrypoint())
					entrypoint = vh;
				h = vh.To<MessageHandlerBase>();
			}

			return h;
		}

		IList<Word> ReadInstructions( vml.InstructionList inssList ) {
			var labelMap = new Dictionary<string, int>();
			inssList.ForEach( ( ins, i ) => { if (ins is vml.Label) labelMap.Add( ((vml.Label) ins).Name, i - labelMap.Count ); } );
			var inss = inssList.Where( i => !(i is vml.Label) );
			var retInss = new List<Word>();

			var trycatchMap = MapTryCatches( inss );
			var trycatchStack = new Stack<TryCatchRecord>();

			int index = 0;
			foreach (var ins in inss) {
				Word eins = (uint) ins.OpCode << 27;
				switch (ins.OpCode) {
					case vml.OpCode.StoreField:
					case vml.OpCode.LoadField:
						eins |= (uint) ins.MessageHandler.Class.Fields.IndexOf( (string) ins.Operand );
						break;
					case vml.OpCode.StoreLocal:
					case vml.OpCode.LoadLocal:
						eins |= (uint) ins.MessageHandler.Locals.IndexOf( (string) ins.Operand );
						break;
					case vml.OpCode.LoadArgument:
						eins |= (uint) ins.MessageHandler.Arguments.IndexOf( (string) ins.Operand ) + 1;
						break;
					case vml.OpCode.LoadThis:
						eins = (uint) vml.OpCode.LoadArgument << 27;
						break;
					case vml.OpCode.PushLiteral:
						if (ins.Operand is string)
							eins = ((uint) vml.OpCode.PushLiteralString << 27) | (uint) ((string) ins.Operand).ToVMString().Intern().GetInternIndex();
						else {
							int i = (int) ins.Operand;
							uint ai = i == int.MinValue ? (uint) i : (uint) Math.Abs( i );
							if (ai > 0x03FFFFFF) {
								uint i1 = (ai >> 16) & 0x0000FFFF;
								uint i2 = ai & 0x0000FFFF;
								eins = ((uint) vml.OpCode.PushLiteralInt << 27) | (uint) (i < 0 ? 1 << 26 : 0) | i1;
								retInss.Add( eins );
								eins = ((uint) vml.OpCode.PushLiteralIntExtend << 27) | (uint) (i < 0 ? 1 << 26 : 0) | i2;
							} else
								eins = ((uint) vml.OpCode.PushLiteralInt << 27) | (uint) (i < 0 ? 1 << 26 : 0) | (uint) Math.Abs( i );
						}
						break;
					case vml.OpCode.Pop:
					case vml.OpCode.Dup:
					case vml.OpCode.NewInstance:
					case vml.OpCode.SendMessage:
					case vml.OpCode.ReturnVoid:
					case vml.OpCode.Return:
					case vml.OpCode.Throw:
						break;
					case vml.OpCode.Jump:
					case vml.OpCode.JumpIfTrue:
					case vml.OpCode.JumpIfFalse:
						var offset = labelMap[(string) ins.Operand] - index;
						if (offset < 0) {
							offset *= -1;
							eins |= 0x04000000;
						}
						eins |= (uint) offset;
						break;
					case vml.OpCode.Try:
						var tryrecord = trycatchMap[index];
						trycatchStack.Push( tryrecord );
						eins |= (uint) (tryrecord.CatchIndex + 1 - index);
						break;
					case vml.OpCode.Catch:
						eins |= (uint) (trycatchStack.Peek().EndTryCatch + 1 - index);
						break;
					case vml.OpCode.EndTryCatch:
						trycatchStack.Pop();
						break;
					default:
						throw new ArgumentException( "Unexpected opcode : " + ins.OpCode );
				}

				retInss.Add( eins );
				index++;
			}

			return retInss;
		}

		Dictionary<int, TryCatchRecord> MapTryCatches( IEnumerable<vml.Instruction> inss ) {
			var stack = new Stack<TryCatchRecord>();
			var map = new Dictionary<int, TryCatchRecord>();

			int index = 0;
			foreach (var ins in inss) {
				if (ins.OpCode == vml.OpCode.Try) {
					stack.Push( new TryCatchRecord { TryIndex = index } );
					map.Add( index, stack.Peek() );
				}

				if (ins.OpCode == vml.OpCode.Catch)
					stack.Peek().CatchIndex = index;

				if (ins.OpCode == vml.OpCode.EndTryCatch) {
					stack.Peek().EndTryCatch = index;
					stack.Pop();
				}

				index++;
			}

			return map;
		}

		class TryCatchRecord {
			public int TryIndex;
			public int CatchIndex;
			public int EndTryCatch;
		}

		public void Dispose() {
			lock (this) {
				if (disposed)
					return;
			}
			disposed = true;

			if (reader != null) {
				reader.Dispose();
				reader = null;
			}
		}
	}
}
