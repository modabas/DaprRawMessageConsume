using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using static Dapr.AppCallback.Autogen.Grpc.v1.AppCallback;

namespace DaprRawMessageConsume.Services
{
    public class DaprService : AppCallbackBase
    {
        private readonly ILogger<DaprService> _logger;
        private const string pubsubName = "take2pubsub";
        private const string legacyEventsTopicName = "legacy_events";

        public DaprService(ILogger<DaprService> logger)
        {
            _logger = logger;
        }

        public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
        {
            var result = new ListTopicSubscriptionsResponse();
            var ts = new TopicSubscription()
            {
                PubsubName = pubsubName,
                Topic = legacyEventsTopicName,
            };
            
            //to signal dapr sidecar consumer that events in this topic are not wrapped in cloudevent envelope
            ts.Metadata.Add("rawPayload", "true");

            result.Subscriptions.Add(ts);

            return Task.FromResult(result);
        }

        public override Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
        {
            using (_logger.BeginScope("{topicEventId}", request.Id))
            {
                _logger.LogInformation("Topic event received. Path: {path}, PubsubName: {pubsubName}, Source: {source}, SpecVersion: {specVersion}, Topic: {topic}, Type: {type}, DataContentType: {dataContentType}",
                    request.Path, request.PubsubName, request.Source, request.SpecVersion, request.Topic, request.Type, request.DataContentType);

                switch (request.PubsubName)
                {
                    case pubsubName:
                        switch (request.Topic)
                        {
                            case legacyEventsTopicName:
                                return LegacyEventHandler(request, context);

                            default:
                                _logger.LogError("Undefined topicName: {topicName}", request.Topic);
                                return Task.FromResult(new TopicEventResponse() { Status = TopicEventResponse.Types.TopicEventResponseStatus.Drop });
                        }

                    default:
                        _logger.LogError("Undefined pubsubName: {pubsubName}", request.PubsubName);
                        return Task.FromResult(new TopicEventResponse() { Status = TopicEventResponse.Types.TopicEventResponseStatus.Drop });
                }
            }
        }

        private Task<TopicEventResponse> LegacyEventHandler(TopicEventRequest request, ServerCallContext context)
        {
            string? payload;
            try
            {
                payload = LegacyEventDeserializer(request.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot deserialize topic event data.");
                return Task.FromResult(new TopicEventResponse() { Status = TopicEventResponse.Types.TopicEventResponseStatus.Drop });
            }
            _logger.LogInformation("Payload : {payload}", payload);

            //Process event
            //...
            //...
            
            return Task.FromResult(new TopicEventResponse() { Status = TopicEventResponse.Types.TopicEventResponseStatus.Success });
        }

        private string LegacyEventDeserializer(ByteString data)
        {
            //use deserializer compatible with legacy publisher on raw data bytes received
            return data.ToStringUtf8();
        }
    }
}
