namespace Alex.API.Data.Options
{
    public class AlexOptions : OptionsBase
    {

        public OptionsProperty<int> FieldOfVision { get; set; }


        public AlexOptions()
        {
            FieldOfVision = DefineRangedProperty(80, 30, 120);

        }
    }
}
