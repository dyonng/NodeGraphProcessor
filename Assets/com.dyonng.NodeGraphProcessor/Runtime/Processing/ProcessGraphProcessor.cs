using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
// using Unity.Entities;

namespace GraphProcessor
{

	/// <summary>
	/// Graph processor
	/// </summary>
	public class ProcessGraphProcessor : BaseGraphProcessor
	{
		List< BaseNode >		processList;
		
		/// <summary>
		/// Manage graph scheduling and processing
		/// </summary>
		/// <param name="graph">Graph to be processed</param>
		public ProcessGraphProcessor(BaseGraph graph) : base(graph) {}

		public override void UpdateComputeOrder()
		{
			var nodesArray = graph.nodes.ToArray();
			var indices = new int[nodesArray.Length];
			for (int i = 0; i < indices.Length; i++)
				indices[i] = i;

			// Stable sort by computeOrder (ties keep original order)
			Array.Sort(indices, (a, b) => {
				int cmp = nodesArray[a].computeOrder.CompareTo(nodesArray[b].computeOrder);
				return cmp != 0 ? cmp : a.CompareTo(b);
			});

			processList = new List<BaseNode>(indices.Length);
			foreach (var idx in indices)
				processList.Add(nodesArray[idx]);
		}

		/// <summary>
		/// Process all the nodes following the compute order.
		/// </summary>
		public override void Run()
		{
			int count = processList.Count;

			for (int i = 0; i < count; i++)
				processList[i].OnProcess();
		}
	}
}
