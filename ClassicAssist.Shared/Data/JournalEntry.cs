using ClassicAssist.UO.Data;

namespace ClassicAssist.Data
{
    public sealed class JournalEntry
    {
        public string[] Arguments { get; internal set; }
        public int Cliloc { get; internal set; }
        public int ID { get; internal set; }
        public string Name { get; internal set; }
        public int Serial { get; internal set; }
        public int SpeechFont { get; internal set; }
        public int SpeechHue { get; internal set; }
        public string SpeechLanguage { get; internal set; }
        public JournalSpeech SpeechType { get; internal set; }
        public string Text { get; internal set; }
    }
}