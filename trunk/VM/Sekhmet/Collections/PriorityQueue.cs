using System;
using System.Collections.Generic;

namespace Sekhmet.Collections {
	/// <summary>
	/// Interface for prioerity queues.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys used.</typeparam>
	/// <typeparam name="TValue">The type of the values.</typeparam>
	public interface IPriorityQueue<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : IComparable<TKey> {
		/// <summary>
		/// Enqueues a new item.
		/// </summary>
		void Enqueue( TKey key, TValue value );
		/// <summary>
		/// Dequeues the value with the lowest key.
		/// </summary>
		/// <returns>The value of the item with the lowest key.</returns>
		TValue Dequeue();
		/// <summary>
		/// Peeks the value with the lowest key.
		/// </summary>
		/// <returns>The value of the item with the lowest key.</returns>
		TValue Peek();
		/// <summary>
		/// Gets the number of items in the queue.
		/// </summary>
		int Count { get; }
	}

	/// <summary>
	/// Default implementation of the priority queue interface.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys used.</typeparam>
	/// <typeparam name="TValue">The type of the values.</typeparam>
	public class PriorityQueue<TKey, TValue> : IPriorityQueue<TKey, TValue> where TKey : IComparable<TKey> {
		List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>();

		/// <summary>
		/// Creates a new instance of <see cref="T:PriorityQueue"/>.
		/// </summary>
		public PriorityQueue() { }

		void SwitchElements( int i, int j ) {
			var h = list[i];
			list[i] = list[j];
			list[j] = h;
		}

		int Compare( int i, int j ) {
			return list[i].Key.CompareTo( list[j].Key );
		}

		/// <summary>
		/// Enqueues a new item.
		/// </summary>
		public void Enqueue( TKey key, TValue value ) {
			int p = list.Count, p2;
			list.Add( new KeyValuePair<TKey, TValue>( key, value ) ); // E[p] = O
			do {
				if (p == 0)
					break;
				p2 = (p - 1) / 2;
				if (Compare( p, p2 ) < 0) {
					SwitchElements( p, p2 );
					p = p2;
				} else
					break;
			} while (true);
		}

		/// <summary>
		/// Dequeues the value with the lowest key.
		/// </summary>
		/// <returns>The value of the item with the lowest key.</returns>
		public TValue Dequeue() {
			if (Count == 0)
				throw new InvalidOperationException( "The queue is empty." );

			var result = list[0];
			int p = 0, p1, p2, pn;
			list[0] = list[list.Count - 1];
			list.RemoveAt( list.Count - 1 );
			do {
				pn = p;
				p1 = 2 * p + 1;
				p2 = 2 * p + 2;
				if (list.Count > p1 && Compare( p, p1 ) > 0) // links kleiner
					p = p1;
				if (list.Count > p2 && Compare( p, p2 ) > 0) // rechts noch kleiner
					p = p2;

				if (p == pn)
					break;
				SwitchElements( p, pn );
			} while (true);
			return result.Value;
		}

		/// <summary>
		/// Peeks the value with the lowest key.
		/// </summary>
		/// <returns>The value of the item with the lowest key.</returns>
		public TValue Peek() {
			if (list.Count > 0)
				return list[0].Value;
			throw new InvalidOperationException( "The queue is empty." );
		}

		/// <summary>
		/// Gets the number of items in the queue.
		/// </summary>
		public int Count { get { return list.Count; } }

		/// <summary>
		/// Returns an enumerator that iterates through the queue.
		/// </summary>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
			return list.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
