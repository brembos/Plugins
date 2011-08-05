using System;

namespace $rootnamespace$
{
	public class MessageProcessingMode
    {
        public const int Synchronous = 0;
        public const int Asynchronous = 1;
    }

    public class MessageProcessingStage
    {
        public const int PreValidation = 10;
		public const int PreOperation = 20;
		public const int MainOperation = 30; // crm only
		public const int PostOperation = 40;
		public const int PostOperationCRM4 = 50; // deprecated
    }
}
