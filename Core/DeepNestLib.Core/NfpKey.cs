namespace DeepNestLib
{
    public class NfpKey : IStringify
    {

        public NFP A;
        public NFP B;
        public float ARotation { get; set; }
        public float BRotation { get; set; }
        public bool Inside { get; set; }

        public int AIndex { get; set; }
        public int BIndex { get; set; }
        public object Asource;
        public object Bsource;


        public string stringify()
        {
            return $"A:{AIndex} B:{BIndex} inside:{Inside} Arotation:{ARotation} Brotation:{BRotation}";
        }
    }
}

