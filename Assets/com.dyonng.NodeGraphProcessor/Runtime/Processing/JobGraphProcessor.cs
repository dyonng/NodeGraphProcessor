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
	public class JobGraphProcessor : BaseGraphProcessor
	{
		GraphScheduleList[]			scheduleList;
		
		internal class GraphScheduleList
		{
			public BaseNode			node;
			public BaseNode[]		dependencies;
	
			public GraphScheduleList(BaseNode node)
			{
				this.node = node;
			}
		}

		/// <summary>
		/// Manage graph scheduling and processing
		/// </summary>
		/// <param name="graph">Graph to be processed</param>
		public JobGraphProcessor(BaseGraph graph) : base(graph) {}

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

			scheduleList = new GraphScheduleList[indices.Length];
			for (int i = 0; i < indices.Length; i++)
			{
				var n = nodesArray[indices[i]];
				GraphScheduleList gsl = new GraphScheduleList(n);
				var inputNodes = new List<BaseNode>();
				foreach (var dep in n.GetInputNodes())
					inputNodes.Add(dep);
				gsl.dependencies = inputNodes.ToArray();
				scheduleList[i] = gsl;
			}
		}

		/// <summary>
		/// Schedule the graph into the job system
		/// </summary>
		public override void Run()
		{
			int count = scheduleList.Length;
			var scheduledHandles = new Dictionary< BaseNode, JobHandle >();

			for (int i = 0; i < count; i++)
			{
				JobHandle dep = default(JobHandle);
				var schedule = scheduleList[i];
				int dependenciesCount = schedule.dependencies.Length;

				for (int j = 0; j < dependenciesCount; j++)
					dep = JobHandle.CombineDependencies(dep, scheduledHandles[schedule.dependencies[j]]);

				// TODO: call the onSchedule on the current node
				// JobHandle currentJob = schedule.node.OnSchedule(dep);
				// scheduledHandles[schedule.node] = currentJob;
			}

			JobHandle.ScheduleBatchedJobs();
		}
	}
}
