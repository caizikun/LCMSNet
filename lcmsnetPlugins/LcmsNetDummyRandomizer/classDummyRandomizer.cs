﻿//*********************************************************************************************************
// Written by Dave Clark, Brian LaMarche for the US Department of Energy 
// Pacific Northwest National Laboratory, Richland, WA
// Copyright 2009, Battelle Memorial Institute
// Created 03/04/2009
//
// Last modified 03/04/2009
//*********************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LcmsNetDataClasses;
using System.Collections;

namespace LcmsNet.SampleQueue
{
	[classPlugInDisplayName("Invert")]
	public class classDummyRandomizer : IRandomizerInterface, IEnumerable
	{
		//*********************************************************************************************************
		// Plugin for testing randomizer loading. This merely inverts the input list
		//**********************************************************************************************************

		#region "Class variables"
			List<long> mobj_Items;
		#endregion

		#region "Methods"
			/// <summary>
			/// Implements IRandomizerInterface's RandomizeSamples method
			/// </summary>
			/// <param name="InputSampleList">List containing samples to be randomized</param>
			/// <returns>List containing same samples with sequence numbers randomized</returns>
			public List<classSampleData> RandomizeSamples(List<classSampleData> InputSampleList)
			{
				// Create a list of the sequence numbers from the input sample list
				List<long> SeqList = GetSeqNumbers(InputSampleList);
				// Invert the sequence number list
				SeqList.Reverse();
				// Reassign the sequence numbers and return
				for (int Indx = 0; Indx < InputSampleList.Count(); Indx++)
				{
					InputSampleList[Indx].SequenceID = SeqList[Indx];
				}
				return InputSampleList;
			}

			/// <summary>
			/// Implements IEnumerable's GetEnumerator method for returning a list's enumerator
			/// </summary>
			/// <returns>Enumerator</returns>
			public IEnumerator GetEnumerator()
			{
				return mobj_Items.GetEnumerator();
			}

			/// <summary>
			/// Retrieves the sequence numbers from input sample list
			/// </summary>
			/// <param name="InpSamples">List of sample data objects</param>
			/// <returns>List of sequence numbers</returns>
			List<long> GetSeqNumbers(List<classSampleData> InpSamples)
			{
				List<long> SeqNums = new List<long>();
				foreach (classSampleData Sample in InpSamples)
				{
					SeqNums.Add(Sample.SequenceID);
				}
				return SeqNums;
			}	
		#endregion
	}	
}	// End namespace
