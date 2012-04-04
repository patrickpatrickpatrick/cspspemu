﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu
{
	unsafe public class GpuImplMock : GpuImpl
	{
		public override void InitializeComponent()
		{
		}

		public override void InitSynchronizedOnce()
		{
		}

		public override void StopSynchronized()
		{
		}

		public override void Prim(GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
		}

		public override void Finish(GpuStateStruct* GpuState)
		{
		}

		public override void End(GpuStateStruct* GpuState)
		{
		}

		public override void AddedDisplayList()
		{
		}

		public override void SetCurrent()
		{
		}

		public override void UnsetCurrent()
		{
		}

		public override PluginInfo PluginInfo
		{
			get
			{
				return new PluginInfo()
				{
					Name = "Mock",
					Version = "1.0",
				};
			}
		}
	}
}