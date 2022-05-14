using System;
using MeltySynth;
using DrippyAL;

public static class Program
{
    public static void Main()
    {
        var sampleRate = 44100;

        var synthesizer = new Synthesizer("TimGM6mb.sf2", sampleRate);
        var sequencer = new MidiFileSequencer(synthesizer);
        var midiFile = new MidiFile(@"C:\Windows\Media\flourish.mid");
        sequencer.Play(midiFile, true);

        using (var device = new AudioDevice())
        using (var stream = new AudioStream(device, sampleRate, 2))
        {
            stream.Play(data => sequencer.RenderInterleavedInt16(data));

            Console.ReadKey();

            stream.Pitch = 0.5F;

            Console.ReadKey();
        }
    }
}
