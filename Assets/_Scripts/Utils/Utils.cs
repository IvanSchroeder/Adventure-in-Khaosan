using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ExtensionMethods {
	/// <summary>
	/// Extension methods for Unity.
	/// https://github.com/lordofduct/spacepuppy-unity-framework/tree/master/SpacepuppyBase/Utils
	/// https://github.com/rocketgames/Extension-Methods-for-Unity
	/// </summary>
	public static class Utils {
		/// Constants
		/// /// <summary>
		/// Number pi.
		/// </summary>
        public const float PI = 3.14159265358979f;
		/// /// <summary>
		/// PI / 2 OR 90 deg.
		/// </summary>
        public const float PI_2 = 1.5707963267949f;
		/// /// <summary>
		/// PI / 2 OR 60 deg.
		/// </summary>
        public const float PI_3 = 1.04719755119659666667f;
		/// /// <summary>
		/// PI / 4 OR 45 deg.
		/// </summary>
        public const float PI_4 = 0.785398163397448f;
		/// /// <summary>
		/// PI / 8 OR 22.5 deg.
		/// </summary>
        public const float PI_8 = 0.392699081698724f;
		/// /// <summary>
		/// PI / 16 OR 11.25 deg.
		/// </summary>
        public const float PI_16 = 0.196349540849362f;
		/// /// <summary>
		/// 2 * PI OR 180 deg.
		/// </summary>
        public const float TWO_PI = 6.28318530717959f;
		/// /// <summary>
		/// 3 * PI_2 OR 270 deg.
		/// </summary>
        public const float THREE_PI_2 = 4.71238898038469f;
		/// /// <summary>
		/// PI / 180.
		/// </summary>
		public const float DEG_TO_RAD = 0.0174532925199433f;
		/// /// <summary>
		/// 180.0 / PI.
		/// </summary>
		public const float RAD_TO_DEG = 57.2957795130823f;
		/// /// <summary>
		/// Single float average epsilon.
		/// </summary>
        public const float EPSILON = 0.0001f;

		public static WaitForEndOfFrame waitForEndOfFrame;

		// this...
		// public static event Action OnGameOver;
		// is the sames as this...
		// public delegate void OnGameOver();
		// public static event OnGameOver onGameOver;

		// for smooth step lerping : floa t = time / duration (percentageComplete) => t = t * t * (3f - 2f * t);

		// private float targetValue;
		// private float valueToChange;

		// void Start() {
		//     StartCoroutine(LerpFunction(targetValue, 5));
		// }

		// IEnumerator LerpFunction(float endValue, float duration) {
		//     float time = 0f;
		//     float startValue = valueToChange;

		//     while (time < duration) {
		//         valueToChange = Mathf.Lerp(startValue, endValue, time / duration);
		//         time += Time.deltaTime;

		//         yield return null;
		//     }
		//     valueToChange = endValue;
		// }

	/// <summary>
	/// Extensions for Camera.
	/// </summary>
	#region ===== Camera =====
		public static Camera GetMainCamera() => Camera.main;
		public static Camera GetMainCamera(this Camera mainCamera) => mainCamera = Camera.main;
		public static Camera GetMainCamera(this Component component) => Camera.main;

		public static Vector3 ScreenToWorld(this Camera camera, Vector3 position) {
			if (camera.orthographic) position.z = camera.nearClipPlane;
			return camera.ScreenToWorldPoint(position);
		}

		public static Vector2 ScreenToWorldV2(this Camera camera, Vector3 position) {
			if (camera.orthographic) position.z = camera.nearClipPlane;
			return camera.ScreenToWorldPoint(position).ToVector2();
		}
	#endregion

	/// <summary>
	/// Extensions for Mouse Input.
	/// </summary>
	#region ===== Mouse =====
		public static Vector3 GetMouseWorldPosition(this Camera camera) {
			Ray ray = camera.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out RaycastHit raycastHit)) {
				return raycastHit.point;
			}

			return Vector3.zero;
		}

		public static Vector3Int GetMouseCellPosition(this Camera camera, GridLayout gridLayout) {
			Vector3 mousePos = camera.GetMouseWorldPosition();
			Vector3Int cellCoordinate = gridLayout.WorldToCell(mousePos);
			return cellCoordinate;
		}
	#endregion

	/// <summary>
	/// Extensions for Scene Management.
	/// </summary>
	#region ===== Scene Management =====
		public static void RestartScene() {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		public static void RestartScene(this Component component) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		public static void LoadNextScene(this Component component) => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
	#endregion

	/// <summary>
	/// Coroutines Extensions.
	/// </summary>
	#region ===== Coroutines =====
		public static Coroutine Run(this MonoBehaviour owner, ref Coroutine coroutine, IEnumerator routine) {
			owner.StartCoroutine(routine);
			return coroutine;
		}

		public static Coroutine Stop(this MonoBehaviour owner, ref Coroutine coroutine) {
			if (coroutine != null) {
				owner.StopCoroutine(coroutine);
			}
			return coroutine;
		}

		public static Coroutine Reset(this MonoBehaviour owner, ref Coroutine coroutine, IEnumerator routine) {
			coroutine = owner.Stop(ref coroutine);
			owner.Run(ref coroutine, routine);
			return coroutine;
		}

		public static IEnumerator Run(this MonoBehaviour owner, ref IEnumerator routine) {
			owner.StartCoroutine(routine);
			return routine;
		}

		public static IEnumerator Stop(this MonoBehaviour owner, ref IEnumerator routine) {
			owner.StopCoroutine(routine);
			return routine;
		}

		public static IEnumerator Reset(this MonoBehaviour owner, ref IEnumerator routine) {
			owner.Stop(ref routine);
			owner.Run(ref routine);
			return routine;
		}

		public static CoroutineHandle RunCoroutine(this MonoBehaviour owner, IEnumerator routine) {
			return new CoroutineHandle(owner, routine);
		}

		public static Coroutine CreateAnimationCoroutine(this MonoBehaviour owner, float duration, Action<float> changeFunction, Action onComplete = null) {
			return owner.StartCoroutine(GenericAnimationRoutine(duration, changeFunction, onComplete));
		}

		public static IEnumerator GenericAnimationRoutine(float duration, Action<float> changeFunction, Action onComplete) {
			float elapsedTime = 0f;
			float progress = 0f;

			while (progress <= 1f) {
				changeFunction(progress);
				progress = elapsedTime / duration;
				elapsedTime += Time.unscaledDeltaTime;
				yield return null;
			}

			changeFunction(1f);
			onComplete?.Invoke();
		}
	#endregion

	#region ===== Async =====
		public static CancellationToken RefreshToken(ref CancellationTokenSource tokenSource) {
			tokenSource?.Cancel();
			tokenSource?.Dispose();
			tokenSource = null;
			tokenSource = new CancellationTokenSource();
			return tokenSource.Token;
		}

		public static CancellationToken RefreshToken(this MonoBehaviour mono, ref CancellationTokenSource tokenSource) {
			tokenSource?.Cancel();
			tokenSource?.Dispose();
			tokenSource = null;
			tokenSource = new CancellationTokenSource();
			return tokenSource.Token;
		}

		public static CancellationToken RefreshToken(this CancellationTokenSource tokenSource) {
			tokenSource?.Cancel();
			tokenSource?.Dispose();
			tokenSource = null;
			tokenSource = new CancellationTokenSource();
			return tokenSource.Token;
		}

		public static CancellationTokenSource RefreshTokenSource(this CancellationTokenSource tokenSource) {
			tokenSource?.Cancel();
			tokenSource?.Dispose();
			tokenSource = null;
			tokenSource = new CancellationTokenSource();
			return tokenSource;
		}
	#endregion

	/// <summary>
	/// Lists Extensions.
	/// </summary>
	#region ===== Lists =====
		public static void Swap<T>(this IList<T> list, int a, int b) {
			T temp = list[a];
			list[a] = list[b];
			list[b] = temp;
		}

		public static IEnumerable<T> Flip<T>(this ICollection<T> list) {
			var flipedList = new List<T>();

			for (int i = list.Count - 1; i >= 0; i--) {
				flipedList.Add(list.ElementAt(i));
			}

			return flipedList;
		}

		public static void AscendingShuffle<T>(this IList<T> list, int iterations = 1, bool guaranteeDiscontinuity = false) {
			int n = list.Count * iterations;

			T last = list.Last();

			for (int i = 0; i < n; i++) {
				for (int j = 0; j < list.Count; j++) {
					int randomIndex = 0;

					while (randomIndex == j) {
						randomIndex = Random.Range(0, list.Count);
					}

					list.Swap(j, randomIndex);
				}

				if (guaranteeDiscontinuity && list[0].NullSafeEquals(last)) {
					list.Swap( 0, list.Count - 1 );
				}
			}
		}

		public static void AscendingForwardShuffle<T>(this IList<T> list, int iterations = 1, bool guaranteeDiscontinuity = false) {
			int n = list.Count * iterations;

			T last = list.Last();

			for (int i = 0; i < n; i++) {
				for (int j = 0; j < list.Count - 1; j++) {
					int randomIndex = 0;

					while (randomIndex == j) {
						randomIndex = Random.Range(j + 1, list.Count);
					}

					list.Swap(j, randomIndex);
				}

				if (guaranteeDiscontinuity && list[0].NullSafeEquals(last)) {
					list.Swap( 0, list.Count - 1 );
				}
			}
		}

		public static void DescendingShuffle<T>(this IList<T> list, int iterations = 1, bool guaranteeDiscontinuity = false) {
			int n = list.Count * iterations;

			T last = list.Last();

			for (int i = 0; i < n; i++) {
				for (int j = list.Count - 1; j >= 0; j--) {
					int randomIndex = 0;

					while (randomIndex == j) {
						randomIndex = Random.Range(0, list.Count);
					}

					list.Swap(j, randomIndex);
				}

				if (guaranteeDiscontinuity && list[0].NullSafeEquals(last)) {
					list.Swap(0, list.Count - 1);
				}
			}
		}

		public static void DescendingReverseShuffle<T>(this IList<T> list, int iterations = 1, bool guaranteeDiscontinuity = false) {
			int n = list.Count * iterations;

			T last = list.Last();

			for (int i = 0; i < n; i++) {
				for (int j = list.Count - 1; j >= 1; j--) {
					int randomIndex = 0;

					while (randomIndex == j) {
						randomIndex = Random.Range(j - 1, list.Count);
					}

					list.Swap(j, randomIndex);
				}

				if (guaranteeDiscontinuity && list[0].NullSafeEquals(last)) {
					list.Swap(0, list.Count - 1);
				}
			}
		}

		public static int GetFirstIndex<T>(this IList<T> list) {
			int firstIndex = 0;
			return firstIndex;
		}

		public static int GetLastIndex<T>(this IList<T> list) {
			int lastIndex = list.Count - 1;
			return lastIndex;
		}

		public static int GetRandomIndex<T>(this IList<T> list) {
			int randomIndex = 0;

			if (list.Count >= 2) {
				randomIndex = Random.Range(0, list.Count - 1);
			}

			return randomIndex;
		}

		public static T GetFirstElement<T>(this IList<T> list) {
			var firstElement = list[list.GetFirstIndex()];
			return firstElement;
		}

		public static T GetLastElement<T>(this IList<T> list) {
			var lastElement = list[list.GetLastIndex()];
			return lastElement;
		}

		public static T GetRandomElement<T>(this IList<T> list) {
			var randomElement = list[list.GetRandomIndex()];
			return randomElement;
		}

		public static List<T> GetTrueRandomElements<T>(this IList<T> inputList, int count, bool throwArgumentOutOfRangeException = true) {
			if (throwArgumentOutOfRangeException && count > inputList.Count) throw new ArgumentOutOfRangeException();

			var outputList = new List<T>(count);
			outputList.AddRandomly(inputList, count);
			return outputList;
		}

		public static List<T> GetDistinctRandomElements<T>(this IList<T> inputList, int count) {
			if (count > inputList.Count) {
				throw new ArgumentOutOfRangeException();
			}

			List<T> outputList = new List<T>();

			if (count == inputList.Count) {
				outputList = new List<T>(inputList);
				return outputList;
			}

			var sourceDictionary = inputList.ToIndexedDictionary();

			if (count > inputList.Count / 2) {
				while (sourceDictionary.Count > count) {
					sourceDictionary.Remove(inputList.GetRandomIndex());
				}

				outputList = sourceDictionary.Select(kvp => kvp.Value).ToList();
				return outputList;
			}

			var randomDictionary = new Dictionary<int, T>(count);

			while (randomDictionary.Count < count) {
				int key = inputList.GetRandomIndex();

				if (!randomDictionary.ContainsKey(key)) {
					randomDictionary.Add(key, sourceDictionary[key]);
				}
			}

			outputList = randomDictionary.Select(kvp => kvp.Value).ToList();
			return outputList;
		}

		public static IList<T> UnifyLists<T>(this IList<T> outputList, IList<T>[] listsArray) {
			outputList = new List<T>();

			foreach (List<T> inputList in listsArray) {
				outputList = outputList.Union(inputList).ToList();
			}

			return outputList;
		}

		/// <summary>
		/// Returns true if the array is null or empty
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty<T>(this T[] data) {
			return ((data == null) || (data.Length == 0));
		}

		/// <summary>
		/// Returns true if the list is null or empty
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty<T>(this List<T> data) {
			return ((data == null) || (data.Count == 0));
		}

		/// <summary>
		/// Returns true if the dictionary is null or empty
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty<T1, T2>(this Dictionary<T1,T2> data) {
			return ((data == null) || (data.Count == 0));
		}

		/// <summary>
		/// Removes items from a collection based on the condition you provide. This is useful if a query gives 
		/// you some duplicates that you can't seem to get rid of. Some Linq2Sql queries are an example of this. 
		/// Use this method afterward to strip things you know are in the list multiple times
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="Predicate"></param>
		/// <remarks>http://extensionmethod.net/csharp/icollection-t/removeduplicates</remarks>
		/// <returns></returns>
		public static IEnumerable<T> RemoveDuplicates<T>(this ICollection<T> list, Func<T, int> Predicate) {
			var dict = new Dictionary<int, T>();

			foreach (var item in list) {
				if (!dict.ContainsKey(Predicate(item))) {
					dict.Add(Predicate(item), item);
				}
			}

			return dict.Values.AsEnumerable();
		}
	#endregion

	/// <summary>
	/// Array Extensions.
	/// </summary>
	#region ===== Arrays =====
		public static bool IsEmpty<T>(this IEnumerable<T> list) {
			if (list is IList) return (list as IList).Count == 0;
			else return !list.GetEnumerator().MoveNext();
		}

		/// <summary>
        /// Get how deep into the Enumerable the first instance of the object is.
        /// </summary>
        public static int Depth(this IEnumerable lst, object obj) {
            int i = 0;

            foreach(var o in lst) {
                if (object.Equals(o, obj)) return i;
                i++;
            }

            return -1;
        }

		/// <summary>
        /// Get how deep into the Enumerable the first instance of the value is.
        /// </summary>
        public static int Depth<T>(this IEnumerable<T> lst, T value) {
            int i = 0;

            foreach (var v in lst) {
                if (object.Equals(v, value)) return i;
                i++;
            }

            return -1;
        }

		public static IEnumerable<T> Like<T>(this IEnumerable lst) {
            foreach (var obj in lst) {
                if (obj is T) yield return (T)obj;
            }
        }

		public static bool Compare<T>(this IEnumerable<T> first, IEnumerable<T> second) {
            var e1 = first.GetEnumerator();
            var e2 = second.GetEnumerator();

            while (true) {
                var b1 = e1.MoveNext();
                var b2 = e2.MoveNext();
                if (!b1 && !b2) break; //reached end of list

                if (b1 && b2) {
                    if (!object.Equals(e1.Current, e2.Current)) return false;
                }
                else {
                    return false;
                }
            }

            return true;
        }

		/// <summary>
        /// Each enumerable contains the same elements, not necessarily in the same order, or of the same count. Just the same elements.
        /// </summary>
        public static bool SimilarTo<T>(this IEnumerable<T> first, IEnumerable<T> second) {
            return first.Except(second).Count() + second.Except(first).Count() == 0;
        }

		public static bool ContainsAny<T>(this IEnumerable<T> lst, params T[] objs) {
            if (objs == null) return false;
            return lst.Intersect(objs).Count() > 0;
        }

		public static bool ContainsAny<T>(this IEnumerable<T> lst, IEnumerable<T> objs) {
            return lst.Intersect(objs).Count() > 0;
		}

		// public static IEnumerable<T> Append<T>(this IEnumerable<T> lst, T obj) {
        //     var e = new LightEnumerator<T>(lst);

        //     while (e.MoveNext()) {
        //         yield return e.Current;
        //     }
        //     yield return obj;
        // }

		// public static IEnumerable<T> Append<T>(this IEnumerable<T> first, IEnumerable<T> next) {
        //     var e = new LightEnumerator<T>(first);

        //     while (e.MoveNext()) {
        //         yield return e.Current;
        //     }
        //     e = new LightEnumerator<T>(next);

        //     while (e.MoveNext()) {
        //         yield return e.Current;
        //     }
        // }

        // public static IEnumerable<T> Prepend<T>(this IEnumerable<T> lst, T obj) {
        //     yield return obj;
        //     var e = new LightEnumerator<T>(lst);

        //     while(e.MoveNext()) {
        //         yield return e.Current;
        //     }
        // }

		// public static bool Contains(this IEnumerable lst, object obj) {
        //     //foreach (var o in lst)
        //     //{
        //     //    if (Object.Equals(o, obj)) return true;
        //     //}
        //     var e = new LightEnumerator(lst);

        //     while(e.MoveNext()) {
        //         if (Object.Equals(e.Current, obj)) return true;
        //     }
        //     return false;
        // }

		// public static void AddRange<T>(this ICollection<T> lst, IEnumerable<T> elements) {
        //     //foreach (var e in elements)
        //     //{
        //     //    lst.Add(e);
        //     //}
        //     var e = new LightEnumerator<T>(elements);

        //     while(e.MoveNext()) {
        //         lst.Add(e.Current);
        //     }
        // }

		public static bool InBounds(this System.Array arr, int index) {
            return index >= 0 && index <= arr.Length - 1;
        }

		public static void Clear(this System.Array arr) {
            if (arr == null) return;
            System.Array.Clear(arr, 0, arr.Length);
        }

		public static void Copy<T>(IEnumerable<T> source, System.Array destination, int index) {
            if (source is System.Collections.ICollection) (source as System.Collections.ICollection).CopyTo(destination, index);
            else {
                int i = 0;
                foreach(var el in source) {
                    destination.SetValue(el, i + index);
                    i++;
                }
            }
        }
	#endregion

	/// <summary>
	/// Dictionary Extensions.
	/// </summary>
	#region ===== Dictionaries =====
		public static void AddRandomly<T>(this ICollection<T> toCollection, IList<T> fromList, int count) {
			while (toCollection.Count < count) {
				toCollection.Add(fromList.GetRandomElement());
			}
		}

		public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>(Dictionary<TKey, TValue> original) where TValue : ICloneable {
			Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);

			foreach (KeyValuePair<TKey, TValue> entry in original) {
				ret.Add(entry.Key, (TValue) entry.Value.Clone());
			}

			return ret;
		}

		public class CloneableDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : ICloneable {
		    public IDictionary<TKey, TValue> Clone() {
		        CloneableDictionary<TKey, TValue> clone = new CloneableDictionary<TKey, TValue>();

		        foreach (KeyValuePair<TKey, TValue> pair in this) {
		            clone.Add(pair.Key, (TValue)pair.Value.Clone());
		        }

		        return clone;
		    }
		}

		public static Dictionary<int, T> ToIndexedDictionary<T>(this IEnumerable<T> list) {
			var dictionary = list.ToIndexedDictionary(t => t);
			return dictionary;
		}

		public static Dictionary<int, T> ToIndexedDictionary<S, T>(this IEnumerable<S> list, Func<S, T> valueSelector) {
			int index = -1;
			var dictionary = list.ToDictionary(t => ++index, valueSelector);
			return dictionary;
		}

		/// <summary>
		/// Method that adds the given key and value to the given dictionary only if the key is NOT present in the dictionary.
		/// This will be useful to avoid repetitive "if(!containskey()) then add" pattern of coding.
		/// </summary>
		/// <param name="dict">The given dictionary.</param>
		/// <param name="key">The given key.</param>
		/// <param name="value">The given value.</param>
		/// <returns>True if added successfully, false otherwise.</returns>
		/// <typeparam name="TKey">Refers the TKey type.</typeparam>
		/// <typeparam name="TValue">Refers the TValue type.</typeparam>
		public static bool AddIfNotExists <TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) {
			if (dict.ContainsKey(key)) return false;

			dict.Add(key, value);
			return true;
		}

		/// <summary>
		/// Method that adds the given key and value to the given dictionary if the key is NOT present in the dictionary.
		/// If present, the value will be replaced with the new value.
		/// </summary>
		/// <param name="dict">The given dictionary.</param>
		/// <param name="key">The given key.</param>
		/// <param name="value">The given value.</param>
		/// <typeparam name="TKey">Refers the Key type.</typeparam>
		/// <typeparam name="TValue">Refers the Value type.</typeparam>
		public static void AddOrReplace <TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) {
			if (dict.ContainsKey(key)) dict[key] = value;
			else dict.Add(key, value);
		}

		/// <summary>
		/// Method that adds the list of given KeyValuePair objects to the given dictionary. If a key is already present in the dictionary,
		/// then an error will be thrown.
		/// </summary>
		/// <param name="dict">The given dictionary.</param>
		/// <param name="kvpList">The list of KeyValuePair objects.</param>
		/// <typeparam name="TKey">Refers the TKey type.</typeparam>
		/// <typeparam name="TValue">Refers the TValue type.</typeparam>
		public static void AddRange <TKey, TValue>(this Dictionary<TKey, TValue> dict, List<KeyValuePair<TKey, TValue>> kvpList) {
			foreach (var kvp in kvpList) {
				dict.Add(kvp.Key, kvp.Value);
			}
		}

		/// <summary>
		/// Converts an enumeration of groupings into a Dictionary of those groupings.
		/// </summary>
		/// <typeparam name="TKey">Key type of the grouping and dictionary.</typeparam>
		/// <typeparam name="TValue">Element type of the grouping and dictionary list.</typeparam>
		/// <param name="groupings">The enumeration of groupings from a GroupBy() clause.</param>
		/// <returns>A dictionary of groupings such that the key of the dictionary is TKey type and the value is List of TValue type.</returns>
		/// <example>results = productList.GroupBy(product => product.Category).ToDictionary();</example>
		/// <remarks>http://extensionmethod.net/csharp/igrouping/todictionary-for-enumerations-of-groupings</remarks>
		public static Dictionary<TKey, List<TValue>> ToDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groupings) {
			return groupings.ToDictionary(group => group.Key, group => group.ToList());
		}
	#endregion

	/// <summary>
	/// Enums Extensions.
	/// </summary>
	#region ===== Enums =====
		/// <summary>
		/// Converts a string to an enum.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		/// <param name="ignoreCase">true to ignore casing in the string.</param>
		public static T ToEnum<T>(this string s, bool ignoreCase) where T : struct {
			// exit if null
			if (s.IsNullOrEmpty()) return default(T);

			Type genericType = typeof(T);

			if (!genericType.IsEnum)
				return default(T);

			try {
				return (T) Enum.Parse(genericType, s, ignoreCase);
			}

			catch (Exception) {
				// couldn't parse, so try a different way of getting the enums
				Array ary = Enum.GetValues(genericType);
				foreach (T en in ary.Cast<T>()
					.Where(en => 
						(string.Compare(en.ToString(), s, ignoreCase) == 0) ||
						(string.Compare((en as Enum).ToString(), s, ignoreCase) == 0))) {
							return en;
						}

				return default(T);
			}
		}

		/// <summary>
		/// Converts a string to an enum
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
		public static T ToEnum<T>(this string s) where T : struct {
			return s.ToEnum<T>(false);
		}
	#endregion

	/// <summary>
	/// File IO extensions.
	/// </summary>
	#region ===== File =====
		/// <summary>
		/// Creates a directory at <paramref name="folder"/> if it doesn't exist
		/// </summary>
		/// <param name="folder"></param>
		public static void CreateDirectoryIfNotExists(this string folder) {
			if (folder.IsNullOrEmpty()) return;

			string path = Path.GetDirectoryName(folder);
			if (path.IsNullOrEmpty()) return;

			if (! Directory.Exists(path)) Directory.CreateDirectory(path);
		}
	#endregion

	/// <summary>
	/// Generic (T) Extensions.
	/// </summary>
	#region ===== Generics =====
		/// <summary>
		/// Returns true if <paramref name="source"/> equals any of the items in <paramref name="list"/> 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool IsIn<T>(this T source, params T[] list) where T : class {
			// return false if the source or list are null
			// otherwise, scan the list
			return (source != null) && (! list.IsNullOrEmpty()) && (list.Contains(source));
		}

		/// <summary>
		/// Returns true if <paramref name="source"/> equals any of the items in <paramref name="list"/> 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool IsIn<T>(this T source, params T?[] list) where T : struct {
			// return false if the list is null
			// otherwise, scan the list
			return (!list.IsNullOrEmpty()) && (list.Contains(source));
		}

		/// <summary>
		/// Returns true if <paramref name="source"/> equals any of the items in <paramref name="list"/> 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool IsIn(this int source, params int[] list) {
			// return false if the list is null
			// otherwise, scan the list
			return (!list.IsNullOrEmpty()) && (list.Contains(source));
		}

		/// <summary>
		/// Returns true if <paramref name="source"/> does not equal all of the items in <paramref name="list"/> 
		/// NOTE: returns false if source is null
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool IsNotIn<T>(this T source, params T[] list) where T : class {
			// false if null
			if (source == null) return false;

			// return true if the list is empty
			if (list.IsNullOrEmpty()) return true;

			// otherwise, scan the list
			return (!list.Contains(source));
		}

		/// <summary>
		/// Returns true if <paramref name="source"/> does not equal all of the items in <paramref name="list"/> 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool IsNotIn<T>(this T source, params T?[] list) where T : struct {
			// return true if the list is empty
			if (list.IsNullOrEmpty()) return true;

			// otherwise, scan the list
			return (!list.Contains(source));
		}

		/// <summary>
		/// Returns true if <paramref name="source"/> does not equal all of the items in <paramref name="list"/> 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool IsNotIn(this int source, params int[] list) {
			// return true if the list is empty
			if (list.IsNullOrEmpty()) return true;

			// otherwise, scan the list
			return (!list.Contains(source));
		}

		    /// <summary>
		/// Wraps the given object into a List{T} and returns the list.
		/// </summary>
		/// <param name="tobject">The object to be wrapped.</param>
		/// <typeparam name="T">Refers the object to be returned as List{T}.</typeparam>
		/// <returns>Returns List{T}.</returns>
		public static List<T> AsList<T>(this T tobject) {
			return new List<T> { tobject };
		}

		/// <summary>
		/// Returns true if the generic T is null or default. 
		/// This will match: null for classes; null (empty) for Nullable&lt;T&gt;; zero/false/etc for other structs
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tObj"></param>
		/// <returns></returns>
		public static bool IsTNull<T>(this T tObj) {
			return (EqualityComparer<T>.Default.Equals(tObj, default(T)));
		}
	#endregion

	/// <summary>
	/// Extensions for LINQ to XML and XElements.
	/// </summary>
	#region ===== XML =====
		/// <summary>
		/// Returns the Value of the element, or null if the element is null.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static string GetValueOrNull(this XElement element) => (element != null) ? element.Value : null;

		/// <summary>
		/// Returns the Value of the element, or string.empty if the element is null.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static string GetValueString(this XElement element) => (element != null) ? element.Value : string.Empty;

		/// <summary>
		/// Returns a nullable decimal.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static decimal? ValueToDecimalNullable(this XElement element) => (element != null) ? element.Value.ToDecimalNull() : null;

		/// <summary>
		/// Returns a nullable int
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static int? ValueToIntNullable(this XElement element) => (element != null) ? element.Value.ToIntNull() : null;

		/// <summary>
		/// Returns a nullable long
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static long? ValueToLongNullable(this XElement element) => (element != null) ? element.Value.ToLongNull() : null;

		/// <summary>
		/// Returns a nullable float
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static float? ValueToFloatNullable(this XElement element) => (element != null) ? element.Value.ToFloatNull() : null;

	#endregion

	/// <summary>
	/// Handles XML serializing and deserializing Unity data.
	/// </summary>
	#region ===== Unity XML =====
		/// <summary>
		/// XML Serializes an object and returns a byte array.
		/// </summary>
		/// <param name="objToSerialize">the object to serialize</param>
		public static byte[] XMLSerialize_ToArray<T>(this T objToSerialize) where T : class {
			if (objToSerialize.IsTNull()) return null;

			// create the serialization object
			XmlSerializer xSerializer = new XmlSerializer(objToSerialize.GetType());

			// create a textwriter to hold the output
			using (MemoryStream ms = new MemoryStream()) {
				using (XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.Unicode)) {
					// serialize it
					xSerializer.Serialize(xtw, objToSerialize);

					// return it
					return ((MemoryStream)xtw.BaseStream).ToArray();
				}
			}
		}

		/// <summary>
		/// XML Serializes an object and returns the serialized string.
		/// </summary>
		/// <param name="objToSerialize">the object to serialize</param>
		public static string XMLSerialize_ToString<T>(this T objToSerialize) where T : class {
			// exit if null
			if (objToSerialize.IsTNull()) return null;

			// create the serialization object
			XmlSerializer xSerializer = new XmlSerializer(objToSerialize.GetType());

			// create a textwriter to hold the output
			using (MemoryStream ms = new MemoryStream()) {
				using (XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.Unicode)) {
					// serialize it
					xSerializer.Serialize(xtw, objToSerialize);

					// return it
					return UnicodeEncoding.Unicode.GetString(((MemoryStream)xtw.BaseStream).ToArray());
				}
			}
		}

		/// <summary>
		/// Deserializes an XML string.
		/// </summary>
		/// <param name="strSerial">the string to deserialize</param>
		/// <returns></returns>
		public static T XMLDeserialize_ToObject<T>(this string strSerial) where T : class {
			// skip if no string
			if (string.IsNullOrEmpty(strSerial))
				return default(T);

			using (MemoryStream ms = new MemoryStream(UnicodeEncoding.Unicode.GetBytes(strSerial))) {
				// create the serialization object
				XmlSerializer xSerializer = new XmlSerializer(typeof(T));

				// deserialize it
				return (T)xSerializer.Deserialize(ms);
			}
		}

		/// <summary>
		/// XML Deserializes a string.
		/// </summary>
		/// <param name="objSerial">the object to deserialize</param>
		/// <returns></returns>
		public static T XMLDeserialize_ToObject<T>(byte[] objSerial) where T : class {
			// skip if no object
			if (objSerial.IsNullOrEmpty()) return default(T);

			// pop the memory string
			using (MemoryStream ms = new MemoryStream(objSerial)) {
				// create the serialization object
				XmlSerializer xSerializer = new XmlSerializer(typeof(T));

				// deserialize it
				return (T)xSerializer.Deserialize(ms);
			}
		}

		/// <summary>
		/// XML Serialize the object, and save it to a file.
		/// </summary>
		/// <param name="objToSerialize"></param>
		/// <param name="path"></param>
		public static void XMLSerialize_AndSaveTo<T>(this T objToSerialize, string path) where T : class {
			// exit if null
			if ((objToSerialize.IsTNull()) || (path.IsNullOrEmpty())) return;

			// create the directory if it doesn't exist
			path.CreateDirectoryIfNotExists();

			// get a serialized on the object
			XmlSerializer serializer = new XmlSerializer(objToSerialize.GetType());

			// write to a filestream
			using (FileStream fs = new FileStream(path, FileMode.Create)) serializer.Serialize(fs, objToSerialize);
		}

		/// <summary>
		/// XML Serialize the object, and save it to the PersistentDataPath, which is a directory where your application can store user specific 
		/// data on the target computer. This is a recommended way to store files locally for a user like highscores or savegames. 
		/// </summary>
		/// <param name="objToSerialize"></param>
		/// <param name="folderName">OPTIONAL - sub folder name (ex. DataFiles\SavedGames</param>
		/// <param name="filename">the filename (ex. SavedGameData.xml)</param>
		public static void XMLSerialize_AndSaveToPersistentDataPath<T>(this T objToSerialize, string folderName, string filename) where T : class {
			// exit if null
			if ((objToSerialize.IsTNull()) || (filename.IsNullOrEmpty())) return;

			// build the path
			string path = folderName.IsNullOrEmpty() ?
				Path.Combine(Application.persistentDataPath, filename) :
				Path.Combine(Path.Combine(Application.persistentDataPath, folderName), filename);

			// create the directory if it doesn't exist
			path.CreateDirectoryIfNotExists();

			// get a serialized on the object
			XmlSerializer serializer = new XmlSerializer(objToSerialize.GetType());

			// write to a filestream
			using (FileStream fs = new FileStream(path, FileMode.Create)) serializer.Serialize(fs, objToSerialize);
		}

		/// <summary>
		/// XML Serialize the object, and save it to the DataPath, which points to your asset/project directory. This directory is typically read-only after
		/// your game has been compiled. Use only from Editor scripts.
		/// </summary>
		/// <param name="objToSerialize"></param>
		/// <param name="folderName">OPTIONAL - sub folder name (ex. DataFiles\SavedGames</param>
		/// <param name="filename">the filename (ex. SavedGameData.xml)</param>
		public static void XMLSerialize_AndSaveToDataPath<T>(this T objToSerialize, string folderName, string filename) where T : class {
			// exit if null
			if ((objToSerialize.IsTNull()) || (filename.IsNullOrEmpty())) return;

			// build the path
			string path = folderName.IsNullOrEmpty() ?
				Path.Combine(Application.persistentDataPath, filename) :
				Path.Combine(Path.Combine(Application.persistentDataPath, folderName), filename);

			// create the directory if it doesn't exist
			path.CreateDirectoryIfNotExists();

			// get a serialized on the object
			XmlSerializer serializer = new XmlSerializer(objToSerialize.GetType());

			// write to a filestream
			using (FileStream fs = new FileStream(path, FileMode.Create)) serializer.Serialize(fs, objToSerialize);
		}

		/// <summary>
		/// Load from a file and XML deserialize the object.
		/// </summary>
		/// <param name="path"></param>
		public static T XMLDeserialize_AndLoadFrom<T>(this string path) where T : class {
			// exit if null
			if (path.IsNullOrEmpty()) return null;

			// exit if the file doesn't exist
			if (!File.Exists(path)) return null;

			// get the serializer
			XmlSerializer serializer = new XmlSerializer(typeof(T));
			using (FileStream fs = new FileStream(path, FileMode.Open)) return serializer.Deserialize(fs) as T;
		}

		/// <summary>
		/// Load from a file and XML deserialize the object.
		/// </summary>
		/// <param name="folderName">OPTIONAL - sub folder name (ex. DataFiles\SavedGames</param>
		/// <param name="filename">the filename (ex. SavedGameData.xml)</param>
		public static T XMLDeserialize_AndLoadFromPersistentDataPath<T>(this string filename, string folderName) where T : class {
			// exit if null
			if (filename.IsNullOrEmpty()) return null;

			// build the path
			string path = folderName.IsNullOrEmpty() ?
				Path.Combine(Application.persistentDataPath, filename) :
				Path.Combine(Path.Combine(Application.persistentDataPath, folderName), filename);

			// load
			return path.XMLDeserialize_AndLoadFrom<T>();
		}

		/// <summary>
		/// Load from a file and XML deserialize the object.
		/// </summary>
		/// <param name="folderName">OPTIONAL - sub folder name (ex. DataFiles\SavedGames</param>
		/// <param name="filename">the filename (ex. SavedGameData.xml)</param>
		public static T XMLDeserialize_AndLoadFromDataPath<T>(this string filename, string folderName) where T : class {
			// exit if null
			if (filename.IsNullOrEmpty()) return null;

			// build the path
			string path = folderName.IsNullOrEmpty() ?
				Path.Combine(Application.dataPath, filename) :
				Path.Combine(Path.Combine(Application.dataPath, folderName), filename);

			// load
			return path.XMLDeserialize_AndLoadFrom<T>();
		}
	#endregion

	/// <summary>
	/// Layers extensions.
	/// </summary>
	#region ===== Layers =====
		private static List<int> layerNumbers;
        private static List<string> layerNames;
        private static long lastUpdateTick;

		public static bool HasLayer(this LayerMask layerMask, int layer) {
			if (layerMask == (layerMask | (1 << layer))) {
				return true;
			}

			return false;
		}

		public static bool[] HasLayers(this LayerMask layerMask) {
			var hasLayers = new bool[32];

			for (int i = 0; i < 32; i++) {
				if (layerMask == (layerMask | (1 << i))) {
					hasLayers[i] = true;
				}
			}

			return hasLayers;
		}

		public static int AddLayerToLayerMask(this LayerMask layerMask, int layer) => layerMask |= 1 << layer;
		public static int RemoveLayerFromLayerMask(this LayerMask layerMask, int layer) => layerMask &= ~(1 << layer);
		public static int AddLayerToLayerMaskInt(this int layerMask, int layer) => layerMask |= 1 << layer;
		public static int RemoveLayerFromLayerMaskInt(this int layerMask, int layer) => layerMask &= ~(1 << layer);
		public static int AddLayerToCullingMask(this Camera camera, int layer) => camera.cullingMask |= 1 << layer;
		public static int RemoveLayerFromCullingMask(this Camera camera, int layer) => camera.cullingMask &= ~(1 << layer);

		public static LayerMask Inverse(this LayerMask mask) => ~mask;
		public static LayerMask Inverse(this int mask) => ~mask;

		private static void TestUpdateLayers() {
            if (layerNumbers == null || (System.DateTime.UtcNow.Ticks - lastUpdateTick > 10000000L && Event.current.type == EventType.Layout)) {
                lastUpdateTick = System.DateTime.UtcNow.Ticks;
                if (layerNumbers == null) {
                    layerNumbers = new List<int>();
                    layerNames = new List<string>();
                }
                else {
                    layerNumbers.Clear();
                    layerNames.Clear();
                }

                for (int i = 0; i < 32; i++) {
                    string layerName = LayerMask.LayerToName(i);

                    if (layerName != "") {
                        layerNumbers.Add(i);
                        layerNames.Add(layerName);
                    }
                }
            }
        }

		public static string[] GetLayerNames() {
            TestUpdateLayers();
            return layerNames.ToArray();
        }

        public static string[] GetAllLayerNames() {
            TestUpdateLayers();
            string[] names = new string[32];

            for (int i = 0; i < 32; i++) {
                if (layerNumbers.Contains(i)) names[i] = LayerMask.LayerToName(i);
                else names[i] = "Layer " + i.ToString();
            }

            return names;
        }

		public static bool Intersects(this LayerMask mask, int layer) => (mask.value & (1 << layer)) != 0;
		public static bool Intersects(this LayerMask mask, LayerMask layers) => (mask.value & (1 << layers)) != 0;
        public static bool Intersects(this LayerMask mask, GameObject go) => (mask.value & (1 << go.layer)) != 0;
        public static bool Intersects(this LayerMask mask, Component c) => (mask.value & (1 << c.gameObject.layer)) != 0;
	#endregion

	/// <summary>
	/// Object extensions.
	/// https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBase/Utils/ObjUtil.cs
	/// </summary>
	#region ===== Objects =====
		public static bool NullSafeEquals(this object a, object other) {
			return (a == null && other == null) || a.Equals( other );
		}

		public static void Destroy(this GameObject obj, float inSeconds = 0f) {
			GameObject.Destroy(obj, inSeconds);
		}

		public static int GetCollisionMask(this GameObject gameObject, int layer = -1) {
			if (layer == -1) {
				layer = gameObject.layer;
			}

			int mask = 0;
			for (int i = 0; i < 32; i++) {
				mask |= (Physics.GetIgnoreLayerCollision(layer, i) ? 0:1) << i;
			}

			return mask;
		}

		/// <summary>
		/// Checks whether the given object is of {T}.
		/// </summary>
		/// <param name="obj">The object to be checked.</param>
		/// <typeparam name="T">Refers the target data type.</typeparam>
		/// <returns>True if the given object is of type T, false otherwise.</returns>
		public static bool IsA<T>(this object obj) => obj is T;

		/// <summary>
		/// Checks whether the given object is NOT of type T.
		/// </summary>
		/// <param name="obj">The object to be checked.</param>
		/// <typeparam name="T">Refers the target data type.</typeparam>
		/// <returns>True if the given object is NOT of type T, false otherwise.</returns>
		public static bool IsNotA<T>(this object obj) => obj.IsA<T>().Toggle();

		/// <summary>
		/// Tries to cast the given object to type T
		/// </summary>
		/// <param name="obj">The object to be casted.</param>
		/// <typeparam name="T">Refers target data type.</typeparam>
		/// <returns>Returns the casted objects. Null if casting fails.</returns>
		public static T As <T>(this object obj) where T : class => obj as T;

		/// <summary>
		/// Checks whether the given object is Null.
		/// </summary>
		/// <param name="obj">The object to be checked.</param>
		/// <returns>True if the object is Null, false otherwise.</returns>
		public static bool IsNull(this object obj) => obj == null;

		    /// <summary>
		/// Checks whether the given object is NOT Null.
		/// </summary>
		/// <param name="obj">The object to be checked.</param>
		/// <returns>True if the object is NOT Null, false otherwise.</returns>
		public static bool IsNotNull(this object obj) => obj.IsNull().Toggle();

		/// <summary>
		/// Makes a copy from the object.
		/// Doesn't copy the reference memory, only data.
		/// </summary>
		/// <typeparam name="T">Type of the return object.</typeparam>
		/// <param name="item">Object to be copied.</param>
		/// <returns>Returns the copied object.</returns>
		public static T Clone<T>(this object item) where T : class => (item != null) ? item.XMLSerialize_ToString().XMLDeserialize_ToObject<T>() : default(T);

		public static bool IntersectsLayerMask(this GameObject obj, int layerMask) {
            if (obj == null) return false;
            return ((1 << obj.layer) & layerMask) != 0;
        }
	#endregion

	/// <summary>
	/// Extensions for Raycasting.
	/// </summary>
	#region ===== Raycast =====
		public static Vector3 GetNormal(this RaycastHit raycastHit) => raycastHit.normal;
		public static Vector3 GetPoint(this RaycastHit raycastHit) => raycastHit.point;
		public static float GetDistance(this RaycastHit raycastHit) => raycastHit.distance;
		public static Vector2 GetNormal(this RaycastHit2D raycastHit2D) => raycastHit2D.normal;
		public static Vector2 GetPoint(this RaycastHit2D raycastHit2D) => raycastHit2D.point;
		public static Vector2 GetCentroid(this RaycastHit2D raycastHit2D) => raycastHit2D.centroid;
		public static float GetDistance(this RaycastHit2D raycastHit2D) => raycastHit2D.distance;
	#endregion

	/// <summary>
	/// Extensions for Tiles.
	/// </summary>
	#region ===== Tiles =====
		public static T GetCell<T>(this Tilemap tilemap, Vector3Int coordinate) where T : TileBase {
			T tile = tilemap.GetTile<T>(coordinate);
			return tile;
		}

		public static T GetTile<T>(this Tilemap tilemap, Vector3Int cellPosition) where T : TileBase {
        	T tile = tilemap.GetTile<T>(cellPosition);
        	return tile;
    	}

		public static void SetTile<T>(this Tilemap tilemap, Vector3Int cellPosition, TileBase tile) where T : TileBase {
			tilemap.SetTile<T>(cellPosition, tile);
		}

		public static Vector3Int GetCellCoordinatePosition(Vector3 cellPosition, GridLayout gridLayout) {
			Vector3Int cellCoordinate = gridLayout.WorldToCell(cellPosition);
			return cellCoordinate;
		}

		public static Vector3 GetCellWorldPosition(Vector3Int cellCoordinate, GridLayout gridLayout) {
			Vector3 cellPosition = gridLayout.CellToWorld(cellCoordinate);
			return cellPosition;
		}

		public static Vector3Int GetObjectCellPosition(GameObject obj, GridLayout gridLayout) {
			Vector3 objectPos = obj.transform.position;
			Vector3Int cellCoordinate = gridLayout.WorldToCell(objectPos);
			return cellCoordinate;
		}
	#endregion

	/// <summary>
	/// Extensions for Colors.
	/// </summary>
	#region ===== Color =====
		public static Color SetColorR(this Color c, float r) {
			c.r = r;
			return c;
		}
		public static Color SetColorG(this Color c, float g) {
			c.g = g;
			return c;
		}
		public static Color SetColorB(this Color c, float b) {
			c.b = b;
			return c;
		}
		public static Color SetColorA(this Color c, float a) {
			c.a = a;
			return c;
		}
		public static Color SetColorRGBA(this Color c, float r = 0f, float g = 0f, float b = 0f, float a = 255f, bool withA = true) {
			c = withA ? new Color(r, g, b, a) : new Color(r, g, b, 255f);
			return c;
		}

		// public static void SetRGBA(this Color c, Color c2) => c = new Color(c2.r, c2.g, c2.g, c2.a);
		// public static void SetR(this Color c, float r) => c.r = r;
		// public static void SetG(this Color c, float g) => c.g = g;
		// public static void SetB(this Color c, float b) => c.b = b;
		// public static void SetA(this Color c, float a) => c.a = a;
		// public static float SetR(this Color c, float r) => c.r = r;
		// public static float SetG(this Color c, float g) => c.g = g;
		// public static float SetB(this Color c, float b) => c.b = b;
		// public static float SetA(this Color c, float a) => c.a = a;
		public static Color SetRGBA(this Color c, float r = 0f, float g = 0f, float b = 0f, float a = 255f, bool withA = true) => withA ? new Color(r, g, b, a) : new Color(r, g, b, 255f);
		public static Color SetR(this Color c, float r) => new Color(r, c.g, c.b, c.a);
		public static Color SetG(this Color c, float g) => new Color(c.r, g, c.b, c.a);
		public static Color SetB(this Color c, float b) => new Color(c.r, c.g, b, c.a);
		public static Color SetA(this Color c, float a) => new Color(c.r, c.g, c.b, a);
		public static Color SetRGBA(this Color c, Color c2, bool withA = true) => withA ? new Color(c2.r, c2.g, c2.b, c2.a) : new Color(c2.r, c2.g, c2.b, 255f);
		public static Color SetR(this Color c, Color c2) => new Color(c2.r, c.g, c.b, c.a);
		public static Color SetG(this Color c, Color c2) => new Color(c.r, c2.g, c.b, c.a);
		public static Color SetB(this Color c, Color c2) => new Color(c.r, c.g, c2.b, c.a);
		public static Color SetA(this Color c, Color c2) => new Color(c.r, c.g, c.b, c2.a);

		public static Color GetR(this Color c, bool withA = false) => withA ? new Color(c.r, 0f, 0f, c.a) : new Color(c.r, 0f, 0f, 255f);
		public static Color GetG(this Color c, bool withA = false) => withA ? new Color(0f, c.g, 0f, c.a) : new Color(0f, c.g, 0f, 255f);
		public static Color GetB(this Color c, bool withA = false) => withA ? new Color(0f, 0f, c.b, c.a) : new Color(0f, 0f, c.b, 255f);
		public static Color GetRGB(this Color c, bool withA = false) => withA ? new Color(c.r, c.g, c.b, c.a) : new Color(c.r, c.g, c.b, 255f);
		public static Color GetA(this Color c) => new Color(0f, 0f, 0f, c.a);
		public static float GetRValue(this Color c) => c.r;
		public static float GetGValue(this Color c) => c.g;
		public static float GetBValue(this Color c) => c.b;
		public static float GetAValue(this Color c) => c.a;

		/// <summary>
        /// Hue is returned as an angle in degrees around the standard hue colour wheel.
        /// See: http://en.wikipedia.org/wiki/HSL_and_HSV
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float ExtractHue(this Color c) {
            var max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            var min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            var delta = max - min;
            if (Mathf.Abs(delta) < 0.0001f) return 0f;
            else if(c.r >= c.g && c.r >= c.b) return 60f * (((c.g - c.b) / delta) % 6f);
            else if(c.g >= c.b) return 60f * ((c.b - c.r) / delta + 2f);
            else return 60f * ((c.r - c.g) / delta + 4f);
        }

		/// <summary>
        /// Returns the value of an RGB color. This can be used in an HSV representation of a colour.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float ExtractValue(this Color c) => Mathf.Max(c.r, Mathf.Max(c.g, c.b));

		public static float ExtractSaturation(this Color c) {
            ////ala HSL formula
            //var max = Mathf.Max(c.r, c.g, c.b);
            //var min = Mathf.Min(c.r, c.g, c.b);
            //var delta = max - min;
            //if (Mathf.Abs(delta) < 0.0001f) return 0f;
            //else
            //    return delta / (1f - Mathf.Abs(max + min - 1f));

            //ala HSV formula
            var max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            if (Mathf.Abs(max) < 0.0001f) return 0f;
            var min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            return (max - min) / max;
        }

		/// <summary>
        /// Unity's Color.Lerp clamps between 0->1, this allows a true lerp of all ranges.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Color Lerp(Color a, Color b, float t) => new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);

        /// <summary>
        /// Unity's Color32.Lerp clamps between 0->1, this allows a true lerp of all ranges.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Color32 Lerp(Color32 a, Color32 b, float t) {
            return new Color32((byte)Clamp((float)a.r + (float)((int)b.r - (int)a.r) * t, 0, 255), 
                               (byte)Clamp((float)a.g + (float)((int)b.g - (int)a.g) * t, 0, 255), 
                               (byte)Clamp((float)a.b + (float)((int)b.b - (int)a.b) * t, 0, 255), 
                               (byte)Clamp((float)a.a + (float)((int)b.a - (int)a.a) * t, 0, 255));
        }

		public static Color Lerp(float t, params Color[] colors) {
            if (colors == null || colors.Length == 0) return Color.black;
            if (colors.Length == 1) return colors[0];

            int i = Mathf.FloorToInt(colors.Length * t);
            if (i < 0) i = 0;
            if (i >= colors.Length - 1) return colors[colors.Length - 1];
            
            t %= 1f / (float)(colors.Length - 1);
            return Color.Lerp(colors[i], colors[i + 1], t);
        }
	#endregion

	/// <summary>
	/// Audio extensions.
	/// </summary>
	#region ===== Audio =====
		public static void Play(this AudioSource src, AudioClip clip, AudioInterruptMode mode) {
            if (src == null) throw new System.ArgumentNullException("src");
            if (clip == null) throw new System.ArgumentNullException("clip");
            
            switch(mode) {
                case AudioInterruptMode.StopIfPlaying:
                    if (src.isPlaying) src.Stop();
                    break;
                case AudioInterruptMode.DoNotPlayIfPlaying:
                    if (src.isPlaying) return;
                    break;
                case AudioInterruptMode.PlayOverExisting:
                    break;
            }

            src.PlayOneShot(clip);
        }

        public static void Play(this AudioSource src, AudioClip clip, float volumeScale, AudioInterruptMode mode) {
            if (src == null) throw new System.ArgumentNullException("src");
            if (clip == null) throw new System.ArgumentNullException("clip");

            switch (mode) {
                case AudioInterruptMode.StopIfPlaying:
                    if (src.isPlaying) src.Stop();
                    break;
                case AudioInterruptMode.DoNotPlayIfPlaying:
                    if (src.isPlaying) return;
                    break;
                case AudioInterruptMode.PlayOverExisting:
                    break;
            }

            src.PlayOneShot(clip, volumeScale);
        }

		public enum AudioInterruptMode {
			StopIfPlaying,
			DoNotPlayIfPlaying,
			PlayOverExisting,
		}
	#endregion

	/// <summary>
	/// Extensions for Prefabs.
	/// </summary>
	#region ===== Prefabs =====
		public static GameObject Create(GameObject prefab) => UnityEngine.Object.Instantiate(prefab) as GameObject;
        public static GameObject Create(GameObject prefab, Vector3 pos, Quaternion rot) => UnityEngine.Object.Instantiate(prefab, pos, rot) as GameObject;
        public static GameObject Create(GameObject prefab, Transform parent = null) {
            if (parent == null) return Create(prefab);
            //NOTE - this appears to work, thanks to help from @Polymorphik
            bool isActive = prefab.activeSelf;
            prefab.SetActive(false);
            var result = UnityEngine.Object.Instantiate(prefab, parent.position, parent.rotation) as GameObject;
            result.transform.parent = parent;
            result.SetActive(isActive);
            prefab.SetActive(isActive);
            return result;
        }
		public static GameObject Create(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent) {
            if (parent == null) return Create(prefab, pos, rot);
            //NOTE - this appears to work, thanks to help from @Polymorphik
            bool isActive = prefab.activeSelf;
            prefab.SetActive(false);
            var result = UnityEngine.Object.Instantiate(prefab, pos, rot) as GameObject;
            result.transform.parent = parent;
            result.SetActive(isActive);
            prefab.SetActive(isActive);
            return result;
        }

	#endregion

	/// <summary>
	/// Extensions for Components.
	/// </summary>
	#region ===== Component =====
		public static bool HasComponent<T>(this GameObject gameObject) {
			return gameObject.GetComponent<T>() != null;
		}

		public static bool HasComponentInChildren<T>(this GameObject gameObject) {
			return gameObject.GetComponentInChildren<T>() != null;
		}

		public static bool HasComponentInHierarchy<T>(this GameObject gameObject) {
			return gameObject.GetComponentInHierarchy<T>() != null;
		}

		public static bool HasComponent<T>(this Component component) {
			return component.GetComponent<T>() != null;
		}

		public static bool HasComponentInChildren<T>(this Component component) {
			return component.GetComponentInChildren<T>() != null;
		}

		public static bool HasComponentInHierarchy<T>(this Component component) {
			return component.GetComponentInHierarchy<T>() != null;
		}

		public static T GetComponentInHierarchy<T>(this GameObject gameObject) {
			var candidate = gameObject.GetComponentInChildren<T>();

			return candidate == null ? gameObject.GetComponentInParent<T>() : candidate;
		}

		public static T GetComponentInHierarchy<T>(this Component component) {
			var candidate = component.GetComponentInChildren<T>();

			return candidate == null ? component.GetComponentInParent<T>() : candidate;
		}

		public static void ValidateComponent<T>(this Component obj, ref T component) where T : Component {
			if (component != null) { return; }
			obj = component.gameObject.GetComponent<T>();
		}
	#endregion

	/// <summary>
	/// Extensions for Sprite Renderers.
	/// </summary>
	#region ===== Sprite Renderer =====
		public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha) {
			Color color = spriteRenderer.color;
			color.a = alpha;
			spriteRenderer.color = color;
		}

		public static void SetSpriteColor(this SpriteRenderer spriteRenderer, Color color) {
			spriteRenderer.color = color;
		}

		public static void SetMaterialColor(this SpriteRenderer spriteRenderer, Color color) {
			spriteRenderer.material.SetColor(color.ToString(), color);
		}
	#endregion

	/// <summary>
	/// Extensions for Transforms.
	/// </summary>
	#region ===== Transform =====
		public static void DestroyChildren(this Transform parent) {
			foreach (Transform child in parent) {
				GameObject.Destroy(child.gameObject);
			}
		}

		public static void SetLayersRecursively(this GameObject gameObject, int layer) {
			gameObject.layer = layer;

			foreach (Transform t in gameObject.transform) {
				t.gameObject.SetLayersRecursively(layer);
			}
		}

		public static void GlobalReset(this Transform trns) {
			trns.position = Vector3.zero;
			trns.rotation = Quaternion.identity;
			// Cant set global scale directly
			trns.localScale = Vector3.one;
		}

		public static void LocalReset(this Transform trns) {
			trns.localPosition = Vector3.zero;
			trns.localRotation = Quaternion.identity;
			trns.localScale = Vector3.one;
		}
	#endregion

	/// <summary>
	/// Extensions for Rigidbodies.
	/// </summary>
	#region ===== Rigidbody =====
		/// <summary>
		/// Changes the direction of a Rigidbody without changing its speed.
		/// </summary>
		/// <param name="rb">Rigidbody.</param>
		/// <param name="direction">New direction.</param>
		public static void ChangeDirection(this Rigidbody rb, Vector3 direction) {
			rb.velocity = direction * rb.velocity.magnitude;
		}

		/// <summary>
		/// Changes the direction of a Rigidbody2D without changing its speed.
		/// </summary>
		/// <param name="rb2D">Rigidbody.</param>
		/// <param name="direction">New direction.</param>
		public static void ChangeDirection(this Rigidbody2D rb2D, Vector2 direction) {
			rb2D.velocity = direction * rb2D.velocity.magnitude;
		}
	#endregion

	/// <summary>
	/// Extensions for Colliders.
	/// </summary>
	#region ===== Colliders =====
		public static void EnableCollider(this GameObject go) {
			if (go.HasComponent<Collider>()) {
				Collider col = go.GetComponent<Collider>();
				col.enabled = true;
			}
		}

		public static void DisableCollider(this GameObject go) {
			if (go.HasComponent<Collider>()) {
				Collider col = go.GetComponent<Collider>();
				col.enabled = false;
			}
		}

		public static void EnableColliders(this GameObject go) {
			if (go.HasComponentInHierarchy<Collider>()) {
				Collider[] col = go.GetComponents<Collider>();

				foreach(Collider c in col) {
					c.enabled = true;
				}
			}
		}

		public static void DisableColliders(this GameObject go) {
			if (go.HasComponentInHierarchy<Collider>()) {
				Collider[] col = go.GetComponents<Collider>();

				foreach(Collider c in col) {
					c.enabled = false;
				}
			}
		}
	#endregion

	/// <summary>
	/// Extensions for Rects.
	/// </summary>
	#region ===== Rect =====
		#region /// Resizing ///
			public static Rect SetWidth(this Rect rect, float width) {
				return new Rect(rect.x, rect.y, width, rect.height);
			}

			public static Rect SetHeight( this Rect rect, float height) {
				return new Rect(rect.x, rect.y, rect.width, height);
			}

			public static Rect SetWidthCentered(this Rect rect, float width) {
				return new Rect(rect.center.x - width * 0.5f, rect.y, width, rect.height);
			}

			public static Rect SetHeightCentered(this Rect rect, float height) {
				return new Rect(rect.x, rect.center.y - height * 0.5f, rect.width, height);
			}

			public static Rect SetSize (this Rect rect, float size) {
				return rect.SetSize( Vector2.one * size );
			}

			public static Rect SetSize(this Rect rect, float width, float height ) {
				return rect.SetSize(new Vector2(width, height));
			}

			public static Rect SetSize(this Rect rect, Vector2 size) {
				return new Rect(rect.position, size);
			}

			public static Rect SetSizeCentered(this Rect rect, float size) {
				return rect.SetSizeCentered(Vector2.one * size);
			}

			public static Rect SetSizeCentered(this Rect rect, float width, float height) {
				return rect.SetSizeCentered(new Vector2(width, height));
			}

			public static Rect SetSizeCentered(this Rect rect, Vector2 size) {
				return new Rect(rect.center - size * 0.5f, size);
			}
		#endregion
	#endregion

	/// <summary>
	/// Extensions for Gizmos.
	/// </summary>
	#region ===== Gizmos =====
		/// <summary>
		/// Draws a wire cube with a given rotation 
		/// </summary>
		/// <param name="center"></param>
		/// <param name="size"></param>
		/// <param name="rotation"></param>
		public static void DrawWireCube(Vector3 center, Vector3 size, Quaternion rotation = default(Quaternion)) {
			var old = Gizmos.matrix;
			if (rotation.Equals(default(Quaternion)))
				rotation = Quaternion.identity;
			Gizmos.matrix = Matrix4x4.TRS(center, rotation, size);
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			Gizmos.matrix = old;
		}

		public static void DrawArrow(Vector3 from, Vector3 to, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
			Gizmos.DrawLine(from, to);
			var direction = to - from;
			var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Gizmos.DrawLine(to, to + right * arrowHeadLength);
			Gizmos.DrawLine(to, to + left * arrowHeadLength);
		}

		public static void DrawWireSphere(Vector3 center, float radius, Color color, Quaternion rotation = default(Quaternion)) {
			var old = Gizmos.matrix;
			if (rotation.Equals(default(Quaternion)))
				rotation = Quaternion.identity;
			Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
			Gizmos.DrawWireSphere(Vector3.zero, radius);
			Gizmos.matrix = old;
			Gizmos.color = color;
		}

		/// <summary>
		/// Draws a flat wire circle (up)
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="segments"></param>
		/// <param name="rotation"></param>
		public static void DrawWireCircle(Vector3 center, float radius, int segments = 20, Quaternion rotation = default(Quaternion)) {
			DrawWireArc(center,radius,360,segments,rotation);
		}

		/// <summary>
		/// Draws an arc with a rotation around the center
		/// </summary>
		/// <param name="center">center point</param>
		/// <param name="radius">radiu</param>
		/// <param name="angle">angle in degrees</param>
		/// <param name="segments">number of segments</param>
		/// <param name="rotation">rotation around the center</param>
		public static void DrawWireArc(Vector3 center, float radius, float angle, int segments = 20, Quaternion rotation = default(Quaternion)) {
			var old = Gizmos.matrix;
		
			Gizmos.matrix = Matrix4x4.TRS(center,rotation,Vector3.one);
			Vector3 from = Vector3.forward * radius;
			var step = Mathf.RoundToInt(angle / segments);
			for (int i = 0; i <= angle; i += step) {
				var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad));
				Gizmos.DrawLine(from, to);
				from = to;
			}

			Gizmos.matrix = old;
		}

		/// <summary>
		/// Draws an arc with a rotation around an arbitraty center of rotation
		/// </summary>
		/// <param name="center">the circle's center point</param>
		/// <param name="radius">radius</param>
		/// <param name="angle">angle in degrees</param>
		/// <param name="segments">number of segments</param>
		/// <param name="rotation">rotation around the centerOfRotation</param>
		/// <param name="centerOfRotation">center of rotation</param>
		public static void DrawWireArc(Vector3 center, float radius, float angle, int segments, Quaternion rotation, Vector3 centerOfRotation) {
			var old = Gizmos.matrix;
			if (rotation.Equals(default(Quaternion)))
				rotation = Quaternion.identity;
			Gizmos.matrix = Matrix4x4.TRS(centerOfRotation, rotation, Vector3.one);
			var deltaTranslation = centerOfRotation - center;
			Vector3 from = deltaTranslation + Vector3.forward * radius;
			var step = Mathf.RoundToInt(angle / segments);
			for (int i = 0; i <= angle; i += step)
			{
				var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad)) + deltaTranslation;
				Gizmos.DrawLine(from, to);
				from = to;
			}

			Gizmos.matrix = old;
		}

		/// <summary>
		/// Draws an arc with a rotation around an arbitraty center of rotation
		/// </summary>
		/// <param name="matrix">Gizmo matrix applied before drawing</param>
		/// <param name="radius">radius</param>
		/// <param name="angle">angle in degrees</param>
		/// <param name="segments">number of segments</param>
		public static void DrawWireArc(Matrix4x4 matrix, float radius, float angle, int segments) {
			var old = Gizmos.matrix;
			Gizmos.matrix = matrix;
			Vector3 from = Vector3.forward * radius;
			var step = Mathf.RoundToInt(angle / segments);
			for (int i = 0; i <= angle; i += step)
			{
				var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad));
				Gizmos.DrawLine(from, to);
				from = to;
			}

			Gizmos.matrix = old;
		}

		/// <summary>
		/// Draws a wire cylinder face up with a rotation around the center
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="height"></param>
		/// <param name="rotation"></param>
		public static void DrawWireCylinder(Vector3 center, float radius, float height, Quaternion rotation = default(Quaternion)) {
			var old = Gizmos.matrix;
			if (rotation.Equals(default(Quaternion)))
				rotation = Quaternion.identity;
			Gizmos.matrix = Matrix4x4.TRS(center,rotation,Vector3.one);
			var half = height / 2;
			
			//draw the 4 outer lines
			Gizmos.DrawLine( Vector3.right * radius - Vector3.up * half,  Vector3.right * radius + Vector3.up * half);
			Gizmos.DrawLine( - Vector3.right * radius - Vector3.up * half,  -Vector3.right * radius + Vector3.up * half);
			Gizmos.DrawLine( Vector3.forward * radius - Vector3.up * half,  Vector3.forward * radius + Vector3.up * half);
			Gizmos.DrawLine( - Vector3.forward * radius - Vector3.up * half,  - Vector3.forward * radius + Vector3.up * half);

			//draw the 2 cricles with the center of rotation being the center of the cylinder, not the center of the circle itself
			DrawWireArc(center + Vector3.up * half,radius,360,20,rotation, center);
			DrawWireArc(center + Vector3.down * half, radius, 360, 20, rotation, center);
			Gizmos.matrix = old;
		}

		/// <summary>
		/// Draws a wire capsule face up
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="height"></param>
		/// <param name="rotation"></param>
		public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation = default(Quaternion)) {
			if (rotation.Equals(default(Quaternion)))
				rotation = Quaternion.identity;
			var old = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(center,rotation,Vector3.one);
			var half = height / 2 - radius;
		
			//draw cylinder base
			DrawWireCylinder(center,radius,height - radius * 2,rotation);

			//draw upper cap
			//do some cool stuff with orthogonal matrices
			var mat = Matrix4x4.Translate(center + rotation * Vector3.up * half) * Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(90,Vector3.forward));
			DrawWireArc(mat,radius,180,20);
			mat = Matrix4x4.Translate(center + rotation * Vector3.up * half) * Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(90,Vector3.up)* Quaternion.AngleAxis(90, Vector3.forward));
			DrawWireArc(mat, radius, 180, 20);
			
			//draw lower cap
			mat = Matrix4x4.Translate(center + rotation * Vector3.down * half) * Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward));
			DrawWireArc(mat, radius, 180, 20);
			mat = Matrix4x4.Translate(center + rotation * Vector3.down * half) * Matrix4x4.Rotate(rotation * Quaternion.AngleAxis(-90, Vector3.forward));
			DrawWireArc(mat, radius, 180, 20);
		
			Gizmos.matrix = old;
		}
	#endregion

	/// <summary>
	/// Extensions for Particle Systems.
	/// </summary>
	#region ===== Particle System =====
		public static void EnableEmission(this ParticleSystem particleSystem, bool enabled) {
			var emission = particleSystem.emission;
			emission.enabled = enabled;
		}
	#endregion

	/// <summary>
	/// Extensions for Vectors.
	/// </summary>
	#region ===== Vectors =====
		#region /// Default Vectors ///
			public static Vector3 NaNVector3 { get { return new Vector3(float.NaN, float.NaN, float.NaN); } }
			public static Vector2 NaNVector2 { get { return new Vector2(float.NaN, float.NaN); } }
			public static Vector3 PosInfVector3 { get { return new Vector3(float.PositiveInfinity, float.PositiveInfinity , float.PositiveInfinity); } }
			public static Vector2 PosInfVector2 { get { return new Vector2(float.PositiveInfinity, float.PositiveInfinity); } }
			public static Vector3 NegInfVector3 { get { return new Vector3(float.NegativeInfinity, float.NegativeInfinity , float.NegativeInfinity); } }
			public static Vector2 NegInfVector2 { get { return new Vector2(float.NegativeInfinity, float.NegativeInfinity); } }
		#endregion

		#region /// Booleans ///
			public static bool IsNan(Vector3 v3) {
				return float.IsNaN(v3.sqrMagnitude);
			}

			public static bool IsNan(Vector2 v2) {
				return float.IsNaN(v2.sqrMagnitude);
			}
		#endregion

		#region /// Setters ///	
			public static Vector3 SetX(this Vector3 v3, float x) => new Vector3(x, v3.y, v3.z);
			public static Vector3 SetY(this Vector3 v3, float y) => new Vector3(v3.x, y, v3.z);
			public static Vector3 SetZ(this Vector3 v3, float z) => new Vector3(v3.x, v3.y, z);
			public static Vector3 SetXY(this Vector3 v3, float x, float y) => new Vector3(x, y, v3.z);
			public static Vector3 SetXZ(this Vector3 v3, float x, float z) => new Vector3(x, v3.y, z);
			public static Vector3 SetYZ(this Vector3 v3, float y, float z) => new Vector3(v3.x, y, z);
			public static Vector3 SetXYZ(this Vector3 v3, float x, float y, float z) => new Vector3(x, y, z);
			public static Vector2 SetX(this Vector2 v2, float x) => new Vector2(x, v2.y);
			public static Vector2 SetY(this Vector2 v2, float y) => new Vector2(v2.x, y);
			public static Vector2 SetXY(this Vector2 v2, float x, float y) => new Vector2(x, y);
			public static Vector3Int SetX(this Vector3Int v3Int, int x) => new Vector3Int(x, v3Int.y, v3Int.z);
			public static Vector3Int SetY(this Vector3Int v3Int, int y) => new Vector3Int(v3Int.x, y, v3Int.z);
			public static Vector3Int SetZ(this Vector3Int v3Int, int z) => new Vector3Int(v3Int.x, v3Int.y, z);
			public static Vector3Int SetXY(this Vector3Int v3Int, int x, int y) => new Vector3Int(x, y, v3Int.z);
			public static Vector3Int SetXZ(this Vector3Int v3Int, int x, int z) => new Vector3Int(x, v3Int.y, z);
			public static Vector3Int SetYZ(this Vector3Int v3Int, int y, int z) => new Vector3Int(v3Int.x, y, z);
			public static Vector3Int SetXYZ(this Vector3Int v3Int, int x, int y, int z) => new Vector3Int(x, y, z);
			public static Vector2Int SetX(this Vector2Int v2Int, int x) => new Vector2Int(x, v2Int.y);
			public static Vector2Int SetY(this Vector2Int v2Int, int y) => new Vector2Int(v2Int.x, y);
			public static Vector2Int SetXY(this Vector2Int v2Int, int x, int y) => new Vector2Int(x, y);

			public static Vector3 FlattenX(this Vector3 v3) => v3.SetX(0f);
			public static Vector3 FlattenY(this Vector3 v3) => v3.SetY(0f);
			public static Vector3 FlattenZ(this Vector3 v3) => v3.SetZ(0f);
			public static Vector3 FlattenXY(this Vector3 v3) => v3.FlattenX().FlattenY();
			public static Vector3 FlattenXZ(this Vector3 v3) => v3.FlattenX().FlattenZ();
			public static Vector3 FlattenYZ(this Vector3 v3) => v3.FlattenY().FlattenZ();
			public static Vector3 Flatten(this Vector3 v3) => v3.FlattenX().FlattenY().FlattenZ();
			public static Vector2 FlattenX(this Vector2 v2) => v2.SetX(0f);
			public static Vector2 FlattenY(this Vector2 v2) => v2.SetY(0f);
			public static Vector2 Flatten(this Vector2 v2) => v2.FlattenX().FlattenY();
			public static Vector3Int FlattenX(this Vector3Int v3Int) => v3Int.SetX(0);
			public static Vector3Int FlattenY(this Vector3Int v3Int) => v3Int.SetY(0);
			public static Vector3Int FlattenZ(this Vector3Int v3Int) => v3Int.SetZ(0);
			public static Vector3Int FlattenXY(this Vector3Int v3Int) => v3Int.FlattenX().FlattenY();
			public static Vector3Int FlattenXZ(this Vector3Int v3Int) => v3Int.FlattenX().FlattenZ();
			public static Vector3Int FlattenYZ(this Vector3Int v3Int) => v3Int.FlattenY().FlattenZ();
			public static Vector3Int Flatten(this Vector3Int v3Int) => v3Int.FlattenX().FlattenY().FlattenZ();
			public static Vector2Int FlattenX(this Vector2Int v2Int) => v2Int.SetX(0);
			public static Vector2Int FlattenY(this Vector2Int v2Int) => v2Int.SetY(0);
			public static Vector2Int Flatten(this Vector2Int v2Int) => v2Int.FlattenX().FlattenY();

			public static Vector3 SwapXY(this Vector3 v3) => v3.SetXY(v3.y, v3.x);
			public static Vector3 SwapXZ(this Vector3 v3) => v3.SetXY(v3.z, v3.x);
			public static Vector3 SwapYZ(this Vector3 v3) => v3.SetXY(v3.z, v3.y);
			public static Vector2 SwapXY(this Vector2 v2) => v2.SetXY(v2.y, v2.x);
			public static Vector3Int SwapXY(this Vector3Int v3Int) => v3Int.SetXY(v3Int.y, v3Int.x);
			public static Vector3Int SwapXZ(this Vector3Int v3Int) => v3Int.SetXZ(v3Int.z, v3Int.x);
			public static Vector3Int SwapYZ(this Vector3Int v3Int) => v3Int.SetYZ(v3Int.z, v3Int.y);
			public static Vector2Int SwapXY(this Vector2Int v2Int) => v2Int.SetXY(v2Int.y, v2Int.x);

			public static Vector3 Abs(this Vector3 v3) => v3.SetXYZ(Mathf.Abs(v3.x), Mathf.Abs(v3.y), Mathf.Abs(v3.z));
			public static Vector3 NegAbs(this Vector3 v3) => v3.Abs() * -1;
			public static Vector2 Abs(this Vector2 v2) => v2.SetXY(Mathf.Abs(v2.x), Mathf.Abs(v2.y));
			public static Vector2 NegAbs(this Vector2 v2) => v2.Abs() * -1;
			public static Vector3Int Abs(this Vector3Int v3Int) => v3Int.SetXYZ(Mathf.Abs(v3Int.x), Mathf.Abs(v3Int.y), Mathf.Abs(v3Int.z));
			public static Vector3Int NegAbs(this Vector3Int v3Int) => v3Int.Abs() * -1;
			public static Vector2Int Abs(this Vector2Int v2Int) => v2Int.SetXY(Mathf.Abs(v2Int.x), Mathf.Abs(v2Int.y));
			public static Vector2Int NegAbs(this Vector2Int v2Int) => v2Int.Abs() * -1;

			public static Vector3 IncrementX(this Vector3 v3, float x) => v3.SetX(v3.x + x);
			public static Vector3 IncrementY(this Vector3 v3, float y) => v3.SetY(v3.y + y);
			public static Vector3 IncrementZ(this Vector3 v3, float z) => v3.SetZ(v3.z + z);
			public static Vector2 IncrementX(this Vector2 v2, float x) => v2.SetX(v2.x + x);
			public static Vector2 IncrementY(this Vector2 v2, float y) => v2.SetY(v2.y + y);
			public static Vector3Int IncrementX(this Vector3Int v3Int, int x) => v3Int.SetX(v3Int.x + x);
			public static Vector3Int IncrementY(this Vector3Int v3Int, int y) => v3Int.SetY(v3Int.y + y);
			public static Vector3Int IncrementZ(this Vector3Int v3Int, int z) => v3Int.SetZ(v3Int.z + z);
			public static Vector2Int IncrementX(this Vector2Int v2Int, int x) => v2Int.SetX(v2Int.x + x);
			public static Vector2Int IncrementY(this Vector2Int v2Int, int y) => v2Int.SetY(v2Int.y + y);

			public static Vector3 MultiplyBy(this Vector3 v3, float f) => v3 * f;
			public static Vector3 MultiplyBy(this Vector3 v3, Vector3 mult) => v3.SetXYZ(v3.x * mult.x, v3.y * mult.y, v3.z * mult.z);
			public static Vector2 MultiplyBy(this Vector2 v2, float f) => v2 * f;
			public static Vector2 MultiplyBy(this Vector2 v2, Vector2 mult) => v2.SetXY(v2.x * mult.x, v2.y * mult.y);
			public static Vector3Int MultiplyBy(this Vector3Int v3Int, int f) => v3Int * f;
			public static Vector3Int MultiplyBy(this Vector3Int v3Int, Vector3Int mult) => v3Int.SetXYZ(v3Int.x * mult.x, v3Int.y * mult.y, v3Int.z * mult.z);
			public static Vector2Int MultiplyBy(this Vector2Int v2Int, int f) => v2Int * f;
			public static Vector2Int MultiplyBy(this Vector2Int v2Int, Vector2Int mult) => v2Int.SetXY(v2Int.x * mult.x, v2Int.y * mult.y);
		#endregion

		#region /// Getters ///
			public static float GetMinScalar(this Vector2 v2) => Mathf.Min(v2.x, v2.y);
			public static float GetMaxScalar(this Vector2 v2) => Mathf.Max(v2.x, v2.y);
			public static float GetMinScalar(this Vector3 v3) => Mathf.Min(v3.x, v3.y, v3.z);
			public static float GetMaxScalar(this Vector3 v3) => Mathf.Max(v3.x, v3.y, v3.z);
			public static int GetMinScalar(this Vector2Int v2Int) => Mathf.Min(v2Int.x, v2Int.y);
			public static int GetMaxScalar(this Vector2Int v2Int) => Mathf.Max(v2Int.x, v2Int.y);
			public static int GetMinScalar(this Vector3Int v3Int) => Mathf.Min(v3Int.x, v3Int.y, v3Int.z);
			public static int GetMaxScalar(this Vector3Int v3Int) => Mathf.Max(v3Int.x, v3Int.y, v3Int.z);

			public static Vector2 Midpoint(this Vector2 v2) => v2.MultiplyBy(0.5f);
			public static Vector3 Midpoint(this Vector3 v3) => v3.MultiplyBy(0.5f);

			public static Vector2 Average(Vector2 a, Vector2 b) => (a + b) / 2f;
			public static Vector2 Average(Vector2 a, Vector2 b, Vector2 c) => (a + b + c) / 3f;
			public static Vector2 Average(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => (a + b + c + d) / 4f;
			public static Vector2 Average(params Vector2[] values) {
				if (values == null || values.Length == 0) return Vector3.zero;

				Vector2 v = Vector2.zero;
				for (int i = 0; i < values.Length; i++)
				{
					v += values[i];
				}
				return v / values.Length;
			}
			public static Vector2 ListAverage(this IList<Vector2> values) {
				if (values == null || values.Count == 0) return Vector3.zero;

				Vector2 v = Vector2.zero;
				for (int i = 0; i < values.Count; i++)
				{
					v += values[i];
				}
				return v / values.Count;
			}
			public static Vector2 Average(this IList<Transform> list) {
				if (list == null || list.Count == 0) return Vector3.zero;

				Vector2 v = Vector2.zero;
				for (int i = 0; i < list.Count; i++) {
					v += list.ElementAt(i).position.ToVector2();
				}
				return v / list.Count;
			}

			/// <summary>
			/// Finds the Vector3 closest to the given Vector3.
			/// </summary>
			/// <param name="position">Original Vector3.</param>
			/// <param name="otherPositions">Others Vector3.</param>
			/// <returns>Closest Vector3.</returns>
			public static Vector3 GetClosestTo(this Vector3 v3, IEnumerable<Vector3> vectors3List) {
				var closestVector3 = Vector3.zero;
				var shortestDistance = Mathf.Infinity;

				foreach (var vector3 in vectors3List) {
					var distance = (v3 - vector3).sqrMagnitude;

					if (distance < shortestDistance) {
						closestVector3 = vector3;
						shortestDistance = distance;
					}
				}

				return closestVector3;
			}

			/// <summary>
			/// Finds the Vector2 closest to the given Vector2.
			/// </summary>
			/// <param name="position">Original Vector2.</param>
			/// <param name="otherPositions">Others Vector2.</param>
			/// <returns>Closest Vector2.</returns>
			public static Vector2 GetClosestTo(this Vector2 v2, IEnumerable<Vector2> vectors2List) {
				var closestVector2 = Vector3.zero;
				var shortestDistance = Mathf.Infinity;

				foreach (var vector2 in vectors2List) {
					var distance = (v2 - vector2).sqrMagnitude;

					if (distance < shortestDistance) {
						closestVector2 = vector2;
						shortestDistance = distance;
					}
				}

				return closestVector2;
			}

			/// <summary>
			/// Finds the Vector3Int closest to the given Vector3Int.
			/// </summary>
			/// <param name="position">Original Vector3Int.</param>
			/// <param name="otherPositions">Others Vector3Int.</param>
			/// <returns>Closest Vector3Int.</returns>
			public static Vector3Int GetClosestTo(this Vector3Int v3Int, IEnumerable<Vector3Int> vectors3IntList) {
				var closestVector3Int = Vector3Int.zero;
				var shortestDistance = Mathf.Infinity;

				foreach (var vector3Int in vectors3IntList) {
					var distance = (v3Int - vector3Int).sqrMagnitude;

					if (distance < shortestDistance) {
						closestVector3Int = v3Int;
						shortestDistance = distance;
					}
				}

				return closestVector3Int;
			}

			/// <summary>
			/// Finds the Vector2Int closest to the given Vector2Int.
			/// </summary>
			/// <param name="position">Original Vector2Int.</param>
			/// <param name="otherPositions">Others Vector2Int.</param>
			/// <returns>Closest Vector2Int.</returns>
			public static Vector2Int GetClosestTo(this Vector2Int v2Int, IEnumerable<Vector2Int> vectors2IntList) {
				var closestVector2Int = Vector2Int.zero;
				var shortestDistance = Mathf.Infinity;

				foreach (var vector2Int in vectors2IntList) {
					var distance = (v2Int - vector2Int).sqrMagnitude;

					if (distance < shortestDistance) {
						closestVector2Int = v2Int;
						shortestDistance = distance;
					}
				}

				return closestVector2Int;
			}

			/// <summary>
			/// Finds the Vector3 farthest from the given Vector3.
			/// </summary>
			/// <param name="position">Original Vector3.</param>
			/// <param name="otherPositions">Others Vector3.</param>
			/// <returns>Farthest Vector3.</returns>
			public static Vector3 GetFarthestFrom(this Vector3 v3, IEnumerable<Vector3> vectors3List) {
				var farthestVector3 = Vector3.zero;
				var farthestDistance = 0f;

				foreach (var vector3 in vectors3List) {
					var distance = (v3 + vector3).sqrMagnitude;

					if (distance > farthestDistance) {
						farthestVector3 = v3;
						farthestDistance = distance;
					}
				}

				return farthestVector3;
			}

			/// <summary>
			/// Finds the Vector2 farthest from the given Vector2.
			/// </summary>
			/// <param name="position">Original Vector2.</param>
			/// <param name="otherPositions">Others Vector2.</param>
			/// <returns>Farthest Vector2.</returns>
			public static Vector2 GetFarthestFrom(this Vector2 v2, IEnumerable<Vector2> vectors2List) {
				var farthestVector2 = Vector2.zero;
				var farthestDistance = 0f;

				foreach (var vector2 in vectors2List) {
					var distance = (v2 + vector2).sqrMagnitude;

					if (distance > farthestDistance) {
						farthestVector2 = v2;
						farthestDistance = distance;
					}
				}

				return farthestVector2;
			}

			/// <summary>
			/// Finds the Vector2Int farthest from the given Vector2Int.
			/// </summary>
			/// <param name="position">Original Vector2Int.</param>
			/// <param name="otherPositions">Others Vector2Int.</param>
			/// <returns>Farthest Vector2Int.</returns>
			public static Vector3Int GetFarthestFrom(this Vector3Int v3Int, IEnumerable<Vector3Int> vectors3IntList) {
				var farthestVector3Int = Vector3Int.zero;
				var farthestDistance = 0f;

				foreach (var vector3Int in vectors3IntList) {
					var distance = (v3Int + vector3Int).sqrMagnitude;

					if (distance > farthestDistance) {
						farthestVector3Int = v3Int;
						farthestDistance = distance;
					}
				}

				return farthestVector3Int;
			}

			/// <summary>
			/// Finds the Vector2Int farthest from the given Vector2Int.
			/// </summary>
			/// <param name="position">Original Vector2Int.</param>
			/// <param name="otherPositions">Others Vector2Int.</param>
			/// <returns>Farthest Vector2Int.</returns>
			public static Vector2Int GetFarthestFrom(this Vector2Int v2Int, IEnumerable<Vector2Int> vectors2IntList) {
				var farthestVector2Int = Vector2Int.zero;
				var farthestDistance = 0f;

				foreach (var vector2Int in vectors2IntList) {
					var distance = (v2Int + vector2Int).sqrMagnitude;

					if (distance > farthestDistance) {
						farthestVector2Int = v2Int;
						farthestDistance = distance;
					}
				}

				return farthestVector2Int;
			}
		#endregion

		#region /// Conversion ///
			public static Vector3 ToVector3(this Vector2 v2, float z = 0f) => new Vector3(v2.x, v2.y, z);
			public static Vector3 ToVector3(this Vector3Int v3Int) => new Vector3(v3Int.x.ToFloat(), v3Int.y.ToFloat(), v3Int.z.ToFloat());
			public static Vector3 ToVector3(this Vector2Int v2Int, float z = 0f) => new Vector3(v2Int.x.ToFloat(), v2Int.y.ToFloat(), z);
			public static Vector2 ToVector2(this Vector3 v3) => new Vector2(v3.x, v3.y);
			public static Vector2 ToVector2(this Vector3Int v3Int) => new Vector2(v3Int.x.ToFloat(), v3Int.y.ToFloat());
			public static Vector2 ToVector2(this Vector2Int v2Int) => new Vector2(v2Int.x.ToFloat(), v2Int.y.ToFloat());
			public static Vector3Int ToVector3Int(this Vector3 v3) => new Vector3Int(v3.x.ToInt(), v3.y.ToInt(), v3.z.ToInt());
			public static Vector3Int ToVector3Int(this Vector2 v2, int z = 0) => new Vector3Int(v2.x.ToInt(), v2.y.ToInt(), z);
			public static Vector3Int ToVector3Int(this Vector2Int v2Int, int z = 0) => new Vector3Int(v2Int.x, v2Int.y, z);
			public static Vector2Int ToVector2Int(this Vector3 v3) => new Vector2Int(v3.x.ToInt(), v3.y.ToInt());
			public static Vector2Int ToVector2Int(this Vector2 v2) => new Vector2Int(v2.x.ToInt(), v2.y.ToInt());
			public static Vector2Int ToVector2Int(this Vector3Int v3Int) => new Vector2Int(v3Int.x, v3Int.y);
		#endregion

		#region /// General ///
			public static Vector3 Normalize(this Vector3 v3) => v3.normalized;
			public static Vector2 Normalize(this Vector2 v2) => v2.normalized;

			public static Vector3 Clamp(this Vector3 input, Vector3 max, Vector3 min) => input.SetXYZ(input.x.Clamp(max.x, min.x), input.y.Clamp(max.y, min.y), input.z.Clamp(max.z, min.z));
			public static Vector2 Clamp(this Vector2 input, Vector2 max, Vector2 min) => input.SetXY(input.x.Clamp(max.x, min.x), input.y.Clamp(max.y, min.y));

			/// <summary>
			/// Unity's Vector2.Lerp clamps between 0->1, this allows a true lerp of all ranges.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <param name="t"></param>
			/// <returns></returns>
			public static Vector2 LerpTo(this Vector2 a, Vector2 b, float t) => (b - a) * t + a;

			/// <summary>
			/// Unity's Vector3.Lerp clamps between 0->1, this allows a true lerp of all ranges.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <param name="t"></param>
			/// <returns></returns>
			public static Vector3 LerpTo(this Vector3 a, Vector3 b, float t) => (b - a) * t + a;

			/// <summary>
			/// Moves from a to b at some speed dependent of a delta time with out passing b.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <param name="speed"></param>
			/// <param name="dt"></param>
			/// <returns></returns>
			public static Vector2 SpeedLerpTo(this Vector2 a, Vector2 b, float speed, float dt) {
				var v = b - a;
				var dv = speed * dt;
				if (dv > v.magnitude)
					return b;
				else
					return a + v.normalized * dv;
			}

			/// <summary>
			/// Moves from a to b at some speed dependent of a delta time with out passing b.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <param name="speed"></param>
			/// <param name="dt"></param>
			/// <returns></returns>
			public static Vector3 SpeedLerpTo(this Vector3 a, Vector3 b, float speed, float dt) {
				var v = b - a;
				var dv = speed * dt;
				if (dv > v.magnitude)
					return b;
				else
					return a + v.normalized * dv;
			}
		#endregion

		#region /// Distance ///
			public static float SqrDistance(this Vector3 from, Vector3 to) {
				return (to - from).sqrMagnitude;
			}

			public static float SqrDistance(this Vector2 from, Vector2 to) {
				return (to - from).sqrMagnitude;
			}

			public static float SqrDistance(this Vector3Int from, Vector3Int to) {
				return (to - from).sqrMagnitude;
			}

			public static float SqrDistance(this Vector2Int from, Vector2Int to) {
				return (to - from).sqrMagnitude;
			}
		#endregion

		#region /// To String ///
			public static string Stringify(this Vector3 v3) {
				return v3.x.ToString() + "," + v3.y.ToString() + "," + v3.z.ToString();
			}

			public static string Stringify(this Vector2 v2) {
				return v2.x.ToString() + "," + v2.y.ToString();
			}
		#endregion

		#region /// Angles ///
			/// <summary>
			/// Get Vector2 from angle.
			/// </summary>
			public static Vector2 AngleFloatToVector2(this float a, bool useRadians = false, bool yDominant = false) {
				float angle = a;
				if (!useRadians) angle *= DEG_TO_RAD;
				if (yDominant) return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				else return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			}

			public static Vector2 AngleIntToVector2(this int a, bool useRadians = false, bool yDominant = false) {
				int angle = a;
				if (!useRadians) angle = (angle * DEG_TO_RAD).ToInt();
				if (yDominant) return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				else return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			}

			/// <summary>
			/// Get the float angle in degrees off the forward defined by x.
			/// </summary>
			/// <param name="v"></param>
			/// <returns></returns>
			public static float AngleFloat(this Vector2 v) {
				return Mathf.Atan2(v.normalized.y, v.normalized.x) * RAD_TO_DEG;
			}

			/// <summary>
			/// Get the int angle in degrees off the forward defined by x.
			/// </summary>
			/// <param name="v"></param>
			/// <returns></returns>
			public static int AngleInt(this Vector2 v) {
				return (Mathf.Atan2(v.normalized.y, v.normalized.x) * RAD_TO_DEG).ToInt();
			}

			/// <summary>
			/// Get the float angle in degrees off the forward defined by x.
			/// Due to float error the dot / mag can sometimes be ever so slightly over 1, which can cause NaN in acos.
			/// </summary>
			/// <param name="v"></param>
			/// <returns>Mathf.Acos(Vector2.Dot(a, b) / (a.magnitude * b.magnitude)) * MathUtil.RAD_TO_DEG;</returns>
			public static float AngleBetween(Vector2 a, Vector2 b) {
				// // Due to float error the dot / mag can sometimes be ever so slightly over 1, which can cause NaN in acos.
				//return Mathf.Acos(Vector2.Dot(a, b) / (a.magnitude * b.magnitude)) * MathUtil.RAD_TO_DEG;
				double d = (double)Vector2.Dot(a, b) / ((double)a.magnitude * (double)b.magnitude);
				if (d >= 1d) return 0f;
				else if (d <= -1d) return 180f;
				return (float)System.Math.Acos(d) * RAD_TO_DEG;
			}
			
			/// <summary>
			/// Angle in degrees off some axis in the counter-clockwise direction. Think of like 'Angle' or 'Atan2' where you get to control
			/// which axis as opposed to only measuring off of <1,0>.
			/// </summary>
			/// <param name="v"></param>
			/// <returns></returns>
			public static float AngleOff(this Vector2 v, Vector2 axis) {
				if (axis.sqrMagnitude < 0.0001f) return float.NaN;
				axis.Normalize();
				var tang = new Vector2(-axis.y, axis.x);
				return AngleBetween(v, axis) * Mathf.Sign(Vector2.Dot(v, tang));
			}

			/// <summary>
			/// Rotate Vector2 counter-clockwise by 'a'.
			/// </summary>
			/// <param name="v"></param>
			/// <param name="a"></param>
			/// <returns></returns>
			public static Vector2 RotateBy(this Vector2 v, float a, bool bUseRadians = false) {
				if (!bUseRadians) a *= DEG_TO_RAD;
				var ca = System.Math.Cos(a);
				var sa = System.Math.Sin(a);
				var rx = v.x * ca - v.y * sa;

				return new Vector2((float)rx, (float)(v.x * sa + v.y * ca));
			}

			/// <summary>
			/// Rotates a vector toward another. Magnitude of the from vector is maintained.
			/// </summary>
			/// <param name="from"></param>
			/// <param name="to"></param>
			/// <param name="a"></param>
			/// <param name="bUseRadians"></param>
			/// <returns></returns>
			public static Vector2 RotateToward(this Vector2 from, Vector2 to, float a, bool bUseRadians = false) {
				if (!bUseRadians) a *= DEG_TO_RAD;
				var a1 = Mathf.Atan2(from.y, from.x);
				var a2 = Mathf.Atan2(to.y, to.x);
				a2 = ShortenAngleToAnother(a2, a1, true);
				var ra = (a2 - a1 >= 0f) ? a1 + a : a1 - a;
				var l = from.magnitude;
				return new Vector2(Mathf.Cos(ra) * l, Mathf.Sin(ra) * l);
			}

			public static Vector2 RotateTowardClamped(this Vector2 from, Vector2 to, float a, bool bUseRadians = false) {
				if (!bUseRadians) a *= DEG_TO_RAD;
				var a1 = Mathf.Atan2(from.y, from.x);
				var a2 = Mathf.Atan2(to.y, to.x);
				a2 = ShortenAngleToAnother(a2, a1, true);

				var da = a2 - a1;
				var ra = a1 + Mathf.Clamp(Mathf.Abs(a), 0f, Mathf.Abs(da)) * Mathf.Sign(da);

				var l = from.magnitude;
				return new Vector2(Mathf.Cos(ra) * l, Mathf.Sin(ra) * l);
			}
		#endregion
	#endregion

	/// <summary>
	/// Extensions for numerical values.
	/// </summary>
	#region ===== Numericals =====
		public static int Sign(this float f) => MathF.Sign(f);
		public static int Sign(this int f) => MathF.Sign(f);

		public static int ToInt(this float f) => Mathf.RoundToInt(f);
		public static int ToIntCeil(this float f) => Mathf.CeilToInt(f);
		public static int ToIntFloor(this float f) => Mathf.FloorToInt(f);

		public static float Round(this float f) => Mathf.Round(f);
		public static float Ceil(this float f) => Mathf.Ceil(f);
		public static float Floor(this float f) => Mathf.Floor(f);

		public static float Clamp(this float f, float min, float max) => Mathf.Clamp(f, min, max);
		public static int Clamp(this int f, int min, int max) => Mathf.Clamp(f, min, max);

		/// <summary>
		/// Negates (* -1) the given integer.
		/// </summary>
		/// <param name="number">The given integer.</param>
		/// <returns>The negated integer.</returns>
		public static int Negate(this int number) {
			return number * -1;
		}

		/// <summary>
		/// Strips out the sign and returns the absolute value of given integer.
		/// </summary>
		/// <param name="number">The given integer.</param>
		/// <returns>The absolute value of given integer.</returns>
		public static int AbsoluteValue(this int number) {
			return Math.Abs(number);
		}

		/// <summary>
		/// Negates (* -1) the given float.
		/// </summary>
		/// <param name="number">The given float.</param>
		/// <returns>The negated float.</returns>
		public static float Negate(this float number) {
			return number * -1;
		}

		/// <summary>
		/// Strips out the sign and returns the absolute value of given float.
		/// </summary>
		/// <param name="number">The given float.</param>
		/// <returns>The absolute value of given float.</returns>
		public static float AbsoluteValue(this float number) {
			return Math.Abs(number);
		}

		/// <summary>
		/// Negates (* -1) the given long number.
		/// </summary>
		/// <param name="number">The given long number.</param>
		/// <returns>The negated long number.</returns>
		public static long Negate(this long number) {
			return number * -1;
		}

		/// <summary>
		/// Strips out the sign and returns the absolute value of given long number.
		/// </summary>
		/// <param name="number">The given long number.</param>
		/// <returns>The absolute value of given long number.</returns>
		public static long AbsoluteValue(this long number) {
			return Math.Abs(number);
		}

		/// <summary>
        /// Returns if the given float is in between or equal to max and min.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InRange(this float value, float max, float min = 0f) {
            if (max < min) return (value >= max && value <= min);
            else return (value >= min && value <= max);

        }
        
		/// <summary>
        /// Returns if the given int is in between or equal to max and min.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InRange(this int value, int max, int min = 0) {
            if (max < min) return (value >= max && value <= min);
            else return (value >= min && value <= max);
        }
        
		/// <summary>
        /// Returns if the given float is in between the max and min.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InRangeExclusive(this float value, float max, float min = 0f) {
            if (max < min) return (value > max && value < min);
            else return (value > min && value < max);
        }

		/// <summary>
        /// Returns if the given int is in between the max and min.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool InRangeExclusive(this int value, int max, int min = 0) {
            if (max < min) return (value > max && value < min);
            else return (value > min && value < max);
        }

		/// <summary>
        /// Clamp a value into a range.
        /// 
        /// If input is LT min, min returned.
        /// If input is GT max, max returned.
        /// Else input returned.
        /// </summary>
        /// <param name="input">Value to clamp</param>
        /// <param name="max">Max in range</param>
        /// <param name="min">Min in range</param>
        /// <returns>Clamped value</returns>
        /// <remarks></remarks>
        public static short Clamp(this short input, short max, short min) {
            return Math.Max(min, Math.Min(max, input));
        }
        public static short Clamp(this short input, short max) {
            return Math.Max((short)0, Math.Min(max, input));
        }

        /*public static int Clamp(this int input, int max, int min) {
            return Math.Max(min, Math.Min(max, input));
        }
        public static int Clamp(this int input, int max) {
            return Math.Max(0, Math.Min(max, input));
        }*/

        public static long Clamp(this long input, long max, long min) {
            return Math.Max(min, Math.Min(max, input));
        }
        public static long Clamp(this long input, long max) {
            return Math.Max(0, Math.Min(max, input));
        }

        /*public static float Clamp(this float input, float max, float min) {
            return Math.Max(min, Math.Min(max, input));
        }
        public static float Clamp(this float input, float max) {
            return Math.Max(0, Math.Min(max, input));
        }*/

		/// <summary>
        /// Test if a value is near some target value, if with in some range of 'epsilon', the target is returned.
        /// 
        /// eg:
        /// Slam(1.52,2,0.1) == 1.52
        /// Slam(1.62,2,0.1) == 1.62
        /// Slam(1.72,2,0.1) == 1.72
        /// Slam(1.82,2,0.1) == 1.82
        /// Slam(1.92,2,0.1) == 2
        /// </summary>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static float Slam(this float value, float target, float epsilon) {
            if (Math.Abs(value - target) < epsilon) return target;
            else return value;
        }

        public static float Slam(this float value, float target) {
            return value.Slam(target, EPSILON);
        }

		/// <summary>
        /// Wraps a value around some significant range.
        /// 
        /// Similar to modulo, but works in a unary direction over any range (including negative values).
        /// 
        /// ex:
        /// Wrap(8,6,2) == 4
        /// Wrap(4,2,0) == 0
        /// Wrap(4,2,-2) == 0
        /// </summary>
        /// <param name="value">value to wrap</param>
        /// <param name="max">max in range</param>
        /// <param name="min">min in range</param>
        /// <returns>A value wrapped around min to max</returns>
        /// <remarks></remarks>
        public static int Wrap(int value, int max, int min) {
            max -= min;
            if (max == 0)
                return min;

            return value - max * (int)Math.Floor((double)(value - min) / max);
        }
        public static int Wrap(int value, int max) {
            return Wrap(value, max, 0);
        }

        public static long Wrap(long value, long max, long min) {
            max -= min;
            if (max == 0)
                return min;

            return value - max * (long)Math.Floor((double)(value - min) / max);
        }
        public static long Wrap(long value, long max) {
            return Wrap(value, max, 0);
        }

        public static float Wrap(float value, float max, float min) {
            max -= min;
            if (max == 0)
                return min;

            return value - max * (float)Math.Floor((value - min) / max);
        }
        public static float Wrap(float value, float max) {
            return Wrap(value, max, 0);
        }

		/// <summary>
        /// Set an angle with in the bounds of -PI to PI.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static float NormalizeAngle(this float angle, bool useRadians) {
            float rd = (useRadians ? PI : 180);
            return Wrap(angle, rd, -rd);
        }

		/// <summary>
        /// Closest angle from a1 to a2.
        /// Absolute value the return for exact angle.
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static float NearestAngleBetween(float a1, float a2, bool useRadians) {
            var rd = useRadians ? PI : 180f;
            var ra = Wrap(a2 - a1, rd * 2f);
            if (ra > rd) ra -= (rd * 2f);
            return ra;
        }

        /// <summary>
        /// Returns a value for dependant that is a value that is the shortest angle between dep and ind from ind.
        /// 
        /// For instance if dep=-190 degrees and ind=10 degrees then 170 degrees will be returned 
        /// since the shortest path from 10 to -190 is to rotate to 170.
        /// </summary>
        /// <param name="dep"></param>
        /// <param name="ind"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static float ShortenAngleToAnother(this float dep, float ind, bool useRadians) {
            return ind + NearestAngleBetween(ind, dep, useRadians);
        }

		/// <summary>
        /// Returns a value for dependant that is the shortest angle in the positive direction from ind.
        /// 
        /// for instance if dep=-170 degrees, and ind=10 degrees, then 190 degrees will be returned as an alternative to -170. 
        /// Since 190 is the smallest angle > 10 equal to -170.
        /// </summary>
        /// <param name="dep"></param>
        /// <param name="ind"></param>
        /// <param name="useRadians"></param>
        /// <returns></returns>
        public static float NormalizeAngleToAnother(this float dep, float ind, bool useRadians) {
            float div = useRadians ? TWO_PI : 360f;
            float v = (dep - ind) / div;
            return dep - (float)Math.Floor(v) * div;
        }

		/// <summary>
		/// Extensions for converting one data type to another
		/// </summary>
		#region /// Conversion ///
			/// /// <summary>
			/// Returns the fractional part of a float.
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			/// <remarks></remarks>
			public static float Shear(float value) {
			    return value % 1.0f;
			}

			/// <summary>
			/// Converts an int to float
			/// </summary>
			public static float ToFloat(this int value) {
				return (float)value;
			}

			/// <summary>
			/// Converts an int to a char
			/// </summary>
			public static char ToChar(this int value) {
				return Convert.ToChar(value);
			}

			/// <summary>
			/// Converts a string to an int
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="defaultValue">default value if could not convert</param>
			public static int ToInt(this string value, int defaultValue) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return defaultValue;

				// convert
				int rVal;
				return int.TryParse(value, out rVal) ? rVal : defaultValue;
			}

			/// <summary>
			/// Converts a string to a nullable int
			/// </summary>
			/// <param name="value">value to convert</param>
			public static int? ToIntNull(this string value) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return null;

				// convert
				int rVal;
				return int.TryParse(value, out rVal) ? rVal : new int?();
			}

			/// <summary>
			/// Converts a string to an long
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="defaultValue">default value if could not convert</param>
			public static long ToLong(this string value, long defaultValue) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return defaultValue;

				// convert
				long rVal;
				return long.TryParse(value, out rVal) ? rVal : defaultValue;
			}

			/// <summary>
			/// Converts a string to a nullable long
			/// </summary>
			/// <param name="value">value to convert</param>
			public static long? ToLongNull(this string value) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return null;

				// convert
				long rVal;
				return long.TryParse(value, out rVal) ? rVal : new long?();
			}

			/// <summary>
			/// Converts a string to a decimal
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="defaultValue">default value if could not convert</param>
			public static double ToDouble(this string value, double defaultValue) {
			    // exit if null
			    if (string.IsNullOrEmpty(value)) return defaultValue;

			    // convert
			    double rVal;
			    return double.TryParse(value, out rVal) ? rVal : defaultValue;
			}

			/// <summary>
			/// Converts a string to a decimal
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="defaultValue">default value if could not convert</param>
			public static decimal ToDecimal(this string value, decimal defaultValue) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return defaultValue;

				// convert
				decimal rVal;
				return decimal.TryParse(value, out rVal) ? rVal : defaultValue;
			}

			/// <summary>
			/// Converts a string to a nullable decimal
			/// </summary>
			/// <param name="value">value to convert</param>
			public static decimal? ToDecimalNull(this string value) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return null;

				// convert
				decimal rVal;
				return decimal.TryParse(value, out rVal) ? rVal : new decimal?();
			}

			/// <summary>
			/// Converts a string to a float
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="defaultValue">default value if could not convert</param>
			public static float ToFloat(this string value, float defaultValue) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return defaultValue;
			
				// convert
				float rVal;
				return float.TryParse(value, out rVal) ? rVal : defaultValue;
			}

			/// <summary>
			/// Converts a string to a nullable float
			/// </summary>
			/// <param name="value">value to convert</param>
			public static float? ToFloatNull(this string value) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return null;

				// convert
				float rVal;
				return float.TryParse(value, out rVal) ? rVal : new float?();
			}

			/// <summary>
			/// Converts a string to a bool
			/// </summary>
			/// <param name="value">value to convert</param>
			/// <param name="defaultValue">default value if could not convert</param>
			public static bool ToBool(this string value, bool defaultValue) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return defaultValue;

				// convert
				bool rVal;
				return bool.TryParse(value, out rVal) ? rVal : defaultValue;
			}

			/// <summary>
			/// Converts a string to a nullable bool
			/// </summary>
			/// <param name="value">value to convert</param>
			public static bool? ToBoolNull(this string value) {
				// exit if null
				if (string.IsNullOrEmpty(value)) return null;

				// convert
				bool rVal;
				return bool.TryParse(value, out rVal) ? rVal : new bool?();
			}

			/// <summary>
			/// Convert radians to degrees.
			/// </summary>
			/// <param name="angle"></param>
			/// <returns></returns>
			/// <remarks></remarks>
			public static float RadiansToDegrees(this float angle) {
			    return angle * RAD_TO_DEG;
			}

			/// <summary>
			/// Convert degrees to radians.
			/// </summary>
			/// <param name="angle"></param>
			/// <returns></returns>
			/// <remarks></remarks>
			public static float DegreesToRadians(this float angle) {
			    return angle * DEG_TO_RAD;
			}
		#endregion
		
		/// <summary>
		/// Extensions for sizing computer terms (KB, MB, GB, etc)
		/// </summary>
		#region /// Computer Sizing ///
			/// <summary>
			/// one kilobyte
			/// </summary>
			private const int INT_OneKB = 1024;

			/// <summary>
			/// Converts to kilobyte size
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static int KB(this int value) {
				return value * INT_OneKB;
			}

			/// <summary>
			/// Converts to megabyte size (1024^2 bytes)
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static int MB(this int value) {
				return value * INT_OneKB * INT_OneKB;
			}

			/// <summary>
			/// Converts to gigabyte size (1024^3 bytes)
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static int GB(this int value) {
				return value * INT_OneKB * INT_OneKB * INT_OneKB;
			}

			/// <summary>
			/// Converts to terabyte size (1024^4 bytes)
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static int TB(this int value) {
				return value * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB;
			}

			/// <summary>
			/// Converts to petabyte size (1024^5 bytes)
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static int PB(this int value) {
				return value * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB;
			}

			/// <summary>
			/// Converts to exabyte size (1024^6 bytes)
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static int EB(this int value) {
				return value * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB;
			}

			/// <summary>
			/// Converts to zettabyte size (1024^7 bytes)
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static int ZB(this int value) {
				return value * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB;
			}

			/// <summary>
			/// Converts to yottabyte size (1024^8 bytes)
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static int YB(this int value) {
				return value * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB * INT_OneKB;
			}
		#endregion
	#endregion

	/// <summary>
	/// Extensions for TimeSpan.
	/// </summary>
	#region ===== TimeSpan =====
	    /// <summary>
		/// Creates a timespan with <paramref name="seconds"/> seconds
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static TimeSpan SecondsToTimeSpan(this int seconds) => TimeSpan.FromSeconds(seconds);

		/// <summary>
		/// Creates a timespan with <paramref name="minutes"/> minutes
		/// </summary>
		/// <param name="minutes"></param>
		/// <returns></returns>
		public static TimeSpan MinutesToTimeSpan(this int minutes) => TimeSpan.FromMinutes(minutes);
	#endregion

	/// <summary>
	/// Extensions for strings.
	/// </summary>
	#region ===== Strings =====
		/// <summary>
    	/// returns true ONLY if the entire string is numeric
    	/// </summary>
    	/// <param name="input">the string to test</param>
		public static bool IsNumeric(this string input) {
			// return false if no string
			return (!String.IsNullOrEmpty(input)) && (new Regex(@"^-?[0-9]*\.?[0-9]+$").IsMatch(input.Trim()));
			//Valid user input
		}

		/// <summary>
		/// returns true if any part of the string is numeric
		/// </summary>
		/// <param name="input">the string to test</param>
		public static bool HasNumeric(this string input) {
			// if no string, return false
			return (!String.IsNullOrEmpty(input)) && (new Regex(@"[0-9]+").IsMatch(input));
		}

		/// <summary>
		/// Returns true if value is a date
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsDate(this string value) {
			try {
				DateTime tempDate;
				return DateTime.TryParse(value, out tempDate);
			}

			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Returns true if value is an int
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsInt(this string value) {
			try {
				int tempInt;
				return int.TryParse(value, out tempInt);
			}

			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Like LINQ take - takes the first x characters
		/// </summary>
		/// <param name="value">the string</param>
		/// <param name="count">number of characters to take</param>
		/// <param name="ellipsis">true to add ellipsis (...) at the end of the string</param>
		/// <returns></returns>
		public static string Take(this string value, int count, bool ellipsis = false) {
			// get number of characters we can actually take
			int lengthToTake = Math.Min(count, value.Length);

			// Take and return
			return (ellipsis && lengthToTake < value.Length) ? string.Format("{0}...", value.Substring(0, lengthToTake)) : value.Substring(0, lengthToTake);
		}

		/// <summary>
		/// like LINQ skip - skips the first x characters and returns the remaining string
		/// </summary>
		/// <param name="value">the string</param>
		/// <param name="count">number of characters to skip</param>
		/// <returns></returns>
		public static string Skip(this string value, int count) => value.Substring(Math.Min(count, value.Length) - 1);

		/// <summary>
		/// Reverses the string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string Reverse(this string input) {
			char[] chars = input.ToCharArray();
			Array.Reverse(chars);
			return new String(chars);
		}

		/// <summary>
		/// Null or empty check as extension
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

		/// <summary>
		/// Returns true if the string is Not null or empty
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsNOTNullOrEmpty(this string value) => (!string.IsNullOrEmpty(value));

		/// <summary>
		/// "a string {0}".Formatted("blah") vs string.Format("a string {0}", "blah")
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string Formatted(this string format, params object[] args) => string.Format(format, args);

		/// <summary>
		/// ditches html tags - note it doesn't get rid of things like nbsp;
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static string StripHtml(this string html) {
			if (html.IsNullOrEmpty()) return string.Empty;

			return Regex.Replace(html, @"<[^>]*>", string.Empty);
		}

		/// <summary>
		/// Returns true if the pattern matches.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static bool Match(this string value, string pattern) => Regex.IsMatch(value, pattern);

		/// <summary>
		/// Remove white space, not line end.
		/// Useful when parsing user input such phone,
		/// price int.Parse("1 000 000".RemoveSpaces(),.....
		/// </summary>
		/// <param name="value"></param>
		public static string RemoveSpaces(this string value) => value.Replace(" ", string.Empty);

		/// <summary>
		/// Converts a null or "" to string.empty. Useful for XML code. Does nothing if <paramref name="value"/> already has a value
		/// </summary>
		/// <param name="value">string to convert</param>
		public static string ToEmptyString(string value) => (string.IsNullOrEmpty(value)) ? string.Empty : value;

		/*
		* Converting a sequence to a nicely-formatted string is a bit of a pain. 
		* The String.Join method definitely helps, but unfortunately it accepts an 
		* array of strings, so it does not compose with LINQ very nicely.
		* 
		* My library includes several overloads of the ToStringPretty operator that 
		* hides the uninteresting code. Here is an example of use:
		* 
		* Console.WriteLine(Enumerable.Range(0, 10).ToStringPretty("From 0 to 9: [", ",", "]"));
		* 
		* The output of this program is:
		* 
		* From 0 to 9: [0,1,2,3,4,5,6,7,8,9]
		*/

		/// <summary>
		/// Returns a comma delimited string.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		public static string ToStringPretty<T>(this IEnumerable<T> source) => (source == null) ? string.Empty : ToStringPretty(source, ",");

		/// <summary>
		/// Returns a single string, delimited with <paramref name="delimiter"/> from source.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="delimiter"></param>
		/// <returns></returns>
		public static string ToStringPretty<T>(this IEnumerable<T> source, string delimiter) => (source == null) ? string.Empty : ToStringPretty(source, string.Empty, delimiter, string.Empty);

		/// <summary>
		/// Returns a delimited string, appending <paramref name="before"/> at the start,
		/// and <paramref name="after"/> at the end of the string
		/// Ex: Enumerable.Range(0, 10).ToStringPretty("From 0 to 9: [", ",", "]")
		/// returns: From 0 to 9: [0,1,2,3,4,5,6,7,8,9]
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="before"></param>
		/// <param name="delimiter"></param>
		/// <param name="after"></param>
		/// <returns></returns>
		public static string ToStringPretty<T>(this IEnumerable<T> source, string before, string delimiter, string after) {
			if (source == null) return string.Empty;

			StringBuilder result = new StringBuilder();
			result.Append(before);

			bool firstElement = true;
			foreach (T elem in source) {
				if (firstElement) firstElement = false;
				else result.Append(delimiter);

				result.Append(elem.ToString());
			}

			result.Append(after);
			return result.ToString();
		}

		/// <summary>
		/// Inverts the case of each character in the given string and returns the new string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>The converted string.</returns>
		public static string InvertCase(this string s) => new string(s.Select(c => char.IsLetter(c) ? (char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)) : c).ToArray());

		/// <summary>
		/// Checks whether the given string is null, else if empty after trimmed.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>True if string is Null or Empty, false otherwise.</returns>
		public static bool IsNullOrEmptyAfterTrimmed(this string s) => (s.IsNullOrEmpty() || s.Trim().IsNullOrEmpty());

		/// <summary>
		/// Converts the given string to <see cref="List{Char}"/>.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>Returns a list of char (or null if string is null).</returns>
		public static List<char> ToCharList(this string s) => (s.IsNOTNullOrEmpty()) ? s.ToCharArray().ToList() : null;

		/// <summary>
		/// Extracts the substring starting from 'start' position to 'end' position.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="start">The start position.</param>
		/// <param name="end">The end position.</param>
		/// <returns>The substring.</returns>
		public static string SubstringFromXToY(this string s, int start, int end) {
			if (s.IsNullOrEmpty()) return string.Empty;

			// if start is past the length of the string
			if (start >= s.Length) return string.Empty;

			// if end is beyond the length of the string, reset
			if (end >= s.Length) end = s.Length - 1;

			return s.Substring(start, end - start);
		}

		/// <summary>
		/// Removes the given character from the given string and returns the new string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="c">The character to be removed.</param>
		/// <returns>The new string.</returns>
		public static string RemoveChar(this string s, char c) => (s.IsNOTNullOrEmpty()) ? s.Replace(c.ToString(), string.Empty) : string.Empty;

		/// <summary>
		/// Returns the number of words in the given string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>The word count.</returns>
		public static int GetWordCount(this string s) => (new Regex(@"\w+")).Matches(s).Count;

		/// <summary>
		/// Checks whether the given string is a palindrome.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>True if the given string is palindrome, false otherwise.</returns>
		public static bool IsPalindrome(this string s) => s.Equals(s.Reverse());

		/// <summary>
		/// Checks whether the given string is NOT a palindrome.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>True if the given string is NOT palindrome, false otherwise.</returns>
		public static bool IsNotPalindrome(this string s) => s.IsPalindrome().Toggle();

		/// <summary>
		/// Checks whether the given string is a valid IP address using regular expressions.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>True if it is a valid IP address, false otherwise.</returns>
		public static bool IsValidIPAddress(this string s) {
			return Regex.IsMatch(s, @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");
		}

		/// <summary>
		/// Appends the given separator to the given string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="sep">The separator to be appended.</param>
		/// <returns>The appended string.</returns>
		public static string AppendSep(this string s, string sep) => s + sep;

		/// <summary>
		/// Appends the a comma to the given string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>The appended string.</returns>
		public static string AppendComma(this string s) => s.AppendSep(",");

		/// <summary>
		/// Appends \r \n to a string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>The appended string.</returns>
		public static string AppendNewLine(this string s) => s.AppendSep("\r\n");

		/// <summary>
		/// Appends \r \n to a string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>The appended string.</returns>
		public static string AppendHtmlBr(this string s) => s.AppendSep("<br />");

		/// <summary>
		/// Appends the a space to the given string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>The appended string.</returns>
		public static string AppendSpace(this string s) => s.AppendSep(" ");

		/// <summary>
		/// Appends the a hyphen to the given string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <returns>The appended string.</returns>
		public static string AppendHyphen(this string s) => s.AppendSep("-");

		/// <summary>
		/// Appends the given character to the given string and returns the new string.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="sep">The character to be appended.</param>
		/// <returns>The appended string.</returns>
		public static string AppendSep(this string s, char sep) => s.AppendSep(sep.ToString());

		/// <summary>
		/// Returns this string + sep + newString.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="newString">The string to be concatenated.</param>
		/// <param name="sep">The separator to be introduced in between these two strings.</param>
		/// <returns>The appended string.</returns>
		/// <remarks>This may give poor performance for large number of strings used in loop. Use <see cref="StringBuilder"/> instead.</remarks>
		public static string AppendWithSep(this string s, string newString, string sep) => s.AppendSep(sep) + newString;

		/// <summary>
		/// Returns this string + sep + newString.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="newString">The string to be concatenated.</param>
		/// <param name="sep">The separator to be introduced in between these two strings.</param>
		/// <returns>The appended string.</returns>
		/// <remarks>This may give poor performance for large number of strings used in loop. Use <see cref="StringBuilder"/> instead.</remarks>
		public static string AppendWithSep(this string s, string newString, char sep) => s.AppendSep(sep) + newString;

		/// <summary>
		/// Returns this string + "," + newString.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="newString">The string to be concatenated.</param>
		/// <returns>The appended string.</returns>
		/// <remarks>This may give poor performance for large number of strings used in loop. Use <see cref="StringBuilder"/> instead.</remarks>
		public static string AppendWithComma(this string s, string newString) => s.AppendWithSep(newString, ",");

		/// <summary>
		/// Returns this string + "\r\n" + newString.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="newString">The string to be concatenated.</param>
		/// <returns>The appended string.</returns>
		/// <remarks>This may give poor performance for large number of strings used in loop. Use <see cref="StringBuilder"/> instead.</remarks>
		public static string AppendWithNewLine(this string s, string newString) => s.AppendWithSep(newString, "\r\n");

		/// <summary>
		/// Returns this string + "\r\n" + newString.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="newString">The string to be concatenated.</param>
		/// <returns>The appended string.</returns>
		/// <remarks>This may give poor performance for large number of strings used in loop. Use <see cref="StringBuilder"/> instead.</remarks>
		public static string AppendWithHtmlBr(this string s, string newString) => s.AppendWithSep(newString, "<br />");

		/// <summary>
		/// Returns this string + "-" + newString.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="newString">The string to be concatenated.</param>
		/// <returns>The appended string.</returns>
		/// <remarks>This may give poor performance for large number of strings used in loop. Use <see cref="StringBuilder"/> instead.</remarks>
		public static string AppendWithHyphen(this string s, string newString) => s.AppendWithSep(newString, "-");

		/// <summary>
		/// Returns this string + " " + newString.
		/// </summary>
		/// <param name="s">The given string.</param>
		/// <param name="newString">The string to be concatenated.</param>
		/// <returns>The appended string.</returns>
		/// <remarks>This may give poor performance for large number of strings used in loop. Use <see cref="StringBuilder"/> instead.</remarks>
		public static string AppendWithSpace(this string s, string newString) => s.AppendWithSep(newString, " ");

		/// <summary>
		/// Converts the specified string to title case (except for words that are entirely in uppercase, which are considered to be acronyms).
		/// </summary>
		/// <param name="mText"></param>
		/// <returns></returns>
		public static string ToTitleCase(this string mText) {
			if (mText.IsNullOrEmpty())  return mText;

			// get globalization info
			System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
			System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;

			// convert to title case
			return textInfo.ToTitleCase(mText);
		}

		/// <summary>
		/// Adds extra spaces to meet the total length
		/// </summary>
		/// <param name="s"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string PadRightEx(this string s, int length) {
			// exit if string is already at length
			if ((!s.IsNullOrEmpty()) && (s.Length >= length)) return s;

			// if string is null, then return empty string
			// else, add spaces and exit
			return (s != null) ? string.Format("{0}{1}", s, new string(' ', length - s.Length)) : new string(' ', length);
		}
	#endregion

	/// <summary>
	/// Extensions for booleans.
	/// </summary>
	#region ===== Booleans =====
		/// <summary>
		/// Checks whether the given boolean item is true.
		/// </summary>
		/// <param name="item">Item to be checked.</param>
		/// <returns>True if the value is true, false otherwise.</returns>
		public static bool IsTrue(this bool item) {
			return item;
		}

		/// <summary>
		/// Checks whether the given boolean item is false.
		/// </summary>
		/// <param name="item">Item to be checked.</param>
		/// <returns>True if the value is false, false otherwise.</returns>
		public static bool IsFalse(this bool item) {
			return !item;
		}

		/// <summary>
		/// Checks whether the given boolean item is NOT true.
		/// </summary>
		/// <param name="item">Item to be checked.</param>
		/// <returns>True if the item is false, false otherwise.</returns>
		public static bool IsNotTrue(this bool item) {
			return !item.IsTrue();
		}

		/// <summary>
		/// Checks whether the given boolean item is NOT false.
		/// </summary>
		/// <param name="item">Item to be checked.</param>
		/// <returns>True if the value is true, false otherwise.</returns>
		public static bool IsNotFalse(this bool item) {
			return !item.IsFalse();
		}

		/// <summary>
		/// Toggles the given boolean item and returns the toggled value.
		/// </summary>
		/// <param name="item">Item to be toggled.</param>
		/// <returns>The toggled value.</returns>
		public static bool Toggle(this bool item) {
		    return !item;
		}

		/// <summary>
		/// Converts the given boolean value to integer.
		/// </summary>
		/// <param name="item">The boolean variable.</param>
		/// <returns>Returns 1 if true , 0 otherwise.</returns>
		public static int ToInt(this bool item) {
		    return item ? 1 : 0;
		}

		/// <summary>
		/// Returns the lower string representation of boolean.
		/// </summary>
		/// <param name="item">The boolean variable.</param>
		/// <returns>Returns "true" or "false".</returns>
		public static string ToLowerString(this bool item) {
		    return item.ToString().ToLower();
		}

		/// <summary>
		/// Returns the trueString or falseString based on the given boolean value.
		/// </summary>
		/// <param name="item">The boolean value.</param>
		/// <param name="trueString">Value to be returned if the condition is true.</param>
		/// <param name="falseString">Value to be returned if the condition is false.</param>
		/// <returns>Returns trueString if the given value is true otherwise falseString.</returns>
		public static string ToString(this bool item, string trueString, string falseString) {
		    return item.ToType<string>(trueString, falseString);
		}

		/// <summary>
		/// Returns the trueValue or the falseValue based on the given boolean value.
		/// </summary>
		/// <param name="item">The boolean value.</param>
		/// <param name="trueValue">Value to be returned if the condition is true.</param>
		/// <param name="falseValue">Value to be returned if the condition is false.</param>
		/// <typeparam name="T">Instance of any class.</typeparam>
		/// <returns>Returns trueValue if the given value is true otherwise falseValue.</returns>
		public static T ToType <T>(this bool item, T trueValue, T falseValue) {
		    return item ? trueValue : falseValue;
		}

		#region /// Comparables ///
			/// <summary>
			/// Returns true if the actual value is between lower and upper, Inclusive (ie, lower an upper are both allowed in the test)
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="actual">actual value to test</param>
			/// <param name="lower">inclusive lower limit</param>
			/// <param name="upper">inclusive upper limit</param>
			/// <returns></returns>
			public static bool IsBetweenInclusive<T>(this T actual, T lower, T upper) where T : IComparable<T> {
				return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) <= 0;
			}

			/// <summary>
			/// Returns true if the actual value is between lower and upper, Exclusive (ie, lower allowed in the test, upper is not allowed in the test)
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="actual">actual value to test</param>
			/// <param name="lower">inclusive lower limit</param>
			/// <param name="upper">exclusive upper limit</param>
			/// <returns></returns>
			public static bool IsBetweenExclusive<T>(this T actual, T lower, T upper) where T : IComparable<T> {
				return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
			}
		#endregion
	#endregion
	}
}

/// <summary>
/// Percentage type.
/// </summary>
public struct Percentage {
	/// <summary>
    /// The percentage as a value
    /// </summary>
    public decimal Value { get; private set; }

    /// <summary>
    /// Returns the value as a percentage (Value / 100)
    /// </summary>
    public decimal ValueAsPercentage {
        get { return Value / 100; }
    }

	/// <summary>
    /// Init
    /// </summary>
    /// <param name="value"></param>
    public Percentage(int value) : this() {
        Value = value;
    }

	/// <summary>
    /// Init
    /// </summary>
    /// <param name="value"></param>
    public Percentage(decimal value) : this() {
        Value = value;
    }

	/// <summary>
    /// Minus (ex. 5 - 10% = 4.5).
    /// </summary>
    /// <param name="value">the decimal value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator -(decimal value, Percentage percentage) => value - (value * percentage);

	/// <summary>
    /// Minus (ex. 5 - 10% = 4.5).
    /// </summary>
    /// <param name="value">the int value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator -(int value, Percentage percentage) => value - (value * percentage);

	/// <summary>
    /// Minus (ex. 10% - 5 = -4.5).
    /// </summary>
    /// <param name="value">the decimal value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator -(Percentage percentage, decimal value) => (value * percentage) - value;

    /// <summary>
    /// Minus (ex. 10% - 5 = -4.5).
    /// </summary>
    /// <param name="value">the int value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator -(Percentage percentage, int value) => (value * percentage) - value;

    /// <summary>
    /// Subtract two percentages (ex. 10% - 8% = 2%).
    /// </summary>
    /// <param name="percentage">value 1</param>
    /// <param name="value">value 2</param>
    public static Percentage operator -(Percentage value, Percentage percentage) => new Percentage(value.Value - percentage.Value);

	/// <summary>
    /// Add (ex. 5 + 10% = 5.5).
    /// </summary>
    /// <param name="value">the decimal value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator +(decimal value, Percentage percentage) => value + (value * percentage);

    /// <summary>
    /// Add (ex. 5 + 10% = 5.5)
    /// </summary>
    /// <param name="value">the int value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator +(int value, Percentage percentage) => value + (value * percentage);

    /// <summary>
    /// Add (ex. 10% + 5 = 5.5)
    /// </summary>
    /// <param name="value">the decimal value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator +(Percentage percentage, decimal value) => value + (value * percentage);

    /// <summary>
    /// Add (ex. 10% + 5 = 5.5)
    /// </summary>
    /// <param name="value">the int value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator +(Percentage percentage, int value) => value + (value * percentage);

    /// <summary>
    /// Add two percentages (ex. 10% + 8% = 18%)
    /// </summary>
    /// <param name="percentage">value 1</param>
    /// <param name="value">value 2</param>
    public static Percentage operator +(Percentage value, Percentage percentage) => new Percentage(value.Value + percentage.Value);

	/// <summary>
    /// Multiply (ex. 5 * 10% = 0.5)
    /// </summary>
    /// <param name="value">the decimal value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator *(decimal value, Percentage percentage) => value * percentage.ValueAsPercentage;

    /// <summary>
    /// Multiply (ex. 5 * 10% = 0.5)
    /// </summary>
    /// <param name="value">the int value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator *(int value, Percentage percentage) => value * percentage.ValueAsPercentage;

    /// <summary>
    /// Multiply (ex. 10% * 5 = 0.5)
    /// </summary>
    /// <param name="value">the decimal value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator *(Percentage percentage, decimal value) => value * percentage.ValueAsPercentage;

    /// <summary>
    /// Multiply (ex. 10% * 5 = 0.5)
    /// </summary>
    /// <param name="value">the int value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator *(Percentage percentage, int value) => value * percentage.ValueAsPercentage;

    /// <summary>
    /// Multiply two percentages (ex. 10% * 8% = 0.8%)  (0.1 * 0.08 = 0.008)
    /// </summary>
    /// <param name="percentage">value 1</param>
    /// <param name="value">value 2</param>
    public static Percentage operator *(Percentage value, Percentage percentage) => new Percentage(value.ValueAsPercentage * percentage.ValueAsPercentage);

	/// <summary>
    /// Divide (ex. 5 / 10% = 50)
    /// </summary>
    /// <param name="value">the decimal value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator /(decimal value, Percentage percentage) => value / percentage.ValueAsPercentage;

    /// <summary>
    /// Divide (ex. 5 / 10% = 50)
    /// </summary>
    /// <param name="value">the int value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator /(int value, Percentage percentage) => value / percentage.ValueAsPercentage;

    /// <summary>
    /// Divide (ex. 10% / 5 = 0.02)
    /// </summary>
    /// <param name="value">the decimal value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator /(Percentage percentage, decimal value) => percentage.ValueAsPercentage / value;

    /// <summary>
    /// Divide (ex. 10% / 5 = 0.002)
    /// </summary>
    /// <param name="value">the int value</param>
    /// <param name="percentage">the percentage value</param>
    public static decimal operator /(Percentage percentage, int value) => percentage.ValueAsPercentage / value;

    /// <summary>
    /// Divide two percentages (ex. 10% / 8% = 125%)  (0.1 / 0.08 = 1.25)
    /// </summary>
    /// <param name="percentage">value 1</param>
    /// <param name="value">value 2</param>
    public static Percentage operator /(Percentage value, Percentage percentage) => new Percentage(value.ValueAsPercentage / percentage.ValueAsPercentage);
}

[Serializable]
public struct Optional<T> {
	[SerializeField] private bool enabled;
	[SerializeField] private T value;

	public Optional(T initialValue) {
		enabled = true;
		value = initialValue;
	}

	public bool Enabled => enabled;
	public T Value => value;
}