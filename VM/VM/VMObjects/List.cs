//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using VMILLib;

//namespace VM.VMObjects {
//    public struct List : IVMObject<List> {
//        #region Constants
//        public const int ARRAY_SIZE_OFFSET = 1;
//        public const int LIST_COUNT_OFFSET = 2;
//        public const int ARRAY_OFFSET = 3;
//        #endregion

//        #region Properties
//        int start;
//        public int Start { get { return start; } }
//        #endregion

//        #region Cons
//        public List( int start ) {
//            this.start = start;
//        }

//        public List New( int start ) {
//            return new List( start );
//        }
//        #endregion

//        #region Casts
//        public static implicit operator int( List list ) {
//            return list.start;
//        }

//        public static explicit operator List( int list ) {
//            return new List { start = list };
//        }
//        #endregion

//        #region Instance methods
//        public override string ToString() {
//            return ExtList.ToString( this );
//        }
//        #endregion

//        #region Static method
//        public static Handle<List> CreateInstance( int initialSize ) {
//            var list = VirtualMachine.MemoryManager.Allocate<List>( 3 );

//            list[ARRAY_SIZE_OFFSET] = initialSize;
//            list[LIST_COUNT_OFFSET] = 0;
//            list[ARRAY_OFFSET] = Array.CreateInstance( initialSize );

//            return list;
//        }

//        public static Handle<List> CreateInstance() {
//            return CreateInstance( 8 );
//        }
//        #endregion
//    }

//    public static class ExtList {
//        public static int Count( this Handle<List> obj ) { return obj[LIST_COUNT_OFFSET]; }
//        static void Count( this Handle<List> obj, int value ) { obj[LIST_COUNT_OFFSET] = value; }

//        public int Capacity( this Handle<List> obj ) { return obj[ARRAY_SIZE_OFFSET]; }
//        static int Capacity( this Handle<List> obj ) { obj[ARRAY_SIZE_OFFSET] = value; }

//        public static int Add<T>( this Handle<List> obj, Word value, bool isReference ) {
//            if (obj.Count() >= obj.Capacity())
//                obj.Expand();

//            ((Array) obj[ARRAY_OFFSET]).ToHandle().Set( Count, value, isReference );
//            return Count++;
//        }

//        public static void Set( this Handle<List> obj, int index, Word value, bool isReference ) {
//            if (index >= obj.Count())
//                throw new ArgumentOutOfBoundsException( "index" );

//            ((Array) this[ARRAY_OFFSET]).ToHandle().Set( index, value, isReference );
//        }

//        public static Word Get( this Handle<List> obj, int index ) {
//            if (index >= obj.Count())
//                throw new ArgumentOutOfBoundsException( "index" );

//            return ((Array) obj[ARRAY_OFFSET]).ToHandle().Get( index );
//        }

//        public static void Remove<T>( this Handle<List> obj, Word value ) {
//            var index = obj.IndexOf( obj );
//            if (index != -1)
//                obj.RemoveAt( index );
//        }

//        public static void RemoveAt( this Handle<List> obj, int index ) {
//            if (index < 0 || obj.Count() <= index)
//                throw new ArgumentOutOfBoundsException( "index" );

//            Array.Copy( (Array) obj[ARRAY_OFFSET], index + 1, (Array) obj[ARRAY_OFFSET], index, Count - index - 1 );
//        }

//        public static void InsertAt<T>( this Handle<List> obj, int index, Word value ) {
//            if (index < 0 || obj.Count() < index)
//                throw new ArgumentOutOfBoundsException( "index" );

//            obj.Add( obj.Get<ObjectBase>( obj.Count() - 1 ) );
//            Array.CopyDescending( (Array) obj[ARRAY_OFFSET], index, (Array) obj[ARRAY_OFFSET], index + 1, Count - 1 );
//            obj.Set( index, value );
//        }

//        public static int IndexOf( this Handle<List> obj, Word value ) {
//            for (var i = 0; i < Count; i++)
//                if (Get<ObjectBase>( i ).Start == obj.Start)
//                    return i;

//            return -1;
//        }

//        public Array ToArray() {
//            Trim();
//            return (Array) this[ARRAY_OFFSET];
//        }

//        public void Trim() {
//            if (Capacity == Count)
//                return;
//            var oldArr = (Array) this[ARRAY_OFFSET];
//            var newArr = Array.CreateInstance( Count );
//            Array.Copy( oldArr, 0, newArr, 0, Count );

//            this[ARRAY_OFFSET] = newArr;
//            Capacity = Count;
//        }

//        void Expand() {
//            var oldArr = (Array) this[ARRAY_OFFSET];
//            var newArr = Array.CreateInstance( Capacity * 2 );
//            this[ARRAY_OFFSET] = newArr;
//            Capacity *= 2;

//            Array.Copy( oldArr, 0, newArr, 0, Count );
//        }

//        public override string ToString() {
//            if (IsNull)
//                return "{NULL}";
//            return "List{Count: " + Count + "}";
//        }
//    }
//}
