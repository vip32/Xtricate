namespace Xtricate.Common
{
    public class SeqStep
    {
        public SeqStepType Type { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Description { get; set; }

        public string Render()
        {
            if (Type == SeqStepType.Call)
                return $"{From} ->+ {To} : {Description}\n";
            if (Type == SeqStepType.CallSelf)
                return $"{From} ->- {To} : {Description}\n";
            if (Type == SeqStepType.Self)
                return $"{From} -> {To} : {Description}\n";
            if (Type == SeqStepType.Note)
                return $"note right of {From} : {Description}\n";
            if (Type == SeqStepType.Return)
                return $"{From} -->- {To} : {Description}\n";

            return "";
        }
    }
}