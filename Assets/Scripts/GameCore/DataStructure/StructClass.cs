//Author@Archer
using System;
using System.Collections.Generic;

namespace DataCore.StructClass
{
	public class PriorityQueue<T> where T : IComparable<T>
	{
		private List<T> list = new List<T>();

		public int Count { get => list.Count; }
		public void Push(T item)
		{
			list.Add(item);
			int i = Count - 1;
			while (i > 0) // if not root
			{
				int j = (i - 1) / 2; // get parent inode
				if (list[j].CompareTo(item) <= 0)
				{
					break;
				}
				list[i] = list[j]; // else swap
				i = j;
			}
			list[i] = item;
		}
		public T Pop()
		{
			T item = list[0];
			T last = list[Count - 1];
			list.RemoveAt(Count - 1);
			if (Count > 0)
			{
				int i = 0; // 从根节点开始向下调整
				while (i * 2 + 1 < Count) // 如果有左子节点
				{
					int j = i * 2 + 1; // 获取左子节点的索引
					if (j < Count - 1 && list[j].CompareTo(list[j + 1]) > 0) // 如果有右子节点，并且右子节点小于左子节点
					{
						j++; // 获取右子节点的索引
					}
					if (list[j].CompareTo(last) >= 0) // 如果子节点大于等于最后一个元素，说明满足最小堆的性质，退出循环
					{
						break;
					}
					list[i] = list[j];
					i = j;
				}
				list[i] = last; // 将最后一个元素放到最终位置
			}
			return item; // 返回最小（或最大）的元素
		}

		// 返回最小（或最大）的元素，但不移除
		public T Peek()
		{
			return list[0]; // 返回根节点，即最小（或最大）的元素
		}
	}
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	//public class ObjectPool<T> where T : new(), 
	//{
	//	private List<T> pool;

	//	public ObjectPool()
	//	{
	//		pool = new List<T>();
	//	}

	//	// 获取一个空闲的对象，如果没有则创建一个新的对象
	//	public T Get()
	//	{
	//		// 遍历列表，寻找空闲的对象
	//		for (int i = 0; i < pool.Count; i++)
	//		{
	//			// 假设有一个IsActive属性来判断对象是否被使用中
	//			if (!pool[i].IsActive)
	//			{
	//				// 返回空闲的对象，并设置其为使用中状态
	//				pool[i].IsActive = true;
	//				return pool[i];
	//			}
	//		}
	//		// 如果没有空闲的对象，则创建一个新的对象，并添加到列表中
	//		T obj = new T();
	//		obj.IsActive = true;
	//		pool.Add(obj);
	//		return obj;
	//	}

	//	// 回收一个使用过的对象，将其设置为空闲状态
	//	public void Recycle(T obj)
	//	{
	//		obj.IsActive = false;
	//	}

	//	// 扩展对象池的容量，创建一定数量的新对象，并添加到列表中
	//	public void Expand(int count)
	//	{
	//		for (int i = 0; i < count; i++)
	//		{
	//			T obj = new T();
	//			obj.IsActive = false;
	//			pool.Add(obj);
	//		}
	//	}

	//	// 清理对象池，移除所有空闲的对象，并释放内存空间
	//	public void Clear()
	//	{
	//		for (int i = pool.Count - 1; i >= 0; i--)
	//		{
	//			if (!pool[i].IsActive)
	//			{
	//				pool.RemoveAt(i);
	//			}
	//		}
	//	}
	//}
}
