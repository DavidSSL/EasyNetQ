using System;
using System.Linq;


namespace EasyNetQ
{
	public delegate string ExchangeNameConvention(Type messageType);
	public delegate string TopicNameConvention(Type messageType);
    public delegate string QueueNameConvention(Type messageType, string subscriberId);
    public delegate string RpcRoutingKeyNamingConvention(Type messageType);

    public delegate string ErrorQueueNameConvention();
    public delegate string ErrorExchangeNameConvention(MessageReceivedInfo info);
    public delegate string RpcExchangeNameConvention();

    public delegate string RpcReturnQueueNamingConvention();

    public delegate string ConsumerTagConvention();

	public interface IConventions
	{
		ExchangeNameConvention ExchangeNamingConvention { get; set; }
		TopicNameConvention TopicNamingConvention { get; set; }
        QueueNameConvention QueueNamingConvention { get; set; }
        RpcRoutingKeyNamingConvention RpcRoutingKeyNamingConvention { get; set; }

        ErrorQueueNameConvention ErrorQueueNamingConvention { get; set; }
        ErrorExchangeNameConvention ErrorExchangeNamingConvention { get; set; }
        RpcExchangeNameConvention RpcExchangeNamingConvention { get; set; }
        RpcReturnQueueNamingConvention RpcReturnQueueNamingConvention { get; set; }

        ConsumerTagConvention ConsumerTagConvention { get; set; }
	}

	public class Conventions : IConventions
	{
		public Conventions(ITypeNameSerializer typeNameSerializer)
		{
		    Preconditions.CheckNotNull(typeNameSerializer, "typeNameSerializer");

		    // Establish default conventions.
            ExchangeNamingConvention = messageType =>
            {
                var attr = GetQueueAttribute(messageType);

                return string.IsNullOrEmpty(attr.ExchangeName)
                    ? typeNameSerializer.Serialize(messageType)
                    : attr.ExchangeName;
            };
			
            TopicNamingConvention = messageType => "";
			
            QueueNamingConvention =
					(messageType, subscriptionId) =>
					{
                        var attr = GetQueueAttribute(messageType);

                        if (string.IsNullOrEmpty(attr.QueueName))
                        {
                            var typeName = typeNameSerializer.Serialize(messageType);
                            return string.Format("{0}_{1}", typeName, subscriptionId);
                        }

                        return string.IsNullOrEmpty(subscriptionId)
                            ? attr.QueueName
                            : string.Format("{0}_{1}", attr.QueueName, subscriptionId);
					};
            RpcRoutingKeyNamingConvention = typeNameSerializer.Serialize;

            ErrorQueueNamingConvention = () => "EasyNetQ_Default_Error_Queue";
		    ErrorExchangeNamingConvention = info => "ErrorExchange_" + info.RoutingKey;
            RpcExchangeNamingConvention = () => "easy_net_q_rpc";
		    RpcReturnQueueNamingConvention = () => "easynetq.response." + Guid.NewGuid().ToString();

            ConsumerTagConvention = () => Guid.NewGuid().ToString();
		}

        private QueueAttribute GetQueueAttribute(Type messageType)
        {
            var attr = messageType.GetCustomAttributes(typeof(QueueAttribute), true).FirstOrDefault() as QueueAttribute;

            return attr ?? new QueueAttribute(string.Empty);
        }

		public ExchangeNameConvention ExchangeNamingConvention { get; set; }
		public TopicNameConvention TopicNamingConvention { get; set; }
        public QueueNameConvention QueueNamingConvention { get; set; }
        public RpcRoutingKeyNamingConvention RpcRoutingKeyNamingConvention { get; set; }

        public ErrorQueueNameConvention ErrorQueueNamingConvention { get; set; }
        public ErrorExchangeNameConvention ErrorExchangeNamingConvention { get; set; }
        public RpcExchangeNameConvention RpcExchangeNamingConvention { get; set; }
        public RpcReturnQueueNamingConvention RpcReturnQueueNamingConvention { get; set; }

        public ConsumerTagConvention ConsumerTagConvention { get; set; }
	}
}