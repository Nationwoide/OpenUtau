﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NAudio.Wave;
using OpenUtau.Core;
using OpenUtau.Core.SignalChain;

namespace OpenUtau.Classic {
    interface IWavtool {
        // <output file> <input file> <STP> <note length>
        // [<p1> <p2> <p3> <v1> <v2> <v3> [<v4> <overlap> <p4> [<p5> <v5>]]]
        float[] Concatenate(List<ResamplerItem> resamplerItems, CancellationTokenSource cancellation);
    }

    class SharpWavtool : IWavtool {
        public float[] Concatenate(List<ResamplerItem> resamplerItems, CancellationTokenSource cancellation) {
            if (cancellation.IsCancellationRequested) {
                return null;
            }
            var mix = new WaveMix(resamplerItems.Select(item => {
                var posMs = item.phone.position * item.phrase.tickToMs - item.phone.preutterMs;
                var source = new WaveSource(posMs, item.requiredLength, item.phone.envelope, item.skipOver, 1);
                if (File.Exists(item.outputFile)) {
                    source.SetWaveData(File.ReadAllBytes(item.outputFile));
                } else {
                    source.SetWaveData(new byte[0]);
                }
                return source;
            }));
            var file = resamplerItems[0].outputFile.Replace("res", "cat");
            WaveFileWriter.CreateWaveFile16(file, new ExportAdapter(mix).ToMono(1, 0));
            Serilog.Log.Information(file);

            //var export = new ExportAdapter(mix);
            //var samples = new List<float>();
            //var buffer = new float[44100];
            //int n;
            //while ((n = export.Read(buffer, 0, buffer.Length)) > 0) {
            //    samples.AddRange(buffer.Take(n));
            //}
            //return samples.ToArray();
            return new float[0];
        }
    }
}
