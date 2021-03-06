// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transports
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Context;
    using Pipeline;


    public class ScopePublishEndpoint :
        IPublishEndpoint
    {
        readonly ConsumeContext _context;
        readonly IPublishEndpoint _publishEndpoint;

        public ScopePublishEndpoint(ConsumeContext context, IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
            _context = context;
        }

        async Task IPublishEndpoint.Publish<T>(T message, CancellationToken cancellationToken)
        {
            var contextPipe = new ScopePublishContextPipe<T>(_context);

            await _publishEndpoint.Publish(message, contextPipe, cancellationToken).ConfigureAwait(false);
        }

        async Task IPublishEndpoint.Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken)
        {
            var contextPipe = new ScopePublishContextPipe<T>(publishPipe, _context);

            await _publishEndpoint.Publish(message, contextPipe, cancellationToken).ConfigureAwait(false);
        }

        async Task IPublishEndpoint.Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken)
        {
            var contextPipe = new ScopePublishContextPipe<T>(publishPipe, _context);

            await _publishEndpoint.Publish(message, contextPipe, cancellationToken).ConfigureAwait(false);
        }

        Task IPublishEndpoint.Publish(object message, CancellationToken cancellationToken)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Type messageType = message.GetType();

            return PublishEndpointConverterCache.Publish(this, message, messageType, cancellationToken);
        }

        Task IPublishEndpoint.Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Type messageType = message.GetType();

            return PublishEndpointConverterCache.Publish(this, message, messageType, publishPipe, cancellationToken);
        }

        Task IPublishEndpoint.Publish(object message, Type messageType, CancellationToken cancellationToken)
        {
            return PublishEndpointConverterCache.Publish(this, message, messageType, cancellationToken);
        }

        Task IPublishEndpoint.Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken)
        {
            return PublishEndpointConverterCache.Publish(this, message, messageType, publishPipe, cancellationToken);
        }

        async Task IPublishEndpoint.Publish<T>(object values, CancellationToken cancellationToken)
        {
            var contextPipe = new ScopePublishContextPipe<T>(_context);

            await _publishEndpoint.Publish(values, contextPipe, cancellationToken).ConfigureAwait(false);
        }

        async Task IPublishEndpoint.Publish<T>(object values, IPipe<PublishContext<T>> publishPipe,
            CancellationToken cancellationToken)
        {
            var contextPipe = new ScopePublishContextPipe<T>(publishPipe, _context);

            await _publishEndpoint.Publish(values, contextPipe, cancellationToken).ConfigureAwait(false);
        }

        async Task IPublishEndpoint.Publish<T>(object values, IPipe<PublishContext> publishPipe,
            CancellationToken cancellationToken)
        {
            var contextPipe = new ScopePublishContextPipe<T>(publishPipe, _context);

            await _publishEndpoint.Publish(values, contextPipe, cancellationToken).ConfigureAwait(false);
        }

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer)
        {
            return _publishEndpoint.ConnectPublishObserver(observer);
        }
    }
}