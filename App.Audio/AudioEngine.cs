using OpenTK.Audio.OpenAL;

namespace App.Audio
{
    public class AudioEngine
    {
        private ALDevice _device;
        private ALContext _context;

        public void Initialize()
        {
            _device = ALC.OpenDevice(null);

            _context = ALC.CreateContext(_device, (int[])null);
            ALC.MakeContextCurrent(_context);

            AL.DistanceModel(ALDistanceModel.LinearDistanceClamped);
        } 

        public void UpdateListener(double x,  double y, double rotationDegrees)
        {
            AL.Listener(ALListener3f.Position, (float)x, 0f, (float)y);

            double radians = rotationDegrees * Math.PI / 180.0;
            float lookX = (float)Math.Cos(radians);
            float lookZ = (float)Math.Sin(radians);

            float[] orientation = { lookX, 0f, lookZ, 0f, 1f, 0f };
            AL.Listener(ALListenerfv.Orientation, ref orientation[0]);
        }

        public void AddLoopingSoundSource(string filePath, double x, double y)
        {
            if (!File.Exists(filePath)) return;

            //Bufor i wirtualny głośnik
            int buffer = AL.GenBuffer();
            int source = AL.GenSource();

            //Odczytanie nagłówka pliku wav
            using (FileStream stream = File.OpenRead(filePath)) 
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.ReadBytes(22);
                short channels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                reader.ReadBytes(6);
                short bitsPerSample = reader.ReadInt16();
                reader.ReadBytes(4);
                int dataSize = reader.ReadInt32();

                byte[] audioData = reader.ReadBytes(dataSize);

                ALFormat format = ALFormat.Mono16;
                if (channels == 1 && bitsPerSample == 8) format = ALFormat.Mono8;
                else if (channels == 1 && bitsPerSample == 16) format = ALFormat.Mono16;
                else if (channels == 2 && bitsPerSample == 8) format = ALFormat.Stereo8;
                else if (channels == 2 && bitsPerSample == 16) format = ALFormat.Stereo16;

                AL.BufferData(buffer, format, audioData, sampleRate);
            }

            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.Source(source, ALSourceb.Looping, true);

            AL.Source(source, ALSource3f.Position, (float)x, 0f, (float)y);

            AL.Source(source, ALSourcef.ReferenceDistance, 2f);

            AL.Source(source, ALSourcef.MaxDistance, 800f);

            AL.Source(source, ALSourcef.RolloffFactor, 1f);

            AL.SourcePlay(source);
        }
    }
}
